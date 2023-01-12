using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace L2JChat
{
    public class EmptyOrNullStringToVisibilityConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(string))
            {
                string Yang = value.ToString();
                return (Yang == "" || Yang == null) ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new InvalidOperationException("Incorrect input.");
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
