using H.NotifyIcon;
using Prism.Ioc;
using System.Threading;
using System.Windows;

namespace MrSquashWatcher
{
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

            UserSettings.Instance.Load();

            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            TaskBarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
            TaskBarIcon.ForceCreate();
            TaskBarIcon.DataContext = Container.Resolve<MainViewModel>();

            return null;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Views and Viewmodels
            containerRegistry.Register<MainViewModel>();
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            UserSettings.Instance.Save();

            TaskBarIcon?.Dispose();
        }
    }
}
