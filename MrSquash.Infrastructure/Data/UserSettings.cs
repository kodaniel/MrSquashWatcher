using Newtonsoft.Json;

namespace MrSquash.Infrastructure.Data;

public class UserSettings
{
    public const double MIN_WATCH_WEEKS = 1;
    public const double MAX_WATCH_WEEKS = 4;

    private static Lazy<UserSettings> _instance = new Lazy<UserSettings>(() => new UserSettings());
    public static UserSettings Instance => _instance.Value;

    private UserSettings()
    {
    }

    private HashSet<CalendarPosition> _selectedGrids = new();

    public string Name
    {
        get => AppUser.Default.Name;
        set => AppUser.Default.Name = value;
    }

    public string Email
    {
        get => AppUser.Default.Email;
        set => AppUser.Default.Email = value;
    }

    public string Phone
    {
        get => AppUser.Default.Phone;
        set => AppUser.Default.Phone = value;
    }

    public int NumOfWeeks
    {
        get => AppUser.Default.NumOfWeeks;
        set => AppUser.Default.NumOfWeeks = Math.Clamp(value, 1, 4);
    }

    public bool ShowNotifications
    {
        get => AppUser.Default.ShowNotifications;
        set => AppUser.Default.ShowNotifications = value;
    }

    public void SetUser(string name, string email, string phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public void Load()
    {
        _selectedGrids = JsonConvert.DeserializeObject<HashSet<CalendarPosition>>(AppUser.Default.SelectedGames) ?? new();
    }

    public void Save()
    {
        string output = JsonConvert.SerializeObject(_selectedGrids);
        AppUser.Default.SelectedGames = output;
        AppUser.Default.Save();
    }

    public bool IsSelected(CalendarPosition calendarPosition)
    {
        return _selectedGrids.Contains(calendarPosition);
    }

    public void SetSelected(CalendarPosition calendarPosition, bool value)
    {
        if (value)
            _selectedGrids.Add(calendarPosition);
        else
            _selectedGrids.Remove(calendarPosition);
    }
}
