namespace MrSquashWatcher.Contracts;

public interface IGamesManager
{
    IReadOnlyCollection<GameViewModel> Games { get; }

    void Start();
    void Stop();
}
