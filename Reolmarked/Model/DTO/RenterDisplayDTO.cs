using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model.DTO
{
    public class RenterDisplayDTO
    {
        public int RenterId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PaymentMethodName { get; set; }
    }
}
