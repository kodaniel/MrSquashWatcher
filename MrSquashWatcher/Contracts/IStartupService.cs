namespace MrSquashWatcher.Contracts;

public interface IStartupService
{
    void AddApplicationToStartup();
    void RemoveApplicationFromStartup();
    bool IsRunApplicationOnStartup();
}
