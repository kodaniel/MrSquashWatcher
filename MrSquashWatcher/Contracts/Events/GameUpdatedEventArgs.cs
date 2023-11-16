namespace MrSquashWatcher.Contracts.Events;

public class GameUpdatedEventArgs : EventArgs
{
    public string ErrorMessage { get; }
    public bool Success { get; }
    public IEnumerable<GameViewModel> FreedGames { get; }

    public GameUpdatedEventArgs() : this(new List<GameViewModel>())
    {
    }

    public GameUpdatedEventArgs(IEnumerable<GameViewModel> freedGames)
    {
        Success = true;
        FreedGames = freedGames;
    }

    public GameUpdatedEventArgs(string errorMessage)
    {
        Success = false;
        ErrorMessage = errorMessage;
        FreedGames = new List<GameViewModel>();
    }
}
