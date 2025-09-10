using System;
using System.ComponentModel;
using System.Windows.Input;
using Reolmarked.Repositories;
using Reolmarked.Model;
using Reolmarked.Commands;

namespace Reolmarked.ViewModel
{
    public class AddRenterViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly IRepository<Renter> _renterRepository;

        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _paymentMethod = "";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public List<string> PaymentMethods { get; } = new()
        {
            "MobilePay",
            "Bankoverførsel",
            "Kontant"
        };

        //IDataErrorInfo
        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(FirstName) => string.IsNullOrWhiteSpace(FirstName) ? "Fornavn må ikke være tomt." : null,
                    nameof(LastName) => string.IsNullOrWhiteSpace(LastName) ? "Efternavn må ikke være tomt." : null,
                    nameof(PhoneNumber) => string.IsNullOrWhiteSpace(PhoneNumber) ? "Telefonnummer må ikke være tomt." : null,
                    nameof(Email) => string.IsNullOrWhiteSpace(Email) ? "Email må ikke være tomt." : null,
                    nameof(PaymentMethod) => string.IsNullOrWhiteSpace(PaymentMethod) ? "Betalingsmetode må ikke være tomt." : null,
                    _ => null
                };
            }
        }

        public ICommand AddRenterCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRenterViewModel(IRepository<Renter> renterRepository)
        {
            _renterRepository = renterRepository;

            AddRenterCommand = new RelayCommand(AddRenter, CanAddRenter);
            CancelCommand = new RelayCommand(Cancel);
        }

        public event EventHandler RequestClose;

        private void Cancel()
        {
            // Signalér til View at vinduet skal lukkes
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void AddRenter()
        {
            var renter = new Renter
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Phone = this.PhoneNumber,
                Email = this.Email,
                BankInfo = this.PaymentMethod
            };

            _renterRepository.Add(renter);

            // Evt. ryd felterne:
            FirstName = "";
            LastName = "";
            PhoneNumber = "";
            Email = "";
            PaymentMethod = "";

            // Signalér til View, at vinduet kan lukkes
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private bool CanAddRenter()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(PhoneNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(PaymentMethod);
        }
    }
}
