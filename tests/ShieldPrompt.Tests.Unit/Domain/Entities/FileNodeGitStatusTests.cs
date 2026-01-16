using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class FileNodeGitStatusTests
{
    [Fact]
    public void FileNode_GitStatus_DefaultsToNone()
    {
        // Arrange & Act
        var node = new FileNode("/path/file.cs", "file.cs", false);

        // Assert
        node.GitStatus.Should().Be(GitFileStatus.None);
    }

    [Fact]
    public void FileNode_IsGitIgnored_WhenStatusHasIgnoredFlag()
    {
        // Arrange
        var node = new FileNode("/path/file.cs", "file.cs", false)
        {
            GitStatus = GitFileStatus.Ignored
        };

        // Assert
        node.IsGitIgnored.Should().BeTrue();
    }

    [Fact]
    public void FileNode_IsGitIgnored_WhenStatusDoesNotHaveIgnoredFlag()
    {
        // Arrange
        var node = new FileNode("/path/file.cs", "file.cs", false)
        {
            GitStatus = GitFileStatus.Modified
        };

        // Assert
        node.IsGitIgnored.Should().BeFalse();
    }

    [Fact]
    public void FileNode_GitStatus_CanBeSet()
    {
        // Arrange
        var node = new FileNode("/path/file.cs", "file.cs", false);

        // Act
        node.GitStatus = GitFileStatus.Modified | GitFileStatus.Staged;

        // Assert
        node.GitStatus.Should().HaveFlag(GitFileStatus.Modified);
        node.GitStatus.Should().HaveFlag(GitFileStatus.Staged);
    }
}

