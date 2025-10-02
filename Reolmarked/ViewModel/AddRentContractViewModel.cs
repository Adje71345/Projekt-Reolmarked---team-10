using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class AddRentContractViewModel : ViewModelBase
    {
        // Lister leveres fra parent
        public ObservableCollection<Renter> Renters { get; }
        private readonly List<PaymentMethod> _paymentMethods;

        // Callbacks til parent
        private readonly Action<SubmitData> _onSubmit;
        private readonly Action _onClose;
        private readonly Action<int>? _onChangeRackId;

        // Commands
        public ICommand SubmitCommand { get; }
        public ICommand CloseCommand { get; }

        //Properties til binding
        // Reol-id
        private int _rackId;
        public int RackId
        {
            get => _rackId;
            set
            {
                if (SetProperty(ref _rackId, value))
                {
                    _onChangeRackId?.Invoke(_rackId);
                }
            }
        }

        // Valgt lejer (ComboBox)
        private Renter? _selectedRenter;
        public Renter? SelectedRenter
        {
            get => _selectedRenter;
            set
            {
                if (SetProperty(ref _selectedRenter, value) && value != null)
                {
                    RenterId = value.RenterId;
                    OnPropertyChanged(nameof(PaymentMethodName));
                    OnPropertyChanged(nameof(CanSubmit));
                    RaiseSubmitCanExecute();
                }
            }
        }


        // Lejer-id (synkroniseres med SelectedRenter)
        private int _renterId;
        public int RenterId
        {
            get => _renterId;
            set
            {
                if (SetProperty(ref _renterId, value))
                {
                    var r = Renters.FirstOrDefault(x => x.RenterId == _renterId);
                    OnPropertyChanged(nameof(PaymentMethodName));
                }
            }
        }

        // Hjælpeproperty til at finde valgt lejer ud fra RenterId
        private Renter? Renter => Renters.FirstOrDefault(r => r.RenterId == RenterId);
        // Betalingsmetode(Visning af betalingsmetode for valgt lejer)
        public string? PaymentMethodName =>
        _paymentMethods.FirstOrDefault(p => p.PaymentMethodId == Renter?.PaymentMethodId)?.Name;


        // Startdato (default i dag)
        private DateTime? _startDateTime = DateTime.Today;
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set
            {
                if (SetProperty(ref _startDateTime, value))
                {
                    OnPropertyChanged(nameof(CanSubmit));
                    RaiseSubmitCanExecute();
                }
            }
        }

        // Slutdato (default null og justeres til sidste dag i måneden)
        private DateTime? _endDateTime = null;
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set
            {
                if (SetProperty(ref _endDateTime, value))
                {
                    if (_endDateTime.HasValue)
                    {
                        var dt = _endDateTime.Value;
                        var lastDay = DateTime.DaysInMonth(dt.Year, dt.Month);
                        var corrected = new DateTime(dt.Year, dt.Month, lastDay);
                        if (corrected != dt)
                        {
                            _endDateTime = corrected;
                            OnPropertyChanged(nameof(EndDateTime));
                        }
                    }

                    OnPropertyChanged(nameof(CanSubmit));
                    RaiseSubmitCanExecute();
                }
            }
        }


        // Checkbox "Ingen slutdato" (nustiller samtidigt EndDateTime)
        private bool _noEnd = false;
        public bool NoEnd
        {
            get => _noEnd;
            set
            {
                if (SetProperty(ref _noEnd, value))
                {
                    OnPropertyChanged(nameof(EndDateEnabled));
                    OnPropertyChanged(nameof(CanSubmit));

                    if (_noEnd)
                    {
                        EndDateTime = null;
                    }

                    RaiseSubmitCanExecute();
                }
            }
        }
        // Wrapper til binding (Slutdato er kun aktiv hvis NoEnd er false)
        public bool EndDateEnabled => !NoEnd;

       
        //Submit logik
        //Submit
        private void Submit()
        {
            if (!NoEnd && EndDateTime.HasValue && EndDateTime < StartDateTime)
            {
                MessageBox.Show("Slutdato må ikke være før startdato.");
                return;
            }

            _onSubmit(new SubmitData(RackId, RenterId, StartDateTime, EndDateTime, NoEnd));
        }
        // CanSubmit (bruges til at enable/disable Submit-knap)
        public bool CanSubmit =>
            SelectedRenter != null &&
            StartDateTime.HasValue &&
            (
                NoEnd ||
                (EndDateTime.HasValue && EndDateTime >= StartDateTime)
            );
        // Metode til at opdatere CanExecute på SubmitCommand
        private void RaiseSubmitCanExecute()
        {
            (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }


        // Constructor
        public AddRentContractViewModel(int rackId, IEnumerable<Renter> renters, IEnumerable<PaymentMethod> paymentMethods,
            Action<SubmitData> onSubmit, Action onClose, Action<int>? onChangeRackId)
        {
            _rackId = rackId;
            Renters = new ObservableCollection<Renter>(renters);
            _paymentMethods = paymentMethods.ToList();

            _onSubmit = onSubmit;
            _onClose = onClose;
            _onChangeRackId = onChangeRackId;

            SubmitCommand = new RelayCommand(Submit, () => CanSubmit);

            CloseCommand = new RelayCommand(_onClose);
        }


        // Payload til parent ved oprettelse af lejekontrakt
        public record SubmitData(int RackId, int RenterId, DateTime? StartDateTime, DateTime? EndDateTime, bool NoEnd);
    }
}
