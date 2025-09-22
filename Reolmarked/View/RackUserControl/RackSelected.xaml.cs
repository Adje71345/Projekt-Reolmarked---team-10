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
using Reolmarked.ViewModel;

namespace Reolmarked.View.RackSideContent
{
    /// <summary>
    /// Interaction logic for RackSelected.xaml
    /// </summary>
    public partial class RackSelected : UserControl
    {
        public RackSelected()
        {
            InitializeComponent();
            RackViewModel viewModel = new RackViewModel();
            DataContext = viewModel;
        }
    }
}
