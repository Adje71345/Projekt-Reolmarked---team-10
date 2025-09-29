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
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly ISaleLineRepository _saleLineRepository;

        private Renter _selectedRenter;
        public Renter SelectedRenter
        {
            get => _selectedRenter;
            set => SetProperty(ref _selectedRenter, value);
        }

        private int _activeShelvesCount;
        public int ActiveShelvesCount
        {
            get => _activeShelvesCount;
            set => SetProperty(ref _activeShelvesCount, value);
        }

        private decimal _soldThisMonth;
        public decimal SoldThisMonth
        {
            get => _soldThisMonth;
            set => SetProperty(ref _soldThisMonth, value);
        }

        private decimal _revenueThisMonth;
        public decimal RevenueThisMonth
        {
            get => _revenueThisMonth;
            set => SetProperty(ref _revenueThisMonth, value);
        }

        public ObservableCollection<RentalContractDisplayModel> ActiveContracts { get; } = new();

        public ICommand CloseCommand { get; }
        public ICommand EditCommand { get; }

        // Commands for contract actions
        public ICommand TerminateCommand { get; }

        private readonly Action _closeAction;
        private readonly Action<Renter> _editAction;

        public RenterInfoViewModel(Action closeAction, Action<Renter> editAction, IRentalContractRepository rentalContractRepository, 
            ISaleLineRepository saleLineRepository)
        {
            _closeAction = closeAction ?? throw new ArgumentNullException(nameof(closeAction));
            _editAction = editAction ?? throw new ArgumentNullException(nameof(editAction));
            _rentalContractRepository = rentalContractRepository ?? throw new ArgumentNullException(nameof(rentalContractRepository));
            _saleLineRepository = saleLineRepository ?? throw new ArgumentNullException(nameof(saleLineRepository));

            CloseCommand = new RelayCommand(() => _closeAction());
            EditCommand = new RelayCommand(() => _editAction(SelectedRenter), () => SelectedRenter != null);

            /*TerminateCommand = new RelayCommand<RentalContract>(c => Terminate(c));*/
        }

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

        /*private void Terminate(RentalContract contract)
        {
            ActiveContracts.Remove(contract);
        }*/
    }
}
