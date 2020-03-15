using MrSquash.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MrSquashWatcher.Converters
{
    public class GridRowMarginConverter : IValueConverter
    {
        private readonly double gap = 4;
        private readonly double mingap = 2;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GameMatch gm = (GameMatch)value;
            if (gm.Row % 2 == 0)
                return new Thickness(mingap, gap, mingap, mingap);
            else
                return new Thickness(mingap, mingap, mingap, gap);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
