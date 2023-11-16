using H.NotifyIcon.Core;
using MrSquashWatcher.Properties;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace MrSquashWatcher;

public class MainViewModel : BindableBase
{
    private static bool ReservationDialogOpened = false;
    private readonly IGamesManager _gamesManager;
    private readonly IStartupService _startupService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<GameViewModel> Games { get; init; } = new ObservableCollection<GameViewModel>();

    private DelegateCommand _openReservationCommand;
    public DelegateCommand OpenReservationCommand => _openReservationCommand ??= new DelegateCommand(OpenReservation, () => Games.Any() && !ReservationDialogOpened);

    private DelegateCommand<GameViewModel> _selectedChangedCommand;
    public DelegateCommand<GameViewModel> SelectedChangedCommand => _selectedChangedCommand ??= new DelegateCommand<GameViewModel>(gm =>
    {
        UserSettings.Instance.SetSelected(gm.Row, gm.Column, gm.Selected);
    });

    private DelegateCommand _exitCommand;
    public DelegateCommand ExitCommand => _exitCommand ??= new DelegateCommand(ExecuteExitCommand);

    public DelegateCommand ReservationCommand { get; }

    private DelegateCommand _openPopupCommand;
    public DelegateCommand OpenPopupCommand => _openPopupCommand ??= new DelegateCommand(ExecuteOpenPopupCommand);

    public bool AutoStartupApplication
    {
        get => _startupService.IsRunApplicationOnStartup();
        set
        {
            if (value)
                _startupService.AddApplicationToStartup();
            else
                _startupService.RemoveApplicationFromStartup();
        }
    }

    public int Week => Games.Any() ?
        CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(Games.First().Date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday) : 0;

    public MainViewModel(IGamesManager gamesManager, IStartupService startupService, IDialogService dialogService)
    {
        _gamesManager = gamesManager;
        _startupService = startupService;
        _dialogService = dialogService;
        _gamesManager.Updated += OnGamesUpdated;
        _gamesManager.Start();

        App.TaskBarIcon.Icon = Resources.icon;
    }

    private void OnGamesUpdated(object sender, GameUpdatedEventArgs e)
    {
        if (e.Success)
        {
            Games.Clear();
            Games.AddRange(_gamesManager.Games);

            RaisePropertyChanged(nameof(Week));
            OpenReservationCommand.RaiseCanExecuteChanged();

            ShowFreedGames(e.FreedGames);
        }
        else
        {
            ShowError(e.ErrorMessage);
        }
    }

    private void ShowFreedGames(IEnumerable<GameViewModel> games)
    {
        if (!games.Any())
            return;

        App.TaskBarIcon.Icon = Resources.icon_active;

        foreach (GameViewModel game in games)
        {
            var culture = new CultureInfo("hu-HU");
            var info = culture.DateTimeFormat;
            var msg = $"{info.DayNames[(int)game.Date.DayOfWeek].FirstCharToUpper()}, {game.StartTime:HH} óra";

            App.TaskBarIcon.ShowNotification("Pálya felszabadult", msg, NotificationIcon.Info);
        }
    }

    private void ShowError(string message)
    {
        App.TaskBarIcon.ShowNotification("Hiba", message, NotificationIcon.Error);
    }

    private void OpenReservation()
    {
        if (ReservationDialogOpened)
            return;
        ReservationDialogOpened = true;

        var parameters = new DialogParameters
        {
            { "selected", Games.FirstOrDefault(g => g.Enabled && !g.Reserved && g.Selected) }
        };

        _dialogService.Show("reservation", parameters, _ =>
        {
            ReservationDialogOpened = false;
            OpenReservationCommand.RaiseCanExecuteChanged();
        });

        OpenReservationCommand.RaiseCanExecuteChanged();
    }

    private void ExecuteOpenPopupCommand()
    {
        App.TaskBarIcon.Icon = Resources.icon;
    }

    private void ExecuteExitCommand()
    {
        Application.Current.Shutdown();
    }
}
