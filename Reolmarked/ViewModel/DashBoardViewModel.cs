using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class DashBoardViewModel : ViewModelBase
    {
        private readonly IRenterRepository _renterRepository;
        private readonly IRackRepository _rackRepository;
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly ISaleLineRepository _saleLineRepository;
        private readonly MainWindowViewModel _main;

        private decimal _salesToday;
        public decimal SalesToday
            {
            get => _salesToday;
            set => SetProperty(ref _salesToday, value);
        }
        private int _occupancyPercentage;
        public int OccupancyPercentage
        {
            get => _occupancyPercentage;
            set => SetProperty(ref _occupancyPercentage, value);
        }

        private int _renterCount;
        public int RenterCount
        {
            get => _renterCount;
            set => SetProperty(ref _renterCount, value);
        }

        public ICommand NavigateCommand { get; }
        public ObservableCollection<object> Events { get; } = new();

        public DashBoardViewModel(MainWindowViewModel main, IRenterRepository renterRepository, IRackRepository rackRepository, IRentalContractRepository rentalContractRepository, ISaleLineRepository saleLineRepository)
        {
            _main = main;
            _renterRepository = renterRepository;
            _rackRepository = rackRepository;
            _rentalContractRepository = rentalContractRepository;
            _saleLineRepository = saleLineRepository;

            NavigateCommand = new RelayCommand<ViewType>(view => _main.SelectedView = view);

            //Starter indlæsning af data som en baggrundsopgave med Task.Run
            StartLoadCounts();

            // Sample data til demonstration
            SalesToday = 1250.75m;

            //Dummydata til events
            SeedEvents();
        }

        private void StartLoadCounts()
        {
            Task.Run(() =>
            {
                try
                {
                    // Hent data fra repository. Hentes i baggrundstråd pga. Task.Run
                    var count = _renterRepository.GetCount();
                    var totalRacks = _rackRepository.GetCount();
                    var occupiedRacks = _rackRepository.GetOccupiedRacks().Count();
                    var salesToday = _saleLineRepository.GetTotalSalesToday();

                    //Beregn belægningsprocent
                    int occupancy = totalRacks > 0
                        ? (int)Math.Round((double)occupiedRacks / totalRacks * 100)
                        : 0;

                    // Når data er hentet, opdater UI-tråden
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenterCount = count;
                        OccupancyPercentage = occupancy;
                        SalesToday = salesToday;
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fejl ved hentning af lejere: {ex}");
                    // Eventuelt marshales en fejlmeddelelse til UI her
                }
            });
        }

        // Dummydata til events
        private void SeedEvents()
        {
            Events.Clear();

            Events.Add(new { Name = "Bagagerumsmarked", Date = "12-10-2025", Description = "Starter kl 9.30. Opsætning kl 9 (afspærring og skilte). Mindst én medarbejder udendørs." });
            Events.Add(new { Name = "Børneaktiviteter", Date = "19-10-2025", Description = "Aktiviteter fra 12-15. Opsætning kl 11.30. " });
            Events.Add(new { Name = "Kreativ workshop", Date = "01-11-2025", Description = "Fra genbrug til guld. Tilmeldninger samles i kassen." });
            Events.Add(new { Name = "Velgørenhedsstand", Date = "15-11-2025", Description = "Ekstra stand sættes op ved kassen dagen før. Donationer bliver uddelt 12/12/25." });
            Events.Add(new { Name = "Julemarked", Date = "01-12-2025", Description = "Opsætning kl 8.30. Pynte butikken, varme gløgg og sætte telt op udendørs." });
        }
    }    
}


