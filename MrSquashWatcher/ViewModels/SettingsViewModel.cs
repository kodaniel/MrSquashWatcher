using MrSquash.Application;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace MrSquashWatcher.ViewModels;

public class SettingsViewModel : BindableBase, IDialogAware
{
    private readonly IStartupService _startupService;
    private readonly IUserSettings _userSettings;

    public int WatchWeeks
    {
        get => _userSettings.NumOfWeeks;
        set
        {
            _userSettings.NumOfWeeks = value;
            RaisePropertyChanged();
        }
    }

    public bool ShowNotifications
    {
        get => _userSettings.ShowNotifications;
        set
        {
            _userSettings.ShowNotifications = value;
            RaisePropertyChanged();
        }
    }

    public bool AutoStartupApplication
    {
        get => _startupService.IsRunApplicationOnStartup();
        set
        {
            _startupService.SetApplicationStartup(value);
            RaisePropertyChanged();
        }
    }

    public AppThemes SelectedTheme
    {
        get => _userSettings.ApplicationTheme;
        set
        {
            _userSettings.ApplicationTheme = value;
            RaisePropertyChanged();
        }
    }

    public string ApplicationVersion => App.Version.ToString(); //Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

    public string Title => "Beállítások";

    public event Action<IDialogResult> RequestClose;

    public SettingsViewModel(IStartupService startupService, IUserSettings userSettings)
    {
        _startupService = startupService;
        _userSettings = userSettings;
    }

    public bool CanCloseDialog() => true;

    public void OnDialogOpened(IDialogParameters parameters)
    {
        
    }

    public void OnDialogClosed()
    {
        
    }
}
