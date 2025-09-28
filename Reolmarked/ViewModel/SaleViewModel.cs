using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Reolmarked.Commands;
using Reolmarked.Model;

namespace Reolmarked.ViewModel
{
    internal class SaleViewModel : ViewModelBase
    {
        // Liste der bindes til DataGrid
        public ObservableCollection<SaleLine> Sale { get; } = new();

        // Simpelt inputfelt (stregkode)
        private string _barCode = "";
        public string BarCode
        {
            get => _barCode;
            set
            {
                if (SetProperty(ref _barCode, value))
                    (AddSaleLineCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Afledte (bind OneWay i XAML)
        public int DisplayQuantity => Sale.Count;
        public decimal DisplayTotalPrice => Sale.Sum(x => x.Price);

        // Commands
        public ICommand AddSaleLineCommand { get; }
        public ICommand RemoveSaleLineCommand { get; }
        public ICommand ClearSaleBasketCommand { get; }
        public ICommand PayCommand { get; }

        public SaleViewModel()
        {
            // Dummy-data — rækkefølge: rackId, date, price, quantity
            Sale.Add(new SaleLine(1, new DateTime(2025, 9, 25), 25m, 10));
            Sale.Add(new SaleLine(2, new DateTime(2025, 9, 25), 100m, 5));
            Sale.Add(new SaleLine(3, new DateTime(2025, 9, 25), 5m, 7));
            Sale.Add(new SaleLine(4, new DateTime(2025, 9, 25), 80m, 10));

            // Opdater afledte når listen ændres
            Sale.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DisplayQuantity));
                OnPropertyChanged(nameof(DisplayTotalPrice));
                (PayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ClearSaleBasketCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };

            // Commands
            AddSaleLineCommand = new RelayCommand(AddSaleLine, () => !string.IsNullOrWhiteSpace(BarCode));
            RemoveSaleLineCommand = new RelayCommand<SaleLine>(RemoveSaleLine);
            ClearSaleBasketCommand = new RelayCommand(ClearSaleBasket, () => Sale.Any());
            PayCommand = new RelayCommand(Pay, () => Sale.Any());
        }

        private void AddSaleLine()
        {
            // For enkelhed:format "rackId;price;quantity"
            int rackId = 0, quantity = 1;
            decimal price = 0m;

            var parts = (BarCode ?? "").Split(';');
            if (parts.Length >= 1 && int.TryParse(parts[0], out var r)) rackId = r;
            if (parts.Length >= 2 && decimal.TryParse(parts[1], out var p)) price = p;
            if (parts.Length >= 3 && int.TryParse(parts[2], out var q)) quantity = q;

            Sale.Add(new SaleLine(rackId, DateTime.Today, price, quantity));
            BarCode = string.Empty;
        }

        private void RemoveSaleLine(SaleLine? line)
        {
            if (line != null) Sale.Remove(line);
        }

        private void ClearSaleBasket()
        {
            Sale.Clear();
        }

        private void Pay()
        {
            // Hvis det ikke skal gemmes i DB/kvittering – lige nu bare ryd
            Sale.Clear();
        }
    }
}
