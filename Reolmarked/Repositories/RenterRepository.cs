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
                SELECT r.RenterID, r.FirstName, r.LastName, r.Email, r.Phone, r.PaymentMethodID, p.PaymentMethod
                FROM Renter r
                JOIN PaymentMethod p ON r.PaymentMethodID = p.PaymentMethodID";

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
                            RenterId = (int)reader["RenterID"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"],
                            PaymentMethodId = (int)reader["PaymentMethodID"],
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
                SELECT r.RenterID, r.FirstName, r.LastName, r.Email, r.Phone, r.PaymentMethodID, p.PaymentMethod
                FROM Renter r
                JOIN PaymentMethod p ON r.PaymentMethodID = p.PaymentMethodID
                WHERE r.RenterID = @RenterID";

            using SqlConnection connection = new SqlConnection(_connectionString);
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", id);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        renter = new Renter
                        {
                            RenterId = (int)reader["RenterID"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"],
                            PaymentMethodId = (int)reader["PaymentMethodID"],
                        };
                    }
                }
            }
            return renter;
        }

        // Metoder til at hente data til visning med Paymentmethod navn inkluderet
        public IEnumerable<RenterDisplayDTO> GetAllDisplay()
        {
            var renters = new List<RenterDisplayDTO>();
            string query = @"
            SELECT r.RenterID, r.FirstName, r.LastName, r.Email, r.Phone, p.PaymentMethod
            FROM Renter r
            JOIN PaymentMethod p ON r.PaymentMethodID = p.PaymentMethodID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        renters.Add(new RenterDisplayDTO
                        {
                            RenterId = (int)reader["RenterID"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"],
                            PaymentMethodName = reader["PaymentMethod"] as string
                        });
                    }
                }
            }
            return renters;
        }

        // Hent en enkelt Renter med PaymentMethod navn til visning
        public RenterDisplayDTO GetByIdDisplay(int id)
        {
            RenterDisplayDTO renter = null;
            string query = @"
            SELECT r.RenterID, r.FirstName, r.LastName, r.Email, r.Phone, p.PaymentMethod
            FROM Renter r
            JOIN PaymentMethod p ON r.PaymentMethodID = p.PaymentMethodID
            WHERE r.RenterID = @RenterID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", id);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        renter = new RenterDisplayDTO
                        {
                            RenterId = (int)reader["RenterID"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"],
                            PaymentMethodName = reader["PaymentMethod"] as string
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
                INSERT INTO Renter (FirstName, LastName, Email, Phone, PaymentMethodID)
                VALUES (@FirstName, @LastName, @Email, @Phone, @PaymentMethodID);
                SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Email", renter.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", renter.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PaymentMethodID", renter.PaymentMethodId);

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
                    PaymentMethodID = @PaymentMethodID
                WHERE RenterID = @RenterID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Email", renter.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", renter.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PaymentMethodID", renter.PaymentMethodId);
                command.Parameters.AddWithValue("@RenterID", renter.RenterId);

                command.ExecuteNonQuery();
            }
        }



        public void Delete(int id)
        {
            string query = "DELETE FROM Renter WHERE RenterID = @RenterID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", id);
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
