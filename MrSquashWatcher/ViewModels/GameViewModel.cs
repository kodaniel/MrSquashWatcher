using Prism.Mvvm;

namespace MrSquashWatcher.ViewModels;

public class GameViewModel : BindableBase, ICloneable
{
    private int _row;
    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    private int _column;
    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    private DateOnly _date;
    public DateOnly Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    private TimeOnly _startTime;
    public TimeOnly StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    private TimeOnly _endTime;
    public TimeOnly EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    private bool _reserved;
    public bool Reserved
    {
        get => _reserved;
        set => SetProperty(ref _reserved, value);
    }

    private bool _enabled;
    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    private bool _selected;
    public bool Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
