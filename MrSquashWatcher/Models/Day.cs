namespace MrSquashWatcher.Models;

public record Day
{
    public DateTime Date { get; set; }
    public List<Track> Tracks { get; set; }
}
