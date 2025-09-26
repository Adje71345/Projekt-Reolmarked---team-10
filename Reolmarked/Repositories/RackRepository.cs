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
                INSERT INTO Rack (RackStatusId)
                VALUES (@RackStatusId);
                SELECT SCOPE_IDENTITY();";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackStatusId", rack.RackStatusId);

            
            rack.RackId = Convert.ToInt32(command.ExecuteScalar());
        }

       // Opdaterer en eksisterende reol i databasen
        public void Update(Rack rack)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Rack
                SET RackStatusId = @RackStatusId
                WHERE RackId = @RackId";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackStatusId", rack.RackStatusId);
            command.Parameters.AddWithValue("@RackId", rack.RackId);

            command.ExecuteNonQuery();
        }

        // Sletter en reol i databasen, ud fra id
        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM Rack WHERE RackId = @RackId";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackId", id);

            command.ExecuteNonQuery();
        }

        // Finder en reol i database ud fra id
        public Rack GetById(int id)
        {
            Rack rack = null;
            string query = @"
                SELECT r.RackId, r.RackStatusId, rs.StatusName
                FROM Rack r
                JOIN RackStatus rs ON r.RackStatusId = rs.RackStatusId
                WHERE r.RackId = @RackId";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackId", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                rack = new Rack(
                    (int)reader["RackId"],
                    (int)reader["RackStatusId"],
                    (string)reader["StatusName"]
                );
            }

            return rack;
        }

        // henter alle reoler fra database 
        public IEnumerable<Rack> GetAll()
        {
            var racks = new List<Rack>();
            string query = @"
                SELECT r.RackId, r.RackStatusId, rs.StatusName
                FROM Rack r
                JOIN RackStatus rs ON r.RackStatusId = rs.RackStatusId";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                racks.Add(new Rack(
                    (int)reader["RackId"],
                    (int)reader["RackStatusId"],
                    (string)reader["StatusName"]
                ));
            }

            return racks;
        }

        // Henter alle reoler med status = Ledig (til Reoloversigt)
        public IEnumerable<Rack> GetAvailableRacks()
        {
            var racks = new List<Rack>();
            string query = @"
                SELECT r.RackId, r.RackStatusId, rs.StatusName
                FROM Rack r
                JOIN RackStatus rs ON r.RackStatusId = rs.RackStatusId
                WHERE r.RackStatusId = @RackStatusId";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackStatusId", 1); // 1 = Ledig

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                racks.Add(new Rack(
                    (int)reader["RackId"],
                    (int)reader["RackStatusId"],
                    (string)reader["StatusName"]
                ));
            }

            return racks;
        }

        // Henter alle reoler med status = Optaget (til reoloversigt)
        public IEnumerable<Rack> GetOccupiedRacks()
        {
            var racks = new List<Rack>();
            string query = @"
                SELECT r.RackId, r.RackStatusId, rs.StatusName
                FROM Rack r
                JOIN RackStatus rs ON r.RackStatusId = rs.RackStatusId
                WHERE r.RackStatusId = @RackStatusId";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackStatusId", 2); // 2 = Optaget

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                racks.Add(new Rack(
                    (int)reader["RackId"],
                    (int)reader["RackStatusId"],
                    (string)reader["StatusName"]
                ));
            }

            return racks;
        }


        // Opdaterer status på en enkelt reol (for læsbarhed i domænemetoder, kan evt fjernes)
        public void UpdateRackStatus(int rackId, int newStatusId)
        {
            string query = "UPDATE Rack SET RackStatusId = @RackStatusId WHERE RackId = @RackId";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackStatusId", newStatusId);
            command.Parameters.AddWithValue("@RackId", rackId);

            connection.Open();
            command.ExecuteNonQuery();
        }

        //  Opdaterer status for alle reoler, hvor kontrakten er udløbet (kan bruges når man åbner reolovertsigt UI, eller evt på en "opdater" knap i UI?
        public void UpdateStatusesForEndedContracts()
        {
            string query = @"
                UPDATE Rack
                SET RackStatusId = @Ledig
                WHERE RackId IN (
                    SELECT RackId
                    FROM RentalContract
                    WHERE EndDate IS NOT NULL AND EndDate <= @Today
                )";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Ledig", 1); // 1 = Ledig
            command.Parameters.AddWithValue("@Today", DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue));

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
