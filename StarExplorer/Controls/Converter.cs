using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    public class SolidColorBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && value is string colorString)
            {
                try
                {
                    return SolidColorBrush.Parse(colorString);
                }
                catch (Exception)
                {
                    //解析失败，返回默认值（透明）
                    return SolidColorBrush.Parse("Transparent");
                }
            }
            else return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridLengthConverter() : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && value is double doubleValue)
            {
                return new GridLength(doubleValue);
            }
            else if (value != null && value is int intValue)
            {
                return new GridLength(System.Convert.ToDouble(value));
            }
            else return null;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CornerRadiusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int radius)
            {
                return new CornerRadius(radius);
            }
            return new CornerRadius(0);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is CornerRadius cornerRadius)
            {
                return (int)cornerRadius.TopLeft;
            }
            return null;
        }
    }
}
