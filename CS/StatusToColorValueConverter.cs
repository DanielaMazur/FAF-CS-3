using System;
using System.Globalization;
using System.Windows.Data;

namespace CS
{
     class StatusToColorValueConverter : IValueConverter
     {
          private readonly string _failColor = "#fa9696";
          private readonly string _successColor = "#a8fa96";
          private readonly string _whiteColor = "#ffffff";
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if ((AuditStatusEnum)value == AuditStatusEnum.Unprocessed){
                    return _whiteColor;
               }
               return (AuditStatusEnum)value == AuditStatusEnum.Success ? _successColor : _failColor;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if(value.ToString() == _whiteColor)
               {
                    return AuditStatusEnum.Unprocessed;
               }
               return value.ToString() == _successColor ? AuditStatusEnum.Success : AuditStatusEnum.Fail;
          }
     }
}
