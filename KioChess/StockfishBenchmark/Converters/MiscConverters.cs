using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StockfishBenchmark.Converters;

/// <summary>Converts bool (IsEngineMove) to a display label.</summary>
[ValueConversion(typeof(bool), typeof(string))]
public class BoolToEngineLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "Engine" : "Stockfish";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Converts bool (HasRegressions) to Red/Black brush.</summary>
[ValueConversion(typeof(bool), typeof(SolidColorBrush))]
public class BoolToRedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Collapses an element when the bound string is null or empty.</summary>
[ValueConversion(typeof(string), typeof(System.Windows.Visibility))]
public class EmptyStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string)
               ? System.Windows.Visibility.Collapsed
               : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Formats a <see cref="TimeSpan"/> or <c>double</c> (milliseconds) as
/// <c>mm:ss:fff.µµµ</c> — e.g. <c>00:03:456.678</c>.
/// </summary>
[ValueConversion(typeof(TimeSpan), typeof(string))]
public class TimeSpanDurationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)  return FormatDuration(ts);
        if (value is double   ms)  return FormatDuration(TimeSpan.FromMilliseconds(ms));
        return "—";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public static string FormatDuration(double ms)
        => FormatDuration(TimeSpan.FromMilliseconds(ms));

    public static string FormatDuration(TimeSpan ts)
        => $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}:{ts.Milliseconds:D3}.{ts.Microseconds:D3}";
}
