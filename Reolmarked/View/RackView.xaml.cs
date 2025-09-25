using System.Windows.Controls;
using Reolmarked.ViewModel;
using System.Configuration;
using Reolmarked.Repositories;


namespace Reolmarked.View
{
    public partial class RackView : UserControl
    {
        public RackView()
        {
            InitializeComponent();

            // Hent connection string fra App.config (navn: ReolmarkedDb)
            var cs = ConfigurationManager
                .ConnectionStrings["ReolmarkedDb"]
                .ConnectionString;

            // Opret repository og giv det til ViewModel
            var repo = new RackRepository(cs);
            DataContext = new RackViewModel(repo);
        }

    }
}
