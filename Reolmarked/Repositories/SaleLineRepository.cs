using Microsoft.Data.SqlClient;
using Reolmarked.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reolmarked.Repositories
{
    public class SaleLineRepository : ISaleLineRepository
    {
        private readonly string _connectionString;
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

            string query = @"
                INSERT INTO SaleLine (SaleDate, Price, RackID)
                VALUES (@SaleDate, @Price, @RackID);
                SELECT SCOPE_IDENTITY();";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleDate", saleLine.SaleDate);
            saleLine.SaleLineId = Convert.ToInt32(command.ExecuteScalar());
        }

        public void Update(SaleLine saleLine)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = @"
                UPDATE SaleLine
                SET SaleDate = @SaleDate,
                    Price = @Price,
                    RackID = @RackID
                WHERE SaleLineID = @SaleLineID";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleDate", saleLine.SaleDate);
            command.Parameters.AddWithValue("@Price", saleLine.Price);
            command.Parameters.AddWithValue("@RackID", saleLine.RackId);
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = "DELETE FROM SaleLine WHERE SaleLineID = @SaleLineID";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineID", id);
            command.ExecuteNonQuery();
        }

        public IEnumerable<SaleLine> GetAll()
        {
            var saleLines = new List<SaleLine>();
            string query = @"
                    SELECT SaleLineID, SaleDate, Price, RackID 
                    FROM SaleLine sl";

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using (SqlDataReader reader = new SqlCommand(query, connection).ExecuteReader())
            {
                while (reader.Read())
                {
                    saleLines.Add(new SaleLine
                    {
                        SaleLineId = (int)reader["SaleLineID"],
                        SaleDate = (DateTime)reader["SaleDate"],
                        Price = (decimal)reader["Price"],
                        RackId = (int)reader["RackID"]
                    });
                }
            }
            return saleLines;
        }

        public SaleLine GetById(int id)
        {
            SaleLine saleLine = null;
            string query = "SELECT SaleLineID, SaleDate, Price, RackID FROM SaleLine WHERE SaleLineID = @SaleLineID";   
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineID", id);
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
