using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model
{
    public class Renter
    {
        // --- Attributes (fra DCD) ---
        public int RenterId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int PaymentMethodId { get; set; }

        // --- Constructor (fra DCD) ---
        public Renter(string firstName, string lastName, string email, string phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
        }

        // Parameterløs constructor til data fra DB
        public Renter() { }

    }
}
