using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class RenterViewModel : ViewModelBase 
    {
        // Repository til kommunikation med databasen
        private readonly IRepository<Renter> _renterRepository;


        // Samling af alle lejere hentet fra databasen
        public ObservableCollection<Renter> Renters { get; set; }


        // Filtreret og sorteret visning af lejere, som bindes til UI
        public ICollectionView RentersView { get; set; }


        // Søgetekst som brugeren indtaster i søgefeltet
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    RentersView.Refresh(); // kun opdater view, hvis værdien faktisk ændres
                }
            }
        }


        // Constructor hvor repository injiceres og data initialiseres
        public RenterViewModel(IRepository<Renter> renterRepository)
        {
            _renterRepository = renterRepository;

            // Hent alle lejere fra databasen og opret ObservableCollection
            Renters = new ObservableCollection<Renter>(_renterRepository.GetAll());

            // Opret en CollectionView til filtrering og sortering
            RentersView = CollectionViewSource.GetDefaultView(Renters);
            RentersView.Filter = FilterRenters; // Sæt filterfunktion
            RentersView.SortDescriptions.Add(new SortDescription("RenterId", ListSortDirection.Ascending)); // Sortér efter Id
        }

        // Filterfunktion som anvendes på RentersView
        private bool FilterRenters(object obj)
        {
            if (obj is Renter renter)
            {
                // Returnér true hvis søgeteksten matcher nogen af felterne
                return string.IsNullOrWhiteSpace(SearchText) ||
                       renter.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       renter.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       renter.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       renter.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // Metode til at opdatere listen af lejere fra databasen
        public void RefreshRenters()
        {
            Renters.Clear();
            foreach (var renter in _renterRepository.GetAll())
            {
                Renters.Add(renter);
            }
            RentersView.Refresh();
        }
    }
}
