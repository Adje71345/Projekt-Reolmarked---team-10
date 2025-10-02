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
        // Repositories
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly IRenterRepository _renterRepository;

        // Callbacks til parent
        private readonly Action _onRefreshRackSlots;

        //Commands
        public ICommand SubmitCommand { get; }
        public ICommand CloseCommand { get; }

        // Properties til binding
        // Reol-id som kontrakten hentes ud fra
        public int RackId { get; }

        // Lejer-id som hentes fra kontrakt
        private int _renterId;
        public int RenterId
        {
            get => _renterId;
            private set => SetProperty(ref _renterId, value);
        }

        // Visningsnavn på lejer
        public string RenterDisplay { get; }

        // Dato for opsigelse
        private DateTime _terminationDate = DateTime.Today;
        public DateTime TerminationDate
        {
            get => _terminationDate;
            set
            {
                if (SetProperty(ref _terminationDate, value))
                {
                    OnPropertyChanged(nameof(CalculatedEndDateText));
                    RaiseSubmitCanExecute();
                }
            }
        }

        // Beregnet slutdato baseret på opsigelsesdato
        private DateTime CalculateEndDate(DateTime terminationDate)
        {
            return terminationDate.Day < 20
                ? new DateTime(
                    terminationDate.Year,
                    terminationDate.Month,
                    DateTime.DaysInMonth(terminationDate.Year, terminationDate.Month))
                : new DateTime(
                    terminationDate.AddMonths(1).Year,
                    terminationDate.AddMonths(1).Month,
                    DateTime.DaysInMonth(terminationDate.AddMonths(1).Year, terminationDate.AddMonths(1).Month));
        }
        public string CalculatedEndDateText => CalculateEndDate(TerminationDate).ToString("dd-MM-yyyy");


        //Constructor
        public EndRentContractViewModel(int rackId, IRentalContractRepository rentalContractRepository, IRenterRepository renterRepository, Action onClose, Action onRefreshRackSlots)
        {
            RackId = rackId;
            _rentalContractRepository = rentalContractRepository ?? throw new ArgumentNullException(nameof(rentalContractRepository));
            _renterRepository = renterRepository ?? throw new ArgumentNullException(nameof(renterRepository));
            _onRefreshRackSlots = onRefreshRackSlots ?? (() => { });

            // Hent aktiv kontrakt og lejer
            var contract = _rentalContractRepository.GetActiveContractByRack(rackId);
            if (contract != null)
            {
                RenterId = contract.RenterId;

                var renter = _renterRepository.GetById(contract.RenterId);
                RenterDisplay = renter != null
                    ? $"{renter.FirstName} {renter.LastName}"
                    : $"Lejer #{contract.RenterId}";
            }
            else
            {
                RenterId = 0;
                RenterDisplay = "-";
            }

            // Commands
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
            CloseCommand = new RelayCommand(onClose);
        }


        // Metode til submit
        private void Submit()
        {
            var contract = _rentalContractRepository.GetActiveContractByRack(RackId);

            var endDate = CalculateEndDate(TerminationDate);
            contract.EndDate = DateOnly.FromDateTime(endDate);
            _rentalContractRepository.Update(contract);

            _onRefreshRackSlots();

            MessageBox.Show("Lejekontrakten er opsagt.");
            CloseCommand.Execute(null);
        }

        // Metode til canexecute på submit
        private bool CanSubmit()
        {
            return TerminationDate >= DateTime.Today &&
                   _rentalContractRepository.GetActiveContractByRack(RackId) != null;
        }

        // Opdaterer canexecute
        private void RaiseSubmitCanExecute()
        {
            (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        // Data der sendes ved submit
        public record SubmitData(DateTime TerminationDate);            
    }
}
