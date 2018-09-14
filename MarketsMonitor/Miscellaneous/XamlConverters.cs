using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using MarketsSystem.Model;

namespace MarketsSystem.Miscellaneous
{
   

    
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {


            return (bool)value;
            

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
  
}