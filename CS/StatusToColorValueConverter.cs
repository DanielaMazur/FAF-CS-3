using System;
using System.Globalization;
using System.Windows.Data;

namespace CS
{
     class StatusToColorValueConverter : IValueConverter
     {
          private readonly string _failColor = "#fa9696";
          private readonly string _successColor = "#a8fa96";
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               return (bool)value ? _successColor : _failColor;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               return value.ToString() == _successColor;
          }
     }
}
