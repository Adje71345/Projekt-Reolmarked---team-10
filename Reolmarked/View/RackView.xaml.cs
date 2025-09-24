using System.Windows.Controls;
using Reolmarked.ViewModel;

namespace Reolmarked.View
{
    public partial class RackView : UserControl
    {
        public RackView()
        {
            InitializeComponent();
            DataContext = new RackViewModel();
        }
    }
}
