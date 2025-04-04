using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace EDAClient.Converters
{
    public class BoolToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if ((bool)value)
            {
                return new GridLength(0, GridUnitType.Auto);
            }
            else
            {
                return new GridLength(0);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}