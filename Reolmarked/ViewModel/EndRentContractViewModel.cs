// Formål: ViewModel til "Opsig lejekontrakt".
// Simple fields (dato + årsag).

using System;
using System.Windows.Input;
using Reolmarked.Commands;

namespace Reolmarked.ViewModel
{
    public class EndRentContractViewModel : ViewModelBase
    {
        public int RackId { get; }
        public string RenterDisplay { get; }
        public string CurrentEndDateText { get; }

        private DateTime _terminationDate = DateTime.Today;
        public DateTime TerminationDate
        {
            get => _terminationDate;
            set => SetProperty(ref _terminationDate, value);
        }

        private string _reason = "";
        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        public ICommand SubmitCommand { get; }
        public ICommand CloseCommand { get; }

        public record SubmitData(DateTime TerminationDate, string Reason);

        public EndRentContractViewModel(int rackId,
                                        string renterDisplay,
                                        DateOnly? currentEndDate,
                                        Action<SubmitData> onSubmit,
                                        Action onClose)
        {
            RackId = rackId;
            RenterDisplay = renterDisplay;
            CurrentEndDateText = currentEndDate.HasValue ? currentEndDate.Value.ToString("dd-MM-yyyy") : "Ingen slutdato";

            SubmitCommand = new RelayCommand(() => onSubmit(new SubmitData(TerminationDate, Reason)));
            CloseCommand = new RelayCommand(onClose);
        }
    }
}
