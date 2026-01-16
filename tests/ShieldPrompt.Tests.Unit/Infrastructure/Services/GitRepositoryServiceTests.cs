using FluentAssertions;
using ShieldPrompt.Infrastructure.Services;
using System.IO;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class GitRepositoryServiceTests : IDisposable
{
    private readonly string _testRepoPath;
    private readonly GitRepositoryService _sut;

    public GitRepositoryServiceTests()
    {
        _testRepoPath = Path.Combine(Path.GetTempPath(), $"test-git-repo-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoPath);
        Directory.CreateDirectory(Path.Combine(_testRepoPath, ".git"));
        _sut = new GitRepositoryService();
    }

    [Fact]
    public async Task GetRepositoryInfoAsync_WithGitRepo_ReturnsInfo()
    {
        // Act
        var info = await _sut.GetRepositoryInfoAsync(_testRepoPath);

        // Assert
        info.Should().NotBeNull();
        info!.RootPath.Should().Be(_testRepoPath);
    }

    [Fact]
    public async Task GetRepositoryInfoAsync_WithoutGitRepo_ReturnsNull()
    {
        // Arrange
        var nonGitPath = Path.Combine(Path.GetTempPath(), $"non-git-{Guid.NewGuid()}");

        // Act
        var result = await _sut.GetRepositoryInfoAsync(nonGitPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentBranch_WithGitRepo_DoesNotThrow()
    {
        // Act & Assert - May be null if git is not initialized, but should not throw
        _sut.Invoking(s => s.GetCurrentBranch(_testRepoPath))
            .Should().NotThrow();
    }

    [Fact]
    public void GetBranches_WithGitRepo_ReturnsBranches()
    {
        // Act
        var branches = _sut.GetBranches(_testRepoPath);

        // Assert - Should return enumerable (may be empty)
        branches.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshStatusAsync_WithGitRepo_DoesNotThrow()
    {
        // Act & Assert
        await _sut.Invoking(s => s.RefreshStatusAsync(_testRepoPath))
            .Should().NotThrowAsync();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRepoPath))
        {
            Directory.Delete(_testRepoPath, true);
        }
    }
}

