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
using Microsoft.Extensions.Configuration;
using Reolmarked.Repositories;
using Reolmarked.ViewModel;
using Reolmarked.Model;

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

                vm.SortRenters(column.SortMemberPath, direction);
                column.SortDirection = direction;

                e.Handled = true;
            }
        }
    }
}