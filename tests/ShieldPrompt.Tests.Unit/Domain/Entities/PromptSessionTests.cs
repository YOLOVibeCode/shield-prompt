using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class PromptSessionTests
{
    [Fact]
    public void CreateNew_GeneratesUniqueId()
    {
        var session1 = PromptSession.CreateNew("ws-1", "Test");
        var session2 = PromptSession.CreateNew("ws-1", "Test");

        session1.Id.Should().NotBe(session2.Id);
    }

    [Fact]
    public void CreateNew_SetsRequiredProperties()
    {
        var session = PromptSession.CreateNew("ws-123", "My Prompt");

        session.WorkspaceId.Should().Be("ws-123");
        session.Name.Should().Be("My Prompt");
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Session_WithSelectedFiles_PersistsCorrectly()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            SelectedFilePaths = new[] { "/file1.cs", "/file2.cs" }
        };

        session.SelectedFilePaths.Should().HaveCount(2);
        session.SelectedFilePaths.Should().Contain("/file1.cs");
    }

    [Fact]
    public void Session_Record_SupportsImmutableUpdate()
    {
        var original = PromptSession.CreateNew("ws-1", "Original");
        var updated = original with { Name = "Updated" };

        original.Name.Should().Be("Original");
        updated.Name.Should().Be("Updated");
        updated.Id.Should().Be(original.Id);
    }
}

