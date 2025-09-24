using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class DashBoardViewModel : ViewModelBase
    {
        private readonly IRenterRepository _renterRepository;

        public decimal SalesToday { get; set; }
        public int OccupancyPercentage { get; set; }
        
        private int _renterCount;
        public int RenterCount
        {
            get => _renterCount;
            set => SetProperty(ref _renterCount, value);
        }

       

        public DashBoardViewModel(IRenterRepository renterRepository) 
        { 
            _renterRepository = renterRepository;

            //Starter indlæsning af data som en baggrundsopgave med Task.Run
            StartLoadCounts();

            // Sample data til demonstration
            SalesToday = 1250.75m;
            OccupancyPercentage = 85;

        }

        private void StartLoadCounts()
        {
            Task.Run(() =>
            {
                try
                {
                    // Hent data fra repository. Hentes i baggrundstråd pga. Task.Run
                    var count = _renterRepository.GetCount();

                    // Når data er hentet, opdater UI-tråden
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenterCount = count;
                        //Her skal de tre andre også hentes og opdateres
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fejl ved hentning af lejere: {ex}");
                    // Eventuelt marshales en fejlmeddelelse til UI her
                }
            });
        }
    }


    /*
    public ObservableCollection<ChecklistItem> Checklist { get; }
    public class ChecklistItem
    {
        public string Task { get; }
        public string Responsible { get; }
        public string TimeFrame { get; }
        public string Status { get; }

    public ChecklistItem(string task, string responsible, string timeFrame, string status)
    {
        Task = task;
        Responsible = responsible;
        TimeFrame = timeFrame;
        Status = status;
    }
    }
    */
}


