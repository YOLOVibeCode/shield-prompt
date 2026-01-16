using FluentAssertions;
using ShieldPrompt.Application.Actions;
using ShieldPrompt.Application.Interfaces.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Actions;

/// <summary>
/// Tests for CompositeAction (batch operations with atomic undo).
/// Following TDD: Tests written FIRST.
/// </summary>
public class CompositeActionTests : IDisposable
{
    private readonly string _testDir;

    public CompositeActionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"CompositeActionTests_{Guid.NewGuid()}");
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
    public async Task ExecuteAsync_WithMultipleActions_ExecutesAllInOrder()
    {
        // Arrange
        var actions = new List<IFileAction>
        {
            new FileCreateAction(Path.Combine(_testDir, "file1.txt"), "Content 1"),
            new FileCreateAction(Path.Combine(_testDir, "file2.txt"), "Content 2"),
            new FileCreateAction(Path.Combine(_testDir, "file3.txt"), "Content 3")
        };

        var composite = new CompositeAction("Create 3 files", actions);

        // Act
        var result = await composite.ExecuteAsync();

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(_testDir, "file1.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_testDir, "file2.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_testDir, "file3.txt")).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedAction_RollsBackPreviousActions()
    {
        // Arrange
        var file1Path = Path.Combine(_testDir, "file1.txt");
        var file2Path = Path.Combine(_testDir, "file2.txt");
        var invalidPath = Path.Combine(_testDir, new string('x', 300), "invalid.txt"); // Path too long

        var actions = new List<IFileAction>
        {
            new FileCreateAction(file1Path, "Content 1"),
            new FileCreateAction(file2Path, "Content 2"),
            new FileCreateAction(invalidPath, "Content 3") // This will fail
        };

        var composite = new CompositeAction("Create files with failure", actions);

        // Act
        var result = await composite.ExecuteAsync();

        // Assert
        result.Success.Should().BeFalse("should fail due to invalid path");
        result.ErrorMessage.Should().Contain("Failed at action");
        result.ErrorMessage.Should().Contain("3/3");
        
        // Verify rollback - files should be cleaned up
        File.Exists(file1Path).Should().BeFalse("should be rolled back");
        File.Exists(file2Path).Should().BeFalse("should be rolled back");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyActions_Succeeds()
    {
        // Arrange
        var composite = new CompositeAction("Empty batch", new List<IFileAction>());

        // Act
        var result = await composite.ExecuteAsync();

        // Assert
        result.Success.Should().BeTrue("empty batch should succeed");
    }

    #endregion

    #region Undo Tests

    [Fact]
    public async Task UndoAsync_UndoesAllActionsInReverseOrder()
    {
        // Arrange
        var file1Path = Path.Combine(_testDir, "undo1.txt");
        var file2Path = Path.Combine(_testDir, "undo2.txt");
        var file3Path = Path.Combine(_testDir, "undo3.txt");

        var actions = new List<IFileAction>
        {
            new FileCreateAction(file1Path, "Content 1"),
            new FileCreateAction(file2Path, "Content 2"),
            new FileCreateAction(file3Path, "Content 3")
        };

        var composite = new CompositeAction("Create for undo test", actions);
        await composite.ExecuteAsync();

        File.Exists(file1Path).Should().BeTrue("files should exist after execute");
        File.Exists(file2Path).Should().BeTrue();
        File.Exists(file3Path).Should().BeTrue();

        // Act
        var result = await composite.UndoAsync();

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(file1Path).Should().BeFalse("all files should be deleted");
        File.Exists(file2Path).Should().BeFalse();
        File.Exists(file3Path).Should().BeFalse();
    }

    [Fact]
    public async Task UndoAsync_WithPartialFailure_ContinuesUndoingOthers()
    {
        // Arrange
        var file1Path = Path.Combine(_testDir, "partial1.txt");
        var file2Path = Path.Combine(_testDir, "partial2.txt");

        var actions = new List<IFileAction>
        {
            new FileCreateAction(file1Path, "Content 1"),
            new FileCreateAction(file2Path, "Content 2")
        };

        var composite = new CompositeAction("Partial failure test", actions);
        await composite.ExecuteAsync();

        // Delete file2 manually to cause undo "failure" (though delete is idempotent)
        File.Delete(file2Path);

        // Act
        var result = await composite.UndoAsync();

        // Assert
        result.Success.Should().BeTrue("should continue despite one file already gone");
        File.Exists(file1Path).Should().BeFalse("file1 should still be undone");
    }

    #endregion

    #region Properties Tests

    [Fact]
    public void Properties_AreSetCorrectly()
    {
        // Arrange
        var actions = new List<IFileAction>
        {
            new FileCreateAction(Path.Combine(_testDir, "test.txt"), "content")
        };

        // Act
        var composite = new CompositeAction("Test Composite", actions);

        // Assert
        composite.Id.Should().NotBeEmpty();
        composite.Description.Should().Be("Test Composite");
        composite.Actions.Should().HaveCount(1);
        composite.Count.Should().Be(1);
    }

    [Fact]
    public void Actions_IsReadOnly()
    {
        // Arrange
        var actions = new List<IFileAction>
        {
            new FileCreateAction(Path.Combine(_testDir, "test.txt"), "content")
        };
        var composite = new CompositeAction("Test", actions);

        // Act
        var actionsList = composite.Actions;

        // Assert
        actionsList.Should().BeAssignableTo<IReadOnlyList<IAction>>();
        actionsList.Should().HaveCount(1);
    }

    #endregion

    #region Integration with UndoRedoManager

    [Fact]
    public async Task CompositeAction_WorksWithUndoRedoManager()
    {
        // Arrange
        var actions = new List<IFileAction>
        {
            new FileCreateAction(Path.Combine(_testDir, "manager1.txt"), "Content 1"),
            new FileCreateAction(Path.Combine(_testDir, "manager2.txt"), "Content 2")
        };

        var composite = new CompositeAction("Test with manager", actions);
        
        // Create a simple adapter for IAction -> IUndoableAction
        var adapter = new SimpleActionAdapter(composite);
        var manager = new ShieldPrompt.Application.Services.UndoRedoManager();

        // Act - Execute via manager
        await manager.ExecuteAsync(adapter);

        // Assert
        File.Exists(Path.Combine(_testDir, "manager1.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_testDir, "manager2.txt")).Should().BeTrue();
        manager.CanUndo.Should().BeTrue();

        // Act - Undo via manager
        await manager.UndoAsync();

        // Assert
        File.Exists(Path.Combine(_testDir, "manager1.txt")).Should().BeFalse();
        File.Exists(Path.Combine(_testDir, "manager2.txt")).Should().BeFalse();
    }

    /// <summary>
    /// Simple adapter for testing CompositeAction with UndoRedoManager.
    /// </summary>
    private class SimpleActionAdapter : ShieldPrompt.Application.Interfaces.IUndoableAction
    {
        private readonly IAction _action;

        public SimpleActionAdapter(IAction action)
        {
            _action = action;
            Timestamp = DateTime.UtcNow;
        }

        public string Description => _action.Description;
        public DateTime Timestamp { get; }

        public async Task ExecuteAsync()
        {
            var result = await _action.ExecuteAsync();
            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed to execute: {result.ErrorMessage}");
            }
        }

        public async Task UndoAsync()
        {
            var result = await _action.UndoAsync();
            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed to undo: {result.ErrorMessage}");
            }
        }

        public bool CanMergeWith(ShieldPrompt.Application.Interfaces.IUndoableAction other) => false;
        public ShieldPrompt.Application.Interfaces.IUndoableAction MergeWith(ShieldPrompt.Application.Interfaces.IUndoableAction other) => throw new NotSupportedException();
        public bool RequiresConfirmation => true;
        public string? ConfirmationMessage => $"Undo {Description}?";
    }

    #endregion
}

