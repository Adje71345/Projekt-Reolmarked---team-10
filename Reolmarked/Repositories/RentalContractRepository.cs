using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public class RentalContractRepository : IRentalContractRepository
    {
        private readonly string _connectionString;

        public RentalContractRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Tilføjer en ny lejekontrakt
        public void Add(RentalContract rentalContract)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                INSERT INTO RentalContract (RenterID, RackID, StartDate, EndDate)
                VALUES (@RenterID, @RackID, @StartDate, @EndDate);
                SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", rentalContract.RenterId);
                command.Parameters.AddWithValue("@RackID", rentalContract.RackId);
                command.Parameters.AddWithValue("@StartDate", rentalContract.StartDate);
                command.Parameters.AddWithValue("@EndDate", rentalContract.EndDate?? (object)DBNull.Value);

                rentalContract.RentalId = Convert.ToInt32(command.ExecuteScalar());
            }

            // Opdater reolstatus til Optaget
            var rackRepo = new RackRepository(_connectionString);
            var rack = new Rack
            {
                RackId = rentalContract.RackId,
                Status = RackStatus.Optaget
            };
            rackRepo.Update(rack);
        }

        // Sletter en lejekontrakt
        public void Delete(int id)
        {
            string query = "DELETE FROM RentalContract WHERE RentalID = @RentalID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RentalID", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Henter aktiv kontrakt for en bestemt reol
        public RentalContract GetActiveContractByRack(int rackId)
        {
            RentalContract contract = null;
            string query = @"
                SELECT RentalID, RenterID, RackID, StartDate, EndDate
                FROM RentalContract
                WHERE RackID = @RackID AND EndDate IS NULL";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackID", rackId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contract = new RentalContract
                        {
                            RentalId = (int)reader["RentalID"],
                            RenterId = (int)reader["RenterID"],
                            RackId = (int)reader["RackID"],
                            StartDate = (DateOnly)reader["StartDate"],
                            EndDate = null
                        };
                    }
                }
            }

            return contract;
        }

        // Henter alle aktive kontrakter for en lejer
        public IEnumerable<RentalContract> GetActiveContractsByRenter(int renterId)
        {
            var contracts = new List<RentalContract>();
            string query = @"
                SELECT RentalID, RenterID, RackID, StartDate, EndDate
                FROM RentalContract
                WHERE RenterID = @RenterID AND EndDate IS NULL";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", renterId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contracts.Add(new RentalContract
                        {
                            RentalId = (int)reader["RentalID"],
                            RenterId = (int)reader["RenterID"],
                            RackId = (int)reader["RackID"],
                            StartDate = (DateOnly)reader["StartDate"],
                            EndDate = null
                        });
                    }
                }
            }

            return contracts;
        }

        // Henter alle kontrakter
        public IEnumerable<RentalContract> GetAll()
        {
            var contracts = new List<RentalContract>();
            string query = "SELECT RentalID, RenterID, RackID, StartDate, EndDate FROM RentalContract";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contracts.Add(new RentalContract
                        {
                            RentalId = (int)reader["RentalID"],
                            RenterId = (int)reader["RenterID"],
                            RackId = (int)reader["RackID"],
                            StartDate = (DateOnly)reader["StartDate"],
                            EndDate = reader["EndDate"] == DBNull.Value ? null : (DateOnly?)reader["EndDate"]
                        });
                    }
                }
            }

            return contracts;
        }

        // Henter én kontrakt baseret på ID
        public RentalContract GetById(int id)
        {
            RentalContract contract = null;
            string query = "SELECT RentalID, RenterID, RackID, StartDate, EndDate FROM RentalContract WHERE RentalID = @RentalID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RentalID", id);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contract = new RentalContract
                        {
                            RentalId = (int)reader["RentalID"],
                            RenterId = (int)reader["RenterID"],
                            RackId = (int)reader["RackID"],
                            StartDate = (DateOnly)reader["StartDate"],
                            EndDate = reader["EndDate"] == DBNull.Value ? null : (DateOnly?)reader["EndDate"]
                        };
                    }
                }
            }

            return contract;
        }

        // Fornyer en kontrakt med ny slutdato (eller fjerner den)
        public void RenewContract(int rentalId, DateTime? newEndDate)
        {
            string query = @"
                UPDATE RentalContract
                SET EndDate = @EndDate
                WHERE RentalID = @RentalID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EndDate", newEndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RentalID", rentalId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Opsiger kontrakt via reol
        public void TerminateContractByRack(int rackId)
        {
            var contract = GetActiveContractByRack(rackId);
            if (contract == null) return;

            string query = "UPDATE RentalContract SET EndDate = @EndDate WHERE RentalID = @RentalID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EndDate", DateTime.Now);
                command.Parameters.AddWithValue("@RentalID", contract.RentalId);

                connection.Open();
                command.ExecuteNonQuery();
            }

            new RackRepository(_connectionString).UpdateRackStatus(rackId, RackStatus.Ledig);
        }

        // Opsiger alle aktive kontrakter for en lejer
        public void TerminateContractByRenter(int renterId)
        {
            var contracts = GetActiveContractsByRenter(renterId);
            foreach (var contract in contracts)
            {
                string query = "UPDATE RentalContract SET EndDate = @EndDate WHERE RentalID = @RentalID";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EndDate", DateTime.Now);
                    command.Parameters.AddWithValue("@RentalID", contract.RentalId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                new RackRepository(_connectionString).UpdateRackStatus(contract.RackId, RackStatus.Ledig);
            }
        }

        // Opdaterer en eksisterende kontrakt
        public void Update(RentalContract rentalContract)
        {
            string query = @"
                UPDATE RentalContract
                SET RenterID = @RenterID,
                    RackID = @RackID,
                    StartDate = @StartDate,
                    EndDate = @EndDate
                WHERE RentalID = @RentalID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterID", rentalContract.RenterId);
                command.Parameters.AddWithValue("@RackID", rentalContract.RackId);
                command.Parameters.AddWithValue("@StartDate", rentalContract.StartDate);
                command.Parameters.AddWithValue("@EndDate", rentalContract.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RentalID", rentalContract.RentalId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

}
