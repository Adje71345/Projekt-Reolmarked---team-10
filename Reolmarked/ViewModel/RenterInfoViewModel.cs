using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;

namespace Reolmarked.ViewModel
{
    public class RenterInfoViewModel : ViewModelBase
    {
        private Renter _selectedRenter;
        public Renter SelectedRenter
        {
            get => _selectedRenter;
            set => SetProperty(ref _selectedRenter, value);
        }

        public DummyStats DummyStats { get; private set; } = new DummyStats();
        public ObservableCollection<RentalContract> ActiveContracts { get; } = new ObservableCollection<RentalContract>();

        public ICommand CloseCommand { get; }
        public ICommand EditCommand { get; }

        // Commands for contract actions
        public ICommand TerminateCommand { get; }

        private readonly Action _closeAction;
        private readonly Action<Renter> _editAction;

        public RenterInfoViewModel(Action closeAction, Action<Renter> editAction)
        {
            _closeAction = closeAction ?? throw new ArgumentNullException(nameof(closeAction));
            _editAction = editAction ?? throw new ArgumentNullException(nameof(editAction));

            CloseCommand = new RelayCommand(() => _closeAction());
            EditCommand = new RelayCommand(() => _editAction(SelectedRenter), () => SelectedRenter != null);

            TerminateCommand = new RelayCommand<RentalContract>(c => Terminate(c));

            // Dummy initial data so UI shows something. Replace when real data is available.
            SeedDummyData();
        }

        public void LoadRenter(Renter renter)
        {
            SelectedRenter = renter;
            // update dummy stats if you want to derive from renter; kept static here
            CommandManager.InvalidateRequerySuggested();
        }

        private void SeedDummyData()
        {
            // Stats
            DummyStats.ActiveShelves = 2;
            DummyStats.SoldThisMonth = 19;
            DummyStats.RevenueThisMonth = 2340;

            // Contracts
            ActiveContracts.Clear();
            ActiveContracts.Add(new RentalContract { ShelfName = "Reol 12", Period = "1. sep - 30. sep" });
            ActiveContracts.Add(new RentalContract { ShelfName = "Reol 13", Period = "1. sep - 30. sep" });
        }

        private void Terminate(RentalContract contract)
        {
            ActiveContracts.Remove(contract);
        }
    }

    // Simple support classes for dummy data
    public class DummyStats
    {
        public int ActiveShelves { get; set; }
        public int ItemsForSale { get; set; }
        public int SoldThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
    }

    public class RentalContract
    {
        public string ShelfName { get; set; }
        public string Period { get; set; }
    }
}
