using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Reolmarked.ViewModel
{
    public class MonthlyStatementRow
    {
        public string LejerNavn { get; set; } = "";
        public int AntalReoler { get; set; }
        public decimal TotalSalg { get; set; }
        public decimal Kommission { get; set; }
        public decimal ReolLeje { get; set; }
        public decimal Nettoresultat { get; set; }
    }

    public class MonthlyStatementViewModel : ViewModelBase
    {
        // Drop-downs
        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<string> Months { get; } = new();

        // Tabel
        public ObservableCollection<MonthlyStatementRow> Rows { get; } = new();

        // Totals til footer
        public decimal SumTotalSalg => Rows?.Sum(r => r.TotalSalg) ?? 0m;
        public decimal SumKommission => Rows?.Sum(r => r.Kommission) ?? 0m;
        public decimal SumReolLeje => Rows?.Sum(r => r.ReolLeje) ?? 0m;
        public decimal SumNettoresultat => Rows?.Sum(r => r.Nettoresultat) ?? 0m;

        // Valgt år
        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    OnPropertyChanged(nameof(PeriodText)); // opdater teksten “September 2025”
                    LoadRows();                            // genindlæs rækker (dummy)
                }
            }
        }

        // Valgt måned (navn)
        private string _selectedMonth = "";
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    OnPropertyChanged(nameof(PeriodText));
                    LoadRows();
                }
            }
        }

        // Teksten der vises ved “Afregning · {PeriodText}”
        public string PeriodText =>
            string.IsNullOrWhiteSpace(SelectedMonth)
                ? SelectedYear.ToString()
                : $"{SelectedMonth} {SelectedYear}";

        public MonthlyStatementViewModel()
        {
            // Fyld ÅR (aktuelt år og 5 tilbage)
            var thisYear = DateTime.Today.Year;
            for (int i = 0; i < 6; i++) Years.Add(thisYear - i);
            SelectedYear = thisYear;

            // Fyld MÅNEDER (dansk)
            var monthNames = new[]
            {
                "Januar","Februar","Marts","April","Maj","Juni",
                "Juli","August","September","Oktober","November","December"
            };
            foreach (var m in monthNames) Months.Add(m);

            // Vælg aktuel måned
            SelectedMonth = Months[DateTime.Today.Month - 1];

            LoadRows();
        }

        // Pris pr. reol pr. måned:
        // 1 reol = 850 kr.
        // 2-3 reoler = 825 kr. pr. reol
        // 4+ reoler = 800 kr. pr. reol
        private static decimal CalcReolLeje(int antalReoler)
        {
            decimal prisPrReol = antalReoler == 1 ? 850m
                                : (antalReoler <= 3 ? 825m : 800m);
            return prisPrReol * antalReoler;
        }

        private void LoadRows()
        {
            Rows.Clear();

            // Dummy dataer
            var dummy = new[]
            {
        new MonthlyStatementRow { LejerNavn = "Anna Jensen",     AntalReoler = 2, TotalSalg = 3150m, Kommission = 630m },
        new MonthlyStatementRow { LejerNavn = "Mads Sørensen",    AntalReoler = 1, TotalSalg =  980m, Kommission = 196m },
        new MonthlyStatementRow { LejerNavn = "Ida Holm",         AntalReoler = 3, TotalSalg = 4210m, Kommission = 842m },
        new MonthlyStatementRow { LejerNavn = "Lars Mikkelsen",   AntalReoler = 4, TotalSalg = 2540m, Kommission = 508m },
        new MonthlyStatementRow { LejerNavn = "Sofie Tran",       AntalReoler = 5, TotalSalg = 3875m, Kommission = 775m },
    };

            foreach (var r in dummy)
            {
                r.ReolLeje = CalcReolLeje(r.AntalReoler);
                r.Nettoresultat = r.TotalSalg - r.Kommission - r.ReolLeje;
                Rows.Add(r);
            }

            // signaler at totals er ændret
            RaiseSumChanges();
        }
        private void RaiseSumChanges()
        {
            OnPropertyChanged(nameof(SumTotalSalg));
            OnPropertyChanged(nameof(SumKommission));
            OnPropertyChanged(nameof(SumReolLeje));
            OnPropertyChanged(nameof(SumNettoresultat));
        }
    }
}
