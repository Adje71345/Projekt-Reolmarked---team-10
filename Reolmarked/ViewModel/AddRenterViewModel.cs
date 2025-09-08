using System;
using System.ComponentModel;

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
    }
}
