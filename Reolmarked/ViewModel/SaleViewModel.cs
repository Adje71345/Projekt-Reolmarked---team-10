using Reolmarked.Commands;
using Reolmarked.Model;
using Reolmarked.Repositories;
using Reolmarked.ViewModel.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace Reolmarked.ViewModel
{
    internal class SaleViewModel : ViewModelBase
    {
        private readonly ISaleLineRepository _saleLineRepository;

        // Liste til DataGrid
        public ObservableCollection<SaleLine> Sale { get; } = new();

        // View-lag ovenpå Sale (til filter/sort og binding i XAML)
        public ICollectionView SalesView { get; private set; }

        // Søgning (bindes fra TextBox i XAML)
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    SalesView?.Refresh();
            }
        }

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

        // Runtime-konstruktør med repository. Kalder eksisterende ctor via ": this()".
        public SaleViewModel(ISaleLineRepository saleLineRepository) : this()
        {
            _saleLineRepository = saleLineRepository ?? throw new ArgumentNullException(nameof(saleLineRepository));

            // Initialiser CollectionView
            SalesView = CollectionViewSource.GetDefaultView(Sale);     
            SalesView.Filter = FilterSales;                                
            SalesView.SortDescriptions.Clear();       
        }


        // Lavet efter samme format (samme som i LabelView): "rackId;price" – evt. "rackId;price;quantity"
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

            // price – har både invariant (.) og dansk (,)
            if (parts.Length >= 2)
            {
                if (!decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out price))
                    decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.GetCultureInfo("da-DK"), out price);
            }
            // Valider reolnummer (1..80)
            if (rackId < 1 || rackId > 80)
            {
                System.Windows.MessageBox.Show(
                    "Reolnummer skal være mellem 1 og 80.",
                    "Ugyldigt reolnummer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // ryd input og stop
                BarCode = string.Empty;
                UpdateBarcodePreview(null);
                return;
            }
            // Løbenummer til SaleLineId
            var nextId = (Sale.LastOrDefault()?.SaleLineId ?? 0) + 1;
            var scanTxt = txt.Replace(";", string.Empty); // fjern ';' fra scankode

            var line = new SaleLine
            {
                SaleLineId = nextId,
                SaleDate = DateTime.Today,
                Price = price,
                RackId = rackId,
                ScanCode = scanTxt,
                BarcodeImage = GenerateBarcodeImage(scanTxt)
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
            // Hvis der ikke er noget at gemme, ryd og returner
            if (Sale.Count == 0)
            {
                Sale.Clear();
                return;
            }

            // Gem alle linjer på en gang i en transaktion til DB
            _saleLineRepository?.AddMany(Sale.ToList());

            // Send event for hver linje
            foreach (var line in Sale)
                AppEvents.RaiseSaleCommitted(line);


            // Tøm kurv efter vellykket betaling
            Sale.Clear();

            // Opdater visning / afledte felter
            SalesView?.Refresh();
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
        private ImageSource? GenerateBarcodeImage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 120,   // thumbnail-bredde til DataGrid
                    Height = 32,
                    Margin = 1,
                    PureBarcode = true
                }
            };

            using var bmp = writer.Write(text);
            return ConvertBitmapToImageSource(bmp);
        }

        private void LoadSalesFromDb()
        {
            if (_saleLineRepository == null) return;
            var rows = _saleLineRepository.GetAll().OrderBy(s => s.SaleDate);

            Sale.Clear();
            foreach (var s in rows)
            {
                // Tilføj billede til rækker fra DB, så "Scan-kode"-kolonnen kan vise stregkoden
                if (s.BarcodeImage == null && !string.IsNullOrWhiteSpace(s.ScanCode))
                    s.BarcodeImage = GenerateBarcodeImage(s.ScanCode);

                Sale.Add(s);
            }
        }

        private bool FilterSales(object obj)
        {
            if (obj is not SaleLine s) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var needle = SearchText.Trim().ToLowerInvariant();
            var culture = System.Globalization.CultureInfo.CurrentCulture;

            // Samler aller public properties på SaleLine som tekst (datoer, tal m.m. konverteres pænt)
            // Det gør det muligt at søge på alle felter uden at specificere dem enkeltvis, ligemget om de er tekst, tal eller datoer.
            var hay = string.Join(" | ",
                s.GetType().GetProperties().Select(pi =>
                {
                    var v = pi.GetValue(s);
                    if (v is null) return "";
                    if (v is DateTime dt) return dt.ToString("yyyy-MM-dd", culture);
                    if (v is DateOnly d) return d.ToString("yyyy-MM-dd", culture);
                    if (v is decimal dec) return dec.ToString(culture);
                    if (v is double db) return db.ToString(culture);
                    if (v is float fl) return fl.ToString(culture);
                    return v.ToString();
                })
            ).ToLowerInvariant();

            return hay.Contains(needle);


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
                // GDI handle frigives automatisk ved GC, men ved vores simpelt preview er det nok ok.
            }
        }
    }
}
