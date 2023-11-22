using Microsoft.Extensions.Logging;
using Prism.Ioc;
using Serilog;

namespace MrSquashWatcher.Extensions;

internal static class ContainerExtensions
{
    public static IContainerRegistry RegisterSerilog(this IContainerRegistry containerRegistry, Serilog.ILogger logger, bool dispose = false)
    {
        ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(logger, dispose);

        containerRegistry.RegisterInstance(loggerFactory);
        containerRegistry.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
        return containerRegistry;
    }
}
