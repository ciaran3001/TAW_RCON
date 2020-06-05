using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TAWRcon_HLL_WPF.Models
{
    [ValueConversion(typeof(Status), typeof(SolidColorBrush))]
    public partial class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Status)value)
            {
                case Status.Ok:
                    return (object)Brushes.ForestGreen;
                case Status.Warning:
                    return (object)Brushes.Yellow;
                case Status.Error:
                    return (object)Brushes.OrangeRed;
                default:
                    return (object)null;
            }
        }

        public object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return (object)null;
        }
    }
}
