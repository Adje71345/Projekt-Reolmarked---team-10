using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class AddRenterViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly IRenterRepository _renterRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;

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

        private int _paymentMethodId;
        public int PaymentMethodId
        {
            get => _paymentMethodId;
            set => SetProperty(ref _paymentMethodId, value);
        }


        public ObservableCollection<PaymentMethod> PaymentMethods { get; }


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
                    nameof(PaymentMethodId) => PaymentMethodId <= 0 ? "Vælg en betalingsmetode." : null,
                    _ => null
                };
            }
        }


        public ICommand AddRenterCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRenterViewModel(IRenterRepository renterRepository, IRepository<PaymentMethod> paymentMethodRepository)
        {
            _renterRepository = renterRepository;
            _paymentMethodRepository = paymentMethodRepository;

            PaymentMethods = new ObservableCollection<PaymentMethod>(_paymentMethodRepository.GetAll());
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
                PaymentMethodId = this.PaymentMethodId
            };

            _renterRepository.Add(renter);

            // Ryd felterne
            FirstName = "";
            LastName = "";
            PhoneNumber = "";
            Email = "";
            PaymentMethodId = 0;

            RequestClose?.Invoke(this, EventArgs.Empty);
        }


        private bool CanAddRenter()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(PhoneNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && PaymentMethodId > 0;
        }
    }
}
