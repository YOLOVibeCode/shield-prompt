using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application;

/// <summary>
/// Tests for FileWriterService.
/// Ensures safe file operations with backups and rollback.
/// </summary>
public class FileWriterServiceTests : IDisposable
{
    private readonly IFileWriterService _service;
    private readonly string _testDirectory;

    public FileWriterServiceTests()
    {
        _service = new FileWriterService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPrompt-Test-{Guid.NewGuid()}");
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
    public async Task ApplyUpdates_WithSingleFileUpdate_ShouldWriteFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "Test.cs");
        File.WriteAllText(filePath, "original content");
        
        var updates = new[]
        {
            new FileUpdate("Test.cs", "new content", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesUpdated.Should().Be(1);
        result.Errors.Should().BeEmpty();
        File.ReadAllText(filePath).Should().Be("new content");
    }

    [Fact]
    public async Task ApplyUpdates_ShouldCreateBackup_BeforeWriting()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "Test.cs");
        File.WriteAllText(filePath, "original content");
        
        var updates = new[]
        {
            new FileUpdate("Test.cs", "new content", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { CreateBackup = true });
        
        // Assert
        result.BackupId.Should().NotBeNullOrEmpty("backup should be created");
        
        // Verify we can restore
        await _service.RestoreBackupAsync(result.BackupId);
        File.ReadAllText(filePath).Should().Be("original content", "should restore from backup");
    }

    [Fact]
    public async Task ApplyUpdates_CreateNewFile_ShouldCreateFile()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("NewFile.cs", "new file content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesCreated.Should().Be(1);
        var filePath = Path.Combine(_testDirectory, "NewFile.cs");
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be("new file content");
    }

    [Fact]
    public async Task ApplyUpdates_CreateFileInNewDirectory_ShouldCreateDirectory()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("src/NewFile.cs", "content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(1);
        var filePath = Path.Combine(_testDirectory, "src", "NewFile.cs");
        File.Exists(filePath).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "src")).Should().BeTrue();
    }

    [Fact]
    public async Task ApplyUpdates_WhenDirectoryCreationNotAllowed_ShouldFail()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("src/NewFile.cs", "content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = false });
        
        // Assert
        result.FilesCreated.Should().Be(0);
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task ApplyUpdates_MultipleFiles_ShouldApplyAllSuccessfully()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "File1.cs"), "original 1");
        File.WriteAllText(Path.Combine(_testDirectory, "File2.cs"), "original 2");
        
        var updates = new[]
        {
            new FileUpdate("File1.cs", "updated 1", FileUpdateType.Update, 1),
            new FileUpdate("File2.cs", "updated 2", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesUpdated.Should().Be(2);
        File.ReadAllText(Path.Combine(_testDirectory, "File1.cs")).Should().Be("updated 1");
        File.ReadAllText(Path.Combine(_testDirectory, "File2.cs")).Should().Be("updated 2");
    }

    [Fact]
    public async Task ApplyUpdates_DryRun_ShouldNotModifyFiles()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "Test.cs");
        File.WriteAllText(filePath, "original");
        
        var updates = new[]
        {
            new FileUpdate("Test.cs", "new content", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { DryRun = true });
        
        // Assert
        result.FilesUpdated.Should().Be(0, "dry run should not modify files");
        File.ReadAllText(filePath).Should().Be("original", "file should be unchanged");
    }

    [Fact]
    public async Task CreateBackup_ShouldBackupAllSpecifiedFiles()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "File1.cs");
        var file2 = Path.Combine(_testDirectory, "File2.cs");
        File.WriteAllText(file1, "content 1");
        File.WriteAllText(file2, "content 2");
        
        // Act
        var backupId = await _service.CreateBackupAsync(new[] { file1, file2 });
        
        // Assert
        backupId.Should().NotBeNullOrEmpty();
        
        // Modify files
        File.WriteAllText(file1, "modified 1");
        File.WriteAllText(file2, "modified 2");
        
        // Restore
        await _service.RestoreBackupAsync(backupId);
        
        // Verify restoration
        File.ReadAllText(file1).Should().Be("content 1");
        File.ReadAllText(file2).Should().Be("content 2");
    }

    [Fact]
    public async Task RestoreBackup_WithInvalidId_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.RestoreBackupAsync("invalid-backup-id"));
    }

    [Fact]
    public async Task ApplyUpdates_WithInvalidPath_ShouldAddError()
    {
        // Arrange - Path with directory traversal attempt
        var updates = new[]
        {
            new FileUpdate("../../../etc/passwd", "malicious", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesUpdated.Should().Be(0);
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Contain("Invalid file path");
    }

    [Fact]
    public async Task ApplyUpdates_DeleteType_WhenNotAllowed_ShouldAddError()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "ToDelete.cs");
        File.WriteAllText(filePath, "content");
        
        var updates = new[]
        {
            new FileUpdate("ToDelete.cs", "", FileUpdateType.Delete, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowDelete = false });
        
        // Assert
        result.FilesDeleted.Should().Be(0);
        result.Errors.Should().ContainSingle();
        File.Exists(filePath).Should().BeTrue("file should still exist");
    }

    [Fact]
    public async Task ApplyUpdates_DeleteType_WhenAllowed_ShouldMoveToBackup()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "ToDelete.cs");
        File.WriteAllText(filePath, "content to delete");
        
        var updates = new[]
        {
            new FileUpdate("ToDelete.cs", "", FileUpdateType.Delete, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowDelete = true, CreateBackup = true });
        
        // Assert
        result.FilesDeleted.Should().Be(1);
        File.Exists(filePath).Should().BeFalse("file should be deleted");
        
        // But can be restored from backup
        await _service.RestoreBackupAsync(result.BackupId);
        File.Exists(filePath).Should().BeTrue("should be restored from backup");
        File.ReadAllText(filePath).Should().Be("content to delete");
    }
}

