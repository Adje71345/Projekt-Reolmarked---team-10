using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;

namespace Reolmarked.ViewModel
{
    public class RackSelectedViewModel : ViewModelBase
    {
        public Rack Rack { get; }

        private bool _isOccupied;
        public bool IsOccupied
        {
            get => _isOccupied;
            private set => SetProperty(ref _isOccupied, value);
        }

        private string _renterName = "-";
        public string RenterName
        {
            get => _renterName;
            private set => SetProperty(ref _renterName, value);
        }

        private DateOnly? _periodStart;
        public DateOnly? PeriodStart
        {
            get => _periodStart;
            private set { if (SetProperty(ref _periodStart, value)) UpdateDerived(); }
        }

        private DateOnly? _periodEnd;
        public DateOnly? PeriodEnd
        {
            get => _periodEnd;
            private set { if (SetProperty(ref _periodEnd, value)) UpdateDerived(); }
        }

        private string _periodText = "-";
        public string PeriodText
        {
            get => _periodText;
            private set => SetProperty(ref _periodText, value);
        }

        private int? _daysLeft;
        public int? DaysLeft
        {
            get => _daysLeft;
            private set => SetProperty(ref _daysLeft, value);
        }

        // Navigation
        public ICommand AddContractCommand { get; }
        public ICommand EndContractCommand { get; }
        private readonly Action _goToAddContract;
        private readonly Action _goToEndContract;

        public RackSelectedViewModel(
            Rack rack,
            Action goToAddContract,
            Action goToEndContract)
        {
            Rack = rack ?? throw new ArgumentNullException(nameof(rack));
            _goToAddContract = goToAddContract ?? (() => { });
            _goToEndContract = goToEndContract ?? (() => { });

            LoadDetails(rack);

            AddContractCommand = new RelayCommand(() => _goToAddContract());
            EndContractCommand = new RelayCommand(() => _goToEndContract());
        }

        // Demo-data indtil DB
        private static readonly HashSet<int> _occupiedIds =
            new HashSet<int>(new[] { 12, 28, 43, 56, 61, 63, 65, 68, 70, 71, 73, 75, 78, 80 });

        private static readonly Dictionary<int, (string renter, DateOnly start, DateOnly? end)> _fake =
            new Dictionary<int, (string renter, DateOnly start, DateOnly? end)>
            {
                { 12, ("Anna Pyjamas", new DateOnly(DateTime.Now.Year, 9, 1), new DateOnly(DateTime.Now.Year, 9, 30)) },
                { 28, ("Niels Ninja", new DateOnly(DateTime.Now.Year, 9, 3), null) },
                { 43, ("Peter Edderkop", new DateOnly(DateTime.Now.Year, 8, 15), new DateOnly(DateTime.Now.Year, 12, 31)) },
            };

        private void LoadDetails(Rack rack)
        {
            if (_occupiedIds.Contains(rack.RackId) && _fake.TryGetValue(rack.RackId, out var c))
            {
                IsOccupied = true;
                RenterName = c.renter;
                PeriodStart = c.start;
                PeriodEnd = c.end;
            }
            else
            {
                IsOccupied = false;
                RenterName = "-";
                PeriodStart = null;
                PeriodEnd = null;
            }

            UpdateDerived();
        }

        private void UpdateDerived()
        {
            var ci = new CultureInfo("da-DK");
            Func<DateOnly?, string> dm = dt =>
            {
                if (!dt.HasValue) return string.Empty;
                var m = dt.Value.ToDateTime(TimeOnly.MinValue).ToString("MMM", ci).ToLower();
                return $"{dt.Value.Day}. {m}";
            };

            if (!PeriodStart.HasValue && !PeriodEnd.HasValue)
                PeriodText = "-";
            else if (PeriodStart.HasValue && !PeriodEnd.HasValue)
                PeriodText = dm(PeriodStart) + " –";
            else if (!PeriodStart.HasValue && PeriodEnd.HasValue)
                PeriodText = "– " + dm(PeriodEnd);
            else
                PeriodText = dm(PeriodStart) + " – " + dm(PeriodEnd);

            if (PeriodEnd.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                int days = (PeriodEnd.Value.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;
                DaysLeft = days < 0 ? 0 : days;
            }
            else
            {
                DaysLeft = null;
            }
        }
    }
}
