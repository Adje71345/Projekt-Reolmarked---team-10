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
        public int PaymentmethodId { get; set; }

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

        // --- Operations (fra DCD) ---
        /*public override string ToString()
        {
            // Format: RenterId;FirstName;LastName;Email;Phone;BankInfo
            return $"{RenterId};{FirstName};{LastName};{Email};{Phone};{BankInfo}";
        }*/

        /*public static Renter FromString(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var parts = data.Split(';');
            if (parts.Length < 6) throw new FormatException("Invalid Renter data");

            var renter = new Renter(parts[1], parts[2], parts[3], parts[4])
            {
                BankInfo = parts[5]
            };

            if (int.TryParse(parts[0], out var id))
                renter.RenterId = id;

            return renter;
        }*/
    }
}
