using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Infrastructure.Services;
using System.IO;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class GitStatusProviderTests : IDisposable
{
    private readonly string _testRepoPath;
    private readonly GitStatusProvider _sut;

    public GitStatusProviderTests()
    {
        _testRepoPath = Path.Combine(Path.GetTempPath(), $"test-git-repo-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoPath);
        Directory.CreateDirectory(Path.Combine(_testRepoPath, ".git"));
        _sut = new GitStatusProvider();
    }

    [Fact]
    public void GetRepositoryRoot_WithGitRepo_ReturnsRoot()
    {
        // Arrange
        var subDir = Path.Combine(_testRepoPath, "subdir");
        Directory.CreateDirectory(subDir);
        var filePath = Path.Combine(subDir, "file.cs");

        // Act
        var root = _sut.GetRepositoryRoot(filePath);

        // Assert
        root.Should().Be(_testRepoPath);
    }

    [Fact]
    public void GetRepositoryRoot_WithoutGitRepo_ReturnsNull()
    {
        // Arrange
        var nonGitPath = Path.Combine(Path.GetTempPath(), $"non-git-{Guid.NewGuid()}");

        // Act
        var result = _sut.GetRepositoryRoot(nonGitPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsInGitRepository_WithGitRepo_ReturnsTrue()
    {
        // Act
        var result = _sut.IsInGitRepository(_testRepoPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInGitRepository_WithoutGitRepo_ReturnsFalse()
    {
        // Arrange
        var nonGitPath = Path.Combine(Path.GetTempPath(), $"non-git-{Guid.NewGuid()}");

        // Act
        var result = _sut.IsInGitRepository(nonGitPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetFileStatus_WithNonExistentFile_ReturnsNone()
    {
        // Arrange
        var filePath = Path.Combine(_testRepoPath, "nonexistent.cs");

        // Act
        var status = _sut.GetFileStatus(filePath);

        // Assert
        status.Should().Be(GitFileStatus.None);
    }

    [Fact]
    public void GetModifiedFiles_WithEmptyDirectory_ReturnsEmpty()
    {
        // Act
        var files = _sut.GetModifiedFiles(_testRepoPath);

        // Assert
        files.Should().BeEmpty();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRepoPath))
        {
            Directory.Delete(_testRepoPath, true);
        }
    }
}

