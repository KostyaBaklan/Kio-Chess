using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace StockfishBenchmark.Controls;

public class TtBarItem
{
    public string Label { get; set; }
    public double[] Values { get; set; } = [];
}

/// <summary>Horizontal bar chart (single series, or stacked series) drawn directly, no external packages.</summary>
public class TtBarChart : FrameworkElement
{
    private const double RowH = 22, LabelW = 280, TopH = 46, ValueW = 70;

    private static readonly Brush[] Palette =
    {
        Freeze(Color.FromRgb(0x3B, 0x82, 0xF6)), Freeze(Color.FromRgb(0x10, 0xB9, 0x81)),
        Freeze(Color.FromRgb(0xF5, 0x9E, 0x0B)), Freeze(Color.FromRgb(0xEF, 0x44, 0x44)),
        Freeze(Color.FromRgb(0x8B, 0x5C, 0xF6)), Freeze(Color.FromRgb(0x64, 0x74, 0x8B))
    };

    private static Brush Freeze(Color c) { var b = new SolidColorBrush(c); b.Freeze(); return b; }

    private const FrameworkPropertyMetadataOptions Both =
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender;

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(IReadOnlyList<TtBarItem>), typeof(TtBarChart), new FrameworkPropertyMetadata(null, Both));
    public static readonly DependencyProperty SeriesNamesProperty = DependencyProperty.Register(
        nameof(SeriesNames), typeof(IReadOnlyList<string>), typeof(TtBarChart), new FrameworkPropertyMetadata(null, Both));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(TtBarChart), new FrameworkPropertyMetadata(string.Empty, Both));
    public static readonly DependencyProperty StackedProperty = DependencyProperty.Register(
        nameof(Stacked), typeof(bool), typeof(TtBarChart), new FrameworkPropertyMetadata(false, Both));
    public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
        nameof(ValueFormat), typeof(string), typeof(TtBarChart), new FrameworkPropertyMetadata("F1", Both));

    public IReadOnlyList<TtBarItem> Items { get => (IReadOnlyList<TtBarItem>)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public IReadOnlyList<string> SeriesNames { get => (IReadOnlyList<string>)GetValue(SeriesNamesProperty); set => SetValue(SeriesNamesProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public bool Stacked { get => (bool)GetValue(StackedProperty); set => SetValue(StackedProperty, value); }
    public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        double w = double.IsInfinity(availableSize.Width) ? 800 : availableSize.Width;
        return new Size(w, TopH + (Items?.Count ?? 0) * RowH + 8);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        dc.DrawText(Text(Title, 13, Brushes.Black, dpi, FontWeights.SemiBold, 0), new Point(0, 0));

        var items = Items;
        if (items == null || items.Count == 0) return;

        var names = SeriesNames;
        if (names != null && names.Count > 1)
        {
            double lx = 0;
            for (int i = 0; i < names.Count; i++)
            {
                dc.DrawRectangle(Palette[i % Palette.Length], null, new Rect(lx, 26, 10, 10));
                var t = Text(names[i], 11, Brushes.DimGray, dpi, FontWeights.Normal, 0);
                dc.DrawText(t, new Point(lx + 14, 23));
                lx += 14 + t.Width + 16;
            }
        }

        double max = Stacked
            ? items.Max(i => i.Values.Sum())
            : items.Max(i => i.Values.Length == 0 ? 0 : i.Values[0]);
        if (max <= 0) max = 1;

        double barW = Math.Max(20, ActualWidth - LabelW - ValueW);
        for (int r = 0; r < items.Count; r++)
        {
            var item = items[r];
            double y = TopH + r * RowH;
            dc.DrawText(Text(item.Label, 11, Brushes.Black, dpi, FontWeights.Normal, LabelW - 8), new Point(0, y + 3));

            if (Stacked)
            {
                double x = LabelW;
                for (int s = 0; s < item.Values.Length; s++)
                {
                    double w = item.Values[s] / max * barW;
                    if (w <= 0) continue;
                    dc.DrawRectangle(Palette[s % Palette.Length], null, new Rect(x, y + 2, w, RowH - 5));
                    if (w > 34)
                        dc.DrawText(Text(item.Values[s].ToString("F0", CultureInfo.InvariantCulture), 10, Brushes.White, dpi, FontWeights.Normal, 0), new Point(x + 3, y + 4));
                    x += w;
                }
            }
            else
            {
                double v = item.Values.Length == 0 ? 0 : item.Values[0];
                dc.DrawRectangle(Palette[0], null, new Rect(LabelW, y + 2, Math.Max(1, v / max * barW), RowH - 5));
                dc.DrawText(Text(v.ToString(ValueFormat, CultureInfo.InvariantCulture), 11, Brushes.Black, dpi, FontWeights.Normal, 0),
                    new Point(LabelW + Math.Max(1, v / max * barW) + 4, y + 3));
            }
        }
    }

    private static FormattedText Text(string text, double size, Brush brush, double dpi, FontWeight weight, double maxWidth)
    {
        var ft = new FormattedText(text ?? string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, weight, FontStretches.Normal), size, brush, dpi);
        if (maxWidth > 0)
        {
            ft.MaxTextWidth = maxWidth;
            ft.MaxLineCount = 1;
            ft.Trimming = TextTrimming.CharacterEllipsis;
        }
        return ft;
    }
}
