using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model
{
    public class RentalContract
    {
        //Attributter
        public int RentalId { get; set; }
        public int RenterId { get; set; } //-----skal den ikke hente RenterId fra Renter klassen???
        public int RackId { get; set; } // FK til Rack
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        //Constructor
        public RentalContract(int rentalId, int renterId, DateOnly startDate, DateOnly endDate)
        {
            RentalId = rentalId;
            RenterId = renterId;
            StartDate = startDate;
            EndDate = endDate;
        }

        //Parameterløs contructor
        public RentalContract() { }
    }
}
