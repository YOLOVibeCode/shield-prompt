using FluentAssertions;
using ShieldPrompt.Application.Actions;
using ShieldPrompt.Application.Interfaces.Actions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Actions;

/// <summary>
/// Tests for FileCreateAction following TDD approach.
/// Tests written FIRST, then implementation.
/// </summary>
public class FileCreateActionTests : IDisposable
{
    private readonly string _testDir;

    public FileCreateActionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"FileActionTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    #region Execute Tests

    [Fact]
    public async Task ExecuteAsync_CreatesNewFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.txt");
        var content = "Hello World";
        var action = new FileCreateAction(filePath, content);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
        var actualContent = await File.ReadAllTextAsync(filePath);
        actualContent.Should().Be(content);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNestedDirectories()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "nested", "deep", "test.txt");
        var content = "Nested content";
        var action = new FileCreateAction(filePath, content);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyContent_CreatesEmptyFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "empty.txt");
        var action = new FileCreateAction(filePath, string.Empty);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().BeEmpty();
    }

    #endregion

    #region Undo Tests (THE KEY FIX)

    [Fact]
    public async Task UndoAsync_DeletesCreatedFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "to-delete.txt");
        var action = new FileCreateAction(filePath, "content");
        await action.ExecuteAsync();
        File.Exists(filePath).Should().BeTrue("file should exist after execute");

        // Act
        var result = await action.UndoAsync();

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse("file should be deleted after undo");
    }

    [Fact]
    public async Task UndoAsync_CleansUpEmptyDirectories()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "temp", "nested", "file.txt");
        var action = new FileCreateAction(filePath, "content");
        await action.ExecuteAsync();

        // Act
        await action.UndoAsync();

        // Assert
        Directory.Exists(Path.Combine(_testDir, "temp")).Should().BeFalse("empty directories should be cleaned up");
    }

    [Fact]
    public async Task UndoAsync_WhenFileDoesNotExist_Succeeds()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "nonexistent.txt");
        var action = new FileCreateAction(filePath, "content");

        // Act (undo without execute)
        var result = await action.UndoAsync();

        // Assert
        result.Success.Should().BeTrue("undo should succeed even if file doesn't exist");
    }

    #endregion

    #region Properties Tests

    [Fact]
    public void Properties_AreSetCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.txt");
        var content = "test content";

        // Act
        var action = new FileCreateAction(filePath, content);

        // Assert
        action.Id.Should().NotBeEmpty();
        action.Description.Should().Contain("test.txt");
        action.FilePath.Should().Be(filePath);
        action.OperationType.Should().Be(ShieldPrompt.Domain.Enums.FileOperationType.Create);
    }

    #endregion
}

