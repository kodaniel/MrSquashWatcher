using Prism.Events;

namespace MrSquashWatcher.Contracts.Events;

public class ErrorEvent : PubSubEvent<Exception>
{
}
