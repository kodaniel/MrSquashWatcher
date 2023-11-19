using ImTools;

namespace MrSquashWatcher.Services;

internal class FakeFamulusService : IFamulusService
{
    private int EmulateServiceResponseTime => new Random().Next(1000, 3000);

    public FakeFamulusService()
    {
        _ = new TimeOnly(10, 0);
    }

    public async Task<bool> Reserve(Models.Reservation reservation, CancellationToken cancellationToken = default!)
    {
        try
        {
            await Task.Delay(EmulateServiceResponseTime, cancellationToken);

            return reservation.StartDate.Day % 2 == 0;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    public async Task<IEnumerable<Day>> FetchCurrentWeek(CancellationToken cancellationToken = default!)
    {
        try
        {
            await Task.Delay(EmulateServiceResponseTime, cancellationToken);

            return PopulateWeek(Week.Now);
        }
        catch (TaskCanceledException)
        {
            return new List<Day>();
        }
    }

    public async Task<IEnumerable<Day>> FetchNextWeek(Week week, CancellationToken cancellationToken = default!)
    {
        try
        {
            await Task.Delay(EmulateServiceResponseTime, cancellationToken);

            return PopulateWeek(week + 1);
        }
        catch (TaskCanceledException)
        {
            return new List<Day>();
        }
    }

    private IEnumerable<Day> PopulateWeek(Week week)
    {
        var days = new List<Day>();
        for (int i = 0; i < 7; i++)
        {
            var date = week.StartDate.AddDays(i);

            var track1 = new Track("1. pálya", Enumerable.Range(0, 15).Select(i => CreateAppointment(date, i)).ToList());
            var track2 = new Track("2. pálya", Enumerable.Range(0, 15).Select(i => CreateAppointment(date, i)).ToList());

            var day = new Day(date, new List<Track> { track1, track2 });
            days.Add(day);
        }

        return days;
    }

    private Appointment CreateAppointment(DateOnly date, int i)
    {
        var startTime = new TimeOnly(7 + i, 0);
        return new()
        {
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            Enabled = date.ToDateTime(startTime) > DateTime.Now,
            Reserved = new Random().NextDouble() > 0.5,
            Price = 2500
        };
    }
}
