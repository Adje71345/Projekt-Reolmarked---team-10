using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reolmarked.Model
{
    public class Renter
    {
        public int RenterID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone {  get; set; }

        public Renter() { }

        Renter(int RenterID, string firstName, string lastName, string email, string phone)
        {
            this.RenterID = RenterID;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
        }

        public override string ToString ()
        {
            throw new NotImplementedException ();
        }
        public Renter FromString ()
        {
            throw new NotImplementedException () ;
        }
    }
}
