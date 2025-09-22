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
        private readonly IRenterRepository _renterRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;


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

        private Renter _selectedRenter;
        public Renter SelectedRenter
        {
            get => _selectedRenter;
            set
            {
                if (SetProperty(ref _selectedRenter, value) && value != null)
                {
                    // Indlæs data i info-viewmodel FØR panelet åbnes
                    RenterInfo.LoadRenter(value);
                    IsInfoPanelOpen = true;
                }
            }
        }


        private bool _isInfoPanelOpen;
        public bool IsInfoPanelOpen { get => _isInfoPanelOpen; set => SetProperty(ref _isInfoPanelOpen, value); }

        public RenterInfoViewModel RenterInfo { get; }
        public AddRenterViewModel AddRenter { get; }

        public ICommand CloseOverlayCommand { get; }

        private void OpenRenterInfo(Renter r)
        {
            RenterInfo.LoadRenter(r);
            IsInfoPanelOpen = true;
        }

        private void EditRenter(Renter r) { /* edit flow */ }

        public ICommand OpenAddPanelCommand { get; }


        private bool _isAddPanelOpen;
        public bool IsAddPanelOpen
        {
            get => _isAddPanelOpen;
            set
            {
                if (SetProperty(ref _isAddPanelOpen, value))
                {
                    OnPropertyChanged(nameof(CurrentOverlayViewModel));
                }
            }
        }

        public object CurrentOverlayViewModel => IsAddPanelOpen ? (object)AddRenter : (object)RenterInfo;


        // Constructor hvor repository injiceres og data initialiseres
        public RenterViewModel(IRenterRepository renterRepository, IRepository<PaymentMethod> paymentMethodRepository)
        {
            _renterRepository = renterRepository;
            _paymentMethodRepository = paymentMethodRepository;

            // Hent alle lejere fra databasen og opret ObservableCollection
            Renters = new ObservableCollection<Renter>(_renterRepository.GetAll());

            // Opret en CollectionView til filtrering og sortering
            RentersView = CollectionViewSource.GetDefaultView(Renters);
            RentersView.Filter = FilterRenters; // Sæt filterfunktion
            RentersView.SortDescriptions.Add(new SortDescription("RenterId", ListSortDirection.Ascending)); // Sortér efter Id

            OpenAddPanelCommand = new RelayCommand(() =>
            {
                IsAddPanelOpen = true;
                IsInfoPanelOpen = true;
            });
            RenterInfo = new RenterInfoViewModel(() => IsInfoPanelOpen = false, EditRenter);
            AddRenter = new AddRenterViewModel(_renterRepository, _paymentMethodRepository);
            AddRenter.RequestClose += (s, e) =>
            {
                IsAddPanelOpen = false;
                IsInfoPanelOpen = false;
                RefreshRenters(); // eller Renters.Add(nyRenter) hvis du har adgang til den
            };
            CloseOverlayCommand = new RelayCommand(() =>
            {
                IsInfoPanelOpen = false;
                IsAddPanelOpen = false;
            });
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
