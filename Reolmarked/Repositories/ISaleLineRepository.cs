using System;
using System.Collections.Generic;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public interface ISaleLineRepository : IRepository<SaleLine>
    {
        //Henter alle salgslinjer for en bestemt reol med aktiv kontrakt
        IEnumerable<SaleLine> GetSalesForRackWithActiveContractLastMonth(int rackId);

        //Henter dagens samlede salg
        decimal GetTotalSalesToday();
        // Tilføjer mange salgslinjer på en gang (bruges ved månedsafslutning)
        void AddMany(IEnumerable<SaleLine> lines);

    }
}
