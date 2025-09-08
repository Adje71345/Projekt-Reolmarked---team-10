using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Reolmarked.ViewModel
{
    public class RenterViewModel : INotifyPropertyChanged
    {
        // Repository til kommunikation med databasen
        private readonly IRenterRepository _renterRepository;


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
                _searchText = value;
                OnPropertyChanged(nameof(SearchText)); // Opdater UI
                RentersView.Refresh(); // Genanvend filteret på listen
            }
        }

        // Kommando til at åbne vinduet for at tilføje en ny lejer
        public ICommand OpenAddRenterWindowCommand { get; }

        // Constructor hvor repository injiceres og data initialiseres
        public RenterViewModel(IRenterRepository renterRepository)
        {
            _renterRepository = renterRepository;

            // Hent alle lejere fra databasen og opret ObservableCollection
            Renters = new ObservableCollection<Renter>(_renterRepository.GetAllRenters());

            // Opret en CollectionView til filtrering og sortering
            RentersView = CollectionViewSource.GetDefaultView(Renters);
            RentersView.Filter = FilterRenters; // Sæt filterfunktion
            RentersView.SortDescriptions.Add(new SortDescription("LastName", ListSortDirection.Ascending)); // Sortér efter efternavn

            // Initialisér kommandoen til at åbne AddRenterView
            OpenAddRenterWindowCommand = new RelayCommand(OpenAddRenterWindow);
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

        // Åbner vinduet for at tilføje en ny lejer og opdaterer listen hvis en lejer blev tilføjet
        private void OpenAddRenterWindow()
        {
            var addWindow = new AddRenterView();
            var result = addWindow.ShowDialog();

            if (result == true)
            {
                // Genindlæs alle lejere fra databasen og opdater listen
                Renters.Clear();
                foreach (var renter in _renterRepository.GetAllRenters())
                {
                    Renters.Add(renter);
                }
                RentersView.Refresh(); // Genanvend filter og sortering
            }
        }

        // Event til at informere UI om ændringer i ViewModel
        public event PropertyChangedEventHandler PropertyChanged;

        // Metode til at udløse PropertyChanged-event
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}