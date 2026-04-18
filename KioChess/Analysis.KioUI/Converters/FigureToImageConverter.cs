using Engine.Models.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Maps a nullable piece byte (Engine.Models.Enums.Pieces) to an ImageSource.
/// Images are embedded WPF resources. The piece set is the classic set
/// from the Application project images; all 12 PNG files are referenced
/// directly via pack URIs so no project reference to Application is needed.
/// </summary>
[ValueConversion(typeof(byte?), typeof(ImageSource))]
public class FigureToImageConverter : IValueConverter
{
    // The images ship with the Application project as resources.
    // We reference them via a pack URI so Analysis.KioUI can use them
    // without a project reference. If the images cannot be found a null
    // is returned and the cell renders empty.
    private static readonly IReadOnlyDictionary<byte, string> _resourceKeys =
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

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not byte piece) return null;
        if (!_resourceKeys.TryGetValue(piece, out var key)) return null;

        // Try the application resource dictionary first (populated by Application project or
        // by future per-theme piece sets).
        var resource = System.Windows.Application.Current.TryFindResource(key);
        if (resource is ImageSource img) return img;

        // Fallback: load from the Application assembly resources pack URI.
        try
        {
            var uri = new Uri(
                $"pack://application:,,,/Application;component/Resources/Images/{key}.png",
                UriKind.Absolute);
            return new BitmapImage(uri);
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
