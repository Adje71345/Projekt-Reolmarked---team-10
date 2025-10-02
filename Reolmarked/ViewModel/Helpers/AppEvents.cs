using System;
using Reolmarked.Model;

namespace Reolmarked.ViewModel.Helpers
{
    // Enkel, app-bred event-bus (pub/sub) for tværgående beskeder
    public static class AppEvents
    {
        // FYR: Når et salg er gemt i DB. Kan også bruges til at opdatere UI andre steder
        public static event Action<SaleLine>? SaleCommitted;

        public static void RaiseSaleCommitted(SaleLine sale)
            => SaleCommitted?.Invoke(sale);
    }
}
