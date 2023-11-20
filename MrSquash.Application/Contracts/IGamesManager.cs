namespace MrSquash.Application.Contracts;

public interface IGamesManager
{
    IEnumerable<Game> GetGamesOnWeek(Week week);
    Task<IEnumerable<Game>> GetOrUpdateGamesOnWeek(Week week, bool forceUpdate = false, CancellationToken cancellationToken = default!);

    void Start();
    void Stop();
}
