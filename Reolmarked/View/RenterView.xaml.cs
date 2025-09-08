using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reolmarked.ViewModel;

namespace Reolmarked.View
{
    /// <summary>
    /// Interaction logic for RenterView.xaml
    /// </summary>
    public partial class RenterView : UserControl
    {
        public RenterView()
        {
            InitializeComponent();
            DataContext = new RenterViewModel(new DataRenterRepository(connectionString));
        }

        //Click-eventhandler til AddRenterButton
        private void AddRenterButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddRenterView();
            var result = dialog.ShowDialog();

            if (result == true && DataContext is RenterViewModel vm)
            {
                vm.RefreshRenters();
            }
        }

        //Eventhandler som håndterer UI specifik sortering
        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (DataContext is RenterViewModel vm)
            {
                var column = e.Column;
                var direction = column.SortDirection != ListSortDirection.Ascending
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;

                vm.RentersView.SortDescriptions.Clear();
                vm.RentersView.SortDescriptions.Add(new SortDescription(column.SortMemberPath, direction));
                column.SortDirection = direction;

                e.Handled = true;
            }
        }
    }
}