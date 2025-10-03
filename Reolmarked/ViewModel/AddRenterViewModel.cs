using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class AddRenterViewModel : ViewModelBase
    {
        // Repositories
        private readonly IRenterRepository _renterRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;

        // Properties til binding
        // Fornavn på lejer
        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                    _addRenterCommand.RaiseCanExecuteChanged();
            }

        }

        // Efternavn på lejer
        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                    _addRenterCommand.RaiseCanExecuteChanged();
            }

        }

        // Telefonnummer på lejer
        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (SetProperty(ref _phoneNumber, value))
                {
                    PhoneErrorText = IsPhoneNumberValid(value) ? "" : "Telefonnummer må kun indeholde tal";
                    DuplicateErrorText = IsDuplicateContact() ? "Telefonnummer eller email findes allerede" : "";
                    _addRenterCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Fejlmeddelelse til telefonnummer validering
        private string _phoneErrorText;
        public string PhoneErrorText
        {
            get => _phoneErrorText;
            private set => SetProperty(ref _phoneErrorText, value);
        }

        // Fejlmeddelelse ved allerede eksisterende telefonnummer eller email
        private string _duplicateErrorText;
        public string DuplicateErrorText
        {
            get => _duplicateErrorText;
            private set => SetProperty(ref _duplicateErrorText, value);
        }

        // Email på lejer
        private string _email = "";
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    DuplicateErrorText = IsDuplicateContact() ? "Telefonnummer eller email findes allerede" : "";
                    _addRenterCommand.RaiseCanExecuteChanged();
                }
            }

        }

        // Betalingsmetode-id på lejer (ComboBox)
        private int _paymentMethodId;
        public int PaymentMethodId
        {
            get => _paymentMethodId;
            set
            {
                if (SetProperty(ref _paymentMethodId, value))
                    _addRenterCommand.RaiseCanExecuteChanged();
            }
        }


        // Liste af betalingsmetoder til ComboBox
        public ObservableCollection<PaymentMethod> PaymentMethods { get; }

        // Commands
        private readonly RelayCommand _addRenterCommand;
        public ICommand AddRenterCommand => _addRenterCommand;
        public ICommand CancelCommand { get; }

        // Constructor
        public AddRenterViewModel(IRenterRepository renterRepository, IRepository<PaymentMethod> paymentMethodRepository)
        {
            _renterRepository = renterRepository;
            _paymentMethodRepository = paymentMethodRepository;

            PaymentMethods = new ObservableCollection<PaymentMethod>(_paymentMethodRepository.GetAll());
            _addRenterCommand = new RelayCommand(AddRenter, CanAddRenter);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Event til at signalere lukning af vindue
        public event EventHandler RequestClose;

        // Ryd felter
        private void ClearFields()
        {
            FirstName = "";
            LastName = "";
            PhoneNumber = "";
            Email = "";
            PaymentMethodId = 0;
            PhoneErrorText = "";
            DuplicateErrorText = "";
        }

        // Metoder til commands
        // Luk vindue uden at gemme
        private void Cancel()
        {
            ClearFields();
            // Signalér til View at vinduet skal lukkes            
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        // Tilføj lejer og luk vindue
        private void AddRenter()
        {
            var renter = new Renter
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Phone = this.PhoneNumber,
                Email = this.Email,
                PaymentMethodId = this.PaymentMethodId
            };

            _renterRepository.Add(renter);
            
            ClearFields();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        // Hjælpefunktion til validering af telefonnummer
        private bool IsPhoneNumberValid(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.All(char.IsDigit);
        }

        // Hjælpefunktion til at afgøre om der allerede findes en lejer med samme telefonnummer eller email
        private bool IsDuplicateContact()
        {
            var allRenters = _renterRepository.GetAll();
            return allRenters.Any(r =>
                string.Equals(r.Phone, PhoneNumber, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Email, Email, StringComparison.OrdinalIgnoreCase));
        }

        // Hjælpefunktion til at afgøre om lejer kan tilføjes - returnerer true hvis alle felter er udfyldt korrekt
        private bool CanAddRenter()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && IsPhoneNumberValid(PhoneNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && PaymentMethodId > 0
                && !IsDuplicateContact();
        }
    }
}
