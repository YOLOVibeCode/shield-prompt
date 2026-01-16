using FluentAssertions;
using ShieldPrompt.Application.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class SessionManagerTests
{
    private readonly SessionManager _sut;

    public SessionManagerTests()
    {
        _sut = new SessionManager();
    }

    [Fact]
    public void CreateSession_AddsToActiveSessions()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.ActiveSessionIds.Should().Contain(session.Id);
    }

    [Fact]
    public void CreateSession_SetsAsActiveSession()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.ActiveSessionId.Should().Be(session.Id);
    }

    [Fact]
    public void SetActiveSession_ChangesActiveSession()
    {
        var session1 = _sut.CreateSession("ws-1", "First");
        var session2 = _sut.CreateSession("ws-1", "Second");

        _sut.SetActiveSession(session1.Id);

        _sut.ActiveSessionId.Should().Be(session1.Id);
    }

    [Fact]
    public void SetActiveSession_WithInvalidId_ThrowsException()
    {
        var act = () => _sut.SetActiveSession("invalid-id");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CloseSession_RemovesFromActiveSessions()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.CloseSession(session.Id);

        _sut.ActiveSessionIds.Should().NotContain(session.Id);
    }

    [Fact]
    public void CloseSession_WhenActive_SwitchesToAnother()
    {
        var session1 = _sut.CreateSession("ws-1", "First");
        var session2 = _sut.CreateSession("ws-1", "Second");
        _sut.SetActiveSession(session2.Id);

        _sut.CloseSession(session2.Id);

        _sut.ActiveSessionId.Should().Be(session1.Id);
    }

    [Fact]
    public void CloseSession_WhenLastSession_ActiveBecomesNull()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.CloseSession(session.Id);

        _sut.ActiveSessionId.Should().BeNull();
    }

    [Fact]
    public void ActiveSessionChanged_FiresWhenSessionChanges()
    {
        var eventFired = false;
        _sut.ActiveSessionChanged += (s, e) => eventFired = true;

        _sut.CreateSession("ws-1", "Test");

        eventFired.Should().BeTrue();
    }
}

