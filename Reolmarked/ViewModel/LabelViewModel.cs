using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Input;
using ZXing;
using ZXing.Windows.Compatibility;
using Reolmarked.Commands;


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
                // laver label billedet, med ekstra plads til at tilføje tekst.
                _labelBitmap = new Bitmap(barcodeBitmap.Width, barcodeBitmap.Height + 22);

                using (var graphics = Graphics.FromImage(_labelBitmap))
                {
                    graphics.Clear(System.Drawing.Color.White);
                    graphics.DrawImage(barcodeBitmap, 0, 0);

                    using var font = new Font("Arial", 8f);
                    using var brush = new SolidBrush(System.Drawing.Color.Black);

                    string text = $"Reol: {rackId}   Pris: {price} kr";

                    // måler tekstbredde og centrerer det under stregkoden
                    var size = graphics.MeasureString(text, font);
                    float x = (barcodeBitmap.Width - size.Width) / 2f;
                    float y = barcodeBitmap.Height + 2f;

                    graphics.DrawString(text, font, brush, x, y);
                }

                // konverter bitmap til imagesource og vis i UI
                var hBitmap = _labelBitmap.GetHbitmap();
                BarcodeImage = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }


        }

        private void PrintLabel()
        { }



    }
}
