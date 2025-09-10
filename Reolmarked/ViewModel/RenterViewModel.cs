using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model;
using Reolmarked.MVVM;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class RenterViewModel : ViewModelBase
    {
        private static Renter selectedRenter;
        string connectionString = "Server=DESKTOP-LC20V6E;Database=ReolMarked;Trusted_Connection=True;";
        IRepository<Renter> renterRepository;

        public RenterViewModel ()
        {
            renterRepository = new RenterRepository (connectionString);
        }

        public Renter SelectedRenter
        {
            get { return selectedRenter; }
            set { selectedRenter = value; OnPropertyChanged(); }
        }

        public RelayCommand AddRenter = new RelayCommand(execute => renterRepository.Add(selectedRenter));


    }
}
