using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Engine.Models.Enums;

namespace Kgb.ChessApp.Converters
{
    public class FigureToImageConvertor : IValueConverter
    {
        private readonly Dictionary<Piece, object> _images;

        public FigureToImageConvertor()
        {
            _images = new Dictionary<Piece, object>
            {
                [Piece.WhitePawn] = System.Windows.Application.Current.FindResource("WhitePawn"),
                [Piece.WhiteKnight] = System.Windows.Application.Current.FindResource("WhiteKnight"),
                [Piece.WhiteBishop] = System.Windows.Application.Current.FindResource("WhiteBishop"),
                [Piece.WhiteKing] = System.Windows.Application.Current.FindResource("WhiteKing"),
                [Piece.WhiteRook] = System.Windows.Application.Current.FindResource("WhiteRook"),
                [Piece.WhiteQueen] = System.Windows.Application.Current.FindResource("WhiteQueen"),
                [Piece.BlackPawn] = System.Windows.Application.Current.FindResource("BlackPawn"),
                [Piece.BlackKnight] = System.Windows.Application.Current.FindResource("BlackKnight"),
                [Piece.BlackBishop] = System.Windows.Application.Current.FindResource("BlackBishop"),
                [Piece.BlackKing] = System.Windows.Application.Current.FindResource("BlackKing"),
                [Piece.BlackRook] = System.Windows.Application.Current.FindResource("BlackRook"),
                [Piece.BlackQueen] = System.Windows.Application.Current.FindResource("BlackQueen")
            };
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var f = value as Piece?;
            if (f == null) return null;

            return _images[f.Value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}