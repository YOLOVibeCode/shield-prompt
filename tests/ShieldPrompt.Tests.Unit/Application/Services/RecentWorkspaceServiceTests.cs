using FluentAssertions;
using NSubstitute;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class RecentWorkspaceServiceTests
{
    private readonly IWorkspaceRepository _repository;
    private readonly RecentWorkspaceService _sut;

    public RecentWorkspaceServiceTests()
    {
        _repository = Substitute.For<IWorkspaceRepository>();
        _sut = new RecentWorkspaceService(_repository);
    }

    [Fact]
    public async Task GetRecentAsync_WithWorkspaces_ReturnsOrderedByLastOpened()
    {
        // Arrange
        var workspaces = new[]
        {
            new Workspace
            {
                Id = "1",
                Name = "Old",
                RootPath = "/old",
                LastOpened = DateTime.UtcNow.AddDays(-2)
            },
            new Workspace
            {
                Id = "2",
                Name = "New",
                RootPath = "/new",
                LastOpened = DateTime.UtcNow
            }
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(workspaces.OrderByDescending(w => w.LastOpened).ToList());

        // Act
        var result = await _sut.GetRecentAsync(10);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("New"); // Most recent first
        result[1].Name.Should().Be("Old");
    }

    [Fact]
    public async Task GetRecentAsync_WithCountLimit_ReturnsLimitedResults()
    {
        // Arrange
        var workspaces = Enumerable.Range(1, 20)
            .Select(i => new Workspace
            {
                Id = i.ToString(),
                Name = $"WS{i}",
                RootPath = $"/ws{i}",
                LastOpened = DateTime.UtcNow.AddDays(-i)
            })
            .ToList();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(workspaces);

        // Act
        var result = await _sut.GetRecentAsync(5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task RecordOpenedAsync_UpdatesLastOpenedTimestamp()
    {
        // Arrange
        var workspace = new Workspace
        {
            Id = "123",
            Name = "Test",
            RootPath = "/test",
            LastOpened = DateTime.UtcNow.AddDays(-1)
        };
        _repository.GetByIdAsync("123", Arg.Any<CancellationToken>()).Returns(workspace);

        // Act
        await _sut.RecordOpenedAsync("123");

        // Assert
        await _repository.Received(1).UpdateLastOpenedAsync("123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFromHistoryAsync_DeletesWorkspace()
    {
        // Act
        await _sut.RemoveFromHistoryAsync("123");

        // Assert
        await _repository.Received(1).DeleteAsync("123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearHistoryAsync_DeletesAllWorkspaces()
    {
        // Arrange
        var workspaces = new[]
        {
            new Workspace { Id = "1", Name = "WS1", RootPath = "/ws1" },
            new Workspace { Id = "2", Name = "WS2", RootPath = "/ws2" }
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(workspaces);

        // Act
        await _sut.ClearHistoryAsync();

        // Assert
        await _repository.Received(1).DeleteAsync("1", Arg.Any<CancellationToken>());
        await _repository.Received(1).DeleteAsync("2", Arg.Any<CancellationToken>());
    }
}

