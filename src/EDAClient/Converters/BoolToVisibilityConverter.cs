using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace EDAClient.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertParam = false;
            try
            {
                convertParam = bool.Parse(parameter?.ToString());
            }
            catch (Exception)
            {
                convertParam = false;
            }
            
            if ((bool)value)
            {
                return !convertParam ? Visibility.Visible: Visibility.Collapsed; 
            }
            else
            {
                return !convertParam ? Visibility.Collapsed: Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
