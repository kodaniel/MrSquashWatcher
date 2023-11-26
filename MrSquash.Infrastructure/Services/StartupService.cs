using Microsoft.Win32;
using System.Diagnostics;

namespace MrSquash.Infrastructure.Services;

public class StartupService : IStartupService
{
    private const string APP_KEY = "MrSquashWatcher";
    public static string ExecutablePath = Process.GetCurrentProcess().MainModule!.FileName;

    public void AddApplicationToStartup()
    {
        using (var regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!)
        {
            regkey.SetValue(APP_KEY, ExecutablePath);
        }
    }

    public void RemoveApplicationFromStartup()
    {
        using (var regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!)
        {
            regkey.DeleteValue(APP_KEY, false);
        }
    }

    public void SetApplicationStartup(bool start)
    {
        if (start)
            AddApplicationToStartup();
        else
            RemoveApplicationFromStartup();
    }

    public bool IsRunApplicationOnStartup()
    {
        using (var regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!)
        {
            if (regkey.GetValue(APP_KEY) is null)
                return false;
            else
                return true;
        }
    }
}
