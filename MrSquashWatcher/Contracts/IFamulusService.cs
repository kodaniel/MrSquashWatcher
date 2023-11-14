namespace MrSquashWatcher.Contracts;

public interface IFamulusService
{
    Task<IEnumerable<Day>> FetchCurrentWeek();
    Task<bool> Reserve(Reservation reservation);
}
