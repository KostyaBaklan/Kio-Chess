using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Engine.Models.Enums;

namespace Kgb.ChessApp.Converters;

public class FigureToImageConvertor : IValueConverter
{
    private readonly Dictionary<byte, object> _images;

    public FigureToImageConvertor()
    {
        _images = new Dictionary<byte, object>
        {
            [Pieces.WhitePawn] = System.Windows.Application.Current.FindResource("WhitePawn"),
            [Pieces.WhiteKnight] = System.Windows.Application.Current.FindResource("WhiteKnight"),
            [Pieces.WhiteBishop] = System.Windows.Application.Current.FindResource("WhiteBishop"),
            [Pieces.WhiteKing] = System.Windows.Application.Current.FindResource("WhiteKing"),
            [Pieces.WhiteRook] = System.Windows.Application.Current.FindResource("WhiteRook"),
            [Pieces.WhiteQueen] = System.Windows.Application.Current.FindResource("WhiteQueen"),
            [Pieces.BlackPawn] = System.Windows.Application.Current.FindResource("BlackPawn"),
            [Pieces.BlackKnight] = System.Windows.Application.Current.FindResource("BlackKnight"),
            [Pieces.BlackBishop] = System.Windows.Application.Current.FindResource("BlackBishop"),
            [Pieces.BlackKing] = System.Windows.Application.Current.FindResource("BlackKing"),
            [Pieces.BlackRook] = System.Windows.Application.Current.FindResource("BlackRook"),
            [Pieces.BlackQueen] = System.Windows.Application.Current.FindResource("BlackQueen")
        };
    }

    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var f = value as byte?;
        if (f == null) return null;

        return _images[f.Value];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}