using Microsoft.Data.SqlClient;
using Reolmarked.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Reolmarked.Repositories
{
    public class SaleLineRepository : ISaleLineRepository
    {
        private readonly string _connectionString;

        // Repository for RentalContract til metoden, der filtrerer salelines med aktive kontakt
        private readonly RentalContractRepository _rentalContractRepository;

        public SaleLineRepository(string connectionString, RentalContractRepository rentalContractRepository)
        {
            _connectionString = connectionString;
            _rentalContractRepository = rentalContractRepository;
        }

        public void Add(SaleLine saleLine)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
             INSERT INTO SaleLine (SaleDate, Price, RackId)
             VALUES (@SaleDate, @Price, @RackId);
             SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SaleDate", saleLine.SaleDate);
            cmd.Parameters.AddWithValue("@Price", saleLine.Price);
            cmd.Parameters.AddWithValue("@RackId", saleLine.RackId);

            saleLine.SaleLineId = (int)cmd.ExecuteScalar();
        }

        // Tilføjer mange salgslinjer på en gang (bruges ved månedsafslutning)
        public void AddMany(IEnumerable<SaleLine> lines)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            const string sql = @"
                INSERT INTO SaleLine (SaleDate, Price, RackId)
                VALUES (@SaleDate, @Price, @RackId);";
            using var cmd = new SqlCommand(sql, connection, transaction);
            cmd.Parameters.Add("@SaleDate", SqlDbType.DateTime);
            cmd.Parameters.Add("@Price", SqlDbType.Decimal);
            cmd.Parameters.Add("@RackId", SqlDbType.Int);
            try
            {
                foreach (var line in lines)
                {
                    cmd.Parameters["@SaleDate"].Value = line.SaleDate;
                    cmd.Parameters["@Price"].Value = line.Price;
                    cmd.Parameters["@RackId"].Value = line.RackId;
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Update(SaleLine saleLine)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = @"
                UPDATE SaleLine
                SET SaleDate = @SaleDate,
                    Price = @Price,
                    RackID = @RackId
                WHERE SaleLineID = @SaleLineId";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleDate", saleLine.SaleDate);
            command.Parameters.AddWithValue("@Price", saleLine.Price);
            command.Parameters.AddWithValue("@RackId", saleLine.RackId);
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = "DELETE FROM SaleLine WHERE SaleLineId = @SaleLineId";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineId", id);
            command.ExecuteNonQuery();
        }

        public IEnumerable<SaleLine> GetAll()
        {
            var saleLines = new List<SaleLine>();
            string query = @"
                    SELECT SaleLineId, SaleDate, Price, RackId 
                    FROM SaleLine sl";

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using (SqlDataReader reader = new SqlCommand(query, connection).ExecuteReader())
            {
                while (reader.Read())
                {
                    saleLines.Add(new SaleLine
                    {
                        SaleLineId = (int)reader["SaleLineId"],
                        SaleDate = (DateTime)reader["SaleDate"],
                        Price = (decimal)reader["Price"],
                        RackId = (int)reader["RackId"]
                    });
                }
            }
            return saleLines;
        }

        public SaleLine GetById(int id)
        {
            SaleLine saleLine = null;
            string query = "SELECT SaleLineId, SaleDate, Price, RackId FROM SaleLine WHERE SaleLineId = @SaleLineId";   
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineId", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                saleLine = new SaleLine
                {
                    SaleLineId = (int)reader["SaleLineId"],
                    SaleDate = (DateTime)reader["SaleDate"],
                    Price = (decimal)reader["Price"],
                    RackId = (int)reader["RackId"]
                };
            }
            return saleLine;
        }

        // Ekstra metode til at hente dagens samlede salg. Bruges i DashBoardViewModel
        public decimal GetTotalSalesToday()
        {
            const string query = @"
                SELECT ISNULL(SUM(Price), 0)
                FROM SaleLine
                WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            connection.Open();
            return (decimal)command.ExecuteScalar();
        }

        public IEnumerable<SaleLine> GetSalesForRackWithActiveContractLastMonth(int rackId)
        {
            // 1. Find forrige måned
            var today = DateTime.Today;
            var lastMonth = today.AddMonths(-1);
            int year = lastMonth.Year;
            int month = lastMonth.Month;
            var startOfMonth = new DateOnly(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 2. Hent alle kontrakter og find én, der var aktiv i forrige måned
            var contracts = _rentalContractRepository.GetAll()
                .Where(c => c.RackId == rackId)
                .ToList();

            var activeContract = contracts
                .FirstOrDefault(c => c.StartDate <= endOfMonth && (c.EndDate == null || c.EndDate > endOfMonth));

            if (activeContract == null)
                return Enumerable.Empty<SaleLine>();

            // 3. Hent alle SaleLines og filtrer på rack og måned
            return GetAll()
                .Where(sl => sl.RackId == rackId &&
                             sl.SaleDate.Year == year &&
                             sl.SaleDate.Month == month)
                .ToList();
        }

        public decimal GetTotalSalesForRackWithActiveContractLastMonth(int rackId)
        {
            var sales = GetSalesForRackWithActiveContractLastMonth(rackId);
            return sales.Sum(sl => sl.Price);
        }

    }
}
