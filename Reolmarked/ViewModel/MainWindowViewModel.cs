using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    //Fungerer som hub for de forskellige viewmodels
    public class MainWindowViewModel : ViewModelBase
    {
        //Navigation
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private ViewType _selectedView;
        public ViewType SelectedView
        {
            get => _selectedView;
            set
            {
                if (SetProperty(ref _selectedView, value))
                    SwitchView(_selectedView);
            }
        }

        private void SwitchView(ViewType view)
        {
            Debug.WriteLine($"Skifter til view: {view}");

            switch (view)
            {
                case ViewType.Renter:
                    CurrentViewModel = new RenterViewModel(_renterRepository, _paymentMethodRepository);
                    break;
                case ViewType.DashBoard:
                    CurrentViewModel = new DashBoardViewModel(_renterRepository);
                    break;
                case ViewType.Rack:
                    CurrentViewModel = new RackViewModel();
                    break;
                    // Tilføj flere cases her
            }
        }

        //Loginstatus
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (SetProperty(ref _isLoggedIn, value))
                {
                    OnPropertyChanged(nameof(IsLoggedOut));
                    if (!value)
                        SwitchView(ViewType.Login); // fx log ud = vis loginview
                }
            }
        }

        public bool IsLoggedOut => !IsLoggedIn;


        //Sub-viewmodels
        public SidebarViewModel Sidebar { get; }
        public TopbarViewModel Topbar { get; }

        //Repositories
        private readonly IRenterRepository _renterRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;

        public MainWindowViewModel()
        {
            string connectionString = ConnectionHelper.GetConnectionString();
            _renterRepository = new RenterRepository(connectionString);
            _paymentMethodRepository = new PaymentMethodRepository(connectionString);

            //Instantier sub-viewmodels
            Sidebar = new SidebarViewModel(this);
            Topbar = new TopbarViewModel(this);

            // Start med dashboard
            SelectedView=ViewType.DashBoard;           

            IsLoggedIn = true;
        }
    }



    //Skal flyttes til et mere passende sted
    public static class ConnectionHelper
    {
        public static string GetConnectionString()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return config.GetConnectionString("DefaultConnection");
        }
    }

    //Converter til enum bindinger i XAML - bruges i Sidebar. Skal flyttes til et mere passende sted
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(targetType, parameter.ToString()) : Binding.DoNothing;
        }
    }
}
