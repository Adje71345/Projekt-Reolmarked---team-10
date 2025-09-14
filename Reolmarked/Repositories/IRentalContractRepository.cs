using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    // Interface for RentalContractRepository - arver fra generisk IRepository
    // og tilføjer specialiserede metoder til lejekontrakter
    public interface IRentalContractRepository : IRepository<RentalContract>
    {
        // Henter aktiv kontrakt for en bestemt reol
        RentalContract GetActiveContractByRack(int rackId);

        // Henter alle aktive kontrakter for en bestemt lejer
        IEnumerable<RentalContract> GetActiveContractsByRenter(int renterId);

        // Opsiger en enkelt kontrakt ved at sætte slutdato
        void TerminateSingleContract(int rentalId, DateOnly endDate);
    }
}
