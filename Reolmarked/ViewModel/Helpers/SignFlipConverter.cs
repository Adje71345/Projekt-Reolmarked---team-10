using System;
using System.Globalization;
using System.Windows.Data;

namespace Reolmarked.ViewModel.Helpers
{
    /// <summary>
    /// WPF ValueConverter som vender fortegnet på tal.
    /// Eksempler:  123   -> -123
    ///             -45.7 ->  45.7
    /// Bevarer decimaler (komma/punkt).
    /// </summary>
    public class SignFlipConverter : IValueConverter
    {
        /// <summary>
        /// Kører når værdien skal vises i UI (Model -> View).
        /// Binding-kildens værdi (kan være int/decimal/double/string m.m.)
        /// Beholder gældende kultur (bruges til korrekt talfortolkning)
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Hvis der ikke er noget at konvertere, så returner uændret (undgå NullReference)
            if (value == null) return value;

            try
            {
                // Konverterer robust til decimal uanset om input er int, double, string osv.
                // Der bruges 'culture' så fx "1,23" i da-DK ikke fejler (komma som decimaltegn).
                var d = System.Convert.ToDecimal(value, culture);

                // Selve fortegns-vendingen: minus foran tallet.
                return -d;
            }
            catch
            {
                // Hvis value ikke kan tolkes som tal (fx "abc"), så forblive værdien være uændret.
                // Er med til at undgå at crashe UI’et på datafejl.
                return value;
            }
        }

        /// <summary>
        /// Kører når View skal tilbage til Model (View -> Model) i two-way binding.
        /// Der genbruges samme logik (symmetrisk: minus af minus = plus), så værdien vendes igen.
        /// Fordi hvis bindingen kun er OneWay, bliver denne aldrig kaldt.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, targetType, parameter, culture);
    }
}
