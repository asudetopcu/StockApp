using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StockApp.Converters
{
    public class LengthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int length && length > 0)
            {
                return Visibility.Collapsed; // Eğer TextBox doluysa Placeholder kaybolur
            }
            return Visibility.Visible; // Eğer TextBox boşsa Placeholder görünür
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
