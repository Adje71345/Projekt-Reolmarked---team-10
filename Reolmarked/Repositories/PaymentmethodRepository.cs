using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public class PaymentmethodRepository : IRepository<Paymentmethod>
    {
        private readonly string _connectionString;

        public PaymentmethodRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        public IEnumerable<Paymentmethod> GetAll()
        {
            var methods = new List<Paymentmethod>();
            string query = "SELECT PaymentmethodID, Paymentmethod FROM Paymentmethod";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        methods.Add(new Paymentmethod
                        {
                            PaymentmethodId = (int)reader["PaymentmethodID"],
                            Name = (string)reader["Paymentmethod"]
                        });
                    }
                }
            }
            return methods;
        }

        public Paymentmethod GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Paymentmethod entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Paymentmethod entity)
        {
            throw new NotImplementedException();
        }
    }
}
