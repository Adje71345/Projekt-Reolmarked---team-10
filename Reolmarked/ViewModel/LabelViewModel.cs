using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reolmarked.Commands;
using ZXing;
using ZXing.Windows.Compatibility;


namespace Reolmarked.ViewModel
{
    public class LabelViewModel : ViewModelBase
    {
        private string _rackId;
        public string RackId
        {
            get => _rackId;
            set => SetProperty(ref _rackId, value);
        }

        private string _price;
        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        private ImageSource _barcodeImage;
        public ImageSource BarcodeImage
        {
            get => _barcodeImage;
            set => SetProperty(ref _barcodeImage, value);
        }

        private string _barcodeText;
        public string BarcodeText
        {
            get => _barcodeText;
            set => SetProperty(ref _barcodeText, value);
        }

        //Commands
        public ICommand GenerateCommand { get; }
        public ICommand PrintCommand { get; }

        // holder på seneste lavede bitmap label til print
        private Bitmap _labelBitmap;

        public LabelViewModel()
        {
            GenerateCommand = new RelayCommand(GenerateLabel);
            PrintCommand = new RelayCommand(PrintLabel);
        }

        private void GenerateLabel()
        {
            // Læser input og fjerner eventuelle mellemrum
            var rackId = (RackId ?? string.Empty).Trim().Replace(" ", "");
            var price = (Price ?? string.Empty).Trim().Replace(" ", "");

            if (string.IsNullOrWhiteSpace(rackId) || string.IsNullOrWhiteSpace(price))
            {
                MessageBox.Show("Udfyld både Reol og Pris");
                return;
            }

            // data som gemmes i selve stregkoden (som kan aflæses senere)
            string barcodeData = $"{rackId};{price}";

            // opretter stregkoden
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

            using (var barcodeBitmap = writer.Write(barcodeData))
            {
                // laver label billedet, med ekstra plads til at tilføje tekst over og under stregkoden.
                int extraHeight = 80;
                _labelBitmap = new Bitmap(barcodeBitmap.Width, barcodeBitmap.Height + extraHeight);

                using (var graphics = Graphics.FromImage(_labelBitmap))
                {
                    graphics.Clear(System.Drawing.Color.White);

                    using var font = new Font("Arial", 10f);
                    using var brush = new SolidBrush(System.Drawing.Color.Black);

                    // Header tekst øverst
                    string headerText = $"Reol: {rackId}\nPris: {price} kr.\n";
                    var headerSize = graphics.MeasureString(headerText, font);
                    float headerX = (_labelBitmap.Width - headerSize.Width) / 2f;
                    graphics.DrawString(headerText, font, brush, headerX, 5);

                    //Stregkoden midt i 
                    int barcodeY = (int)headerSize.Height + 10;
                    graphics.DrawImage(barcodeBitmap, 0, barcodeY);

                    // Footer
                    string footerText = barcodeData;
                    var footerSize = graphics.MeasureString(footerText, font);
                    float footerX = (_labelBitmap.Width - footerSize.Width) / 2f;
                    float footerY = barcodeY + barcodeBitmap.Height + 5;
                    graphics.DrawString(footerText, font, brush, footerX, footerY);

                }

                // konverter bitmap til imagesource og vis i UI
                var hBitmap = _labelBitmap.GetHbitmap();
                BarcodeImage = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private void PrintLabel()
        { 
            if (_labelBitmap == null)
            {
                MessageBox.Show("Generér først en stregkode");
                    return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // konverter bitmap til en imagesource
                var hBitmap = _labelBitmap.GetHbitmap();
                var imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                // laver printbart billede
                var imageToPrint = new System.Windows.Controls.Image
                {
                    Source = imageSource,
                    Width = imageSource.Width,
                    Height = imageSource.Height
                };

                // åbner printerdialog og sender billede af label til printer
                printDialog.PrintVisual(imageToPrint, "Label Print");
            }
        }
    }
}
