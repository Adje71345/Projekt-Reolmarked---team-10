using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Reolmarked.Model
{
    public class MonthlyStatement
    {
        //Attributter
        public int StatementId { get; set; }
        public int RenterId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public double TotalSales { get; set; }
        public double Commission { get; set; }
        public double RentalFee { get; set; }
        public double NetResult { get; set; }
        public DateTime CreatedDate { get; set; }
        public enum Status
        {
            Active,
            Locked
        }

        //Constructor
        public MonthlyStatement(int statementId, int renterId, int month, int year, double totalSales, double commission, double rentalFee, double netResult, DateTime createdDate)
        {
            StatementId = statementId;
            RenterId = renterId;
            Month = month;
            Year = year;
            TotalSales = totalSales;
            Commission = commission;
            RentalFee = rentalFee;
            NetResult = netResult;
            CreatedDate = createdDate;
        }
        public MonthlyStatement() { }
    }
}
