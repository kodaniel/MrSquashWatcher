namespace MrSquash.Application.Contracts;

public interface IUserSettings
{
    AppThemes ApplicationTheme { get; set; }
    string Email { get; set; }
    string Name { get; set; }
    int NumOfWeeks { get; set; }
    string Phone { get; set; }
    bool ShowNotifications { get; set; }

    void Load();
    void Save();

    IEnumerable<CalendarPosition> GetAllSelected();
    bool IsSelected(CalendarPosition calendarPosition);
    void SetSelected(CalendarPosition calendarPosition, bool value);
    void SetUser(string name, string email, string phone);
}
