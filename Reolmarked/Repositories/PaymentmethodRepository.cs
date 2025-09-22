using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public class PaymentMethodRepository : IRepository<PaymentMethod>
    {
        private readonly string _connectionString;

        public PaymentMethodRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        public IEnumerable<PaymentMethod> GetAll()
        {
            var methods = new List<PaymentMethod>();
            string query = "SELECT PaymentmethodID, Paymentmethod FROM Paymentmethod";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        methods.Add(new PaymentMethod
                        {
                            PaymentMethodId = (int)reader["PaymentmethodID"],
                            Name = (string)reader["Paymentmethod"]
                        });
                    }
                }
            }
            return methods;
        }

        public PaymentMethod GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(PaymentMethod entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(PaymentMethod entity)
        {
            throw new NotImplementedException();
        }
    }
}
