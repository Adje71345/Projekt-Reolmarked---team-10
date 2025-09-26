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

        
        // Tilføjer en ny kontrakt
        public void Add(RentalContract rentalContract)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO RentalContract (RenterID, RackID, StartDate, EndDate)
                VALUES (@RenterID, @RackID, @StartDate, @EndDate);
                SELECT SCOPE_IDENTITY();";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RenterID", rentalContract.RenterId);
            command.Parameters.AddWithValue("@RackID", rentalContract.RackId);

            // konverterer DateOnly til DateTime, fordi sql ikke kender dateonly
            command.Parameters.AddWithValue("@StartDate", rentalContract.StartDate.ToDateTime(TimeOnly.MinValue));
            command.Parameters.AddWithValue("@EndDate", rentalContract.EndDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);

            rentalContract.RentalId = Convert.ToInt32(command.ExecuteScalar());
        }

        // Opdaterer en kontrakt
        public void Update(RentalContract rentalContract)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE RentalContract
                SET RenterID = @RenterID,
                    RackID = @RackID,
                    StartDate = @StartDate,
                    EndDate = @EndDate
                WHERE RentalID = @RentalID";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RenterID", rentalContract.RenterId);
            command.Parameters.AddWithValue("@RackID", rentalContract.RackId);
            command.Parameters.AddWithValue("@StartDate", rentalContract.StartDate.ToDateTime(TimeOnly.MinValue));
            command.Parameters.AddWithValue("@EndDate", rentalContract.EndDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RentalID", rentalContract.RentalId);

            command.ExecuteNonQuery();
        }

        // Sletter en kontrakt ud fra ID
        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM RentalContract WHERE RentalID = @RentalID";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RentalID", id);

            command.ExecuteNonQuery();
        }

        // Finder kontrakt ud fra ID
        public RentalContract GetById(int id)
        {
            RentalContract contract = null;
            string query = "SELECT RentalID, RenterID, RackID, StartDate, EndDate FROM RentalContract WHERE RentalID = @RentalID";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RentalID", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                contract = new RentalContract
                {
                    RentalId = (int)reader["RentalID"],
                    RenterId = (int)reader["RenterID"],
                    RackId = (int)reader["RackID"],

                    StartDate = DateOnly.FromDateTime((DateTime)reader["StartDate"]),
                    EndDate = reader["EndDate"] == DBNull.Value
                        ? null
                        : DateOnly.FromDateTime((DateTime)reader["EndDate"])
                };
            }

            return contract;
        }

        // Henter alle kontrakter
        public IEnumerable<RentalContract> GetAll()
        {
            var contracts = new List<RentalContract>();
            string query = "SELECT RentalID, RenterID, RackID, StartDate, EndDate FROM RentalContract";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                contracts.Add(new RentalContract
                {
                    RentalId = (int)reader["RentalID"],
                    RenterId = (int)reader["RenterID"],
                    RackId = (int)reader["RackID"],

                    StartDate = DateOnly.FromDateTime((DateTime)reader["StartDate"]),
                    EndDate = reader["EndDate"] == DBNull.Value
                        ? null
                        : DateOnly.FromDateTime((DateTime)reader["EndDate"])
                });
            }

            return contracts;
        }

        

        // Finder aktiv kontrakt for en bestemt reol
        public RentalContract GetActiveContractByRack(int rackId)
        {
            RentalContract contract = null;
            string query = @"
                SELECT RentalID, RenterID, RackID, StartDate, EndDate
                FROM RentalContract
                WHERE RackID = @RackID AND EndDate IS NULL";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackID", rackId);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                contract = new RentalContract
                {
                    RentalId = (int)reader["RentalID"],
                    RenterId = (int)reader["RenterID"],
                    RackId = (int)reader["RackID"],

                    StartDate = DateOnly.FromDateTime((DateTime)reader["StartDate"]),
                    EndDate = null
                };
            }

            return contract;
        }

        // Finder alle aktive kontrakter for en bestemt lejer
        public IEnumerable<RentalContract> GetActiveContractsByRenter(int renterId)
        {
            var contracts = new List<RentalContract>();
            string query = @"
                SELECT RentalID, RenterID, RackID, StartDate, EndDate
                FROM RentalContract
                WHERE RenterID = @RenterID AND EndDate IS NULL";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RenterID", renterId);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                contracts.Add(new RentalContract
                {
                    RentalId = (int)reader["RentalID"],
                    RenterId = (int)reader["RenterID"],
                    RackId = (int)reader["RackID"],

                    StartDate = DateOnly.FromDateTime((DateTime)reader["StartDate"]),
                    EndDate = null
                });
            }

            return contracts;
        }

        // Opsiger en enkelt kontrakt med slutdato 
        public void EndSingleContract(int rentalId, DateOnly endDate)
        {
            var contract = GetById(rentalId);
            if (contract == null) return;

            contract.EndDate = endDate;
            Update(contract);

            // Bemærk: Rack status ændres ikke her
            // Det håndteres i RackRepository.UpdateStatusesForEndedContracts()
        }
    }
}
