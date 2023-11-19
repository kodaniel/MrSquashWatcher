namespace MrSquashWatcher.Data;

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
        get => user.Default.Name;
        set => user.Default.Name = value;
    }

    public string Email
    {
        get => user.Default.Email;
        set => user.Default.Email = value;
    }

    public string Phone
    {
        get => user.Default.Phone;
        set => user.Default.Phone = value;
    }

    public int NumOfWeeks
    {
        get => user.Default.NumOfWeeks;
        set => user.Default.NumOfWeeks = Math.Clamp(value, 1, 4);
    }

    public void SetUser(string name, string email, string phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public void Load()
    {
        _selectedGrids = JsonConvert.DeserializeObject<HashSet<CalendarPosition>>(user.Default.SelectedGames) ?? new();
    }

    public void Save()
    {
        string output = JsonConvert.SerializeObject(_selectedGrids);
        user.Default.SelectedGames = output;
        user.Default.Save();
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
