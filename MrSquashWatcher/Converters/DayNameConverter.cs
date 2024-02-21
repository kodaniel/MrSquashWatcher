using System.Globalization;
using System.Windows.Data;

namespace MrSquashWatcher.Converters;

public class DayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DateTime d = (DateTime)value;

        culture = new CultureInfo("hu-HU");
        var info = culture.DateTimeFormat;
        return char.ToString(info.DayNames[(int)d.DayOfWeek].ToUpper()[0]);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
