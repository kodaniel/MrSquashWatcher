using H.NotifyIcon.Core;
using Microsoft.Toolkit.Uwp.Notifications;
using MrSquashWatcher.Properties;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Diagnostics;
using System.Windows;
using Windows.Foundation.Collections;

namespace MrSquashWatcher.ViewModels;

public class TaskbarViewModel : BindableBase
{
    private const int WEEKS_MAX_FORECAST = 6;

    private readonly IEventAggregator _eventAggregator;
    private readonly IGamesManager _gamesManager;
    private readonly IStartupService _startupService;
    private readonly IDialogService _dialogService;

    private bool _isLoadingCalendar;
    private Week _currentWeek = Week.Now;
    private IEnumerable<GameViewModel> _calendarGames = new List<GameViewModel>();

    public IEnumerable<GameViewModel> CalendarGames => _calendarGames;

    public bool IsLoadingCalendar
    {
        get => _isLoadingCalendar;
        set => SetProperty(ref _isLoadingCalendar, value);
    }

    public Week CurrentWeek
    {
        get => _currentWeek;
        set
        {
            if (SetProperty(ref _currentWeek, value))
            {
                RaisePropertyChanged(nameof(StartDate));
                RaisePropertyChanged(nameof(EndDate));

                CurrentWeekCommand.RaiseCanExecuteChanged();
                NextWeekCommand.RaiseCanExecuteChanged();
                PreviousWeekCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public DateOnly StartDate => CurrentWeek.StartDate;

    public DateOnly EndDate => CurrentWeek.EndDate;

    public bool AutoStartupApplication
    {
        get => _startupService.IsRunApplicationOnStartup();
        set => _startupService.SetApplicationStartup(value);
    }

    public DelegateCommand CurrentWeekCommand { get; init; }
    public DelegateCommand NextWeekCommand { get; init; }
    public DelegateCommand PreviousWeekCommand { get; init; }
    public DelegateCommand OpenPopupCommand { get; init; }
    public DelegateCommand<GameViewModel> ReserveCommand { get; init; }

    public TaskbarViewModel(IEventAggregator eventAggregator, IGamesManager gamesManager, IStartupService startupService, IDialogService dialogService)
    {
        _eventAggregator = eventAggregator;
        _gamesManager = gamesManager;
        _startupService = startupService;
        _dialogService = dialogService;

        _eventAggregator.GetEvent<GameUpdatedEvent>().Subscribe(OnGamesUpdated, ThreadOption.UIThread);
        _eventAggregator.GetEvent<OpenReservationDialogEvent>().Subscribe(OnOpenReservationWindow, ThreadOption.UIThread);

        CurrentWeekCommand = new DelegateCommand(MoveCurrentWeek, () => CurrentWeek != Week.Now);
        NextWeekCommand = new DelegateCommand(MoveNextWeek, () => CurrentWeek < Week.Now + WEEKS_MAX_FORECAST);
        PreviousWeekCommand = new DelegateCommand(MovePreviousWeek, () => CurrentWeek > Week.Now);
        OpenPopupCommand = new DelegateCommand(OpenPopup);
        ReserveCommand = new DelegateCommand<GameViewModel>(OnReserve);

        ApplicationCommands.ExitCommand.RegisterCommand(new DelegateCommand(Exit));
        ApplicationCommands.SettingsCommand.RegisterCommand(new DelegateCommand(OpenSettings));

        ToastNotificationManagerCompat.OnActivated += OnToastNotification;
        
        App.TaskBarIcon.Icon = Resources.trayicon;
    }

    private void OnToastNotification(ToastNotificationActivatedEventArgsCompat e)
    {
        ToastArguments args = ToastArguments.Parse(e.Argument);
        ValueSet userInput = e.UserInput;
        var toastId = args.TryGetValue("toastId", out string id) ? id : string.Empty;

        if (toastId == "game-freed")
        {
            var game = JsonConvert.DeserializeObject<Game>(args["game"]);
            _eventAggregator.GetEvent<OpenReservationDialogEvent>().Publish(game);
        }
        else if (toastId == "games-freed")
        {
            var game = JsonConvert.DeserializeObject<Game>(args[(string)userInput["selected"]]);
            _eventAggregator.GetEvent<OpenReservationDialogEvent>().Publish(game);
        }
    }

    private CancellationTokenSource _cancellationTokenSource;

    private async Task UpdateCalendar()
    {
        Debug.WriteLine($"Updating week {CurrentWeek.StartDate}");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        IsLoadingCalendar = true;

        var games = await _gamesManager.GetOrUpdateGamesOnWeek(CurrentWeek, forceUpdate: false, cancellationToken: token);

        if (token.IsCancellationRequested)
        {
            Debug.WriteLine($"Updating week {CurrentWeek.StartDate} cancelled");
            return;
        }

        IsLoadingCalendar = false;

        Debug.WriteLine($"Updated week {CurrentWeek.StartDate} successfully");
        _calendarGames = GamesToViewModel(games);
        RaisePropertyChanged(nameof(CalendarGames));
    }

    private IEnumerable<GameViewModel> GamesToViewModel(IEnumerable<Game> games) =>
        games.Select(g => new GameViewModel(g));

    #region Command handlers

    private void OnReserve(GameViewModel gameViewModel) => 
        _eventAggregator.GetEvent<OpenReservationDialogEvent>().Publish(gameViewModel.GetModel());

    private async void MoveCurrentWeek()
    {
        CurrentWeek = Week.Now;

        await UpdateCalendar();
    }

    private async void MoveNextWeek()
    {
        CurrentWeek++;

        await UpdateCalendar();
    }

    private async void MovePreviousWeek()
    {
        CurrentWeek--;

        await UpdateCalendar();
    }

    private static bool _settingsDialogOpened = false;
    private void OpenSettings()
    {
        if (_settingsDialogOpened) return;
        _settingsDialogOpened = true;

        _dialogService.ShowDialog("settings", _ => _settingsDialogOpened = false);
    }

    private void OpenPopup()
    {
        App.TaskBarIcon.Icon = Resources.trayicon;
    }

    private void Exit()
    {
        //ToastNotificationManagerCompat.History.Clear();
        Application.Current.Shutdown();
    }

    #endregion Command handlers

    #region Event handlers

    private void OnOpenReservationWindow(Game game)
    {
        if (!game.Enabled || game.Reserved)
            return;

        DialogParameters parameters = new()
        {
            { "game", game }
        };

        _dialogService.Show("reservation", parameters, _ => { });
    }

    private void OnGamesUpdated(GameUpdatedEventArgs e)
    {
        if (e.Success)
        {
            var games = _gamesManager.GetGamesOnWeek(CurrentWeek);
            _calendarGames = GamesToViewModel(games);
            RaisePropertyChanged(nameof(CalendarGames));

            ShowFreedGames(e.FreedGames.ToList());
        }
        else
        {
            ShowError(e.ErrorMessage);
        }
    }

    private void ShowFreedGames(List<Game> games)
    {
        if (!games.Any())
            return;

        App.TaskBarIcon.Icon = Resources.trayicon_active;

        if (!UserSettings.Instance.ShowNotifications)
            return;

        var generateGameTitle = (Game g) => $"{g.Date:yyyy.MM.dd} {g.StartTime:HH} óra, {g.Track + 1}. pálya";

        var toastBuilder = new ToastContentBuilder()
            .AddButton(new ToastButton()
                .SetContent("Mégse")
                .SetDismissActivation())
            .AddButton(new ToastButton()
                .SetContent("Foglalás")
                .AddArgument("action", "reserve")
                .SetBackgroundActivation())
            .SetToastScenario(ToastScenario.Reminder);

        if (games.Count == 1)
        {
            toastBuilder
                .AddArgument("toastId", "game-freed")
                .AddText("Egy pálya felszabadult")
                .AddText(generateGameTitle(games[0]))
                .AddArgument("game", JsonConvert.SerializeObject(games[0]));
        }
        else
        {
            var choices = games.Take(5).Select((g, i) => (i.ToString(), generateGameTitle(g))).ToList();
            for (int i = 0; i < choices.Count; i++)
                toastBuilder.AddArgument(i.ToString(), JsonConvert.SerializeObject(games[i]));

            toastBuilder
                .AddArgument("toastId", "games-freed")
                .AddText($"{games.Count} pálya felszabadult")
                .AddComboBox("selected", "Válassz pályát", "0", choices);
        }

        toastBuilder.Show(toast =>
        {
            toast.ExpirationTime = DateTime.Now.AddDays(1);
            toast.Group = "freedGame";
        });
    }

    private void ShowError(string message)
    {
        if (!UserSettings.Instance.ShowNotifications)
            return;

        App.TaskBarIcon.ShowNotification("Hiba", message, NotificationIcon.Error);
    }

    #endregion Event handlers
}
