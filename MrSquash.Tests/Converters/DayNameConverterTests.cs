using MrSquashWatcher.Converters;
using System.Globalization;

namespace MrSquash.Tests.Converters;

[TestFixture]
public class DayNameConverterTests
{
    [TestCase("2024-01-01", "H")]
    [TestCase("2024-01-02", "K")]
    [TestCase("2024-01-03", "S")]
    [TestCase("2024-01-04", "C")]
    [TestCase("2024-01-05", "P")]
    [TestCase("2024-01-06", "S")]
    [TestCase("2024-01-07", "V")]
    public void Convert(string date, string dayLabel)
    {
        var converter = new DayNameConverter();
        var result = converter.Convert(DateTime.Parse(date), typeof(DateTime), null, CultureInfo.InvariantCulture);
        
        Assert.IsNotNull(result);
        Assert.That(result, Is.EqualTo(dayLabel));
    }
}
