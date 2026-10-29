using Engine.DataStructures;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace StockfishBenchmark.Controls;

/// <summary>Depth (rows) x age (columns) heat map of a flattened [depth * AgeBins + age] counter array.</summary>
public class TtHeatmap : FrameworkElement
{
    private const double CellH = 18, LeftW = 44, TopH = 48, MinCellW = 14, MaxCellW = 40;
    private double _cellW = MaxCellW;

    private const FrameworkPropertyMetadataOptions Both =
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender;

    public static readonly DependencyProperty CellsProperty = DependencyProperty.Register(
        nameof(Cells), typeof(long[]), typeof(TtHeatmap), new FrameworkPropertyMetadata(null, Both));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(TtHeatmap), new FrameworkPropertyMetadata(string.Empty, Both));

    public long[] Cells { get => (long[])GetValue(CellsProperty); set => SetValue(CellsProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    private bool TryRange(out int minD, out int maxD)
    {
        minD = int.MaxValue; maxD = -1;
        var cells = Cells;
        if (cells == null) return false;
        int ages = TtTableStats.AgeBins;
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == 0) continue;
            int d = i / ages;
            if (d < minD) minD = d;
            if (d > maxD) maxD = d;
        }
        return maxD >= 0;
    }

    private int UsedAges()
    {
        var cells = Cells;
        if (cells == null) return 0;
        int ages = TtTableStats.AgeBins, used = 0;
        for (int i = 0; i < cells.Length; i++)
            if (cells[i] != 0 && i % ages >= used) used = i % ages + 1;
        return used;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int rows = TryRange(out int lo, out int hi) ? hi - lo + 1 : 0;
        int ages = Math.Max(UsedAges(), 1);
        _cellW = double.IsInfinity(availableSize.Width)
            ? MaxCellW
            : Math.Clamp((availableSize.Width - LeftW - 8) / ages, MinCellW, MaxCellW);
        return new Size(LeftW + ages * _cellW + 8, TopH + rows * CellH + 8);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        dc.DrawText(Text(Title, 13, Brushes.Black, dpi, FontWeights.SemiBold), new Point(0, 0));
        if (!TryRange(out int minD, out int maxD)) return;

        int ageBins = TtTableStats.AgeBins;
        int ages = UsedAges();
        double CellW = _cellW;
        int labelStep = CellW >= 28 ? 1 : CellW >= 18 ? 2 : 4;
        var cells = Cells;
        long max = cells.Max();
        double logMax = Math.Log(1 + max);

        dc.DrawText(Text("age (generations) →   rows: depth", 10, Brushes.DimGray, dpi, FontWeights.Normal), new Point(LeftW, 18));
        for (int a = 0; a < ages; a++)
        {
            if (a % labelStep != 0 && a != ageBins - 1) continue;
            string label = a == ageBins - 1 ? $"{a}+" : a.ToString();
            dc.DrawText(Text(label, 10, Brushes.DimGray, dpi, FontWeights.Normal), new Point(LeftW + a * CellW + 2, 32));
        }

        for (int d = minD; d <= maxD; d++)
        {
            double y = TopH + (d - minD) * CellH;
            dc.DrawText(Text("d" + d, 10, Brushes.DimGray, dpi, FontWeights.Normal), new Point(4, y + 2));
            for (int a = 0; a < ages; a++)
            {
                long v = cells[d * ageBins + a];
                double t = v == 0 || logMax == 0 ? 0 : Math.Log(1 + v) / logMax;
                var fill = new SolidColorBrush(Lerp(Color.FromRgb(0xF3, 0xF4, 0xF6), Color.FromRgb(0xDC, 0x26, 0x26), t));
                dc.DrawRectangle(fill, new Pen(Brushes.White, 1), new Rect(LeftW + a * CellW, y, CellW, CellH));
                if (v > 0 && CellW >= 28)
                    dc.DrawText(Text(Short(v), 9, t > 0.55 ? Brushes.White : Brushes.Black, dpi, FontWeights.Normal),
                        new Point(LeftW + a * CellW + 3, y + 3));
            }
        }
    }

    private static string Short(long v) =>
        v >= 1_000_000 ? (v / 1_000_000.0).ToString("F1", CultureInfo.InvariantCulture) + "M"
        : v >= 1_000 ? (v / 1_000.0).ToString("F0", CultureInfo.InvariantCulture) + "k"
        : v.ToString(CultureInfo.InvariantCulture);

    private static Color Lerp(Color a, Color b, double t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));

    private static FormattedText Text(string text, double size, Brush brush, double dpi, FontWeight weight) =>
        new(text ?? string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, weight, FontStretches.Normal), size, brush, dpi);
}
