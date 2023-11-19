namespace MrSquashWatcher.Contracts;

public interface IFamulusService
{
    Task<IEnumerable<Day>> FetchCurrentWeek(CancellationToken cancellationToken = default!);
    Task<IEnumerable<Day>> FetchNextWeek(Week week, CancellationToken cancellationToken = default!);
    Task<bool> Reserve(Models.Reservation reservation, CancellationToken cancellationToken = default!);
}
