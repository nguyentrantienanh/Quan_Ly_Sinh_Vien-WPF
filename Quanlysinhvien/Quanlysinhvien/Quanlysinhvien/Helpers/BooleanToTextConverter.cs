using System;
using System.Globalization;
using System.Windows.Data;

namespace Quanlysinhvien.Helpers
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool status)
            {
                return status ? "Kích hoạt" : "Không kích hoạt";
            }
            return "Không xác định";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text == "Kích hoạt";
            }
            return false;
        }
    }
}