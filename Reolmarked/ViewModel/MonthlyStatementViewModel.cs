using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Reolmarked.Repositories;
using Reolmarked.Model;
using Reolmarked.ViewModel.Helpers;

namespace Reolmarked.ViewModel
{
    /// <summary>
    /// En række i månedsopgørelsen (det som bindes til DataGridet i viewet).
    /// </summary>
    public class MonthlyStatementRow
    {
        // Lejerens fulde navn
        public string LejerNavn { get; set; } = "";

        // Antal reoler som lejeren aktuelt har
        public int AntalReoler { get; set; }

        // Månedens samlede salg for lejeren
        public decimal TotalSalg { get; set; }

        // Kommission (pt. 10% af TotalSalg)
        public decimal Kommission { get; set; }

        // Samlet reolleje for måneden (pristrappe)
        public decimal ReolLeje { get; set; }

        // Nettoresultat = TotalSalg - Kommission - ReolLeje
        public decimal Nettoresultat { get; set; }
    }

    public class MonthlyStatementViewModel : ViewModelBase
    {
        // Fast kommissionssats (10 %)
        private const decimal CommissionRate = 0.10m;

        // Repositories (DI)
        private readonly IRenterRepository _renterRepository;
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly ISaleLineRepository _saleLineRepository;

        // ========= Drop-downs =========

        // Årsværdier til ComboBox i UI
        public ObservableCollection<int> Years { get; } = new();

        // Månedsnavne til ComboBox i UI (Januar..December)
        public ObservableCollection<string> Months { get; } = new();

        // ========= Tabel =========

        // Rækkerne der vises i DataGrid'et
        public ObservableCollection<MonthlyStatementRow> Rows { get; } = new();

        // ========= Totals =========

        // SUM: antal reoler på tværs af alle rækker
        public int SumAntalReoler => Rows?.Sum(r => r.AntalReoler) ?? 0;

        // SUM: TotalSalg
        public decimal SumTotalSalg => Rows?.Sum(r => r.TotalSalg) ?? 0m;

        // SUM: Kommission
        public decimal SumKommission => Rows?.Sum(r => r.Kommission) ?? 0m;

        // SUM: ReolLeje
        public decimal SumReolLeje => Rows?.Sum(r => r.ReolLeje) ?? 0m;

        // SUM: Nettoresultat
        public decimal SumNettoresultat => Rows?.Sum(r => r.Nettoresultat) ?? 0m;

