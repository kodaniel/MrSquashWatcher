using Microsoft.Extensions.Logging;
using MrSquash.Infrastructure.Services;
using MrSquash.Tests.Services.Mocks;
using Prism.Events;

namespace MrSquash.Tests.Services;

public class GameManagerTests
{
    private GamesManager _gamesManager;

    [SetUp]
    public void Setup()
    {
        var logger = new Logger<GamesManager>(LoggerFactory.Create(options => { }));
        var eventAggregator = new EventAggregator();
        var famulusService = new FamulusServiceMock();

        _gamesManager = new GamesManager(eventAggregator, famulusService, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _gamesManager.Dispose();
    }

    [Test]
    public void Test_GetGamesOnWeek_NoCachedGames()
    {
        // Assign & Act
        var games = _gamesManager.GetGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task Test_GetGamesOnWeek()
    {
        // Assign - Fill the cache
        await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now);
        
        // Act
        var games = _gamesManager.GetGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(210));
    }

    [Test]
    public async Task Test_GetOrUpdateGamesOnWeek()
    {
        // Assign & Act
        var games = await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(210));
    }
}