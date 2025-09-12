using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model.DTO;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public interface IRenterRepository : IRepository<Renter>
    {
        IEnumerable<RenterDisplayDTO> GetAllDisplay();
        RenterDisplayDTO GetByIdDisplay(int id);
    }
}
