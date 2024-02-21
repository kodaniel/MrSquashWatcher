﻿using H.NotifyIcon;
using MrSquashWatcher.Extensions;
using MrSquashWatcher.Views;
using Prism.Ioc;
using Serilog;
using Squirrel;
using System.Windows;

namespace MrSquashWatcher;

public partial class App
{
    private static Mutex _mutex;

    public static TaskbarIcon TaskBarIcon { get; private set; }

    public static SemanticVersion Version { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        const string appName = "MrSquashWatcher";
        bool createdNew;

        _mutex = new Mutex(true, appName, out createdNew);

        if (!createdNew)
        {
            //app is already running! Exiting the application  
            Current.Shutdown();
        }

        SquirrelAwareApp.HandleEvents(
            onInitialInstall: OnAppInstall,
            onAppUninstall: OnAppUninstall,
            onEveryRun: OnAppRun);

        // Configure Serilog and the sinks at the startup of the app
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(path: "MrSquashWatcher.log")
            .CreateLogger();

        base.OnStartup(e);

        var userSettings = Container.Resolve<IUserSettings>();
        userSettings.Load();
    }

    protected override Window CreateShell()
    {
        TaskBarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
        TaskBarIcon.ForceCreate();
        TaskBarIcon.DataContext = Container.Resolve<TaskbarViewModel>();

        return null;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSerilog(Log.Logger);

        // Views and Viewmodels
        containerRegistry.Register<TaskbarViewModel>();
        containerRegistry.RegisterDialog<Views.Reservation, ReservationViewModel>("reservation");
        containerRegistry.RegisterDialog<Settings, SettingsViewModel>("settings");

        // Services
        containerRegistry.RegisterSingleton<IUserSettings, UserSettings>();
        containerRegistry.Register<IStartupService, StartupService>();
        containerRegistry.Register<IFamulusService, FamulusService>();
        containerRegistry.RegisterSingleton<IGamesManager, GamesManager>();
        containerRegistry.Register<IUpdateService, UpdateService>();
    }

    protected override async void Initialize()
    {
        Log.Information("Initializing application");
        base.Initialize();

        Log.Information("Checking for updates");
        var updater = Container.Resolve<IUpdateService>();
        await updater.UpdateApp();

        Log.Information("Start watching");
        var gamesManager = Container.Resolve<IGamesManager>();
        gamesManager.Start();
    }

    private void OnApplicationExit(object sender, ExitEventArgs e)
    {
        Log.Information("Exiting application");

        var gamesManager = Container.Resolve<IGamesManager>();
        gamesManager.Stop();

        var userSettings = Container.Resolve<IUserSettings>();
        userSettings.Save();

        TaskBarIcon?.Dispose();

        // Flush all Serilog sinks before the app closes
        Log.CloseAndFlush();
    }

    private static void OnAppInstall(SemanticVersion version, IAppTools tools)
    {
        tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu);
    }

    private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu);
    }

    private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        Version = version ?? new SemanticVersion("0.0.0");

        tools.SetProcessAppUserModelId();
    }
}
