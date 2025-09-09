using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Reolmarked.ViewModel
{
    public class AddRenterViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }

        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string _paymentMethod = "";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand AddRenterCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRenterViewModel()
        {
            AddRenterCommand = new RelayCommand(AddRenter, CanAddRenter);
            CancelCommand = new RelayCommand(Cancel);
        }

        public event EventHandler RequestClose;

        private void Cancel(object parameter)
        {
            // Her kan du gemme data hvis nødvendigt
            // Fx: SaveDraft(); eller lignende

            // Signalér til View at vinduet skal lukkes
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void AddRenter(object parameter)
        {
            var renter = new Renter
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                PhoneNumber = this.PhoneNumber,
                Email = this.Email,
                PaymentMethod = this.PaymentMethod
            };
            RenterList.Add(renter);

            // Evt. ryd felterne:
            FirstName = "";
            LastName = "";
            PhoneNumber = "";
            Email = "";
            PaymentMethod = "";
        }

        private bool CanAddRenter(object parameter)
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(PhoneNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(PaymentMethod);
        }

        // RelayCommand implementation
        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }

        public class Renter
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public string PaymentMethod { get; set; }
        }

        public BindingList<Renter> RenterList { get; } = new();
    }
}
