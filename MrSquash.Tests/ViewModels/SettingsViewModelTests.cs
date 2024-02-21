using MrSquash.Application;
using MrSquashWatcher.ViewModels;

namespace MrSquash.Tests.ViewModels;

[TestFixture]
public class SettingsViewModelTests
{
    [Test]
    public void WatchWeeks_SetValue()
    {
        // Arrange
        var viewModel = new SettingsViewModel(Mock.Of<IStartupService>(), Mock.Of<IUserSettings>());
        using var propertyChangedObserver = new PropertyChangedObserver<SettingsViewModel>(viewModel);

        // Act
        viewModel.WatchWeeks = 5;

        // Assert
        Assert.IsTrue(propertyChangedObserver.IsDirty);
        Assert.That(viewModel.WatchWeeks, Is.EqualTo(5));
    }

    [Test]
    public void ShowNotifications_SetValue()
    {
        // Arrange
        var viewModel = new SettingsViewModel(Mock.Of<IStartupService>(), Mock.Of<IUserSettings>());
        using var propertyChangedObserver = new PropertyChangedObserver<SettingsViewModel>(viewModel);

        // Act
        viewModel.ShowNotifications = true;

        // Assert
        Assert.IsTrue(propertyChangedObserver.IsDirty);
        Assert.IsTrue(viewModel.ShowNotifications);
    }

    [Test]
    public void AutoStartupApplication_SetValue()
    {
        // Arrange
        var mockStartupService = new Mock<IStartupService>();
        var viewModel = new SettingsViewModel(mockStartupService.Object, Mock.Of<IUserSettings>());
        using var propertyChangedObserver = new PropertyChangedObserver<SettingsViewModel>(viewModel);

        // Act
        viewModel.AutoStartupApplication = true;

        // Assert
        Assert.IsTrue(propertyChangedObserver.IsDirty);
        mockStartupService.Verify(s => s.SetApplicationStartup(true), Times.Once);
    }

    [Test]
    public void SelectedTheme_SetValue()
    {
        // Arrange
        var viewModel = new SettingsViewModel(Mock.Of<IStartupService>(), Mock.Of<IUserSettings>());
        using var propertyChangedObserver = new PropertyChangedObserver<SettingsViewModel>(viewModel);

        // Act
        viewModel.SelectedTheme = AppThemes.Dark;

        // Assert
        Assert.IsTrue(propertyChangedObserver.IsDirty);
        Assert.That(viewModel.SelectedTheme, Is.EqualTo(AppThemes.Dark));
    }
}
