using MrSquashWatcher.ViewModels;

namespace MrSquash.Tests.ViewModels;

[TestFixture]
public class GameViewModelTests
{
    [Test]
    public void Instantiate()
    {
        Game game = new()
        {
            CalendarPosition = new CalendarPosition(1, 3),
            Date = new DateOnly(2024, 1, 1),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(9, 30),
            Enabled = true,
            Price = 1000,
            Reserved = true,
            Track = 2
        };

        var viewmodel = new GameViewModel(game, Mock.Of<IUserSettings>());
        using var viewmodelObserver = new PropertyChangedObserver<GameViewModel>(viewmodel);

        Assert.That(viewmodel, Is.Not.Null);
        Assert.That(viewmodel.Row, Is.EqualTo(game.CalendarPosition.Row));
        Assert.That(viewmodel.Column, Is.EqualTo(game.CalendarPosition.Column));
        Assert.That(viewmodel.Enabled, Is.EqualTo(game.Enabled));
        Assert.That(viewmodel.Date, Is.EqualTo(game.Date));
        Assert.That(viewmodel.StartTime, Is.EqualTo(game.StartTime));
        Assert.That(viewmodel.EndTime, Is.EqualTo(game.EndTime));
        Assert.That(viewmodel.Reserved, Is.EqualTo(game.Reserved));
        Assert.That(viewmodel.Track, Is.EqualTo(game.Track));

        Assert.That(viewmodel.GetModel(), Is.EqualTo(game));
        Assert.That(viewmodelObserver.IsDirty, Is.False);

        viewmodel.Selected = true;

        Assert.That(viewmodelObserver.IsDirty, Is.True);
    }
}
