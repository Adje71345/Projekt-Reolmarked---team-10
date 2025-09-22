using System;
using System.Collections.Generic;
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
using Reolmarked.Model;
using Reolmarked.View.RackSideContent;
using Reolmarked.ViewModel;

namespace Reolmarked.View
{
    /// <summary>
    /// Interaction logic for RackView.xaml
    /// </summary>
    public partial class RackView : UserControl
    {
        public RackView()
        {
            InitializeComponent();
            RackViewModel viewModel = new RackViewModel();
            DataContext=viewModel;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRadioButton = sender as RadioButton;
            Rack selectedRack = selectedRadioButton.DataContext as Rack;
            MessageBox.Show($"Selected Rack ID: {selectedRack.RackId}");
            if (DataContext is RackViewModel viewModel && viewModel.SelectRackCommand.CanExecute(selectedRack))
            {
                viewModel.SelectRackCommand.Execute(selectedRack);
            }
        }

    }
}
