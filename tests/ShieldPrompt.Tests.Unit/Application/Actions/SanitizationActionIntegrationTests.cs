using FluentAssertions;
using ShieldPrompt.Application.Actions;
using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Actions;

/// <summary>
/// Integration tests for SanitizeAction and DesanitizeAction.
/// Tests the full undo/redo cycle with real sanitization engines.
/// </summary>
public class SanitizationActionIntegrationTests : IDisposable
{
    private readonly MappingSession _session;
    private readonly SanitizationEngine _sanitizationEngine;
    private readonly DesanitizationEngine _desanitizationEngine;

    public SanitizationActionIntegrationTests()
    {
        _session = new MappingSession();
        var patternRegistry = new PatternRegistry();
        var aliasGenerator = new AliasGenerator();
        _sanitizationEngine = new SanitizationEngine(patternRegistry, _session, aliasGenerator);
        _desanitizationEngine = new DesanitizationEngine();
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    [Fact]
    public async Task SanitizeAction_ExecuteAndUndo_MaintainsOriginalContent()
    {
        // Arrange
        var originalContent = "This is a test with no sensitive data.";
        var options = new SanitizationOptions
        {
            EnableInfrastructure = true,
            EnablePII = false,
            Mode = PolicyMode.SanitizedOnly
        };

        var action = new SanitizeAction(originalContent, options, _sanitizationEngine);

        // Act - Execute
        var executeResult = await action.ExecuteAsync();

        // Assert - Execute succeeded
        executeResult.Success.Should().BeTrue();
        action.CurrentContent.Should().Be(originalContent, "no sensitive data to sanitize");

        // Act - Undo
        var undoResult = await action.UndoAsync();

        // Assert - Undo succeeded
        undoResult.Success.Should().BeTrue();
        action.CurrentContent.Should().Be(originalContent, "undo should restore original");
    }

    [Fact]
    public async Task DesanitizeAction_ExecuteAndUndo_RestoresCorrectStates()
    {
        // Arrange
        var sanitizedContent = "Connect to SERVER_0 at PORT_1";
        
        // Manually add mappings to session (simulating prior sanitization)
        _session.AddMapping("prod-server-01", "SERVER_0", PatternCategory.Server);
        _session.AddMapping("8080", "PORT_1", PatternCategory.Custom);

        var action = new DesanitizeAction(sanitizedContent, _desanitizationEngine, _session);

        // Act - Execute desanitization
        var executeResult = await action.ExecuteAsync();

        // Assert - Execute succeeded
        executeResult.Success.Should().BeTrue();
        action.CurrentContent.Should().Contain("prod-server-01");
        action.CurrentContent.Should().Contain("8080");

        // Act - Undo desanitization
        var undoResult = await action.UndoAsync();

        // Assert - Undo restores sanitized form
        undoResult.Success.Should().BeTrue();
        action.CurrentContent.Should().Be(sanitizedContent);
    }

    [Fact]
    public async Task SanitizeAction_Properties_AreCorrect()
    {
        // Arrange
        var content = "Test content";
        var options = new SanitizationOptions();
        var action = new SanitizeAction(content, options, _sanitizationEngine);

        // Assert
        action.Id.Should().NotBeEmpty();
        action.Description.Should().Contain("Sanitize");
        action.OperationType.Should().Be(SanitizationOperationType.Sanitize);
        action.CurrentContent.Should().Be(content);
    }

    [Fact]
    public async Task DesanitizeAction_Properties_AreCorrect()
    {
        // Arrange
        var sanitizedContent = "Sanitized text";
        var action = new DesanitizeAction(sanitizedContent, _desanitizationEngine, _session);

        // Assert
        action.Id.Should().NotBeEmpty();
        action.Description.Should().Contain("Desanitize");
        action.OperationType.Should().Be(SanitizationOperationType.Desanitize);
        action.CurrentContent.Should().Be(sanitizedContent);
    }

    [Fact]
    public void SanitizeAction_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new SanitizeAction(
            null!,
            new SanitizationOptions(),
            _sanitizationEngine);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DesanitizeAction_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new DesanitizeAction(
            null!,
            _desanitizationEngine,
            _session);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SanitizeAction_WorksWithUndoRedoManager()
    {
        // Arrange
        var content = "Test content for undo/redo";
        var options = new SanitizationOptions();
        var action = new SanitizeAction(content, options, _sanitizationEngine);
        var adapter = new SimpleActionAdapter(action);
        var manager = new ShieldPrompt.Application.Services.UndoRedoManager();

        // Act - Execute via manager
        await manager.ExecuteAsync(adapter);

        // Assert
        manager.CanUndo.Should().BeTrue();
        manager.CanRedo.Should().BeFalse();

        // Act - Undo
        await manager.UndoAsync();

        // Assert
        manager.CanUndo.Should().BeFalse();
        manager.CanRedo.Should().BeTrue();

        // Act - Redo
        await manager.RedoAsync();

        // Assert
        manager.CanUndo.Should().BeTrue();
        manager.CanRedo.Should().BeFalse();
    }

    /// <summary>
    /// Simple adapter for testing ISanitizationAction with UndoRedoManager.
    /// </summary>
    private class SimpleActionAdapter : ShieldPrompt.Application.Interfaces.IUndoableAction
    {
        private readonly ShieldPrompt.Application.Interfaces.Actions.IAction _action;

        public SimpleActionAdapter(ShieldPrompt.Application.Interfaces.Actions.IAction action)
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
        public bool RequiresConfirmation => false;
        public string? ConfirmationMessage => null;
    }
}

