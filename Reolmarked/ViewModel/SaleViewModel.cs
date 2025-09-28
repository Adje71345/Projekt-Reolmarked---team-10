using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reolmarked.Commands;

namespace Reolmarked.ViewModel
{
    // En linje i salget (bindes til DataGrid)
    public class SaleLine
    {
        public string RackNo { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
    }

    public class SaleViewModel : ViewModelBase
    {
        // ===== Properties =====

        private string _barcodeInput;
        public string BarcodeInput
        {
            get => _barcodeInput;
            set => SetProperty(ref _barcodeInput, value);
        }

        public ObservableCollection<SaleLine> Lines { get; } = new();

        public int ItemCount => Lines.Count;

        public decimal Total => Lines.Sum(l => l.Price);

        private bool _canCheckout;
        public bool CanCheckout
        {
            get => _canCheckout;
            set => SetProperty(ref _canCheckout, value);
        }

        // ===== Commands =====
        public ICommand ScanCommand { get; }
        public ICommand RemoveLineCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        // ===== Constructor =====
        public SaleViewModel()
        {
            ScanCommand = new RelayCommand(Scan);
            RemoveLineCommand = new RelayCommand<SaleLine>(RemoveLine);
            ClearCartCommand = new RelayCommand(ClearCart);
            CheckoutCommand = new RelayCommand(Checkout, () => CanCheckout);
        }

        // ===== Methods =====

        private void Scan()
        {
            if (string.IsNullOrWhiteSpace(BarcodeInput))
                return;

            // For demo: split "RackNo;Price"
            var parts = BarcodeInput.Split(';');
            if (parts.Length != 2)
            {
                MessageBox.Show("Ugyldig stregkode (forventet format: RackNo;Pris)");
                return;
            }

            var rackNo = parts[0];
            if (!decimal.TryParse(parts[1], out var price))
            {
                MessageBox.Show("Ugyldig pris i stregkode");
                return;
            }

            // Tilføj ny linje til kurven
            Lines.Add(new SaleLine
            {
                RackNo = rackNo,
                Price = price,
                Barcode = BarcodeInput
            });

            // Ryd inputfelt
            BarcodeInput = string.Empty;

            // Opdater calculated properties
            OnPropertyChanged(nameof(ItemCount));
            OnPropertyChanged(nameof(Total));

            // Nu kan man betale
            CanCheckout = Lines.Any();
            (CheckoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void RemoveLine(SaleLine line)
        {
            if (line != null && Lines.Contains(line))
            {
                Lines.Remove(line);

                OnPropertyChanged(nameof(ItemCount));
                OnPropertyChanged(nameof(Total));

                CanCheckout = Lines.Any();
                (CheckoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }


        private void ClearCart()
        {
            Lines.Clear();

            OnPropertyChanged(nameof(ItemCount));
            OnPropertyChanged(nameof(Total));

            CanCheckout = false;
            (CheckoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void Checkout()
        {
            if (!Lines.Any())
            {
                MessageBox.Show("Ingen varer i kurven.");
                return;
            }

            // Opmærksom: Her kan man gemme salget i DB eller lave kvittering
            MessageBox.Show($"Salg gennemført. Total: {Total:N2} kr.");

            ClearCart();
        }
    }
}
