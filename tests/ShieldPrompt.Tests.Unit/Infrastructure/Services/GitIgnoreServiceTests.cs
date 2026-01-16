using FluentAssertions;
using ShieldPrompt.Infrastructure.Services;
using System.IO;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class GitIgnoreServiceTests : IDisposable
{
    private readonly string _testRepoPath;
    private readonly GitIgnoreService _sut;

    public GitIgnoreServiceTests()
    {
        _testRepoPath = Path.Combine(Path.GetTempPath(), $"test-git-repo-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoPath);
        Directory.CreateDirectory(Path.Combine(_testRepoPath, ".git"));
        _sut = new GitIgnoreService();
    }

    [Fact]
    public void ShouldIgnore_WithNoGitIgnore_ReturnsFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testRepoPath, "file.cs");

        // Act
        var result = _sut.ShouldIgnore(filePath, _testRepoPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldIgnore_WithMatchingPattern_ReturnsTrue()
    {
        // Arrange
        var gitignorePath = Path.Combine(_testRepoPath, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "*.cs\n");
        _sut.ReloadPatterns(_testRepoPath);
        var filePath = Path.Combine(_testRepoPath, "file.cs");

        // Act
        var result = _sut.ShouldIgnore(filePath, _testRepoPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetIgnorePatterns_WithGitIgnore_ReturnsPatterns()
    {
        // Arrange
        var gitignorePath = Path.Combine(_testRepoPath, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "*.log\nnode_modules/\n");
        _sut.ReloadPatterns(_testRepoPath);

        // Act
        var patterns = _sut.GetIgnorePatterns(_testRepoPath);

        // Assert
        patterns.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReloadPatterns_ReloadsFromGitIgnore()
    {
        // Arrange
        var gitignorePath = Path.Combine(_testRepoPath, ".gitignore");
        await File.WriteAllTextAsync(gitignorePath, "*.tmp\n");
        _sut.ReloadPatterns(_testRepoPath);

        // Act
        var patterns = _sut.GetIgnorePatterns(_testRepoPath);

        // Assert
        patterns.Should().Contain("*.tmp");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRepoPath))
        {
            Directory.Delete(_testRepoPath, true);
        }
    }
}

