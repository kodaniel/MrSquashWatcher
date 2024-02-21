using Microsoft.Toolkit.Uwp.Notifications;
using MrSquashWatcher.Properties;

namespace MrSquashWatcher.Toasts;

public class GameFreedToastFactory
{
    private readonly Func<Game, string> _titleFactory;

    public GameFreedToastFactory(Func<Game, string> titleFactory)
    {
        _titleFactory = titleFactory ?? throw new ArgumentNullException(nameof(titleFactory));
    }

    public ToastContentBuilder Create(List<Game> games)
    {
        ArgumentNullException.ThrowIfNull(games);

        if (games.Count == 0)
            throw new InvalidOperationException("Collection can not be empty.");

        var toastBuilder = new ToastContentBuilder()
            .AddButton(new ToastButton()
                .SetContent(Localization.Cancel)
                .SetDismissActivation())
            .AddButton(new ToastButton()
                .SetContent(Localization.Reserve)
                .AddArgument("action", "reserve")
                .SetBackgroundActivation())
            .SetToastScenario(ToastScenario.Reminder);

        if (games.Count == 1)
        {
            toastBuilder
                .AddArgument("toastId", "game-freed")
                .AddText(Localization.GameFreed)
                .AddText(_titleFactory(games[0]))
                .AddArgument("game", JsonConvert.SerializeObject(games[0]));
        }
        else
        {
            var choices = games.Take(5).Select((g, i) => (i.ToString(), _titleFactory(g))).ToList();
            for (int i = 0; i < choices.Count; i++)
                toastBuilder.AddArgument(i.ToString(), JsonConvert.SerializeObject(games[i]));

            toastBuilder
                .AddArgument("toastId", "games-freed")
                .AddText(string.Join(Localization.GamesFreed, games.Count))
                .AddComboBox("selected", Localization.SelectGame, "0", choices);
        }

        return toastBuilder;
    }
}
