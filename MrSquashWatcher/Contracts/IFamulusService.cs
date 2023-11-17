namespace MrSquashWatcher.Contracts;

public interface IFamulusService
{
    Task<IEnumerable<Day>> FetchCurrentWeek();
    Task<IEnumerable<Day>> FetchNextWeek(DateOnly date);
    Task<bool> Reserve(Reservation reservation);
}
