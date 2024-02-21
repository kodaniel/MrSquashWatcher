using System.ComponentModel;

namespace MrSquash.Tests;

internal class PropertyChangedObserver<T> : IDisposable
    where T : class, INotifyPropertyChanged
{
    private readonly T _instance;
    private List<PropertyChangedEventArgs> _events = new();
    
    public IEnumerable<string?> PropertyNames => _events.Select(e => e.PropertyName);

    public int Count => _events.Count;

    public bool IsDirty => _events.Any();

    public PropertyChangedObserver(T instance)
    {
        _instance = instance;
        _instance.PropertyChanged += Internal_PropertyChanged;
    }

    public void Reset() => _events.Clear();

    public void Dispose()
    {
        _instance.PropertyChanged -= Internal_PropertyChanged;
    }

    private void Internal_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _events.Add(e);
    }
}
