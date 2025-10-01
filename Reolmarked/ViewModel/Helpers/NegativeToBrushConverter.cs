// Path: ViewModel/Helpers/NegativeToBrushConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Reolmarked.ViewModel.Helpers
{
    /// <summary>
    /// Returnerer rød pensel, hvis værdien er negativ; ellers neutral mørk.
    /// Bruges til total-Nettoresultat i footer.
    /// </summary>
    public class NegativeToBrushConverter : IValueConverter
    {
        private static readonly Brush Neutral =
            (Brush)new BrushConverter().ConvertFromString("#111827")!;
        private static readonly Brush Negative =
            (Brush)new BrushConverter().ConvertFromString("#DC2626")!; // rød

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is null) return Neutral;
                var d = System.Convert.ToDecimal(value, culture);
                return d < 0 ? Negative : Neutral;
            }
            catch { return Neutral; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
