using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;
using Reolmarked.ViewModel.Helpers;

namespace Reolmarked.ViewModel
{
    public class RenterInfoViewModel : ViewModelBase
    {
        // Repositories
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly ISaleLineRepository _saleLineRepository;

        // Properties til binding
        // Valgt lejer
        private Renter _selectedRenter;
        public Renter SelectedRenter
        {
            get => _selectedRenter;
            set => SetProperty(ref _selectedRenter, value);
        }

        // Antal aktive reoler
        private int _activeShelvesCount;
        public int ActiveShelvesCount
        {
            get => _activeShelvesCount;
            set => SetProperty(ref _activeShelvesCount, value);
        }

        // Salg denne måned
        private decimal _soldThisMonth;
        public decimal SoldThisMonth
        {
            get => _soldThisMonth;
            set => SetProperty(ref _soldThisMonth, value);
        }

        // Omsætning denne måned
        private decimal _revenueThisMonth;
        public decimal RevenueThisMonth
        {
            get => _revenueThisMonth;
            set => SetProperty(ref _revenueThisMonth, value);
        }

        // Liste af aktive kontrakter
        public ObservableCollection<RentalContractDisplayModel> ActiveContracts { get; } = new();

        // Commands
        public ICommand CloseCommand { get; }

        // Callbacks til parent
        private readonly Action _closeAction;

        // Constructor
        public RenterInfoViewModel(Action closeAction, IRentalContractRepository rentalContractRepository, 
            ISaleLineRepository saleLineRepository)
        {
            _closeAction = closeAction ?? throw new ArgumentNullException(nameof(closeAction));
            _rentalContractRepository = rentalContractRepository ?? throw new ArgumentNullException(nameof(rentalContractRepository));
            _saleLineRepository = saleLineRepository ?? throw new ArgumentNullException(nameof(saleLineRepository));

            CloseCommand = new RelayCommand(() => _closeAction());
        }

        // Metode til at loade lejer data
        public void LoadRenter(Renter renter)
        {
            SelectedRenter = renter;

            // Hent aktive kontrakter
            var contracts = _rentalContractRepository.GetActiveContractsByRenter(renter.RenterId).ToList();
            ActiveContracts.Clear();
            contracts.ForEach(c => ActiveContracts.Add(new RentalContractDisplayModel(c)));

            // Beregn antal aktive reoler
            var rackIds = contracts.Select(c => c.RackId).Distinct().ToList();
            ActiveShelvesCount = rackIds.Count;

            // Hent salg for denne måned
            var today = DateTime.Today;
            var sales = _saleLineRepository.GetAll()
                .Where(sl => rackIds.Contains(sl.RackId) &&
                            sl.SaleDate.Year == today.Year &&
                            sl.SaleDate.Month == today.Month)
                .ToList();

            SoldThisMonth = sales.Count;
            RevenueThisMonth = sales.Sum(sl => sl.Price);

            CommandManager.InvalidateRequerySuggested();
        }
    }
}
