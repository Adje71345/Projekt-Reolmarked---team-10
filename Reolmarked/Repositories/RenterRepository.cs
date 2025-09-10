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
            string query = @"
                SELECT r.RenterID, r.FirstName, r.LastName, r.Email, r.Phone, b.BankInfo
                FROM Renter r
                LEFT JOIN RenterBankInfo b ON r.RenterID = b.RenterID";
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
                            BankInfo = reader["BankInfo"] as string
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
                SELECT r.RenterID, r.FirstName, r.LastName, r.Email, r.Phone, b.BankInfo
                FROM Renter r
                LEFT JOIN RenterBankInfo b ON r.RenterID = b.RenterID
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
                            BankInfo = reader["BankInfo"] as string
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

                // Start en transaktion, så begge indsættelser sker atomisk. Hvis én indsættelse fejler, rulles begge tilbage.
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Indsæt i Renter-tabellen
                        string renterQuery = @"
                            INSERT INTO Renter (FirstName, LastName, Email, Phone)
                            VALUES (@FirstName, @LastName, @Email, @Phone);
                            SELECT SCOPE_IDENTITY();";

                        SqlCommand renterCommand = new SqlCommand(renterQuery, connection, transaction);
                        renterCommand.Parameters.AddWithValue("@FirstName", renter.FirstName);
                        renterCommand.Parameters.AddWithValue("@LastName", renter.LastName);
                        renterCommand.Parameters.AddWithValue("@Email", renter.Email ?? (object)DBNull.Value);
                        renterCommand.Parameters.AddWithValue("@Phone", renter.Phone ?? (object)DBNull.Value);

                        renter.RenterId = Convert.ToInt32(renterCommand.ExecuteScalar());

                        // Indsæt i RenterBankInfo-tabellen
                        string bankQuery = @"
                            INSERT INTO RenterBankInfo (RenterID, BankInfo)
                            VALUES (@RenterID, @BankInfo);";

                        SqlCommand bankCommand = new SqlCommand(bankQuery, connection, transaction);
                        bankCommand.Parameters.AddWithValue("@RenterID", renter.RenterId);
                        bankCommand.Parameters.AddWithValue("@BankInfo", renter.BankInfo ?? (object)DBNull.Value);

                        bankCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public void Update(Renter renter)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Start en transaktion
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Opdater Renter-tabellen
                        string renterQuery = @"
                    UPDATE Renter
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        Email = @Email,
                        Phone = @Phone
                    WHERE RenterID = @RenterID";

                        SqlCommand renterCommand = new SqlCommand(renterQuery, connection, transaction);
                        renterCommand.Parameters.AddWithValue("@FirstName", renter.FirstName);
                        renterCommand.Parameters.AddWithValue("@LastName", renter.LastName);
                        renterCommand.Parameters.AddWithValue("@Email", renter.Email ?? (object)DBNull.Value);
                        renterCommand.Parameters.AddWithValue("@Phone", renter.Phone ?? (object)DBNull.Value);
                        renterCommand.Parameters.AddWithValue("@RenterID", renter.RenterId);

                        renterCommand.ExecuteNonQuery();

                        // Opdater RenterBankInfo-tabellen
                        string bankQuery = @"
                    UPDATE RenterBankInfo
                    SET BankInfo = @BankInfo
                    WHERE RenterID = @RenterID";

                        SqlCommand bankCommand = new SqlCommand(bankQuery, connection, transaction);
                        bankCommand.Parameters.AddWithValue("@BankInfo", renter.BankInfo ?? (object)DBNull.Value);
                        bankCommand.Parameters.AddWithValue("@RenterID", renter.RenterId);

                        bankCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
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

    }
}
