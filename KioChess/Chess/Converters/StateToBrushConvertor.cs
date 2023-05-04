using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Chess.Models;

namespace Chess.Converters
{
    class StateToBrushConvertor : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value as State?;
            if (state == null) return Brushes.Black;

            switch (state.Value)
            {
                case State.MoveFrom: return Brushes.Blue;
                case State.MoveTo:  return Brushes.Yellow;
                case State.LastMoveFrom: return Brushes.Red;
                case State.LastMoveTo: return Brushes.Green;
                default: return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
