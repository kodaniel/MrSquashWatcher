using System.Collections.ObjectModel;

namespace MrSquash.Application.Extensions;

public static class DictionaryExtensions
{
    public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        return new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}
