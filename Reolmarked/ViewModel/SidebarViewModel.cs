using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reolmarked.Commands;


namespace Reolmarked.ViewModel
{
    public class SidebarViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _main;

        public SidebarViewModel(MainWindowViewModel main)
        {
            _main = main;

            _main.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.SelectedView))
                    OnPropertyChanged(nameof(SelectedView));
            };
        }

        

        public ViewType SelectedView
        {
            get => _main.SelectedView;
            set => _main.SelectedView = value;
        }
    }

    public enum ViewType
    {
        Login,
        DashBoard,
        Renter,
        Rack,
        Labels,
        Salg,
        Månedsopgørelse,
        AddRenter
    }
}
