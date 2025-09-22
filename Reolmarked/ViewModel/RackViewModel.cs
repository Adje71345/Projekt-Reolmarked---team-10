using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;
using Reolmarked.View;
using Reolmarked.View.RackSideContent;

namespace Reolmarked.ViewModel
{
    internal class RackViewModel : ViewModelBase
    {        
        public ObservableCollection<Rack> Racks { get; set; }
        public IEnumerable<Rack> Rack1to13 => Racks.Take(13);
        public IEnumerable<Rack> Rack14to18 => Racks.Where(c => c.RackId >= 14 && c.RackId <= 18);
        public IEnumerable<Rack> Rack19to21 => Racks.Where(c => c.RackId >= 19 && c.RackId <= 21);
        public IEnumerable<Rack> Rack22to24 => Racks.Where(c => c.RackId >= 22 && c.RackId <= 24);
        public IEnumerable<Rack> Rack25to31 => Racks.Where(c => c.RackId >= 25 && c.RackId <= 31);
        public IEnumerable<Rack> Rack32to38 => Racks.Where(c => c.RackId >= 32 && c.RackId <= 38);
        public IEnumerable<Rack> Rack39to45 => Racks.Where(c => c.RackId >= 39 && c.RackId <= 45);
        public IEnumerable<Rack> Rack46to52 => Racks.Where(c => c.RackId >= 46 && c.RackId <= 52);
        public IEnumerable<Rack> Rack53to59 => Racks.Where(c => c.RackId >= 53 && c.RackId <= 59);
        public IEnumerable<Rack> Rack60to66 => Racks.Where(c => c.RackId >= 60 && c.RackId <= 66);
        public IEnumerable<Rack> Rack67to71 => Racks.Where(c => c.RackId >= 67 && c.RackId <= 71);
        public IEnumerable<Rack> Rack72to76 => Racks.Where(c => c.RackId >= 72 && c.RackId <= 76);
        public IEnumerable<Rack> Rack77to80 => Racks.Where(c => c.RackId >= 77 && c.RackId <= 80);


        private object currentRackView;
        public object CurrentRackView
        {
            get => currentRackView;
            set
            {
                currentRackView = value;OnPropertyChanged(nameof(CurrentRackView));
            }
        }

        private static Rack selectedRack;
        public Rack SelectedRack
        {
            get => selectedRack;
            set { selectedRack = value; OnPropertyChanged(); }
        }

        public RelayCommand SelectRackCommand { get; }


        public RackViewModel ()
        {
            Racks = new ObservableCollection<Rack> (Rack.CreateDefaultRacks ());
            CurrentRackView = new RackHome();
            SelectRackCommand = new RelayCommand (ViewRackInfo);
        }

        public void ViewRackInfo ()
        {
            CurrentRackView = new RackSelected();
        }

    }
        public class RackHomeViewModel : ViewModelBase
        { }
        public class RackSelectedViewModel : ViewModelBase
        { }
}
