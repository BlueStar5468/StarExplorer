using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace StarExplorer.Controls
{
    public class BrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Colors.Transparent);
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int margin)
            {
                return new Thickness(margin);
            }
            return new Thickness(0);
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return (int)thickness.Left;
            }
            return 0;
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
