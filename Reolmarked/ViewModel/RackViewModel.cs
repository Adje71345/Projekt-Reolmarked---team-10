using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Standard;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;
using Reolmarked.ViewModel.Helpers;


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
        private readonly IRackRepository _repo;
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly IRenterRepository _renterRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;
        private System.Collections.Generic.HashSet<int> _occupiedIds = new();

        private Brush rackBackGround = Brushes.Green;
        public Brush RackBackground
        {
            get => rackBackGround;
            set
            {
                rackBackGround = value;
                OnPropertyChanged(nameof(RackBackground));
            }
        }



        public ObservableCollection<Rack> Racks { get; }
        public ObservableCollection<RackSlot> RackSlots { get; } = new();

        // (Valgfrit) datakilder til Add-panelet. Kan være tomme indtil DB er klar.
        public ObservableCollection<Renter> Renters { get; } = new();
        public ObservableCollection<PaymentMethod> PaymentMethods { get; } = new();

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

        private bool _isOverlayOpen;
        public bool IsOverlayOpen
        {
            get => _isOverlayOpen;
            set => SetProperty(ref _isOverlayOpen, value);
        }

        public ICommand CloseOverlayCommand { get; }
        public ICommand SelectRackCommand { get; }
        public ICommand SelectRackByIdCommand { get; }

        // Mål (vandret/lodret)
        private const double W_H = 44;  // vandret bredde
        private const double H_H = 28;  // vandret højde
        private const double W_V = 28;  // lodret bredde
        private const double H_V = 44;  // lodret højde
        private const double GAP = 10;

        public RackViewModel(IRackRepository repo, IRentalContractRepository rentalContractRepository, IRenterRepository renterRepository, IRepository<PaymentMethod> paymentMethodRepository)
        {
            _repo = repo;
            _rentalContractRepository = rentalContractRepository;
            _renterRepository = renterRepository;
            _paymentMethodRepository = paymentMethodRepository;

            // Hent racks fra DB
            Racks = new ObservableCollection<Rack>(_repo.GetAll());

            // Opdater rackstatus baseret på kontrakter
            RefreshRackSlots();

            // Kommandoer
            SelectRackCommand = new DelegateCommand<Rack>(
                r => { if (r != null) SelectAndShow(r); },
                r => r != null
            );

            SelectRackByIdCommand = new DelegateCommand<int>(
                id => { var r = FindRack(id); if (r != null) SelectAndShow(r); },
                id => id > 0
            );

            CloseOverlayCommand = new RelayCommand(() => IsOverlayOpen = false);
        }

        private void SelectAndShow(Rack r)
        {
            SelectedRack = r;
            CurrentRackPanel = new RackSelectedViewModel(
                r,
                _rentalContractRepository,
                _renterRepository,
                goToAddContract: () => ShowAddRentContract(),
                goToEndContract: () => ShowEndRentContract()
            );
            IsOverlayOpen = true; // Dette aktiverer overlayet
        }

        private Rack FindRack(int id) =>
            id <= 0 ? null : System.Linq.Enumerable.FirstOrDefault(Racks, x => x.RackId == id);

        // ---------- NAVIGATION TIL NYE PANELER ----------

        private void ShowAddRentContract()
        {
            var renters = _renterRepository.GetAll();
            var paymentMethods = _paymentMethodRepository.GetAll();

            CurrentRackPanel = new AddRentContractViewModel(
                SelectedRack.RackId,
                renters,
                paymentMethods,
                onSubmit: data =>
                {
                    var contract = new RentalContract
                    {
                        RackId = data.RackId,
                        RenterId = data.RenterId,
                        StartDate = DateOnly.FromDateTime(data.StartDateTime ?? DateTime.Today),
                        EndDate = data.NoEnd ? null : DateOnly.FromDateTime(data.EndDateTime ?? DateTime.Today),
                    };

                    _rentalContractRepository.Add(contract);
                    RefreshRackSlots(); // opdater status og visning

                    // Gå tilbage til rackvisning
                    CurrentRackPanel = new RackSelectedViewModel(
                        SelectedRack,
                        _rentalContractRepository,
                        _renterRepository,
                        () => ShowAddRentContract(),
                        () => ShowEndRentContract()
                    );
                },
                onClose: () =>
                {
                    CurrentRackPanel = new RackSelectedViewModel(
                        SelectedRack,
                        _rentalContractRepository,
                        _renterRepository,
                        () => ShowAddRentContract(),
                        () => ShowEndRentContract()
                    );
                },
                onChangeRackId: null
            );
            IsOverlayOpen = true;
        }

        private void ShowEndRentContract()
        {
            var rack = SelectedRack;
            if (rack == null) return;

            CurrentRackPanel = new EndRentContractViewModel(
                rack.RackId,
                _rentalContractRepository,
                _renterRepository,
                onClose: () =>
                {
                    CurrentRackPanel = new RackSelectedViewModel(
                        rack,
                        _rentalContractRepository,
                        _renterRepository,
                        () => ShowAddRentContract(),
                        () => ShowEndRentContract()
                    );
                },
                onRefreshRackSlots: RefreshRackSlots
            );

            IsOverlayOpen = true;
        }

        // ---------- Tais eksisterende LAYOUT-KODE (placeringer) ----------

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
                IsOccupied = _occupiedIds.Contains(rackId)
            });
        }

        private void BuildSlots()
        {
            // Byg lookup over optagede reoler (RackStatusId == 2)
            _occupiedIds = Racks
                .Where(r => r.RackStatusId == 2)
                .Select(r => r.RackId)
                .ToHashSet();

            RackSlots.Clear();

            double OX_LEFT = 10;
            double OY_TOP = 10;
            double OX_BLOCK2 = 170;
            double OX_COLS1 = 300;
            double OX_COLS2 = 500;
            double OX_COLS3 = 700;
            double OX_RIGHT = 900;
            double OX_STACKS = 1060;
            double OY_BOTTOM = 460;

            double OY_LEFTCOL = OY_BOTTOM - H_V - 12 - 4 * (H_V + GAP);

            AddSlot(14, 0, 4, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(15, 0, 3, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(16, 0, 2, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(17, 0, 1, false, OX_LEFT, OY_LEFTCOL);
            AddSlot(18, 0, 0, false, OX_LEFT, OY_LEFTCOL);

            AddSlot(21, 0, 0, false, OX_BLOCK2, OY_TOP);
            AddSlot(20, 0, 1, false, OX_BLOCK2, OY_TOP);
            AddSlot(19, 0, 2, false, OX_BLOCK2, OY_TOP);
            AddSlot(24, 1, 0, false, OX_BLOCK2, OY_TOP);
            AddSlot(23, 1, 1, false, OX_BLOCK2, OY_TOP);
            AddSlot(22, 1, 2, false, OX_BLOCK2, OY_TOP);

            for (int i = 0; i < 7; i++)
            {
                AddSlot(25 + i, 0, 6 - i, false, OX_COLS1, OY_TOP);
                AddSlot(32 + i, 1, 6 - i, false, OX_COLS1, OY_TOP);
            }

            for (int i = 0; i < 7; i++)
            {
                AddSlot(39 + i, 0, 6 - i, false, OX_COLS2, OY_TOP);
                AddSlot(46 + i, 1, 6 - i, false, OX_COLS2, OY_TOP);
            }

            for (int i = 0; i < 7; i++)
            {
                AddSlot(53 + i, 0, 6 - i, false, OX_COLS3, OY_TOP);
                AddSlot(60 + i, 1, 6 - i, false, OX_COLS3, OY_TOP);
            }

            for (int i = 0; i < 5; i++)
            {
                AddSlot(67 + i, 0, 4 - i, false, OX_RIGHT, OY_TOP);
                AddSlot(72 + i, 1, 4 - i, false, OX_RIGHT, OY_TOP);
            }

            double OY_STACKS = 190;
            AddSlot(80, 0, 0, true, OX_STACKS, OY_STACKS);
            AddSlot(79, 0, 1, true, OX_STACKS, OY_STACKS);
            AddSlot(78, 0, 2, true, OX_STACKS, OY_STACKS);
            AddSlot(77, 0, 3, true, OX_STACKS, OY_STACKS);

            for (int i = 0; i < 13; i++)
            {
                int id = 13 - i;
                AddSlot(id, i, 0, true, OX_LEFT + 120, OY_BOTTOM);
            }
        }
        public void RefreshRackSlots()
        {
            RackStatusHelper.UpdateRackStatusesBasedOnContracts(_repo, _rentalContractRepository);

            // Genindlæs racks fra databasen
            Racks.Clear();
            foreach (var rack in _repo.GetAll())
                Racks.Add(rack);

            BuildSlots();
        }
    }
}
