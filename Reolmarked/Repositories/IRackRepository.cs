using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    // Interface for RackRepository - arver fra generisk IRepository og tilføjer specialiserede metoder 
    public interface IRackRepository : IRepository<Rack>
    {
        // Filtrerer reoler med ledig status
        IEnumerable<Rack> GetAvailableRacks();

        // Filtrerer reoler med optaget status
        IEnumerable<Rack> GetOccupiedRacks();

        // Opdatere status for en reol, ved opsigelse eller fornyelse af lejekontrakt
        void UpdateRackStatus(int rackId, RackStatus? newStatus);
    }
}
