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
        private readonly IRenterRepository _renterRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;

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

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (SetProperty(ref _phoneNumber, value))
                    _addRenterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                    _addRenterCommand.RaiseCanExecuteChanged();
            }

        }

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


        public ObservableCollection<PaymentMethod> PaymentMethods { get; }

        private readonly RelayCommand _addRenterCommand;
        public ICommand AddRenterCommand => _addRenterCommand;
        public ICommand CancelCommand { get; }

        public AddRenterViewModel(IRenterRepository renterRepository, IRepository<PaymentMethod> paymentMethodRepository)
        {
            _renterRepository = renterRepository;
            _paymentMethodRepository = paymentMethodRepository;

            PaymentMethods = new ObservableCollection<PaymentMethod>(_paymentMethodRepository.GetAll());
            _addRenterCommand = new RelayCommand(AddRenter, CanAddRenter);
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
