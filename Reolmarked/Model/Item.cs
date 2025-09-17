using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model
{
    public class Item
    {
        //Attributter
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int RenterId { get; set; } // ------- Hvem ejer item (Renter)
        public int RackId { get; set; } // ------- Hvor item tilhører (Rack)
  

        //Constructor
        public Item(string name, string description, decimal price, int renterId, int rackId)
        {
            Name = name;
            Description = description;
            Price = price;
            RenterId = renterId;
            RackId = rackId;
        }

        //Parameterløs contructor
        public Item() { }
    }
}
