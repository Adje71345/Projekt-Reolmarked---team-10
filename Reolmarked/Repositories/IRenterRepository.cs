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
        IEnumerable<Renter> GetAll();
        Renter GetById(int id);
        void Add(Renter entity);
        void Update(Renter entity);
        void Delete(int id);
    }
}
