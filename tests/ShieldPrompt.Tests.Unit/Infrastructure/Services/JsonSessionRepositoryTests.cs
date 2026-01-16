using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Infrastructure.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class JsonSessionRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly JsonSessionRepository _sut;

    public JsonSessionRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"shieldprompt-sessions-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Use reflection or create a testable version
        // For now, we'll use the actual implementation
        _sut = new JsonSessionRepository();
    }

    [Fact]
    public async Task GetByWorkspaceAsync_WithNoSessions_ReturnsEmpty()
    {
        // Arrange - Use a unique workspace ID
        var workspaceId = $"ws-{Guid.NewGuid()}";

        // Act
        var result = await _sut.GetByWorkspaceAsync(workspaceId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_WithNewSession_CreatesSession()
    {
        // Arrange
        var workspaceId = $"ws-{Guid.NewGuid()}";
        var session = PromptSession.CreateNew(workspaceId, "Test Session");

        // Act
        await _sut.SaveAsync(session);

        // Assert
        var retrieved = await _sut.GetByIdAsync(session.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Session");
    }

    [Fact]
    public async Task SaveAsync_WithExistingSession_UpdatesSession()
    {
        // Arrange
        var workspaceId = $"ws-{Guid.NewGuid()}";
        var session = PromptSession.CreateNew(workspaceId, "Original");
        await _sut.SaveAsync(session);

        var updated = session with { Name = "Updated" };

        // Act
        await _sut.SaveAsync(updated);

        // Assert
        var retrieved = await _sut.GetByIdAsync(session.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsSession()
    {
        // Arrange
        var workspaceId = $"ws-{Guid.NewGuid()}";
        var session = PromptSession.CreateNew(workspaceId, "Test");
        await _sut.SaveAsync(session);

        // Act
        var result = await _sut.GetByIdAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesSession()
    {
        // Arrange
        var workspaceId = $"ws-{Guid.NewGuid()}";
        var session = PromptSession.CreateNew(workspaceId, "Test");
        await _sut.SaveAsync(session);

        // Act
        await _sut.DeleteAsync(session.Id);

        // Assert
        var result = await _sut.GetByIdAsync(session.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByWorkspaceAsync_OrdersByLastModifiedDescending()
    {
        // Arrange
        var workspaceId = $"ws-{Guid.NewGuid()}";
        var oldSession = PromptSession.CreateNew(workspaceId, "Old") with
        {
            LastModified = DateTime.UtcNow.AddDays(-2)
        };
        var newSession = PromptSession.CreateNew(workspaceId, "New") with
        {
            LastModified = DateTime.UtcNow
        };
        await _sut.SaveAsync(oldSession);
        await _sut.SaveAsync(newSession);

        // Act
        var result = await _sut.GetByWorkspaceAsync(workspaceId);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(newSession.Id); // Most recent first
        result[1].Id.Should().Be(oldSession.Id);
    }

    [Fact]
    public async Task DeleteByWorkspaceAsync_RemovesAllSessionsForWorkspace()
    {
        // Arrange
        var workspaceId = $"ws-{Guid.NewGuid()}";
        var session1 = PromptSession.CreateNew(workspaceId, "Session 1");
        var session2 = PromptSession.CreateNew(workspaceId, "Session 2");
        await _sut.SaveAsync(session1);
        await _sut.SaveAsync(session2);

        // Act
        await _sut.DeleteByWorkspaceAsync(workspaceId);

        // Assert
        var result = await _sut.GetByWorkspaceAsync(workspaceId);
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        // Cleanup handled by JsonSessionRepository using user profile directory
    }
}

