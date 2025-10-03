using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Model;

namespace Reolmarked.ViewModel.Helpers
{
    public class RentalContractDisplayModel : ViewModelBase
    {
        private readonly RentalContract _contract;

        public RentalContractDisplayModel(RentalContract contract)
        {
            _contract = contract ?? throw new ArgumentNullException(nameof(contract));
            EndDate = contract.EndDate;
        }

        public string ShelfName => $"Reol {_contract.RackId}";

        public string Period =>
            $"{_contract.StartDate:dd. MMM yyyy}" +
            (_contract.EndDate.HasValue ? $" – {_contract.EndDate:dd. MMM yyyy}" : " – aktiv");

        public DateOnly? EndDate { get; }
        public bool CanTerminate => EndDate == null;

    }
}
