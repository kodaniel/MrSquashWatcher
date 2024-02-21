using Microsoft.Extensions.Logging;
using MrSquash.Domain;
using MrSquash.Infrastructure.Services;
using MrSquash.Tests.Services.Mocks;
using MrSquashWatcher.ViewModels;
using Prism.Events;
using Prism.Services.Dialogs;

namespace MrSquash.Tests.ViewModels;

[TestFixture]
public class ReservationViewModelTests
{
    private Mock<IUserSettings> _userSettingsMock;

    [SetUp]
    public void Setup()
    {
        _userSettingsMock = new Mock<IUserSettings>();
        _userSettingsMock.Setup(x => x.Name).Returns("name");
        _userSettingsMock.Setup(x => x.Email).Returns("email@email.com");
        _userSettingsMock.Setup(x => x.Phone).Returns("123456");
    }

    [Test]
    public void NameEmailAndPhone_SetValue()
    {
        // Arrange
        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), Mock.Of<IUserSettings>(), Mock.Of<ILogger<ReservationViewModel>>());
        using var propertyChangedObserver = new PropertyChangedObserver<ReservationViewModel>(viewModel);

        // Act
        viewModel.Name = "name";
        viewModel.Email = "email@email.com";
        viewModel.Phone = "123456";

        // Assert
        Assert.IsTrue(propertyChangedObserver.IsDirty);
        Assert.That(viewModel.Name, Is.EqualTo("name"));
        Assert.That(viewModel.Email, Is.EqualTo("email@email.com"));
        Assert.That(viewModel.Phone, Is.EqualTo("123456"));
    }

    [Test]
    public void OpenDialog_WithoutArgument()
    {
        // Arrange
        var parameters = new DialogParameters();
        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), Mock.Of<IUserSettings>(), Mock.Of<ILogger<ReservationViewModel>>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => viewModel.OnDialogOpened(parameters));
    }

    [Test]
    public void Appointment_GetValue()
    {
        // Arrange
        var game = CreateDefaultGame();
        var parameters = new DialogParameters()
        {
            { "game", game },
        };

        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), Mock.Of<IUserSettings>(), Mock.Of<ILogger<ReservationViewModel>>());

        // Act
        viewModel.OnDialogOpened(parameters);

        // Assert
        Assert.That(viewModel.Appointment, Is.EqualTo("2024.01.01 Hétfő 08:00-09:00 1. pálya"));
    }

    [Test]
    public void NameEmailAndPhone_GetValue_From_UserSettings()
    {
        // Arrange
        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), _userSettingsMock.Object, Mock.Of<ILogger<ReservationViewModel>>());
        var game = CreateDefaultGame();
        var parameters = new DialogParameters() { { "game", game } };

        // Act
        viewModel.OnDialogOpened(parameters);

        // Assert
        Assert.That(viewModel.Name, Is.EqualTo("name"));
        Assert.That(viewModel.Email, Is.EqualTo("email@email.com"));
        Assert.That(viewModel.Phone, Is.EqualTo("123456"));
    }

    [Test]
    public void CanCloseDialog()
    {
        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), Mock.Of<IUserSettings>(), Mock.Of<ILogger<ReservationViewModel>>());
        Assert.That(viewModel.CanCloseDialog(), Is.True);
    }

    [Test]
    public void CanReserve()
    {
        var game = CreateDefaultGame();
        var parameters = new DialogParameters() { { "game", game } };
        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), _userSettingsMock.Object, Mock.Of<ILogger<ReservationViewModel>>());

        viewModel.Name = "";
        viewModel.Email = "";
        viewModel.Phone = "";

        Assert.IsFalse(viewModel.ReserveCommand.CanExecute());

        viewModel.Name = "name";
        viewModel.Email = "";
        viewModel.Phone = "";

        Assert.IsFalse(viewModel.ReserveCommand.CanExecute());

        viewModel.Name = "name";
        viewModel.Email = "email@email.com";
        viewModel.Phone = "";

        Assert.IsFalse(viewModel.ReserveCommand.CanExecute());

        viewModel.Name = "name";
        viewModel.Email = "email@email.com";
        viewModel.Phone = "123456";

        Assert.IsFalse(viewModel.ReserveCommand.CanExecute());

        viewModel.OnDialogOpened(parameters);

        Assert.IsTrue(viewModel.ReserveCommand.CanExecute());
    }

    [Test]
    public async Task Reserve()
    {
        // Assign
        var famulusServiceMock = new Mock<IFamulusService>();
        famulusServiceMock.Setup(x => x.Reserve(It.IsAny<Reservation>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        IDialogResult dialogResult = null!;
        var game = CreateDefaultGame();
        var parameters = new DialogParameters() { { "game", game } };
        var viewModel = new ReservationViewModel(famulusServiceMock.Object, _userSettingsMock.Object, Mock.Of<ILogger<ReservationViewModel>>());
        viewModel.RequestClose += (e) => dialogResult = e;
        
        // Act
        viewModel.OnDialogOpened(parameters);
        viewModel.ReserveCommand.Execute();

        // Assert
        famulusServiceMock.Verify(x => x.Reserve(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(viewModel.IsSubmitting, Is.True);
        Assert.That(dialogResult, Is.Null);

        await Task.Delay(2500);

        Assert.That(viewModel.IsSubmitting, Is.False);
        Assert.That(dialogResult, Is.Not.Null);
    }

    [Test]
    public void DialogClose_Save_UserSettings()
    {
        // Assign
        var userSettingsMock = new Mock<IUserSettings>();
        var viewModel = new ReservationViewModel(Mock.Of<IFamulusService>(), userSettingsMock.Object, Mock.Of<ILogger<ReservationViewModel>>());

        // Act
        viewModel.Name = "name";
        viewModel.Email = "email@email.com";
        viewModel.Phone = "123456";

        viewModel.OnDialogClosed();

        // Assert
        userSettingsMock.Verify(x => x.SetUser(viewModel.Name, viewModel.Email, viewModel.Phone), Times.Once);
    }

    private static Game CreateDefaultGame() => new()
    {
        Date = new DateOnly(2024, 1, 1),
        StartTime = new TimeOnly(8, 0),
        EndTime = new TimeOnly(9, 0),
        Track = 0,
        Enabled = true,
        Reserved = false
    };
}
