using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MrSquashWatcher.ViewModels;

public class ReservationViewModel : BindableBase, IDialogAware
{
    private readonly IGamesManager _gamesManager;
    private readonly IFamulusService _famulusService;

    private bool _isGamePickerOpened = false;
    private GameViewModel _selectedGame;
    private string _name;
    private string _email;
    private string _phone;

    public event Action<IDialogResult> RequestClose;

    public string Title => "Pályafoglalás";

    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            ReserveCommand.RaiseCanExecuteChanged();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ReserveCommand.RaiseCanExecuteChanged();
        }
    }

    public string Phone
    {
        get => _phone;
        set
        {
            SetProperty(ref _phone, value);
            ReserveCommand.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<GameViewModel> _games;
    public ObservableCollection<GameViewModel> Games => _games ??= new ObservableCollection<GameViewModel>();

    public GameViewModel SelectedGame
    {
        get => _selectedGame;
        private set
        {
            var oldSelectedGame = _selectedGame;
            SetProperty(ref _selectedGame, value);

            if (oldSelectedGame != null)
                oldSelectedGame.Selected = false;
            if (_selectedGame != null)
                _selectedGame.Selected = true;

            RaisePropertyChanged(nameof(SelectedGameTitle));
            ReserveCommand.RaiseCanExecuteChanged();
        }
    }

    public string SelectedGameTitle
    {
        get
        {
            if (SelectedGame == null)
                return string.Empty;

            var culture = new CultureInfo("hu-HU");
            var info = culture.DateTimeFormat;
            var day = info.DayNames[(int)SelectedGame.Date.DayOfWeek].FirstCharToUpper();
            return $"{SelectedGame.Date:yyyy.MM.dd} {day} {SelectedGame.StartTime:HH:mm}-{SelectedGame.EndTime:HH:mm}";
        }
    }

    public bool IsGamePickerOpened
    {
        get => _isGamePickerOpened;
        set => SetProperty(ref _isGamePickerOpened, value);
    }

    public DelegateCommand<GameViewModel> SelectCommand { get; }

    public DelegateCommand ReserveCommand { get; }

    public ReservationViewModel(IGamesManager gamesManager, IFamulusService famulusService)
    {
        _gamesManager = gamesManager;
        _famulusService = famulusService;

        ReserveCommand = new DelegateCommand(OnReserve, CanReserve);
        SelectCommand = new DelegateCommand<GameViewModel>(OnSelectGame, g => g.Enabled && !g.Reserved);
    }

    private void OnSelectGame(GameViewModel game)
    {
        IsGamePickerOpened = false;
        SelectedGame = game;
        ReserveCommand.RaiseCanExecuteChanged();
    }

    private async void OnReserve()
    {
        Reservation reservation = new()
        {
            Name = Name,
            Email = Email,
            Phone = Phone,
            StartDate = SelectedGame.Date,
            StartTime = SelectedGame.StartTime,
            EndTime = SelectedGame.EndTime
        };

        var result = await _famulusService.Reserve(reservation);

        if (result)
        {
            RequestClose(new DialogResult());
        }
    }

    private bool CanReserve()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;
        if (string.IsNullOrWhiteSpace(Email))
            return false;
        if (string.IsNullOrWhiteSpace(Phone))
            return false;
        if (SelectedGame is null)
            return false;

        return SelectedGame.Enabled && !SelectedGame.Reserved;
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        Name = UserSettings.Instance.Name;
        Email = UserSettings.Instance.Email;
        Phone = UserSettings.Instance.Phone;

        // Extract params
        var selectedByDefault = parameters.GetValue<GameViewModel>("selected");

        PopulateGames(_gamesManager.Games, selectedByDefault);

        SelectedGame = _games.FirstOrDefault(g => g.Selected);
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
        UserSettings.Instance.SetUser(Name, Email, Phone);
    }

    private void PopulateGames(IEnumerable<GameViewModel> games, GameViewModel selected)
    {
        _games = new ObservableCollection<GameViewModel>();
        foreach (var game in games)
        {
            var newGame = (GameViewModel)game.Clone();

            newGame.Selected = game == selected;
            newGame.Enabled = game.Enabled && !game.Reserved;

            _games.Add(newGame);
        }
    }
}