        // ========= Valgte filterværdier =========

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                // Opdater kun hvis værdien faktisk ændrer sig (SetProperty håndterer INotifyPropertyChanged)
                if (SetProperty(ref _selectedYear, value))
                {
                    // Periodeteksten ændrer sig når år/måned ændres
                    OnPropertyChanged(nameof(PeriodText));

                    // Genindlæs data baseret på nyt filter
                    LoadRowsFromDb();
                }
            }
        }

        private string _selectedMonth = "";
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    OnPropertyChanged(nameof(PeriodText));
                    LoadRowsFromDb();
                }
            }
        }

        /// <summary>
        /// Tekst der vises under overskriften "Månedopgørelse".
        /// </summary>
        public string PeriodText =>
            string.IsNullOrWhiteSpace(SelectedMonth)
                ? SelectedYear.ToString()
                : $"{SelectedYear} {SelectedMonth}";

        /// <summary>
        /// Udleder start- og sluttidspunkt for den valgte måned (inkl. hele måneden).
        /// </summary>
        private (DateTime start, DateTime end) GetSelectedMonthBounds()
        {
            int monthIndex = Months.IndexOf(SelectedMonth) + 1; // 1..12
            if (monthIndex <= 0) monthIndex = DateTime.Today.Month; // fallback til indeværende måned
            var start = new DateTime(SelectedYear, monthIndex, 1);
            var end = start.AddMonths(1).AddTicks(-1); // inkl. hele måneden (sidste tick i måneden)
            return (start, end);
        }

        /// <summary>
        /// Pristrappe: 1 reol = 850, 2–3 reoler = 825/stk, 4+ reoler = 800/stk.
        /// Returnerer samlet reolleje for det angivne antal reoler.
        /// </summary>
        private static decimal CalcReolLeje(int antalReoler)
        {
            decimal prisPrReol = antalReoler == 1 ? 850m
                                : (antalReoler <= 3 ? 825m : 800m);
            return prisPrReol * antalReoler;
        }

        /// <summary>
        /// Beregner kommission (afrundet til 2 decimaler, AwayFromZero).
        /// </summary>
        private decimal ComputeCommission(decimal totalSalg)
        {
            return Math.Round(totalSalg * CommissionRate, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Udregner reolleje inkl. synliggørelse af rabat ift. fuld pris (850).
        /// Returnerer:
        ///  - faktisk pris pr. reol (efter pristrappe)
        ///  - rabat pr. reol
        ///  - samlet leje
        ///  - samlet rabat
        /// </summary>
        private (decimal prisPrReol, decimal rabatPrReol, decimal samletLeje, decimal samletRabat)
            ComputeRackRentWithDiscount(int antalReoler)
        {
            const decimal fuldPris = 850m; // uden mængderabat
            decimal faktiskPris = antalReoler == 1 ? 850m
                               : (antalReoler <= 3 ? 825m : 800m);

            decimal rabatPrReol = Math.Max(0, fuldPris - faktiskPris);
            decimal samletLeje = faktiskPris * antalReoler;
            decimal samletRabat = rabatPrReol * antalReoler;

            return (faktiskPris, rabatPrReol, samletLeje, samletRabat);
        }

        /// <summary>
        /// Nettoresultat = totalSalg - kommission - reolLeje.
        /// </summary>
        private decimal ComputeNet(decimal totalSalg, decimal kommission, decimal reolLeje)
        {
            return totalSalg - kommission - reolLeje;
        }

        // ========= Constructors =========

        /// <summary>
        /// Design-time ctor (for Blend/Designer). Kalder runtime-ctor med nulls.
        /// </summary>
        public MonthlyStatementViewModel() : this(null, null, null) { }

        /// <summary>
        /// Runtime ctor (DI): repositories injiceres udefra.
        /// Initialiserer år/måned, tilmelder events og loader data.
        /// </summary>
        public MonthlyStatementViewModel(
            IRenterRepository renterRepository,
            IRentalContractRepository rentalContractRepository,
            ISaleLineRepository saleLineRepository)
        {
            _renterRepository = renterRepository;
            _rentalContractRepository = rentalContractRepository;
            _saleLineRepository = saleLineRepository;

            // Opdater totals når Rows ændres (add/remove/reset)
            Rows.CollectionChanged += Rows_CollectionChanged;

            // Fyld år-dropdown (indeværende år + 5 tilbage)
            var thisYear = DateTime.Today.Year;
            for (int i = 0; i < 6; i++) Years.Add(thisYear - i);
            SelectedYear = thisYear;

            // Fyld måned-dropdown (danske navne)
            var monthNames = new[]
            {
                "Januar","Februar","Marts","April","Maj","Juni",
                "Juli","August","September","Oktober","November","December"
            };
            foreach (var m in monthNames) Months.Add(m);

            // Forvalg: indeværende måned
            SelectedMonth = Months[DateTime.Today.Month - 1];

            // Lyt efter “salg gennemført”-event, så tabel automatisk opdateres
            AppEvents.SaleCommitted += OnSaleCommitted;

            // Initial load
            LoadRowsFromDb();
        }

        // ========= Event handlers & helpers =========

        /// <summary>
        /// Når Rows ændres i antal/indhold, skal summationsfelter raises.
        /// </summary>
        private void Rows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // når rækker ændres, opdater totalfelter
            RaiseSumChanges();
        }

        /// <summary>
        /// Rejser PropertyChanged for alle totalfelter (footer).
        /// </summary>
        private void RaiseSumChanges()
        {
            OnPropertyChanged(nameof(SumAntalReoler));
            OnPropertyChanged(nameof(SumTotalSalg));
            OnPropertyChanged(nameof(SumKommission));
            OnPropertyChanged(nameof(SumReolLeje));
            OnPropertyChanged(nameof(SumNettoresultat));
        }

        /// <summary>
        /// Indlæser rækker fra DB baseret på valgt år/måned.
        /// (Hvis repositories er null => design-time: gør ingenting.)
        /// </summary>
        private void LoadRowsFromDb()
        {
            // Start med at rydde listen (så UI opdateres automatisk)
            Rows.Clear();

            // design-time short-circuit: ingen DB-arbejde uden repositories
            if (_renterRepository == null || _rentalContractRepository == null || _saleLineRepository == null)
                return;

            // Brug GetSelectedMonthBounds i stedet for selv at udregne
            // Det sikrer at vi altid har en gyldig måned og år
            var (start, end) = GetSelectedMonthBounds();

            // Kald repository-metoden med start.Year og start.Month
            var monthContracts = _rentalContractRepository
                .GetActiveRentalContractsByMonth(start.Year, start.Month)
                .ToList();

            // Hent alle lejere
            var allRenters = _renterRepository.GetAll().ToList();

            foreach (var renter in allRenters)
            {
                // Find de kontrakter som tilhører denne lejer i den valgte måned
                var contractsForRenter = monthContracts
                    .Where(c => c.RenterId == renter.RenterId)
                    .ToList();

                // Udtræk unikke reoler
                var rackIds = contractsForRenter
                    .Select(c => c.RackId)
                    .Distinct()
                    .ToList();

                int antalReoler = rackIds.Count;

                // Hent salg for disse reoler i den valgte måned
                var monthSales = _saleLineRepository
                    .GetAll()
                    .Where(sl => rackIds.Contains(sl.RackId)
                              && sl.SaleDate >= start
                              && sl.SaleDate <= end)
                    .ToList();

                // Udregn totals
                var totalSalg = monthSales.Sum(sl => sl.Price);
                var kommission = ComputeCommission(totalSalg);
                var (_, _, reolLeje, _) = ComputeRackRentWithDiscount(antalReoler);
                var netto = ComputeNet(totalSalg, kommission, reolLeje);

                // Tilføj kun en række hvis lejeren havde reoler eller salg i måneden
                if (antalReoler > 0 || totalSalg > 0)
                {
                    Rows.Add(new MonthlyStatementRow
                    {
                        LejerNavn = $"{renter.FirstName} {renter.LastName}".Trim(),
                        AntalReoler = antalReoler,
                        TotalSalg = totalSalg,
                        Kommission = kommission,
                        ReolLeje = reolLeje,
                        Nettoresultat = netto
                    });
                }
            }

            // Opdater totalfelter i footeren
            RaiseSumChanges();
        }



        /// <summary>
        /// Reagerer på et netop bogført salg.
        /// Hvis salget ligger i den aktuelt valgte måned, genindlæses rækkerne.
        /// </summary>
        private void OnSaleCommitted(Reolmarked.Model.SaleLine sale)
        {
            var (start, end) = GetSelectedMonthBounds();
            if (sale.SaleDate >= start && sale.SaleDate <= end)
                LoadRowsFromDb();
        }
    }
}
