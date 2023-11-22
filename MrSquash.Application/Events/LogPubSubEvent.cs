using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Ioc;

namespace MrSquash.Application.Events;

public class LogPubSubEvent<TPayload> : PubSubEvent<TPayload>
{
    private readonly ILogger<LogPubSubEvent<TPayload>> _logger;

    public LogPubSubEvent()
    {
        _logger = ContainerLocator.Container.Resolve<ILogger<LogPubSubEvent<TPayload>>>();
    }

    public override void Publish(TPayload payload)
    {
        var payloadJson = JsonConvert.SerializeObject(payload);
        var eventName = GetType().Name;

        _logger.LogDebug("Publishing event '{eventName}' {payloadJson}", eventName, payloadJson);

        base.Publish(payload);
    }
}
