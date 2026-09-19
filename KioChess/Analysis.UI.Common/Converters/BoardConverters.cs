using Analysis.UI.Common.Models;
using Engine.Models.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Analysis.UI.Common.Converters;

/// <summary>
/// Returns the light/dark square brush from the active theme.
/// </summary>
[ValueConversion(typeof(CellType), typeof(Brush))]
public class CellTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CellType ct)
        {
            var key = ct == CellType.Light ? "BoardLightSquareBrush" : "BoardDarkSquareBrush";
            if (Application.Current.Resources[key] is Brush b)
                return b;
            return ct == CellType.Light ? Brushes.AntiqueWhite : Brushes.SaddleBrown;
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Returns an overlay brush for a cell based on its current highlight state.
/// Priority: Check > Selected > ValidTarget > LastMoveTo > LastMoveFrom > Transparent
/// </summary>
public class CellOverlayMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 5) return Brushes.Transparent;

        bool isInCheck     = values[0] is bool b0 && b0;
        bool isSelected    = values[1] is bool b1 && b1;
        bool isValidTarget = values[2] is bool b2 && b2;
        bool isLastMoveTo  = values[3] is bool b3 && b3;
        bool isLastMoveFrom = values[4] is bool b4 && b4;

        var res = Application.Current.Resources;

        if (isInCheck)     return res["BoardCheckBrush"]       as Brush ?? Brushes.Transparent;
        if (isSelected)    return res["BoardSelectedBrush"]    as Brush ?? Brushes.Transparent;
        if (isValidTarget) return res["BoardValidTargetBrush"] as Brush ?? Brushes.Transparent;
        if (isLastMoveTo)  return res["BoardLastMoveBrush"]    as Brush ?? Brushes.Transparent;
        if (isLastMoveFrom) return res["BoardLastMoveBrush"]   as Brush ?? Brushes.Transparent;

        return Brushes.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Returns a frozen WPF Path for the given piece byte.
/// </summary>
[ValueConversion(typeof(byte?), typeof(Path))]
public class FigureToPathConverter : IValueConverter
{
    private static readonly IReadOnlyDictionary<byte, string> _names =
        new Dictionary<byte, string>
        {
            [Pieces.WhitePawn]   = "WhitePawn",
            [Pieces.WhiteKnight] = "WhiteKnight",
            [Pieces.WhiteBishop] = "WhiteBishop",
            [Pieces.WhiteRook]   = "WhiteRook",
            [Pieces.WhiteQueen]  = "WhiteQueen",
            [Pieces.WhiteKing]   = "WhiteKing",
            [Pieces.BlackPawn]   = "BlackPawn",
            [Pieces.BlackKnight] = "BlackKnight",
            [Pieces.BlackBishop] = "BlackBishop",
            [Pieces.BlackRook]   = "BlackRook",
            [Pieces.BlackQueen]  = "BlackQueen",
            [Pieces.BlackKing]   = "BlackKing",
        };

    private static readonly HashSet<byte> _whitePieces =
    [
        Pieces.WhitePawn, Pieces.WhiteKnight, Pieces.WhiteBishop,
        Pieces.WhiteRook, Pieces.WhiteQueen,  Pieces.WhiteKing,
    ];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not byte piece) return null;
        if (!_names.TryGetValue(piece, out var name)) return null;

        var app = Application.Current;

        // Resolve active piece-set key (default "Classic")
        var setKey = app.TryFindResource("ActivePieceSet") as string ?? "Classic";

        // Look up geometry
        var geoKey = $"{setKey}_{name}";
        if (app.TryFindResource(geoKey) is not Geometry geo) return null;

        bool isWhite = _whitePieces.Contains(piece);

        var fill   = (app.TryFindResource(isWhite ? "PieceWhiteFill"   : "PieceBlackFill")   as Brush) ?? Brushes.White;
        var stroke = (app.TryFindResource(isWhite ? "PieceWhiteStroke" : "PieceBlackStroke") as Brush) ?? Brushes.Black;

        return new Path
        {
            Data            = geo,
            Fill            = fill,
            Stroke          = stroke,
            StrokeThickness = 0.8,
            Stretch         = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment   = VerticalAlignment.Stretch,
            Margin          = new Thickness(4),
            IsHitTestVisible = false,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
