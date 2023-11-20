using Prism.Mvvm;

namespace MrSquashWatcher.ViewModels;

public class GameViewModel : BindableBase
{
    private readonly Game _game;

    public int Row => _game.CalendarPosition.Row;

    public int Column => _game.CalendarPosition.Column;

    public int Track => _game.Track;

    public DateOnly Date => _game.Date;

    public TimeOnly StartTime => _game.StartTime;

    public TimeOnly EndTime => _game.EndTime;

    public bool Reserved => _game.Reserved;

    public bool Enabled => _game.Enabled;

    public bool Selected
    {
        get => UserSettings.Instance.IsSelected(_game.CalendarPosition);
        set
        {
            UserSettings.Instance.SetSelected(_game.CalendarPosition, value);
            RaisePropertyChanged();
        }
    }

    public GameViewModel(Game game) => _game = game;

    public Game GetModel() => _game;
}
