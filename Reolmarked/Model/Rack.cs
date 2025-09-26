using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Reolmarked.Model
{
    public class Rack
    {
        //Attributter
        public int RackId { get; set; }
        public int RackStatusId  { get; set; }
        public string StatusName {get; set; }
        //Constructor
        public Rack(int rackId, int rackStatusId, string statusName)
        {
            RackId = rackId;
            RackStatusId = rackStatusId;
            StatusName = statusName;
        }

        public Rack() { }
    }
}
