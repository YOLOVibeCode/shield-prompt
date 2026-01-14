using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;

namespace ShieldPrompt.Tests.Unit.Application.UndoRedo;

public class UndoRedoManagerTests
{
    [Fact]
    public void CanUndo_WhenEmpty_ReturnsFalse()
    {
        // Arrange
        var sut = new UndoRedoManager();

        // Act & Assert
        sut.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void CanRedo_WhenEmpty_ReturnsFalse()
    {
        // Arrange
        var sut = new UndoRedoManager();

        // Act & Assert
        sut.CanRedo.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithAction_AddsToHistory()
    {
        // Arrange
        var sut = new UndoRedoManager();
        var action = new TestAction("Test Action");

        // Act
        await sut.ExecuteAsync(action);

        // Assert
        sut.CanUndo.Should().BeTrue();
        sut.UndoDescription.Should().Be("Test Action");
    }

    [Fact]
    public async Task UndoAsync_AfterExecute_CallsUndo()
    {
        // Arrange
        var sut = new UndoRedoManager();
        var action = new TestAction("Test");
        await sut.ExecuteAsync(action);

        // Act
        var result = await sut.UndoAsync();

        // Assert
        result.Should().BeTrue();
        action.UndoCalled.Should().BeTrue();
        sut.CanUndo.Should().BeFalse();
        sut.CanRedo.Should().BeTrue();
    }

    [Fact]
    public async Task RedoAsync_AfterUndo_CallsExecute()
    {
        // Arrange
        var sut = new UndoRedoManager();
        var action = new TestAction("Test");
        await sut.ExecuteAsync(action);
        await sut.UndoAsync();

        // Act
        var result = await sut.RedoAsync();

        // Assert
        result.Should().BeTrue();
        action.ExecuteCount.Should().Be(2); // Once for initial, once for redo
        sut.CanRedo.Should().BeFalse();
        sut.CanUndo.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_AfterUndo_ClearsRedoHistory()
    {
        // Arrange
        var sut = new UndoRedoManager();
        await sut.ExecuteAsync(new TestAction("Action 1"));
        await sut.ExecuteAsync(new TestAction("Action 2"));
        await sut.UndoAsync();

        // Act - Execute new action while redo is available
        await sut.ExecuteAsync(new TestAction("Action 3"));

        // Assert
        sut.CanRedo.Should().BeFalse(); // Redo history cleared
    }

    [Fact]
    public async Task UndoAsync_Multiple_UndoesInReverseOrder()
    {
        // Arrange
        var sut = new UndoRedoManager();
        var action1 = new TestAction("First");
        var action2 = new TestAction("Second");
        var action3 = new TestAction("Third");
        
        await sut.ExecuteAsync(action1);
        await sut.ExecuteAsync(action2);
        await sut.ExecuteAsync(action3);

        // Act
        await sut.UndoAsync(); // Undo Third
        await sut.UndoAsync(); // Undo Second
        await sut.UndoAsync(); // Undo First

        // Assert
        action3.UndoCalled.Should().BeTrue();
        action2.UndoCalled.Should().BeTrue();
        action1.UndoCalled.Should().BeTrue();
        sut.CanUndo.Should().BeFalse();
    }

    [Fact]
    public async Task StateChanged_WhenHistoryChanges_RaisesEvent()
    {
        // Arrange
        var sut = new UndoRedoManager();
        var eventRaised = false;
        sut.StateChanged += (s, e) => eventRaised = true;

        // Act
        await sut.ExecuteAsync(new TestAction("Test"));

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void Clear_RemovesAllHistory()
    {
        // Arrange
        var sut = new UndoRedoManager();
        sut.ExecuteAsync(new TestAction("Action")).Wait();

        // Act
        sut.Clear();

        // Assert
        sut.CanUndo.Should().BeFalse();
        sut.CanRedo.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithMergeableActions_MergesThem()
    {
        // Arrange
        var sut = new UndoRedoManager();
        var action1 = new MergeableAction("Select File 1");
        var action2 = new MergeableAction("Select File 2");

        // Act
        await sut.ExecuteAsync(action1);
        await sut.ExecuteAsync(action2);

        // Assert - Should have merged into one undo action
        sut.UndoDescription.Should().Contain("Merged");
    }
}

// Test helper class
internal class TestAction : IUndoableAction
{
    public TestAction(string description)
    {
        Description = description;
        Timestamp = DateTime.UtcNow;
    }

    public string Description { get; }
    public DateTime Timestamp { get; }
    public int ExecuteCount { get; private set; }
    public bool UndoCalled { get; private set; }

    public Task ExecuteAsync()
    {
        ExecuteCount++;
        return Task.CompletedTask;
    }

    public Task UndoAsync()
    {
        UndoCalled = true;
        return Task.CompletedTask;
    }

    public bool CanMergeWith(IUndoableAction other) => false;

    public IUndoableAction MergeWith(IUndoableAction other) =>
        throw new NotSupportedException();
}

internal class MergeableAction : IUndoableAction
{
    private readonly List<string> _descriptions = new();

    public MergeableAction(string description)
    {
        _descriptions.Add(description);
        Timestamp = DateTime.UtcNow;
    }

    public string Description => _descriptions.Count > 1 
        ? $"Merged: {string.Join(", ", _descriptions)}"
        : _descriptions[0];

    public DateTime Timestamp { get; }

    public Task ExecuteAsync() => Task.CompletedTask;
    public Task UndoAsync() => Task.CompletedTask;

    public bool CanMergeWith(IUndoableAction other) => 
        other is MergeableAction;

    public IUndoableAction MergeWith(IUndoableAction other)
    {
        if (other is MergeableAction mergeable)
        {
            _descriptions.AddRange(mergeable._descriptions);
        }
        return this;
    }
}

