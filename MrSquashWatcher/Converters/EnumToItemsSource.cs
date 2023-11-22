using System.ComponentModel;
using System.Windows.Markup;

namespace MrSquashWatcher.Converters;

public class EnumToItemsSource : MarkupExtension
{
    private readonly Type _type;

    public EnumToItemsSource(Type type)
    {
        _type = type;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetValues(_type).Cast<object>().Select(GetEnumItem);
    }

    private object GetEnumItem(object item)
    {
        var description = item.ToString()!;
        var fieldInfo = item.GetType().GetField(item.ToString()!);

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return new
        {
            Value = item,
            DisplayName = description
        };
    }
}
