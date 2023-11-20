namespace MrSquash.Domain;

public class Game
{
    public CalendarPosition CalendarPosition { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Track { get; set; }
    public bool Reserved { get; set; }
    public bool Enabled { get; set; }
}
