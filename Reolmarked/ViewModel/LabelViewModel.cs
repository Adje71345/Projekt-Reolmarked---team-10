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
        { }

        private void PrintLabel()
        { }



    }
}
