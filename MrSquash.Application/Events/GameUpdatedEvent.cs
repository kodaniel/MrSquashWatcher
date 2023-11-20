using Prism.Events;

namespace MrSquash.Application.Events;

public sealed class GameUpdatedEvent : PubSubEvent<GameUpdatedEventArgs>
{
}

public sealed class GameUpdatedEventArgs : EventArgs
{
    public string? ErrorMessage { get; }
    public bool Success { get; }
    public IEnumerable<Game> FreedGames { get; }

    public GameUpdatedEventArgs() : this(new List<Game>())
    {
    }

    public GameUpdatedEventArgs(IEnumerable<Game> freedGames)
    {
        Success = true;
        FreedGames = freedGames;
    }

    public GameUpdatedEventArgs(string errorMessage)
    {
        Success = false;
        ErrorMessage = errorMessage;
        FreedGames = new List<Game>();
    }
}
