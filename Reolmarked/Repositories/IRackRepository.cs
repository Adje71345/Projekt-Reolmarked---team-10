using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    // Interface for RackRepository - arver fra generisk IRepository og tilføjer specialiserede metoder
    public interface IRackRepository : IRepository<Rack>
    {
        // Henter alle reoler med ledig status
        IEnumerable<Rack> GetAvailableRacks();

        // Henter alle reoler med optaget status
        IEnumerable<Rack> GetOccupiedRacks();

        // Opdaterer status for en reol direkte (bruges fx til at frigive en reol)
        void UpdateRackStatus(int rackId, int newStatus);

        // Opdaterer status på alle reoler, hvor kontrakter er udløbet
        void UpdateStatusesForEndedContracts();
    }
}
