using Engine.Models.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Returns a frozen WPF <see cref="Path"/> for the given piece byte.
/// The geometry data for each piece set is stored in the application
/// resource dictionary (keyed as e.g. "Classic_WhiteKing").
/// Piece fill and stroke colours come from the active theme brushes
/// "PieceWhiteFill", "PieceWhiteStroke", "PieceBlackFill", "PieceBlackStroke".
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

        var app = System.Windows.Application.Current;

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
