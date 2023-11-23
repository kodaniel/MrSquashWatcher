using H.NotifyIcon;
using MrSquashWatcher.Extensions;
using MrSquashWatcher.Views;
using Prism.Ioc;
using Serilog;
using Squirrel;
using System.Windows;

namespace MrSquashWatcher;

public partial class App
{
    private static Mutex _mutex = null;

    public static TaskbarIcon TaskBarIcon { get; private set; }

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

        UserSettings.Instance.Load();

        base.OnStartup(e);
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
        containerRegistry.Register<IStartupService, StartupService>();
        containerRegistry.Register<IFamulusService, FamulusService>();
        containerRegistry.RegisterSingleton<IGamesManager, GamesManager>();
    }

    protected override void Initialize()
    {
        Log.Information("Initializing application");

        base.Initialize();

        var gamesManager = Container.Resolve<IGamesManager>();
        gamesManager.Start();
    }

    private void OnApplicationExit(object sender, ExitEventArgs e)
    {
        Log.Information("Exiting application");

        var gamesManager = Container.Resolve<IGamesManager>();
        gamesManager.Stop();

        UserSettings.Instance.Save();

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
        tools.SetProcessAppUserModelId();
    }
}
