namespace MrSquashWatcher.Contracts;

public interface IGamesManager
{
    event EventHandler<GameUpdatedEventArgs> Updated;

    IReadOnlyCollection<GameViewModel> Games { get; }

    void Start();
    void Stop();
}
