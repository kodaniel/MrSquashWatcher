﻿using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace MrSquashWatcher.ViewModels;

public class SettingsViewModel : BindableBase, IDialogAware
{
    private readonly IStartupService _startupService;

    public int WatchWeeks
    {
        get => UserSettings.Instance.NumOfWeeks;
        set
        {
            UserSettings.Instance.NumOfWeeks = value;
            RaisePropertyChanged();
        }
    }

    public bool ShowNotifications
    {
        get => UserSettings.Instance.ShowNotifications;
        set
        {
            UserSettings.Instance.ShowNotifications = value;
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

    public string Title => "Beállítások";

    public event Action<IDialogResult> RequestClose;

    public SettingsViewModel(IStartupService startupService)
    {
        _startupService = startupService;
    }

    public bool CanCloseDialog() => true;

    public void OnDialogOpened(IDialogParameters parameters)
    {
        
    }

    public void OnDialogClosed()
    {
        
    }
}
