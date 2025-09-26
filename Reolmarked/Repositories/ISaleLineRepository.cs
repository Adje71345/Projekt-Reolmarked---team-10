using System;
using System.Collections.Generic;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public interface ISaleLineRepository : IRepository<SaleLine>
    {
        //Henter alle salgslinjer for en bestemt reol med aktiv kontrakt
        IEnumerable<SaleLine> GetSalesForRackWithActiveContractLastMonth(int rackId);
    }
}
