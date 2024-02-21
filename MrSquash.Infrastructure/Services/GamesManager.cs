using Microsoft.Extensions.Logging;
using Prism.Events;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MrSquash.Infrastructure.Services;

public class GamesManager : IGamesManager, IDisposable
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IUserSettings _userSettings;
    private readonly IFamulusService _famulusService;
    private readonly ILogger<GamesManager> _logger;
    private readonly BackgroundWorker _worker;

    private ConcurrentDictionary<Week, ReadOnlyDictionary<CalendarPosition, Game>> _games;
    private ConcurrentDictionary<Week, DateTime> _lastFetchTimes;
#if DEBUG
    private TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);
#else
    private TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);
#endif
    private CancellationTokenSource _cts = null!;

    public GamesManager(IEventAggregator eventAggregator, IUserSettings userSettings, IFamulusService famulusService, ILogger<GamesManager> logger)
    {
        _eventAggregator = eventAggregator;
        _userSettings = userSettings;
        _famulusService = famulusService;
        _logger = logger;

        _games = new ConcurrentDictionary<Week, ReadOnlyDictionary<CalendarPosition, Game>>();
        _lastFetchTimes = new ConcurrentDictionary<Week, DateTime>();

        _worker = new BackgroundWorker();
        _worker.DoWork += DoWork;
    }

    public async Task<IEnumerable<Game>> GetOrUpdateGamesOnWeek(Week week, bool forceUpdate = false, CancellationToken cancellationToken = default!)
    {
        var isOutdated = (_lastFetchTimes.ContainsKey(week) && DateTime.UtcNow - _lastFetchTimes[week] > RefreshInterval);
        if (forceUpdate || !_games.ContainsKey(week) || isOutdated)
        {
            var games = await FetchGamesOnWeek(week, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return new List<Game>();

            var newGames = ConvertGamesToCalendar(games);
            _lastFetchTimes[week] = DateTime.UtcNow;

            if (_games.ContainsKey(week) && week < Week.Now.AddWeeks(_userSettings.NumOfWeeks))
            {
                var freedGames = CheckFreedGames(_games[week], newGames);
                _eventAggregator.GetEvent<GameUpdatedEvent>().Publish(new GameUpdatedEventArgs(freedGames));
            }

            _games[week] = newGames;
        }

        return _games[week].Values;
    }

    public IEnumerable<Game> GetGamesOnWeek(Week week) =>
        _games.TryGetValue(week, out var gamesOnWeek) ? gamesOnWeek.Values : new List<Game>();

    public void Start()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _worker.RunWorkerAsync();
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async void DoWork(object? sender, DoWorkEventArgs e)
    {
        while (!_cts.IsCancellationRequested)
        {
            await UpdateWeeks(_cts.Token);
            await Task.Delay(RefreshInterval);
        }
    }

    internal async Task UpdateWeeks(CancellationToken cancellationToken = default)
    {
        List<Game> allFreedGames = new();
        var currentWeek = Week.Now;

        for (int i = 0; i < _userSettings.NumOfWeeks; i++)
        {
            var oldGames = _games.GetValueOrDefault(currentWeek, new Dictionary<CalendarPosition, Game>().ToReadOnlyDictionary());
            var games = await FetchGamesOnWeek(currentWeek, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            _lastFetchTimes[currentWeek] = DateTime.UtcNow;

            if (!games.Any())
            {
                const string errorMsg = "Nem sikerült letölteni a pályafoglaltságot.";
                _eventAggregator.GetEvent<GameUpdatedEvent>().Publish(new GameUpdatedEventArgs(errorMsg));
                break;
            }

            _games[currentWeek] = ConvertGamesToCalendar(games);

            var freedGames = CheckFreedGames(oldGames, _games[currentWeek]);
            allFreedGames.AddRange(freedGames);

            currentWeek++;
        }

        _eventAggregator.GetEvent<GameUpdatedEvent>().Publish(new GameUpdatedEventArgs(allFreedGames));
    }

    private IEnumerable<Game> CheckFreedGames(IDictionary<CalendarPosition, Game> oldGames, IDictionary<CalendarPosition, Game> newGames)
    {
        foreach (var oldGamePair in oldGames)
        {
            var oldGame = oldGamePair.Value;
            var newGame = newGames[oldGamePair.Key];

            if (!_userSettings.IsSelected(oldGamePair.Key))
                continue;

            if (oldGame.Reserved && !newGame.Reserved && newGame.Enabled)
            {
                yield return newGame;
            }
        }
    }

    private async Task<IEnumerable<Game>> FetchGamesOnWeek(Week week, CancellationToken cancellationToken)
    {
        IEnumerable<Day> days;

        if (Week.Now == week)
            days = await _famulusService.FetchCurrentWeek(cancellationToken);
        else
            days = await _famulusService.FetchNextWeek(week - 1, cancellationToken);

        return BuildGames(days);
    }

    private IEnumerable<Game> BuildGames(IEnumerable<Day> days)
    {
        int row = 0;
        foreach (Day day in days)
        {
            for (int trackId = 0; trackId < day.Tracks.Count; trackId++)
            {
                for (int col = 0; col < day.Tracks[trackId].Times.Count; col++)
                {
                    Appointment appointment = day.Tracks[trackId].Times[col];

                    Game game = new()
                    {
                        Date = day.Date,
                        StartTime = appointment.StartTime,
                        EndTime = appointment.EndTime,
                        Track = trackId,
                        Reserved = appointment.Reserved,
                        Enabled = appointment.Enabled,
                        Price = appointment.Price,
                        CalendarPosition = new CalendarPosition(row, col)
                    };

                    yield return game;
                }

                row++;
            }
        }
    }

    private static ReadOnlyDictionary<CalendarPosition, Game> ConvertGamesToCalendar(IEnumerable<Game> games) =>
        new Dictionary<CalendarPosition, Game>(games.Select(game => new KeyValuePair<CalendarPosition, Game>(game.CalendarPosition, game)))
            .ToReadOnlyDictionary();

    public void Dispose()
    {
        _cts?.Cancel();
        _worker.Dispose();
    }
}
