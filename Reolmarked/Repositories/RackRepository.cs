using Microsoft.Data.SqlClient;
using Reolmarked.Model;

namespace Reolmarked.Repositories
{
    public class RackRepository : IRackRepository
    {
        private readonly string _connectionString;

        public RackRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Tilføjer en ny reol i databasen
        public void Add(Rack rack)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO Rack (Status)
                VALUES (@Status);
                SELECT SCOPE_IDENTITY();";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", (int)rack.Status);

            
            rack.RackId = Convert.ToInt32(command.ExecuteScalar());
        }

       // Opdaterer en eksisterende reol i databasen
        public void Update(Rack rack)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Rack
                SET Status = @Status
                WHERE RackID = @RackID";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", (int)rack.Status);
            command.Parameters.AddWithValue("@RackID", rack.RackId);

            command.ExecuteNonQuery();
        }

        // Sletter en reol i databasen, ud fra id
        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM Rack WHERE RackID = @RackID";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackID", id);

            command.ExecuteNonQuery();
        }

        // Finder en reol i database ud fra id
        public Rack GetById(int id)
        {
            Rack rack = null;
            string query = "SELECT RackID, Status FROM Rack WHERE RackID = @RackID";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackID", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                rack = new Rack
                {
                    RackId = (int)reader["RackID"],
                    Status = (RackStatus)(int)reader["Status"]
                };
            }

            return rack;
        }

        // henter alle reoler fra database 
        public IEnumerable<Rack> GetAll()
        {
            var racks = new List<Rack>();
            string query = "SELECT RackID, Status FROM Rack";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                racks.Add(new Rack
                {
                    RackId = (int)reader["RackID"],
                    Status = (RackStatus)(int)reader["Status"]
                });
            }

            return racks;
        }


        // Henter alle reoler med status = Ledig (til Reoloversigt)
        public IEnumerable<Rack> GetAvailableRacks()
        {
            var racks = new List<Rack>();
            string query = "SELECT RackID, Status FROM Rack WHERE Status = @Status";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", (int)RackStatus.Ledig);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                racks.Add(new Rack
                {
                    RackId = (int)reader["RackID"],
                    Status = (RackStatus)(int)reader["Status"]
                });
            }

            return racks;
        }

        // Henter alle reoler med status = Optaget (til reoloversigt)
        public IEnumerable<Rack> GetOccupiedRacks()
        {
            var racks = new List<Rack>();
            string query = "SELECT RackID, Status FROM Rack WHERE Status = @Status";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", (int)RackStatus.Optaget);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                racks.Add(new Rack
                {
                    RackId = (int)reader["RackID"],
                    Status = (RackStatus)(int)reader["Status"]
                });
            }

            return racks;
        }

        // Opdaterer status på en enkelt reol (for læsbarhed i domænemetoder, kan evt fjernes)
        public void UpdateRackStatus(int rackId, RackStatus newStatus)
        {
            string query = "UPDATE Rack SET Status = @Status WHERE RackID = @RackID";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", (int)newStatus);
            command.Parameters.AddWithValue("@RackID", rackId);

            connection.Open();
            command.ExecuteNonQuery();
        }

        //  Opdaterer status for alle reoler, hvor kontrakten er udløbet (kan bruges når man åbner reolovertsigt UI, eller evt på en "opdater" knap i UI?
        public void UpdateStatusesForEndedContracts()
        {
            string query = @"
                UPDATE Rack
                SET Status = @Ledig
                WHERE RackID IN (
                    SELECT RackID
                    FROM RentalContract
                    WHERE EndDate IS NOT NULL AND EndDate <= @Today
                )";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Ledig", (int)RackStatus.Ledig);

            
            command.Parameters.AddWithValue("@Today", DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue));

            connection.Open();
            command.ExecuteNonQuery();
        }

    }
}
