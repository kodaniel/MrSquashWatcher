using H.NotifyIcon.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using MrSquashWatcher.Properties;
using MrSquashWatcher.Toasts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Windows;
using Windows.Foundation.Collections;

namespace MrSquashWatcher.ViewModels;

public class TaskbarViewModel : BindableBase
{
    private const int WEEKS_MAX_FORECAST = 6;

    private readonly IEventAggregator _eventAggregator;
    private readonly IGamesManager _gamesManager;
    private readonly IStartupService _startupService;
    private readonly IUserSettings _userSettings;
    private readonly IDialogService _dialogService;
    private readonly ILogger<TaskbarViewModel> _logger;

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

    public TaskbarViewModel(
        IEventAggregator eventAggregator, IGamesManager gamesManager, IStartupService startupService, 
        IUserSettings userSettings, IDialogService dialogService, ILogger<TaskbarViewModel> logger)
    {
        _eventAggregator = eventAggregator;
        _gamesManager = gamesManager;
        _startupService = startupService;
        _userSettings = userSettings;
        _dialogService = dialogService;
        _logger = logger;

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
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        IsLoadingCalendar = true;

        var games = await _gamesManager.GetOrUpdateGamesOnWeek(CurrentWeek, forceUpdate: false, cancellationToken: token);

        if (token.IsCancellationRequested)
        {
            return;
        }

        IsLoadingCalendar = false;

        _calendarGames = GamesToViewModel(games);
        RaisePropertyChanged(nameof(CalendarGames));
    }

    private IEnumerable<GameViewModel> GamesToViewModel(IEnumerable<Game> games) =>
        games.Select(g => new GameViewModel(g, _userSettings));

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

        if (!_userSettings.ShowNotifications)
            return;

        var toastFactory = new GameFreedToastFactory((Game g) => $"{g.Date:yyyy.MM.dd} {g.StartTime:HH} óra, {g.Track + 1}. pálya");
        var toast = toastFactory.Create(games);
        
        toast.Show(t =>
        {
            t.ExpirationTime = DateTime.Now.AddDays(1);
            t.Group = "freedGame";
        });
    }

    private void ShowError(string message)
    {
        if (!_userSettings.ShowNotifications)
            return;

        _logger.LogError("An error occured during downloading the games: {message}", message);
        App.TaskBarIcon.ShowNotification("Hiba", message, NotificationIcon.Error);
    }

    #endregion Event handlers
}
