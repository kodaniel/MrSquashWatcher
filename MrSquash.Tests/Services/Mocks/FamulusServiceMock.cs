namespace MrSquash.Tests.Services.Mocks;

internal class FamulusServiceMock : IFamulusService
{
    private int _emulateServiceResponseTime = 0;
    private Dictionary<Week, IEnumerable<Day>> _weeks = new();

    public FamulusServiceMock()
    {
    }

    public FamulusServiceMock(int responseTime)
    {
        _emulateServiceResponseTime = responseTime;
    }

    public async Task<bool> Reserve(Reservation reservation, CancellationToken cancellationToken = default!)
    {
        try
        {
            await Task.Delay(_emulateServiceResponseTime, cancellationToken);

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
            await Task.Delay(_emulateServiceResponseTime, cancellationToken);

            return GetWeekOrPopulate(Week.Now);
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
            await Task.Delay(_emulateServiceResponseTime, cancellationToken);

            return GetWeekOrPopulate(week + 1);
        }
        catch (TaskCanceledException)
        {
            return new List<Day>();
        }
    }

    public void SetResponseTime(int milliseconds) => _emulateServiceResponseTime = milliseconds;

    public void PopulateWeek(Week week, IEnumerable<Day> days)
    {
        _weeks[week] = days;
    }

    private IEnumerable<Day> GetWeekOrPopulate(Week week)
    {
        if (_weeks.ContainsKey(week))
            return _weeks[week];

        var days = new List<Day>();
        for (int i = 0; i < 7; i++)
        {
            var date = week.StartDate.AddDays(i);

            var track1 = new Track("1. pálya", Enumerable.Range(0, 15).Select(i => CreateAppointment(date, i)).ToList());
            var track2 = new Track("2. pálya", Enumerable.Range(0, 15).Select(i => CreateAppointment(date, i)).ToList());

            var day = new Day(date, new List<Track> { track1, track2 });
            days.Add(day);
        }

        return _weeks[week] = days;
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
