using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model
{
    public enum RackStatus
    {
        Ledig,
        Optaget,
        Defekt
    }

    public class Rack
    {
        //Attributter
        public int RackId { get; set; }
        public RackStatus Status { get; set; }

        //Constructor
        public Rack(int rackId, RackStatus status)
        {
            RackId = rackId;
            Status = status;
        }

        public Rack() { }

        //Statisk metode til at oprette 80 reoler
        public static List<Rack> CreateDefaultRacks()
        {
            var racks = new List<Rack>();
            for (int i = 1; i <= 80; i++)
            {
                racks.Add(new Rack(i, RackStatus.Ledig));
            }
            return racks;
        }
    }
}
