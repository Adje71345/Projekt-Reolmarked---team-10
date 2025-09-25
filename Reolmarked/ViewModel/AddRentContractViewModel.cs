// Formål: ViewModel til "Opret lejekontrakt"-panelet.
// Bruger WRAPPERS i stedet for converters (EndDateEnabled => !NoEnd). Kunne ikke få converters til at virke!

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;

namespace Reolmarked.ViewModel
{
    public class AddRentContractViewModel : ViewModelBase
    {
        // Lister leveres fra parent (Tais RackViewModel)
        public ObservableCollection<Renter> Renters { get; }
        public ObservableCollection<PaymentMethod> PaymentMethods { get; }

        // Reol-id (kan tastes/ændres)
        private int _rackId;
        public int RackId
        {
            get => _rackId;
            set { if (SetProperty(ref _rackId, value)) _onChangeRackId?.Invoke(_rackId); } // Lui: sync med grid-valg
        }

        // Lejer-id (kan vælges via navneforslag)
        private int _renterId;
        public int RenterId
        {
            get => _renterId;
            set
            {
                if (SetProperty(ref _renterId, value))
                {
                    var r = Renters.FirstOrDefault(x => x.RenterId == _renterId);
                    RenterNameQuery = r == null ? "" : $"{r.FirstName} {r.LastName}";
                }
            }
        }

        // Søgetekst til navneforslag
        private string _renterNameQuery = "";
        public string RenterNameQuery
        {
            get => _renterNameQuery;
            set
            {
                if (SetProperty(ref _renterNameQuery, value))
                {
                    SuggestionsVisible = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
                    OnPropertyChanged(nameof(NameSuggestions)); // opdater liste
                }
            }
        }

        // Model for forslag
        public class NameSuggestion { public int RenterId { get; set; } public string Display { get; set; } = ""; }

        // Filtrerede forslag
        public System.Collections.Generic.List<NameSuggestion> NameSuggestions =>
            Renters
                .Where(r => string.IsNullOrWhiteSpace(RenterNameQuery)
                         || ($"{r.FirstName} {r.LastName}".ToLower().Contains(RenterNameQuery.Trim().ToLower())))
                .Select(r => new NameSuggestion { RenterId = r.RenterId, Display = $"{r.FirstName} {r.LastName} (ID {r.RenterId})" })
                .ToList();

        // Wrapper, gør det synligt for forslag (slipper for BoolToVisibilityConverter)
        private Visibility _suggestionsVisible = Visibility.Collapsed;
        public Visibility SuggestionsVisible
        {
            get => _suggestionsVisible;
            set => SetProperty(ref _suggestionsVisible, value);
        }

        // Datoer (DatePicker binder til DateTime)
        private DateTime? _startDateTime = DateTime.Today;
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set => SetProperty(ref _startDateTime, value);
        }

        private DateTime? _endDateTime = DateTime.Today.AddMonths(1);
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set => SetProperty(ref _endDateTime, value);
        }

        // Vores checkbox"Ingen slutdato" - slår EndDateEnable fra og til og nustiller EndDateTime
        private bool _noEnd = false;
        public bool NoEnd
        {
            get => _noEnd;
            set { if (SetProperty(ref _noEnd, value)) OnPropertyChanged(nameof(EndDateEnabled)); } // Lui
        }

        // Wrapper, som bliver brugt direkte i XAML
        public bool EndDateEnabled => !NoEnd;

        // Betalingsmetode
        private PaymentMethod? _selectedPaymentMethod;
        public PaymentMethod? SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set => SetProperty(ref _selectedPaymentMethod, value);
        }

        // Commands
        public ICommand PickRenterCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand CloseCommand { get; }

        // Callbacks til parent
        private readonly Action<SubmitData> _onSubmit;
        private readonly Action _onClose;
        private readonly Action<int>? _onChangeRackId;

        // Internt payload til submit 
        public record SubmitData(int RackId, int RenterId, DateTime? StartDateTime, DateTime? EndDateTime, bool NoEnd, PaymentMethod? SelectedPaymentMethod);

        public AddRentContractViewModel(
            int rackId,
            System.Collections.Generic.IEnumerable<Renter> renters,
            System.Collections.Generic.IEnumerable<PaymentMethod> paymentMethods,
            Action<SubmitData> onSubmit,
            Action onClose,
            Action<int>? onChangeRackId)
        {
            _rackId = rackId;
            Renters = new ObservableCollection<Renter>(renters);
            PaymentMethods = new ObservableCollection<PaymentMethod>(paymentMethods);
            _onSubmit = onSubmit;
            _onClose = onClose;
            _onChangeRackId = onChangeRackId;

            // Defaults
            var first = Renters.FirstOrDefault();
            if (first != null)
            {
                RenterId = first.RenterId;
                RenterNameQuery = $"{first.FirstName} {first.LastName}";
            }
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault();

            // Vælg lejer fra forslag
            PickRenterCommand = new RelayCommand<int>(rid =>
            {
                RenterId = rid;
                SuggestionsVisible = Visibility.Collapsed;
            });

            // Gem (let validering)
            SubmitCommand = new RelayCommand(() =>
            {
                if (RenterId <= 0 || RackId <= 0 || !StartDateTime.HasValue)
                {
                    MessageBox.Show("Udfyld mindst: Reol, Lejer og Startdato.");
                    return;
                }

                _onSubmit(new SubmitData(RackId, RenterId, StartDateTime, EndDateTime, NoEnd, SelectedPaymentMethod));
            });

            CloseCommand = new RelayCommand(_onClose);
        }
    }
}
