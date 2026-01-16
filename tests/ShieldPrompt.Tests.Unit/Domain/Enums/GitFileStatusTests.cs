using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Enums;

public class GitFileStatusTests
{
    [Fact]
    public void GitFileStatus_Flags_CanBeCombined()
    {
        // Arrange & Act
        var combined = GitFileStatus.Modified | GitFileStatus.Staged;

        // Assert
        combined.HasFlag(GitFileStatus.Modified).Should().BeTrue();
        combined.HasFlag(GitFileStatus.Staged).Should().BeTrue();
    }

    [Fact]
    public void GitFileStatus_None_HasNoFlags()
    {
        // Assert
        GitFileStatus.None.Should().Be(GitFileStatus.None);
        ((int)GitFileStatus.None).Should().Be(0);
    }

    [Fact]
    public void GitFileStatus_CanCheckMultipleFlags()
    {
        // Arrange
        var status = GitFileStatus.Modified | GitFileStatus.Untracked;

        // Assert
        status.HasFlag(GitFileStatus.Modified).Should().BeTrue();
        status.HasFlag(GitFileStatus.Untracked).Should().BeTrue();
        status.HasFlag(GitFileStatus.Staged).Should().BeFalse();
    }
}

