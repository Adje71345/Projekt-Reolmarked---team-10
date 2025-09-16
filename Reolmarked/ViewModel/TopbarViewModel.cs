using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Reolmarked.Commands;

namespace Reolmarked.ViewModel
{
    public class TopbarViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _main;

        public TopbarViewModel(MainWindowViewModel main)
        {
            _main = main;

            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        // Loginstatus via MainViewModel
        public bool IsLoggedIn
        {
            get => _main.IsLoggedIn;
            set
            {
                if (_main.IsLoggedIn != value)
                {
                    _main.IsLoggedIn = value;
                    OnPropertyChanged(nameof(IsLoggedIn));
                    OnPropertyChanged(nameof(LoginButtonText));
                }
            }

        }
        public string LoginButtonText => IsLoggedIn ? "Log ud" : "Log ind";

        public ICommand LoginCommand { get; }

        private void ExecuteLogin()
        {
            IsLoggedIn = !IsLoggedIn;

            _main.SelectedView = IsLoggedIn ? ViewType.DashBoard : ViewType.Login;
            OnPropertyChanged(nameof(LoginButtonText));
        }

        //Navigation direkte via SelectedView skal evt. bruges til login/logud
        public ViewType SelectedView
        {
            get => _main.SelectedView;
            set => _main.SelectedView = value;
        }
    }
}
