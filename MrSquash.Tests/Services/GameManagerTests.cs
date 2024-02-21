using Microsoft.Extensions.Logging;
using MrSquash.Application.Events;
using MrSquash.Infrastructure.Services;
using MrSquash.Tests.Services.Mocks;
using Prism.Events;

namespace MrSquash.Tests.Services;

[TestFixture]
public class GameManagerTests
{
    private GamesManager _gamesManager;
    private FamulusServiceMock _famulusService;
    private Mock<IEventAggregator> _eventAggregatorMock;

    [SetUp]
    public void Setup()
    {
        var logger = new Logger<GamesManager>(LoggerFactory.Create(options => { }));
        var userSettings = Mock.Of<IUserSettings>();
        userSettings.NumOfWeeks = 1;

        _eventAggregatorMock = new Mock<IEventAggregator>();
        _famulusService = new FamulusServiceMock();
        _gamesManager = new GamesManager(_eventAggregatorMock.Object, userSettings, _famulusService, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _gamesManager.Dispose();
    }

    [Test]
    public void GetGamesOnWeek_NoCachedGames()
    {
        // Assign & Act
        var games = _gamesManager.GetGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.Zero);
    }

    [Test]
    public async Task GetGamesOnWeek_HasCachedGames()
    {
        // Assign
        await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now); // Fill the cache

        // Act
        var games = _gamesManager.GetGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(210));
    }

    [Test]
    public async Task GetOrUpdateGamesOnWeek_Succeed()
    {
        // Assign & Act
        var games = await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(210));
    }

    [Test]
    public async Task GetOrUpdateGamesOnWeek_CancelAsync()
    {
        // Assign
        IEnumerable<Game> games = null!;
        var cts = new CancellationTokenSource();
        _famulusService.SetResponseTime(1000); // Set 1 second response time

        // Act
        _ = Task.Run(async () =>
        {
            games = await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now, cancellationToken: cts.Token);
        }).ContinueWith(t =>
        {
            // Assert
            Assert.NotNull(games);
            Assert.That(games.Count(), Is.Zero);
        });

        await Task.Delay(100);
        cts.Cancel();
    }

    [Test]
    public async Task GetOrUpdateGamesOnWeek_GamesUpdated()
    {
        // Assign
        var mockedEvent = new Mock<GameUpdatedEvent>();
        _eventAggregatorMock.Setup(x => x.GetEvent<GameUpdatedEvent>()).Returns(mockedEvent.Object);

        // Act
        await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now);
        var games = await _gamesManager.GetOrUpdateGamesOnWeek(Week.Now, forceUpdate: true);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(210));
        mockedEvent.Verify(x => x.Publish(It.IsAny<GameUpdatedEventArgs>()), Times.Once);
    }

    [Test]
    public async Task UpdateWeeks_Succeeded()
    {
        // Assign
        var mockedEvent = new Mock<GameUpdatedEvent>();
        _eventAggregatorMock.Setup(x => x.GetEvent<GameUpdatedEvent>()).Returns(mockedEvent.Object);

        // Act
        await _gamesManager.UpdateWeeks();
        var games = _gamesManager.GetGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.EqualTo(210));
        mockedEvent.Verify(x => x.Publish(It.IsAny<GameUpdatedEventArgs>()), Times.Once);
    }

    [Test]
    public async Task UpdateWeeks_DownloadFailed()
    {
        // Assign
        var mockedEvent = new Mock<GameUpdatedEvent>();
        _eventAggregatorMock.Setup(x => x.GetEvent<GameUpdatedEvent>()).Returns(mockedEvent.Object);
        _famulusService.PopulateWeek(Week.Now, new List<Day>());

        // Act
        await _gamesManager.UpdateWeeks();
        var games = _gamesManager.GetGamesOnWeek(Week.Now);

        // Assert
        Assert.NotNull(games);
        Assert.That(games.Count(), Is.Zero);
        mockedEvent.Verify(x => x.Publish(It.IsAny<GameUpdatedEventArgs>()), Times.Exactly(2));
    }

    [Test]
    public async Task UpdateWeeks_CancelAsync()
    {
        // Assign
        IEnumerable<Game> games = null!;
        var cts = new CancellationTokenSource();
        _famulusService.SetResponseTime(1000);

        var mockedEvent = new Mock<GameUpdatedEvent>();
        _eventAggregatorMock.Setup(x => x.GetEvent<GameUpdatedEvent>()).Returns(mockedEvent.Object);

        // Act
        _ = Task.Run(async () =>
        {
            await _gamesManager.UpdateWeeks(cts.Token);
            var games = _gamesManager.GetGamesOnWeek(Week.Now);
        }).ContinueWith(t =>
        {
            // Assert
            Assert.NotNull(games);
            Assert.That(games.Count(), Is.Zero);
            mockedEvent.Verify(x => x.Publish(It.IsAny<GameUpdatedEventArgs>()), Times.Never);
        });

        await Task.Delay(100);
        cts.Cancel();
    }
}