// Formål: ViewModel til "Opsig lejekontrakt".
// Simple fields (dato + årsag).

using System;
using System.Windows;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class EndRentContractViewModel : ViewModelBase
    {
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly IRenterRepository _renterRepository;
        private readonly Action _onRefreshRackSlots;

        public int RackId { get; }
        
        private int _renterId;
        public int RenterId
        {
            get => _renterId;
            private set => SetProperty(ref _renterId, value);
        }
        public string RenterDisplay { get; }
        public string CurrentEndDateText { get; }

        private DateTime _terminationDate = DateTime.Today;
        public DateTime TerminationDate
        {
            get => _terminationDate;
            set => SetProperty(ref _terminationDate, value);
        }

        public ICommand SubmitCommand { get; }
        public ICommand CloseCommand { get; }

        public record SubmitData(DateTime TerminationDate);

        public EndRentContractViewModel(int rackId, IRentalContractRepository rentalContractRepository, IRenterRepository renterRepository,
            Action onClose, Action onRefreshRackSlots)
        {
            RackId = rackId;
            _rentalContractRepository = rentalContractRepository ?? throw new ArgumentNullException(nameof(rentalContractRepository));
            _renterRepository = renterRepository ?? throw new ArgumentNullException(nameof(renterRepository));
            _onRefreshRackSlots = onRefreshRackSlots ?? (() => { });

            var contract = _rentalContractRepository.GetActiveContractByRack(rackId);
            if (contract != null)
            {
                RenterId = contract.RenterId;

                var renter = _renterRepository.GetById(contract.RenterId);
                RenterDisplay = renter != null
                    ? $"{renter.FirstName} {renter.LastName}"
                    : $"Lejer #{contract.RenterId}";

                CurrentEndDateText = contract.EndDate.HasValue
                    ? contract.EndDate.Value.ToString("dd-MM-yyyy")
                    : "Ingen slutdato";
            }
            else
            {
                RenterId = 0;
                RenterDisplay = "-";
                CurrentEndDateText = "Ingen slutdato";
            }

            SubmitCommand = new RelayCommand(Submit);
            CloseCommand = new RelayCommand(onClose);
        }

        private void Submit()
        {
            if (TerminationDate < DateTime.Today)
            {
                MessageBox.Show("Opsigelsesdato kan ikke være før i dag.");
                return;
            }

            var contract = _rentalContractRepository.GetActiveContractByRack(RackId);
            if (contract == null)
            {
                MessageBox.Show("Der findes ingen aktiv kontrakt for denne reol.");
                return;
            }

            contract.EndDate = DateOnly.FromDateTime(TerminationDate);
            _rentalContractRepository.Update(contract);

            _onRefreshRackSlots();

            MessageBox.Show("Lejekontrakten er opsagt.");
            CloseCommand.Execute(null);
        }
    }
}
