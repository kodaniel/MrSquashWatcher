namespace MrSquashWatcher.Models;

public record Track
{
    public string Name { get; set; }
    public List<Appointment> Times { get; set; }
}
