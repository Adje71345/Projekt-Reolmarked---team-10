using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel.Helpers
{
    // Hjælpeklasse til at opdatere RackStatus baseret på aktive kontrakter
    public static class RackStatusHelper
    {
        public static void UpdateRackStatusesBasedOnContracts(
            IRackRepository rackRepo,
            IRentalContractRepository contractRepo)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var racks = rackRepo.GetAll();

            foreach (var rack in racks)
            {
                var contract = contractRepo.GetActiveContractByRack(rack.RackId);
                bool isOccupied = contract != null && (!contract.EndDate.HasValue || contract.EndDate.Value >= today);

                rack.RackStatusId = isOccupied ? 2 : 1;
                rackRepo.Update(rack);
            }
        }
    }
}
