using Prism.Commands;

namespace MrSquashWatcher;

public static class ApplicationCommands
{
    public static CompositeCommand ExitCommand = new CompositeCommand();
    public static CompositeCommand SettingsCommand = new CompositeCommand();
}
