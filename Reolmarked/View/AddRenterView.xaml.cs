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
using System.Windows.Shapes;
using Microsoft.Extensions.Configuration;
using Reolmarked.Repositories;
using Reolmarked.ViewModel;
using Reolmarked.Model;


namespace Reolmarked.View
{
    /// <summary>
    /// Interaction logic for AddRenterView.xaml
    /// </summary>
    public partial class AddRenterView : Window
    {
        public event EventHandler RenterAdded;

        public AddRenterView(IRenterRepository renterRepository, IRepository<PaymentMethod> paymentMethodRepository)
        {
            InitializeComponent();

            var vm = new AddRenterViewModel(renterRepository, paymentMethodRepository);

            vm.RequestClose += (s, e) =>
            {
                RenterAdded?.Invoke(this, EventArgs.Empty);
                this.Close();
            };

            DataContext = vm;
        }
    }
}
