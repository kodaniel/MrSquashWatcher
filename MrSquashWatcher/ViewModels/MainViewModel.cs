using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Win32;
using MrSquash.Core;
using MrSquash.Core.Events;
using MrSquash.Services.Famulus;
using MrSquashWatcher.Properties;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace MrSquashWatcher
{
    public class MainViewModel : BindableBase
    {
        public static string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
        private const string APP_KEY = "MrSquashWatcher";

        private readonly IEventAggregator _eventAggregator;
        private readonly DispatcherTimer _refreshTimer;
        private readonly string FamulusSquashURL = "https://famulushotel.hu/palyafoglalas/squash";
        private readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);

        private ObservableCollection<GameMatch> _games;
        public ObservableCollection<GameMatch> Games =>
            _games ?? (_games = new ObservableCollection<GameMatch>());

        private DelegateCommand _refreshCommand;
        public DelegateCommand RefreshCommand =>
            _refreshCommand ?? (_refreshCommand = new DelegateCommand(ExecuteRefreshCommand, CanExecuteRefreshCommand));

        private DelegateCommand<GameMatch> _watchingChangedCommand;
        public DelegateCommand<GameMatch> WatchingChangedCommand =>
            _watchingChangedCommand ?? (_watchingChangedCommand = new DelegateCommand<GameMatch>(gm =>
            {
                UserSettings.Instance.SetWatching(gm.Row, gm.Column, gm.Watching);
            }));

        private DelegateCommand _exitCommand;
        public DelegateCommand ExitCommand =>
            _exitCommand ?? (_exitCommand = new DelegateCommand(ExecuteExitCommand));

        private DelegateCommand _openPopupCommand;
        public DelegateCommand OpenPopupCommand => _openPopupCommand ?? (_openPopupCommand = new DelegateCommand(ExecuteOpenPopupCommand));

        private bool _refresing;
        public bool Refreshing
        {
            get => _refresing;
            set
            {
                if (SetProperty(ref _refresing, value))
                    RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        public bool AutoStartupApplication
        {
            get => IsRunApplicationOnStartup();
            set
            {
                if (value)
                    AddApplicationToStartup();
                else
                    RemoveApplicationFromStartup();
            }
        }

        private int _week;
        public int Week
        {
            get => _week;
            set => SetProperty(ref _week, value);
        }

        public MainViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _refreshTimer = new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.Background, OnRefresh, Application.Current.Dispatcher);
            _refreshTimer.Start();

            _eventAggregator.GetEvent<GameChagedEvent>().Subscribe(gm =>
            {
                try
                {
                    var culture = new CultureInfo("hu-HU");
                    var info = culture.DateTimeFormat;
                    var msg = $"{info.DayNames[(int)gm.StartTime.DayOfWeek].FirstCharToUpper()}, {gm.StartTime.ToString("HH")} óra";

                    App.TaskBarIcon.ShowNotification("Pálya felszabadult", msg, NotificationIcon.Info);
                    App.TaskBarIcon.Icon = Resources.icon_active;
                }
                catch
                {
                }
            });

            _eventAggregator.GetEvent<ErrorEvent>().Subscribe(ex =>
            {
                App.TaskBarIcon.ShowNotification("Hiba", ex.Message, NotificationIcon.Error);
            });

            App.TaskBarIcon.Icon = Resources.icon;
        }

        private async void LoadLayout()
        {
            if (Refreshing)
                return;

            Refreshing = true;
            _refreshTimer.Stop();

            var fs = new FamulusService();
            var days = await fs.LoadDays(FamulusSquashURL);

            if (days != null)
            {
                int row = 0, col = 0;

                foreach (Day day in days)
                {
                    foreach (Track track in day.Tracks)
                    {
                        col = 0;
                        foreach (Reservation r in track.Times)
                        {
                            GameMatch gm = GetGameFromGrid(row, col);
                            if (gm is null)
                            {
                                gm = new GameMatch();
                                gm.StartTime = day.Date.Add(r.StartTime);
                                gm.EndTime = day.Date.Add(r.EndTime);
                                gm.Busy = r.Busy;
                                gm.Enabled = r.Enabled;
                                gm.Row = row;
                                gm.Column = col;
                                gm.Watching = UserSettings.Instance.IsWatching(row, col);

                                Games.Add(gm);
                            }
                            else
                            {
                                if (gm.Enabled && gm.Busy && !r.Busy) // pálya felszabadult
                                {
                                    if (gm.Watching)
                                        _eventAggregator.GetEvent<GameChagedEvent>().Publish(gm);
                                }

                                gm.Busy = r.Busy;
                                gm.Enabled = r.Enabled;
                            }

                            col++;
                        }

                        row++;
                    }
                }

                if (Games.Count > 0)
                {
                    CultureInfo cul = CultureInfo.CurrentCulture;
                    Week = cul.Calendar.GetWeekOfYear(Games.First().StartTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                }
            }
            else
            {
                // Connection error or something else, but failed to refresh
                _eventAggregator.GetEvent<ErrorEvent>().Publish(new InvalidOperationException("Nem sikerült letölteni a pályafoglaltságot."));
            }

            Refreshing = false;
            _refreshTimer.Interval = RefreshInterval;
            _refreshTimer.Start();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            Debug.WriteLine("Auto refresh");
            LoadLayout();
        }

        private void ExecuteRefreshCommand()
        {
            LoadLayout();
        }

        private bool CanExecuteRefreshCommand()
        {
            return !Refreshing;
        }

        private void ExecuteOpenPopupCommand()
        {
            App.TaskBarIcon.Icon = Resources.icon;
        }

        private void ExecuteExitCommand()
        {
            Application.Current.Shutdown();
        }

        private GameMatch GetGameFromGrid(int row, int column)
        {
            if (Games.Count == 0)
                return null;

            return Games.FirstOrDefault(x => x.Row == row && x.Column == column);
        }

        public static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(APP_KEY, ExecutablePath);
            }
        }

        public static void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(APP_KEY, false);
            }
        }

        public static bool IsRunApplicationOnStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key.GetValue(APP_KEY) is null)
                    return false;
                else
                    return true;
            }
        }
    }
}
