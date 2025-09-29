using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reolmarked.Commands;
using Reolmarked.Model;
using ZXing;
using ZXing.Windows.Compatibility;

namespace Reolmarked.ViewModel
{
    internal class SaleViewModel : ViewModelBase
    {
        // Liste til DataGrid
        public ObservableCollection<SaleLine> Sale { get; } = new();

        // Inputfelt (scanner skriver her)
        private string _barCode = "";
        public string BarCode
        {
            get => _barCode;
            set
            {
                if (SetProperty(ref _barCode, value))
                {
                    (AddSaleLineCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    UpdateBarcodePreview(_barCode); // live preview
                }
            }
        }

        // Live "for udskrift" af stregkodebilledet
        private ImageSource _barcodePreview;
        public ImageSource BarcodePreview
        {
            get => _barcodePreview;
            set => SetProperty(ref _barcodePreview, value);
        }

        // Afledte værdier
        public int DisplayQuantity => Sale.Count;
        public decimal DisplayTotalPrice => Sale.Sum(x => x.Price);

        // Commands
        public ICommand AddSaleLineCommand { get; }
        public ICommand RemoveSaleLineCommand { get; }
        public ICommand ClearSaleBasketCommand { get; }
        public ICommand PayCommand { get; }

        public SaleViewModel()
        {
            // Opdater afledte når listen ændres
            Sale.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DisplayQuantity));
                OnPropertyChanged(nameof(DisplayTotalPrice));
                (PayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ClearSaleBasketCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };

            AddSaleLineCommand = new RelayCommand(AddSaleLine, () => !string.IsNullOrWhiteSpace(BarCode));
            RemoveSaleLineCommand = new RelayCommand<SaleLine>(RemoveSaleLine);
            ClearSaleBasketCommand = new RelayCommand(ClearSaleBasket, () => Sale.Any());
            PayCommand = new RelayCommand(Pay, () => Sale.Any());
        }

        /// <summary>
        /// Der bør komme samme format (samme som i LabelView): "rackId;price" – evt. "rackId;price;quantity"
        /// Eksempler: "12;49,95" eller "12;49.95;2"
        /// </summary>
        private void AddSaleLine()
        {
            var txt = (BarCode ?? "").Trim();
            if (txt.Length == 0) return;

            int rackId = 0;
            decimal price = 0m;

            var parts = txt.Split(';');

            // rackId
            if (parts.Length >= 1)
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out rackId);

            // price – prøv både invariant (.) og dansk (,)
            if (parts.Length >= 2)
            {
                if (!decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out price))
                    decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.GetCultureInfo("da-DK"), out price);
            }

            // simpelt løbenummer til SaleLineId
            var nextId = (Sale.LastOrDefault()?.SaleLineId ?? 0) + 1;

            var line = new SaleLine
            {
                SaleLineId = nextId,
                SaleDate = DateTime.Today,
                Price = price,
                RackId = rackId
            };

            Sale.Add(line);

            // ryd input + preview
            BarCode = string.Empty;
            UpdateBarcodePreview(null);
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
            // Kan godt gemmmes i DB/kvittering; men lige nu blot ryd
            Sale.Clear();
        }

        // === ZXing preview (samme princip som i (Sabines) LabelViewModel) ===
        private void UpdateBarcodePreview(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                BarcodePreview = null;
                return;
            }

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 260,
                    Height = 80,
                    Margin = 2,
                    PureBarcode = true
                }
            };

            using var bmp = writer.Write(text);
            BarcodePreview = ConvertBitmapToImageSource(bmp);
        }

        private static ImageSource ConvertBitmapToImageSource(System.Drawing.Bitmap bmp)
        {
            var hBitmap = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                // GDI handle frigives automatisk ved GC; ved vores simpelt preview er det ok.
            }
        }
    }
}
