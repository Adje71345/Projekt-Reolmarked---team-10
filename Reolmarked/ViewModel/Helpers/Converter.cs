using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reolmarked.ViewModel.Helpers
{
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
            bool isNull = value == null;
            if (invert) isNull = !isNull;
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public sealed class BoolToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "Optaget";
        public string FalseText { get; set; } = "Ledig";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? (b ? TrueText : FalseText) : "-";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public sealed class BoolNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
