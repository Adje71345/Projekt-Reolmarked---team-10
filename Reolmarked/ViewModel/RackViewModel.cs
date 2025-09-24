using System.Collections.ObjectModel;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;

namespace Reolmarked.ViewModel
{
    public sealed class RackSlot
    {
        public int RackId { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public bool IsOccupied { get; init; }
        public bool IsHorizontal { get; init; }
    }

    public class RackViewModel : ViewModelBase
    {
        public ObservableCollection<Rack> Racks { get; }
        public ObservableCollection<RackSlot> RackSlots { get; } = new();

        private Rack _selectedRack;
        public Rack SelectedRack
        {
            get => _selectedRack;
            set => SetProperty(ref _selectedRack, value);
        }

        private ViewModelBase _currentRackPanel;
        public ViewModelBase CurrentRackPanel
        {
            get => _currentRackPanel;
            private set => SetProperty(ref _currentRackPanel, value);
        }

        public ICommand SelectRackCommand { get; }
        public ICommand SelectRackByIdCommand { get; }

        // Mål (vandret/lodret)
        private const double W_H = 44;  // vandret bredde
        private const double H_H = 28;  // vandret højde
        private const double W_V = 28;  // lodret bredde
        private const double H_V = 44;  // lodret højde
        private const double GAP = 10;

        public RackViewModel()
        {
            Racks = new ObservableCollection<Rack>(Rack.CreateDefaultRacks());
            CurrentRackPanel = new RackHomeViewModel();

            SelectRackCommand = new DelegateCommand<Rack>(
                r => { if (r != null) SelectAndShow(r); },
                r => r != null
            );

            SelectRackByIdCommand = new DelegateCommand<int>(
                id => { var r = FindRack(id); if (r != null) SelectAndShow(r); },
                id => id > 0
            );

            BuildSlots();
        }

        private void SelectAndShow(Rack r)
        {
            SelectedRack = r;
            CurrentRackPanel = new RackSelectedViewModel(r);
        }

        private Rack FindRack(int id) =>
            id <= 0 ? null : System.Linq.Enumerable.FirstOrDefault(Racks, x => x.RackId == id);

        private static readonly System.Collections.Generic.HashSet<int> OCCUPIED =
            new System.Collections.Generic.HashSet<int>(new[] { 12, 28, 43, 56, 61, 63, 65, 68, 70, 71, 73, 75, 78, 80 });

        private (double x, double y) Cell(int col, int row, bool horizontal, double ox = 0, double oy = 0)
        {
            double w = horizontal ? W_H : W_V;
            double h = horizontal ? H_H : H_V;
            return (ox + col * (w + GAP), oy + row * (h + GAP));
        }

        private void AddSlot(int rackId, int col, int row, bool horizontal, double ox = 0, double oy = 0)
        {
            var p = Cell(col, row, horizontal, ox, oy);
            RackSlots.Add(new RackSlot
            {
                RackId = rackId,
                X = p.x,
                Y = p.y,
                IsHorizontal = horizontal,
                IsOccupied = OCCUPIED.Contains(rackId)
            });
        }

        private void BuildSlots()
        {
            RackSlots.Clear();

            // Offsets for grupper
            double OX_LEFT = 10;
            double OY_TOP = 10;   // fælles top for midter- og højreblokke
            double OX_BLOCK2 = 170;  // 19..24
            double OX_COLS1 = 300;  // 25..38
            double OX_COLS2 = 500;  // 39..52
            double OX_COLS3 = 700;  // 53..66
            double OX_RIGHT = 900;  // 67..76
            double OX_STACKS = 1060; // 77..80 (samme lodrette række)
            double OY_BOTTOM = 460;  // bundrække 13..1 rykket længere ned

            // Venstre enkelt søjle med reoler (14..18)
            // Reol 14 ligger på "rækkeindex 4" i denne kolonne.
            double OY_LEFTCOL = OY_BOTTOM - H_V - 12 - 4 * (H_V + GAP);

            AddSlot(14, 0, 4, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(15, 0, 3, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(16, 0, 2, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(17, 0, 1, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(18, 0, 0, false, OX_LEFT, OY_LEFTCOL);

            // 2x3 blok (lodrette reoler) 19..24
            AddSlot(21, 0, 0, false, OX_BLOCK2, OY_TOP);
            AddSlot(20, 0, 1, false, OX_BLOCK2, OY_TOP);
            AddSlot(19, 0, 2, false, OX_BLOCK2, OY_TOP);
            AddSlot(24, 1, 0, false, OX_BLOCK2, OY_TOP);
            AddSlot(23, 1, 1, false, OX_BLOCK2, OY_TOP);
            AddSlot(22, 1, 2, false, OX_BLOCK2, OY_TOP);

            // (25..31) og (32..38) – lodrette
            for (int i = 0; i < 7; i++)
            {
                AddSlot(25 + i, 0, 6 - i, false, OX_COLS1, OY_TOP);
                AddSlot(32 + i, 1, 6 - i, false, OX_COLS1, OY_TOP);
            }

            // (39..45) og (46..52) – lodrette
            for (int i = 0; i < 7; i++)
            {
                AddSlot(39 + i, 0, 6 - i, false, OX_COLS2, OY_TOP);
                AddSlot(46 + i, 1, 6 - i, false, OX_COLS2, OY_TOP);
            }

            // (53..59) og (60..66) – lodrette
            for (int i = 0; i < 7; i++)
            {
                AddSlot(53 + i, 0, 6 - i, false, OX_COLS3, OY_TOP);
                AddSlot(60 + i, 1, 6 - i, false, OX_COLS3, OY_TOP);
            }

            // (67..71) og (72..76) – 5 i højden, lodrette
            for (int i = 0; i < 5; i++)
            {
                AddSlot(67 + i, 0, 4 - i, false, OX_RIGHT, OY_TOP);
                AddSlot(72 + i, 1, 4 - i, false, OX_RIGHT, OY_TOP);
            }

            // 77..80 i SAMME lodrette række (vandrette kasser)
            double OY_STACKS = 190;   // lodret start for 80
            AddSlot(80, 0, 0, true, OX_STACKS, OY_STACKS);
            AddSlot(79, 0, 1, true, OX_STACKS, OY_STACKS);
            AddSlot(78, 0, 2, true, OX_STACKS, OY_STACKS);
            AddSlot(77, 0, 3, true, OX_STACKS, OY_STACKS);

            // Bundrække 13..1 – vandrette fra venstre mod højre
            for (int i = 0; i < 13; i++)
            {
                int id = 13 - i;
                AddSlot(id, i, 0, true, OX_LEFT + 120, OY_BOTTOM);
            }
        }

        public void GoToCreate(Rack rack)
        {
            if (rack == null) return;
            SelectedRack = rack;
            CurrentRackPanel = new RackCreateRentalContactViewModel(rack);
        }
    }
}
