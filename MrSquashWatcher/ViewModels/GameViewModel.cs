using Prism.Mvvm;

namespace MrSquashWatcher.ViewModels;

public class GameViewModel(Game game, IUserSettings userSettings) : BindableBase
{
    private readonly Game _game = game ?? throw new ArgumentNullException(nameof(game));
    private readonly IUserSettings _userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

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
        get => _userSettings.IsSelected(_game.CalendarPosition);
        set
        {
            _userSettings.SetSelected(_game.CalendarPosition, value);
            RaisePropertyChanged();
        }
    }

    public Game GetModel() => _game;
}
