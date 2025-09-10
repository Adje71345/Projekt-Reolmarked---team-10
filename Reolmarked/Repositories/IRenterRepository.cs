using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    
    public interface IRepository<Renter> where Renter : class
    {
        IEnumerable<Renter> GetAllRenters();
        Renter GetById(int id);
        void AddRenter(Renter entity);
        void UpdateRenter(Renter entity);
        void DeleteRenter(int id);
    }
}
