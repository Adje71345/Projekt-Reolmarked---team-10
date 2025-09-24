using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model
{
    public class SaleLine
    {
        public int SaleLineId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Price { get; set; }
        public int RackId { get; set; } // FK til Rack

        //Constructor
        public SaleLine(int saleLineId, DateTime saleDate, decimal price, int rackId)
        {
            SaleLineId = saleLineId;
            SaleDate = saleDate;
            Price = price;
            RackId = rackId;
        }

        //Parameterløs contructor
        public SaleLine() { }
    }
}
