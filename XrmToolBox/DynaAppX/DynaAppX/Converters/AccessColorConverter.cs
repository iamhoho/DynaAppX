using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DynaAppX.Converters
{
    public class AccessColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasAccess)
            {
                return hasAccess
                    ? new SolidColorBrush(Color.FromRgb(0x5c, 0xbc, 0x5c))  // Green
                    : new SolidColorBrush(Color.FromRgb(0xd9, 0x53, 0x53)); // Red
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
