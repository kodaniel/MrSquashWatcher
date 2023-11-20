namespace MrSquash.Application.Contracts;

public interface IStartupService
{
    void AddApplicationToStartup();
    void RemoveApplicationFromStartup();
    void SetApplicationStartup(bool start);
    bool IsRunApplicationOnStartup();
}
