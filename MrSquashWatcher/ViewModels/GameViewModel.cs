using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace MrSquashWatcher.ViewModels;

public class GameViewModel : BindableBase, ICloneable
{
    private readonly IDialogService _dialogService;
    private readonly IFamulusService _famulusService;

    private bool _isDialogOpened = false;
    private int _row;
    private int _column;
    private DateOnly _date;
    private TimeOnly _startTime;
    private TimeOnly _endTime;
    private bool _reserved;
    private bool _enabled;
    private bool _selected;

    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    public DateOnly Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public TimeOnly StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public TimeOnly EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public bool Reserved
    {
        get => _reserved;
        set => SetProperty(ref _reserved, value);
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public bool Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public DelegateCommand ReserveCommand { get; }

    public GameViewModel(IDialogService dialogService, IFamulusService famulusService)
    {
        _dialogService = dialogService;
        _famulusService = famulusService;

        ReserveCommand = new DelegateCommand(OnReserve);
    }

    private void OnReserve()
    {
        if (!Enabled || Reserved)
            return;

        if (_isDialogOpened)
            return;
        _isDialogOpened = true;

        var parameters = new DialogParameters
        {
            { "game", this }
        };

        _dialogService.ShowDialog("reservation", parameters, _ =>
        {
            _isDialogOpened = false;
        });
    }

    private bool CanReserve()
    {
        return Enabled && !Reserved;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
