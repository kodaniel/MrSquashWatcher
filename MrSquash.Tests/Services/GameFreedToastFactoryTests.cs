using MrSquashWatcher.Toasts;

namespace MrSquash.Tests.Services;

[TestFixture]
public class GameFreedToastFactoryTests
{
    [Test]
    public void Test_Initialization()
    {
        var factory = new GameFreedToastFactory(_ => "title");

        Assert.IsNotNull(factory);
        Assert.Throws<ArgumentNullException>(() => new GameFreedToastFactory(null));
    }

    [Test]
    public void Test_CreateToast_WithNull()
    {
        var factory = new GameFreedToastFactory(game => game.Date.ToString("yyyy-MM-dd"));

        Assert.Throws<ArgumentNullException>(() => factory.Create(null));
    }

    [Test]
    public void Test_CreateToast_WithNoGame()
    {
        List<Game> games = new();
        var factory = new GameFreedToastFactory(game => game.Date.ToString("yyyy-MM-dd"));

        Assert.Throws< InvalidOperationException>(() => factory.Create(games));
    }

    [Test]
    public void Test_CreateToast_WithOneGame()
    {
        List<Game> games = new()
        {
            CreateGame(2024, 1, 1, 8)
        };

        var factory = new GameFreedToastFactory(game => game.Date.ToString("yyyy-MM-dd"));
        var toastBuilder = factory.Create(games);

        Assert.IsNotNull(toastBuilder);

        var content = toastBuilder.GetToastContent();

        Assert.IsNotNull(content);
        Assert.That(content.Launch, Is.EqualTo(
            "toastId=game-freed;" +
            "game={\"CalendarPosition\":{\"Row\":1,\"Column\":1},\"Date\":\"2024-01-01\",\"StartTime\":\"08:00:00\",\"EndTime\":\"09:00:00\",\"Track\":1,\"Reserved\":false,\"Enabled\":true,\"Price\":1000}"));
    }

    [Test]
    public void Test_CreateToast_WithMoreGames()
    {
        List<Game> games = new()
        {
            CreateGame(2024, 1, 1, 8),
            CreateGame(2024, 1, 2, 8),
            CreateGame(2024, 1, 3, 8)
        };

        var factory = new GameFreedToastFactory(game => game.Date.ToString("yyyy-MM-dd"));
        var toastBuilder = factory.Create(games);

        Assert.IsNotNull(toastBuilder);

        var content = toastBuilder.GetToastContent();

        Assert.IsNotNull(content);
        Assert.That(content.Launch, Is.EqualTo(
            "0={\"CalendarPosition\":{\"Row\":1,\"Column\":1},\"Date\":\"2024-01-01\",\"StartTime\":\"08:00:00\",\"EndTime\":\"09:00:00\",\"Track\":1,\"Reserved\":false,\"Enabled\":true,\"Price\":1000};" +
            "1={\"CalendarPosition\":{\"Row\":1,\"Column\":1},\"Date\":\"2024-01-02\",\"StartTime\":\"08:00:00\",\"EndTime\":\"09:00:00\",\"Track\":1,\"Reserved\":false,\"Enabled\":true,\"Price\":1000};" +
            "2={\"CalendarPosition\":{\"Row\":1,\"Column\":1},\"Date\":\"2024-01-03\",\"StartTime\":\"08:00:00\",\"EndTime\":\"09:00:00\",\"Track\":1,\"Reserved\":false,\"Enabled\":true,\"Price\":1000};" +
            "toastId=games-freed"));
    }

    private static Game CreateGame(int year, int month, int day, int hour) =>
        new()
        {
            Date = new DateOnly(year, month, day),
            StartTime = new TimeOnly(hour, 0),
            EndTime = new TimeOnly(hour + 1, 0),
            CalendarPosition = new CalendarPosition(1, 1),
            Enabled = true,
            Price = 1000,
            Reserved = false,
            Track = 1
        };
}
