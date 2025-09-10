using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model;

using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;


namespace Reolmarked.Repositories
{
    public class RenterRepository : IRepository <Renter>
    {
        
        private readonly string _connectionString;

        public RenterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Renter> GetAll()
        {
            var renters = new List<Renter>();
            string query = "SELECT * FROM RENTER";
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
                            RenterID = (int)reader["RenterID"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"]
                        });
                    }
                }
            }
            return renters;

        }
        public Renter GetById(int id)
        {
            Renter renter = null;
            string query = "SELECT * FROM RENTER WHERE RenterID = @RenterID";
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
                            RenterID = (int)reader["RenterID"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"]
                        };
                    }
                }
            }
            return renter;
        }


        public void Add(Renter renter)
        {
            string query = "INSERT INTO RENTER (FirstName) VALUES (@FirstName)" +
                "INSERT INTO RENTER (LastName) VALUES (@LastName)" +
                "INSERT INTO RENTER (Email) VALUES (@Email)" +
                "INSERT INTO RENTER (Phone) VALUES (@Phone)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Email", renter.Email);
                command.Parameters.AddWithValue("@Phone", renter.Phone);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(Renter renter)
        {
            string query = "UPDATE RENTER SET FirstName = @FirstName WHERE RenterID = @RenterID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.FirstName);
                command.Parameters.AddWithValue("@Email", renter.FirstName);
                command.Parameters.AddWithValue("@Phone", renter.FirstName);
                command.Parameters.AddWithValue("@RenterID", renter.RenterID);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM RENTER WHERE RenterID = @RenterID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
