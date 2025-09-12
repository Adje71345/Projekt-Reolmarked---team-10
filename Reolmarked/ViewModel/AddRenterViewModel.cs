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
        private readonly IRepository<Paymentmethod> _paymentmethodRepository;

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

        private int _paymentmethodId;
        public int PaymentmethodId
        {
            get => _paymentmethodId;
            set => SetProperty(ref _paymentmethodId, value);
        }


        public ObservableCollection<Paymentmethod> PaymentMethods { get; }


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
                    nameof(PaymentmethodId) => PaymentmethodId <= 0 ? "Vælg en betalingsmetode." : null,
                    _ => null
                };
            }
        }


        public ICommand AddRenterCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRenterViewModel(IRenterRepository renterRepository, IRepository<Paymentmethod> paymentmethodRepository)
        {
            _renterRepository = renterRepository;
            _paymentmethodRepository = paymentmethodRepository;

            PaymentMethods = new ObservableCollection<Paymentmethod>(_paymentmethodRepository.GetAll());
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
                PaymentmethodId = this.PaymentmethodId
            };

            _renterRepository.Add(renter);

            // Ryd felterne
            FirstName = "";
            LastName = "";
            PhoneNumber = "";
            Email = "";
            PaymentmethodId = 0;

            RequestClose?.Invoke(this, EventArgs.Empty);
        }


        private bool CanAddRenter()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(PhoneNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && PaymentmethodId > 0;
        }
    }
}
