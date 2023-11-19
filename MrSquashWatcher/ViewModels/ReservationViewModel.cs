using MrSquashWatcher.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Globalization;

namespace MrSquashWatcher.ViewModels;

public enum ReservationResults
{
    None,
    Failed,
    Succeeded
}

public class ReservationViewModel : BindableBase, IDialogAware
{
    private readonly IFamulusService _famulusService;

    private Game _game;
    private bool _isSubmitting;
    private ReservationResults _reservationResult = ReservationResults.None;
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

    public string Appointment
    {
        get
        {
            var culture = new CultureInfo("hu-HU");
            var info = culture.DateTimeFormat;
            var dayName = info.DayNames[(int)_game.Date.DayOfWeek].FirstCharToUpper();
            return $"{_game.Date:yyyy.MM.dd} {dayName} {_game.StartTime:HH:mm}-{_game.EndTime:HH:mm} {_game.Track + 1}. pálya";
        }
    }

    public bool IsSubmitting
    {
        get => _isSubmitting;
        set
        {
            if (SetProperty(ref _isSubmitting, value))
                ReserveCommand.RaiseCanExecuteChanged();
        }
    }

    public ReservationResults ReservationResult
    {
        get => _reservationResult;
        set => SetProperty(ref _reservationResult, value);
    }

    public DelegateCommand ReserveCommand { get; init; }

    public DelegateCommand CloseCommand { get; init; }

    public ReservationViewModel(IFamulusService famulusService)
    {
        _famulusService = famulusService;

        ReserveCommand = new DelegateCommand(OnReserve, CanReserve);
        CloseCommand = new DelegateCommand(() => RequestClose(new DialogResult(ButtonResult.Cancel)));
    }

    private async void OnReserve()
    {
        ReservationResult = ReservationResults.None;
        IsSubmitting = true;

        ReservationResult = await _famulusService.Reserve(new()
        {
            Name = Name,
            Email = Email,
            Phone = Phone,
            TrackId = _game.Track,
            StartDate = _game.Date,
            StartTime = _game.StartTime,
            EndTime = _game.EndTime
        }) ? ReservationResults.Succeeded : ReservationResults.Failed;

        await Task.Delay(2000);

        IsSubmitting = false;

        if (ReservationResult == ReservationResults.Succeeded)
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

        return _game.Enabled && !_game.Reserved && !IsSubmitting;
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        _game = parameters.GetValue<Game>("game") ?? throw new ArgumentNullException("game");

        Name = UserSettings.Instance.Name;
        Email = UserSettings.Instance.Email;
        Phone = UserSettings.Instance.Phone;
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
        UserSettings.Instance.SetUser(Name, Email, Phone);
    }
}
