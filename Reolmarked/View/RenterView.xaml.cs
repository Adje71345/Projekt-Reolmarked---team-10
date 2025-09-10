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
        private readonly IRepository<Renter> _renterRepository;
        private readonly RenterViewModel _viewModel;

        public RenterView()
        {
            InitializeComponent();

            // Læs connection string fra appsettings.json
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connectionString = config.GetConnectionString("DefaultConnection");

            // Opret repository
            _renterRepository = new RenterRepository(connectionString);

            // Opret ViewModel og sæt som DataContext
            _viewModel = new RenterViewModel(_renterRepository);
            DataContext = _viewModel;
        }

        // Click-eventhandler til AddRenterButton
        private void AddRenterButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddRenterView(_renterRepository);

            // Abonner på RenterAdded event
            dialog.RenterAdded += (s, args) =>
            {
                _viewModel.RefreshRenters();
            };

            dialog.ShowDialog();
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