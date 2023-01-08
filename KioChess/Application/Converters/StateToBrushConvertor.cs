using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Kgb.ChessApp.Models;

namespace Kgb.ChessApp.Converters
{
    class StateToBrushConvertor:IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value as State?;
            if (state == null) return Brushes.Black;

            switch (state.Value)
            {
                case State.MoveFrom: return Brushes.Blue;
                case State.MoveTo:
                    return Brushes.Yellow;
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
