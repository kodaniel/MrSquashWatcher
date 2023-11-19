using System.Collections.ObjectModel;

namespace MrSquashWatcher.Extensions;

public static class DictionaryExtensions
{
    public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary)
    {
        return new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}
