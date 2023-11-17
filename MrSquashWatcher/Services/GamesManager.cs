using Prism.Events;
using System.ComponentModel;
using System.Threading;

namespace MrSquashWatcher.Services;

internal class GamesManager : IGamesManager, IDisposable
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IFamulusService _famulusService;
    private readonly BackgroundWorker _worker;

    private Dictionary<(int, int), GameViewModel> _games;
    private TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);
    private CancellationTokenSource _cts;

    public IReadOnlyCollection<GameViewModel> Games => _games.Values;

    public GamesManager(IEventAggregator eventAggregator, IFamulusService famulusService)
    {
        _eventAggregator = eventAggregator;
        _famulusService = famulusService;

        _games = new Dictionary<(int, int), GameViewModel>(210);

        _worker = new BackgroundWorker();
        _worker.DoWork += DoWork;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _worker.RunWorkerAsync();
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private void DoWork(object sender, DoWorkEventArgs e)
    {
        while (!_cts.IsCancellationRequested)
        {
            UpdateLayout();
            Thread.Sleep(RefreshInterval);
        }
    }

    private async void UpdateLayout()
    {
        var freedAppointments = new List<GameViewModel>();
        var days = await _famulusService.FetchCurrentWeek();
        if (!days.Any())
        {
            const string errorMsg = "Nem sikerült letölteni a pályafoglaltságot.";
            _eventAggregator.GetEvent<GameUpdatedEvent>().Publish(new GameUpdatedEventArgs(errorMsg));
            return;
        }

        int row = 0;
        foreach (Day day in days)
        {
            foreach (Track track in day.Tracks)
            {
                for (int col = 0; col < track.Times.Count; col++)
                {
                    Appointment appointment = track.Times[col];

                    if (_games.TryGetValue((row, col), out GameViewModel gm))
                    {
                        gm = _games[(row, col)];
                        if (gm.Enabled && gm.Reserved && !appointment.Reserved) // pálya felszabadult
                        {
                            if (gm.Selected)
                                freedAppointments.Add(gm);
                        }

                        gm.Reserved = appointment.Reserved;
                        gm.Enabled = appointment.Enabled;
                    }
                    else
                    {
                        gm = new();
                        gm.Date = day.Date;
                        gm.StartTime = appointment.StartTime;
                        gm.EndTime = appointment.EndTime;
                        gm.Reserved = appointment.Reserved;
                        gm.Enabled = appointment.Enabled;
                        gm.Row = row;
                        gm.Column = col;
                        gm.Selected = UserSettings.Instance.IsWatching(row, col);

                        _games.Add((row, col), gm);
                    }
                }

                row++;
            }
        }

        _eventAggregator.GetEvent<GameUpdatedEvent>().Publish(new GameUpdatedEventArgs(freedAppointments));
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _worker.Dispose();
    }
}
