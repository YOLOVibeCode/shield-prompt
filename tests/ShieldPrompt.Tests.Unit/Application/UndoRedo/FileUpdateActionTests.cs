using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Application.Services.UndoRedo;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.UndoRedo;

/// <summary>
/// Tests for FileUpdateAction undo/redo behavior with confirmation.
/// </summary>
public class FileUpdateActionTests : IDisposable
{
    private readonly IFileWriterService _fileWriter;
    private readonly string _testDirectory;

    public FileUpdateActionTests()
    {
        _fileWriter = new FileWriterService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPrompt-UndoTest-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task FileUpdateAction_ShouldRequireConfirmation()
    {
        // Arrange
        var result = new FileWriteResult(1, 1, 0, "backup-123", Array.Empty<string>());
        var action = new FileUpdateAction("backup-123", result, _fileWriter);
        
        // Assert
        action.RequiresConfirmation.Should().BeTrue("file changes are important!");
        action.ConfirmationMessage.Should().NotBeNullOrEmpty("should have confirmation text");
        action.ConfirmationMessage.Should().Contain("⚠️");
        action.ConfirmationMessage.Should().Contain("undo");
    }

    [Fact]
    public async Task FileUpdateAction_Description_ShouldSummarizeChanges()
    {
        // Arrange
        var result = new FileWriteResult(2, 3, 1, "backup-123", Array.Empty<string>());
        var action = new FileUpdateAction("backup-123", result, _fileWriter);
        
        // Assert
        action.Description.Should().Contain("AI file changes");
        action.Description.Should().Contain("3 updated");
        action.Description.Should().Contain("2 created");
        action.Description.Should().Contain("1 deleted");
    }

    [Fact]
    public async Task FileUpdateAction_Undo_ShouldRestoreFromBackup()
    {
        // Arrange - Create files and backup
        var file1 = Path.Combine(_testDirectory, "File1.cs");
        var file2 = Path.Combine(_testDirectory, "File2.cs");
        File.WriteAllText(file1, "original 1");
        File.WriteAllText(file2, "original 2");
        
        var backupId = await _fileWriter.CreateBackupAsync(new[] { file1, file2 });
        
        // Simulate AI modifications
        File.WriteAllText(file1, "AI modified 1");
        File.WriteAllText(file2, "AI modified 2");
        
        var result = new FileWriteResult(0, 2, 0, backupId, Array.Empty<string>());
        var action = new FileUpdateAction(backupId, result, _fileWriter);
        
        // Act - Undo
        await action.UndoAsync();
        
        // Assert - Files restored
        File.ReadAllText(file1).Should().Be("original 1", "should restore original content");
        File.ReadAllText(file2).Should().Be("original 2", "should restore original content");
    }

    [Fact]
    public void FileUpdateAction_CanMergeWith_ShouldReturnFalse()
    {
        // Arrange
        var result = new FileWriteResult(1, 0, 0, "backup-123", Array.Empty<string>());
        var action1 = new FileUpdateAction("backup-1", result, _fileWriter);
        var action2 = new FileUpdateAction("backup-2", result, _fileWriter);
        
        // Act & Assert
        action1.CanMergeWith(action2).Should().BeFalse(
            "file updates should never merge - each is distinct");
    }

    [Fact]
    public async Task FileUpdateAction_CompleteWorkflow_WithUndo()
    {
        // Arrange - Original files
        var programPath = Path.Combine(_testDirectory, "Program.cs");
        var databasePath = Path.Combine(_testDirectory, "Database.cs");
        File.WriteAllText(programPath, "class Program { }");
        File.WriteAllText(databasePath, "class Database { }");
        
        // Act 1 - Apply AI changes
        var updates = new[]
        {
            new FileUpdate("Program.cs", "class Program { /* AI improved */ }", FileUpdateType.Update, 1),
            new FileUpdate("Database.cs", "class Database { /* AI improved */ }", FileUpdateType.Update, 1)
        };
        
        var result = await _fileWriter.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { CreateBackup = true });
        
        result.FilesUpdated.Should().Be(2);
        
        // Verify AI changes applied
        File.ReadAllText(programPath).Should().Contain("AI improved");
        File.ReadAllText(databasePath).Should().Contain("AI improved");
        
        // Act 2 - User presses Ctrl+Z
        var undoAction = new FileUpdateAction(result.BackupId, result, _fileWriter);
        await undoAction.UndoAsync();
        
        // Assert - Back to original
        File.ReadAllText(programPath).Should().Be("class Program { }");
        File.ReadAllText(databasePath).Should().Be("class Database { }");
    }

    [Fact]
    public void FileUpdateAction_ConfirmationMessage_ShouldDescribeUndo()
    {
        // Arrange
        var result = new FileWriteResult(2, 3, 1, "backup-123", Array.Empty<string>());
        var action = new FileUpdateAction("backup-123", result, _fileWriter);
        
        // Assert
        action.ConfirmationMessage.Should().Contain("⚠️");
        action.ConfirmationMessage.Should().Contain("undo");
        action.ConfirmationMessage.Should().Contain("AI file changes");
        action.ConfirmationMessage.Should().Contain("restore");
    }
}

