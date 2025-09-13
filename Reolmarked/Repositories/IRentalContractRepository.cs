using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public interface IRentalContractRepository : IRepository<RentalContract>
    {
        RentalContract GetActiveContractByRack(int rackId);
        IEnumerable<RentalContract> GetActiveContractsByRenter(int renterId);
        void TerminateContractByRack(int rackId);
        void TerminateContractByRenter(int renterId);
        void RenewContract(int rentalId, DateTime? newEndDate);
    }
}
