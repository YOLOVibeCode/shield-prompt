using FluentAssertions;
using NSubstitute;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class WorkspaceSettingsServiceTests
{
    private readonly IWorkspaceRepository _repository;
    private readonly WorkspaceSettingsService _sut;

    public WorkspaceSettingsServiceTests()
    {
        _repository = Substitute.For<IWorkspaceRepository>();
        _sut = new WorkspaceSettingsService(_repository);
    }

    [Fact]
    public async Task GetSettingsAsync_WithExistingWorkspace_ReturnsSettings()
    {
        // Arrange
        var workspace = new Workspace
        {
            Id = "ws-123",
            Name = "Test",
            RootPath = "/test",
            Settings = new WorkspaceSettings
            {
                SanitizationEnabled = false,
                DefaultRoleId = "security_expert",
                IgnorePatterns = new[] { "*.log", "node_modules" }
            }
        };
        _repository.GetByIdAsync("ws-123", Arg.Any<CancellationToken>()).Returns(workspace);

        // Act
        var result = await _sut.GetSettingsAsync("ws-123");

        // Assert
        result.Should().NotBeNull();
        result.SanitizationEnabled.Should().BeFalse();
        result.DefaultRoleId.Should().Be("security_expert");
        result.IgnorePatterns.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSettingsAsync_WithNonExistentWorkspace_ReturnsDefaultSettings()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent", Arg.Any<CancellationToken>()).Returns((Workspace?)null);

        // Act
        var result = await _sut.GetSettingsAsync("nonexistent");

        // Assert
        result.Should().NotBeNull();
        result.SanitizationEnabled.Should().BeTrue(); // Default value
        result.DefaultRoleId.Should().Be("general_review"); // Default value
    }

    [Fact]
    public async Task SaveSettingsAsync_UpdatesWorkspaceSettings()
    {
        // Arrange
        var workspace = new Workspace
        {
            Id = "ws-123",
            Name = "Test",
            RootPath = "/test",
            Settings = new WorkspaceSettings()
        };
        _repository.GetByIdAsync("ws-123", Arg.Any<CancellationToken>()).Returns(workspace);

        var newSettings = new WorkspaceSettings
        {
            SanitizationEnabled = false,
            DefaultRoleId = "debug_assistant",
            IgnorePatterns = new[] { "*.tmp" }
        };

        // Act
        await _sut.SaveSettingsAsync("ws-123", newSettings);

        // Assert
        await _repository.Received(1).SaveAsync(
            Arg.Is<Workspace>(w => w.Id == "ws-123" && w.Settings.DefaultRoleId == "debug_assistant"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetSettingsAsync_RestoresDefaultSettings()
    {
        // Arrange
        var workspace = new Workspace
        {
            Id = "ws-123",
            Name = "Test",
            RootPath = "/test",
            Settings = new WorkspaceSettings
            {
                SanitizationEnabled = false,
                DefaultRoleId = "custom_role",
                IgnorePatterns = new[] { "custom" }
            }
        };
        _repository.GetByIdAsync("ws-123", Arg.Any<CancellationToken>()).Returns(workspace);

        // Act
        await _sut.ResetSettingsAsync("ws-123");

        // Assert
        await _repository.Received(1).SaveAsync(
            Arg.Is<Workspace>(w => 
                w.Id == "ws-123" && 
                w.Settings.SanitizationEnabled == true &&
                w.Settings.DefaultRoleId == "general_review" &&
                w.Settings.IgnorePatterns.Count == 0),
            Arg.Any<CancellationToken>());
    }
}

