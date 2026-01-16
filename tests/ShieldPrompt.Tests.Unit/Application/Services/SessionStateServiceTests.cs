using FluentAssertions;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class SessionStateServiceTests
{
    private readonly SessionStateService _sut;

    public SessionStateServiceTests()
    {
        _sut = new SessionStateService();
    }

    [Fact]
    public void GetState_WithNonExistentSession_ReturnsNull()
    {
        // Act
        var result = _sut.GetState("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UpdateState_SetsState()
    {
        // Arrange
        var state = new SessionState(
            "session-1",
            "Generated prompt",
            1000,
            false);

        // Act
        _sut.UpdateState("session-1", state);

        // Assert
        var retrieved = _sut.GetState("session-1");
        retrieved.Should().NotBeNull();
        retrieved!.SessionId.Should().Be("session-1");
        retrieved.GeneratedPrompt.Should().Be("Generated prompt");
    }

    [Fact]
    public void MarkDirty_SetsHasUnsavedChanges()
    {
        // Arrange
        var state = new SessionState("session-1", "prompt", 100, false);
        _sut.UpdateState("session-1", state);

        // Act
        _sut.MarkDirty("session-1");

        // Assert
        var updated = _sut.GetState("session-1");
        updated.Should().NotBeNull();
        updated!.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ClearsHasUnsavedChanges()
    {
        // Arrange
        var state = new SessionState("session-1", "prompt", 100, true);
        _sut.UpdateState("session-1", state);

        // Act
        _sut.MarkClean("session-1");

        // Assert
        var updated = _sut.GetState("session-1");
        updated.Should().NotBeNull();
        updated!.HasUnsavedChanges.Should().BeFalse();
    }
}

