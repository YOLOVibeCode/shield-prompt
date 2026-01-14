using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Infrastructure.Persistence;

namespace ShieldPrompt.Tests.Unit.Infrastructure;

public class SettingsRepositoryTests : IDisposable
{
    private readonly ISettingsRepository _sut;
    private readonly string _testSettingsPath;

    public SettingsRepositoryTests()
    {
        _testSettingsPath = Path.Combine(Path.GetTempPath(), $"ShieldPromptSettings_{Guid.NewGuid()}.json");
        _sut = new SettingsRepository(_testSettingsPath);
    }

    public void Dispose()
    {
        if (File.Exists(_testSettingsPath))
            File.Delete(_testSettingsPath);
    }

    [Fact]
    public async Task LoadAsync_WithNoFile_ReturnsDefaultSettings()
    {
        // Act
        var result = await _sut.LoadAsync();

        // Assert
        result.Should().NotBeNull();
        result.LastFolderPath.Should().BeNull();
        result.LastFormatName.Should().BeNull();
        result.Window.Width.Should().Be(1200);
        result.Window.Height.Should().Be(700);
    }

    [Fact]
    public async Task SaveAsync_WithSettings_CreatesFile()
    {
        // Arrange
        var settings = new AppSettings
        {
            LastFolderPath = "/test/path",
            LastFormatName = "Markdown",
            LastModelName = "gpt-4o"
        };

        // Act
        await _sut.SaveAsync(settings);

        // Assert
        File.Exists(_testSettingsPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new AppSettings
        {
            LastFolderPath = "/my/project",
            LastFormatName = "XML",
            LastModelName = "claude-3.5-sonnet",
            LastSelectedFiles = new[] { "/file1.cs", "/file2.cs" },
            Window = new WindowSettings
            {
                Left = 150,
                Top = 200,
                Width = 1400,
                Height = 800,
                IsMaximized = true
            }
        };

        // Act
        await _sut.SaveAsync(original);
        var loaded = await _sut.LoadAsync();

        // Assert
        loaded.LastFolderPath.Should().Be(original.LastFolderPath);
        loaded.LastFormatName.Should().Be(original.LastFormatName);
        loaded.LastModelName.Should().Be(original.LastModelName);
        loaded.LastSelectedFiles.Should().BeEquivalentTo(original.LastSelectedFiles);
        loaded.Window.Left.Should().Be(original.Window.Left);
        loaded.Window.Width.Should().Be(original.Window.Width);
        loaded.Window.IsMaximized.Should().Be(original.Window.IsMaximized);
    }

    [Fact]
    public async Task SaveAsync_MultipleTimes_OverwritesPrevious()
    {
        // Arrange
        var first = new AppSettings { LastFolderPath = "/first" };
        var second = new AppSettings { LastFolderPath = "/second" };

        // Act
        await _sut.SaveAsync(first);
        await _sut.SaveAsync(second);
        var loaded = await _sut.LoadAsync();

        // Assert
        loaded.LastFolderPath.Should().Be("/second");
    }

    [Fact]
    public async Task LoadAsync_WithCorruptFile_ReturnsDefaults()
    {
        // Arrange
        await File.WriteAllTextAsync(_testSettingsPath, "{ invalid json !!!!");

        // Act
        var result = await _sut.LoadAsync();

        // Assert
        result.Should().NotBeNull();
        result.LastFolderPath.Should().BeNull(); // Defaults returned
    }
}

