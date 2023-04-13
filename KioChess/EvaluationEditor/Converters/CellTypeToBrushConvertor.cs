using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EvaluationEditor.Models;

namespace EvaluationEditor.Converters
{
    class CellTypeToBrushConvertor : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value as CellType?;
            if (state == null || state.Value == CellType.Black) return Brushes.Brown;
            return Brushes.AntiqueWhite;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}