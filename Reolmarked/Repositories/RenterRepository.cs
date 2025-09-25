using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Reolmarked.Model;
using Reolmarked.Model.DTO;


namespace Reolmarked.Repositories
{
    public class RenterRepository : IRenterRepository
    {
        
        private readonly string _connectionString;

        public RenterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Renter> GetAll()
        {
            var renters = new List<Renter>();
            string query = @"
                SELECT r.RenterId, r.FirstName, r.LastName, r.Email, r.Phone, r.PaymentMethodId, p.PaymentMethodName
                FROM Renter r
                JOIN PaymentMethod p ON r.PaymentMethodId = p.PaymentMethodId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        renters.Add(new Renter
                        {
                            RenterId = (int)reader["RenterId"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"],
                            PaymentMethodId = (int)reader["PaymentMethodId"],
                        });
                    }
                }
            }
            return renters;

        }
        public Renter GetById(int id)
        {
            Renter renter = null;
            string query = @"
                SELECT r.RenterId, r.FirstName, r.LastName, r.Email, r.Phone, r.PaymentMethodId, p.PaymentMethodName
                FROM Renter r
                JOIN PaymentMethod p ON r.PaymentMethodId = p.PaymentMethodId
                WHERE r.RenterId = @RenterId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", id);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        renter = new Renter
                        {
                            RenterId = (int)reader["RenterId"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"],
                            PaymentMethodId = (int)reader["PaymentMethodId"],
                        };
                    }
                }
            }
            return renter;
        }

        public void Add(Renter renter)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                INSERT INTO Renter (FirstName, LastName, Email, Phone, PaymentMethodId)
                VALUES (@FirstName, @LastName, @Email, @Phone, @PaymentMethodId);
                SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Email", renter.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", renter.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PaymentMethodId", renter.PaymentMethodId);

                renter.RenterId = Convert.ToInt32(command.ExecuteScalar());
            }
        }
        


        public void Update(Renter renter)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                UPDATE Renter
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Phone = @Phone,
                    PaymentMethodId = @PaymentMethodId
                WHERE RenterId = @RenterId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Email", renter.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", renter.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PaymentMethodId", renter.PaymentMethodId);
                command.Parameters.AddWithValue("@RenterId", renter.RenterId);

                command.ExecuteNonQuery();
            }
        }



        public void Delete(int id)
        {
            string query = "DELETE FROM Renter WHERE RenterId = @RenterId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int GetCount()
        {
            const string query = "SELECT COUNT(*) FROM Renter";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }
}
