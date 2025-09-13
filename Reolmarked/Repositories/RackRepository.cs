using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

        // Henter alle reoler fra databasen 

        public IEnumerable<Rack> GetAll()
        {
            var racks = new List<Rack>();
            string query = @"
                SELECT RackId, Status
                FROM Rack";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        racks.Add(new Rack
                        {
                            RackId = (int)reader["RackId"],
                            Status = (RackStatus)(int)reader["Status"]
                        });
                    }
                }
            }
            return racks;
        }

        // Henter en enkelt reol baseret på Id
        public Rack GetById(int id)
        {
            Rack rack = null;
            string query = @"
                SELECT RackID, Status
                FROM RACK
                WHERE RackId = @RackId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackID", id);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rack = new Rack
                        {
                            RackId = (int)reader["RackID"],
                            Status = (RackStatus)(int)reader["Status"]
                        };
                    }
                }
            }
            return rack;
        }

        // Henter alle reoler med ledig status 
        public IEnumerable<Rack> GetAvailableRacks()
        {
            var racks = new List<Rack>();
            string query = "SELECT RackID, Status FROM Rack WHERE Status = @Status";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", (int)RackStatus.Ledig);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        racks.Add(new Rack
                        {
                            RackId = (int)reader["RackID"],
                            Status = (RackStatus)(int)reader["Status"]
                        });
                    }
                }
            }

            return racks;
        }

        // Henter alle reoler med optaget status 
        public IEnumerable<Rack> GetOccupiedRacks()
        {
            var racks = new List<Rack>();
            string query = "SELECT RackID, Status FROM Rack WHERE Status = @Status";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", (int)RackStatus.Optaget);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        racks.Add(new Rack
                        {
                            RackId = (int)reader["RackID"],
                            Status = (RackStatus)(int)reader["Status"]
                        });
                    }
                }
            }

            return racks;
        }

        // Opdaterer status for en reol 
        public void UpdateRackStatus(int rackId, RackStatus? newStatus)
        {
            string query = "UPDATE Rack SET Status = @Status WHERE RackID = @RackID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", (int)newStatus);
                command.Parameters.AddWithValue("@RackID", rackId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        // Tilføjer en ny reol til databasen
        public void Add(Rack rack)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                INSERT INTO Rack (Status)
                VALUES (@Status);
                SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", (int)rack.Status);

                rack.RackId = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        // Opdaterer en eksisterense reoli databasen
        public void Update(Rack rack)
        {
            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                UPDATE Rack
                SET Status = @Status
                WHERE RackID = @RackID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", (int)rack.Status);
                command.Parameters.AddWithValue("@RackID", rack.RackId);

                command.ExecuteNonQuery();
            }
        }

        // Sletter en reol fra databasen baseret på RackId
        public void Delete(int id)
        {
            string query = "DELETE FROM Rack WHERE RackID = @RackID";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }

        }


    }



}
