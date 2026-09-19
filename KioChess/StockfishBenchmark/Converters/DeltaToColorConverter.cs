using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StockfishBenchmark.Converters;

[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public class DeltaToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() switch
        {
            "FASTER"     => new SolidColorBrush(Color.FromRgb(209, 250, 229)),  // #D1FAE5 mint
            "SLOWER"     => new SolidColorBrush(Color.FromRgb(254, 226, 226)),  // #FEE2E2 rose
            "REGRESSION" => new SolidColorBrush(Color.FromRgb(254, 243, 199)),  // #FEF3C7 amber
            "DIVERGED"   => new SolidColorBrush(Color.FromRgb(241, 245, 249)),  // #F1F5F9 slate
            _            => Brushes.Transparent
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
