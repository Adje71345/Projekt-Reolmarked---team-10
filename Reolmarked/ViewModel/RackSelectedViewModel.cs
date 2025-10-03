using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;

namespace Reolmarked.ViewModel
{
    public class RackSelectedViewModel : ViewModelBase
    {
        // Repositories
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly IRenterRepository _renterRepository;

        // Reolen der vises detaljer for
        public Rack Rack { get; }

        //Properties til binding
        // Kontrakt status
        private bool _isOccupied;
        public bool IsOccupied
        {
            get => _isOccupied;
            private set => SetProperty(ref _isOccupied, value);
        }

        // Lejerens navn
        private string _renterName = "-";
        public string RenterName
        {
            get => _renterName;
            private set => SetProperty(ref _renterName, value);
        }

        // Startdato
        private DateOnly? _periodStart;
        public DateOnly? PeriodStart
        {
            get => _periodStart;
            private set { if (SetProperty(ref _periodStart, value)) UpdateDerived(); }
        }

        // Slutdato
        private DateOnly? _periodEnd;
        public DateOnly? PeriodEnd
        {
            get => _periodEnd;
            private set { if (SetProperty(ref _periodEnd, value)) UpdateDerived(); }
        }

        // UI visning af periode
        private string _periodText = "-";
        public string PeriodText
        {
            get => _periodText;
            private set => SetProperty(ref _periodText, value);
        }

        // UI visning af antal dage tilbage
        private int? _daysLeft;
        public int? DaysLeft
        {
            get => _daysLeft;
            private set => SetProperty(ref _daysLeft, value);
        }

        // Hjælpeproperty til at afgøre om der kan opsiges en kontrakt
        public bool CanTerminate => IsOccupied && PeriodEnd == null;

        // Navigation
        public ICommand AddContractCommand { get; }
        public ICommand EndContractCommand { get; }
        private readonly Action _goToAddContract;
        private readonly Action _goToEndContract;

        // Constructor
        public RackSelectedViewModel(
            Rack rack, IRentalContractRepository rentalContractRepository,
            IRenterRepository renterRepository,
            Action goToAddContract,
            Action goToEndContract)
        {
            Rack = rack ?? throw new ArgumentNullException(nameof(rack));
            _rentalContractRepository = rentalContractRepository;
            _renterRepository = renterRepository;
            _goToAddContract = goToAddContract ?? (() => { });
            _goToEndContract = goToEndContract ?? (() => { });

            LoadDetails(rack.RackId);

            AddContractCommand = new RelayCommand(() => _goToAddContract());
            EndContractCommand = new RelayCommand(() => _goToEndContract());
        }

        // Henter kontrakt og lejer detaljer for den valgte reol
        private void LoadDetails(int rackId)
        {
            var contract = _rentalContractRepository.GetActiveContractByRack(rackId);

            if (contract != null)
            {
                IsOccupied = true;

                var renter = _renterRepository.GetById(contract.RenterId);
                RenterName = renter != null
                    ? $"{renter.FirstName} {renter.LastName}"
                    : $"Lejer #{contract.RenterId}";

                PeriodStart = contract.StartDate;
                PeriodEnd = contract.EndDate;
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

        // Opdaterer afledte properties baseret på start- og slutdato
        private void UpdateDerived()
        {
            var ci = new CultureInfo("da-DK");
            // Formaterer dato som "d. mmm" eller tom hvis null
            Func<DateOnly?, string> dm = dt =>
            {
                if (!dt.HasValue) return string.Empty;
                var m = dt.Value.ToDateTime(TimeOnly.MinValue).ToString("MMM", ci).ToLower();
                return $"{dt.Value.Day}. {m}";
            };
            // Sætter PeriodText baseret på start- og slutdato
            if (!PeriodStart.HasValue && !PeriodEnd.HasValue)
                PeriodText = "-";
            else if (PeriodStart.HasValue && !PeriodEnd.HasValue)
                PeriodText = dm(PeriodStart) + " –";
            else if (!PeriodStart.HasValue && PeriodEnd.HasValue)
                PeriodText = "– " + dm(PeriodEnd);
            else
                PeriodText = dm(PeriodStart) + " – " + dm(PeriodEnd);
            // Beregner DaysLeft baseret på slutdato
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
