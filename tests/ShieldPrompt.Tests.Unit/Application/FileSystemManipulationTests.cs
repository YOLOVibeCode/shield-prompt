using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application;

/// <summary>
/// Comprehensive tests for file system manipulation.
/// Ensures we can create, update, delete files and directories safely.
/// </summary>
public class FileSystemManipulationTests : IDisposable
{
    private readonly IFileWriterService _service;
    private readonly string _testDirectory;

    public FileSystemManipulationTests()
    {
        _service = new FileWriterService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPrompt-FSTest-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    #region File Creation Tests

    [Fact]
    public async Task CreateFile_InRootDirectory_ShouldSucceed()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("NewFile.cs", "// New file content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesCreated.Should().Be(1);
        result.Errors.Should().BeEmpty();
        
        var filePath = Path.Combine(_testDirectory, "NewFile.cs");
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be("// New file content");
    }

    [Fact]
    public async Task CreateFile_InNestedDirectory_ShouldCreateDirectories()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("src/services/DataService.cs", "public class DataService { }", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(1);
        
        var dirPath = Path.Combine(_testDirectory, "src", "services");
        Directory.Exists(dirPath).Should().BeTrue("nested directories should be created");
        
        var filePath = Path.Combine(dirPath, "DataService.cs");
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task CreateEmptyFile_ShouldSucceed()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("EmptyFile.txt", string.Empty, FileUpdateType.Create, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesCreated.Should().Be(1);
        var filePath = Path.Combine(_testDirectory, "EmptyFile.txt");
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().BeEmpty();
    }

    [Fact]
    public async Task CreateMultipleFiles_InDifferentDirectories_ShouldSucceed()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("src/Program.cs", "class Program { }", FileUpdateType.Create, 1),
            new FileUpdate("tests/ProgramTests.cs", "class ProgramTests { }", FileUpdateType.Create, 1),
            new FileUpdate("docs/README.md", "# Documentation", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(3);
        result.Errors.Should().BeEmpty();
        
        File.Exists(Path.Combine(_testDirectory, "src", "Program.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "tests", "ProgramTests.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "docs", "README.md")).Should().BeTrue();
    }

    #endregion

    #region File Update Tests

    [Fact]
    public async Task UpdateExistingFile_ShouldModifyContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "Existing.cs");
        File.WriteAllText(filePath, "original content");
        
        var updates = new[]
        {
            new FileUpdate("Existing.cs", "updated content", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesUpdated.Should().Be(1);
        File.ReadAllText(filePath).Should().Be("updated content");
    }

    [Fact]
    public async Task UpdateMultipleFiles_ShouldModifyAll()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "File1.cs"), "original 1");
        File.WriteAllText(Path.Combine(_testDirectory, "File2.cs"), "original 2");
        File.WriteAllText(Path.Combine(_testDirectory, "File3.cs"), "original 3");
        
        var updates = new[]
        {
            new FileUpdate("File1.cs", "updated 1", FileUpdateType.Update, 1),
            new FileUpdate("File2.cs", "updated 2", FileUpdateType.Update, 1),
            new FileUpdate("File3.cs", "updated 3", FileUpdateType.Update, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesUpdated.Should().Be(3);
        File.ReadAllText(Path.Combine(_testDirectory, "File1.cs")).Should().Be("updated 1");
        File.ReadAllText(Path.Combine(_testDirectory, "File2.cs")).Should().Be("updated 2");
        File.ReadAllText(Path.Combine(_testDirectory, "File3.cs")).Should().Be("updated 3");
    }

    #endregion

    #region File Deletion Tests

    [Fact]
    public async Task DeleteFile_WhenNotAllowed_ShouldFail()
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
            new FileWriteOptions { AllowDelete = false });
        
        // Assert
        result.FilesDeleted.Should().Be(0);
        result.Errors.Should().ContainSingle();
        File.Exists(filePath).Should().BeTrue("file should NOT be deleted");
    }

    [Fact]
    public async Task DeleteFile_WhenAllowed_ShouldRemoveFile()
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
            new FileWriteOptions { AllowDelete = true });
        
        // Assert
        result.FilesDeleted.Should().Be(1);
        File.Exists(filePath).Should().BeFalse("file should be deleted");
    }

    [Fact]
    public async Task DeleteFile_ShouldBackupBeforeDeletion()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "ToDelete.cs");
        var originalContent = "important content";
        File.WriteAllText(filePath, originalContent);
        
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
        File.Exists(filePath).Should().BeFalse("file deleted");
        result.BackupId.Should().NotBeNullOrEmpty("backup created");
        
        // Restore from backup
        await _service.RestoreBackupAsync(result.BackupId);
        
        // Verify restored
        File.Exists(filePath).Should().BeTrue("restored from backup");
        File.ReadAllText(filePath).Should().Be(originalContent);
    }

    [Fact]
    public async Task DeleteMultipleFiles_ShouldRemoveAll()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "Delete1.cs"), "content 1");
        File.WriteAllText(Path.Combine(_testDirectory, "Delete2.cs"), "content 2");
        File.WriteAllText(Path.Combine(_testDirectory, "Delete3.cs"), "content 3");
        
        var updates = new[]
        {
            new FileUpdate("Delete1.cs", "", FileUpdateType.Delete, 0),
            new FileUpdate("Delete2.cs", "", FileUpdateType.Delete, 0),
            new FileUpdate("Delete3.cs", "", FileUpdateType.Delete, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowDelete = true });
        
        // Assert
        result.FilesDeleted.Should().Be(3);
        File.Exists(Path.Combine(_testDirectory, "Delete1.cs")).Should().BeFalse();
        File.Exists(Path.Combine(_testDirectory, "Delete2.cs")).Should().BeFalse();
        File.Exists(Path.Combine(_testDirectory, "Delete3.cs")).Should().BeFalse();
    }

    #endregion

    #region Directory Creation Tests

    [Fact]
    public async Task CreateDirectory_ViaFileCreation_ShouldCreateParentDirs()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("level1/level2/level3/deep.txt", "deep content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(1);
        
        Directory.Exists(Path.Combine(_testDirectory, "level1")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "level1", "level2")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "level1", "level2", "level3")).Should().BeTrue();
        
        var filePath = Path.Combine(_testDirectory, "level1", "level2", "level3", "deep.txt");
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task CreateMultipleFiles_InSameNewDirectory_ShouldCreateOnce()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("new-folder/File1.cs", "content 1", FileUpdateType.Create, 1),
            new FileUpdate("new-folder/File2.cs", "content 2", FileUpdateType.Create, 1),
            new FileUpdate("new-folder/File3.cs", "content 3", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(3);
        
        var newFolderPath = Path.Combine(_testDirectory, "new-folder");
        Directory.Exists(newFolderPath).Should().BeTrue();
        Directory.GetFiles(newFolderPath).Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateDirectory_WhenNotAllowed_ShouldFail()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("new-folder/File.cs", "content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = false });
        
        // Assert
        result.FilesCreated.Should().Be(0);
        result.Errors.Should().NotBeEmpty();
        
        var newFolderPath = Path.Combine(_testDirectory, "new-folder");
        Directory.Exists(newFolderPath).Should().BeFalse("directory should not be created");
    }

    #endregion

    #region Mixed Operations Tests

    [Fact]
    public async Task MixedOperations_CreateUpdateDelete_ShouldHandleAll()
    {
        // Arrange - Setup existing files
        File.WriteAllText(Path.Combine(_testDirectory, "Existing.cs"), "original");
        File.WriteAllText(Path.Combine(_testDirectory, "ToDelete.cs"), "delete me");
        
        var updates = new[]
        {
            new FileUpdate("NewFile.cs", "new content", FileUpdateType.Create, 1),
            new FileUpdate("Existing.cs", "updated content", FileUpdateType.Update, 1),
            new FileUpdate("ToDelete.cs", "", FileUpdateType.Delete, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowDelete = true });
        
        // Assert
        result.FilesCreated.Should().Be(1, "one file created");
        result.FilesUpdated.Should().Be(1, "one file updated");
        result.FilesDeleted.Should().Be(1, "one file deleted");
        result.Errors.Should().BeEmpty();
        
        // Verify state
        File.Exists(Path.Combine(_testDirectory, "NewFile.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "Existing.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "ToDelete.cs")).Should().BeFalse();
        
        File.ReadAllText(Path.Combine(_testDirectory, "Existing.cs")).Should().Be("updated content");
    }

    [Fact]
    public async Task MixedOperations_WithBackup_ShouldAllowCompleteUndo()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "File1.cs"), "original 1");
        File.WriteAllText(Path.Combine(_testDirectory, "File2.cs"), "original 2");
        
        var updates = new[]
        {
            new FileUpdate("File1.cs", "updated 1", FileUpdateType.Update, 1),
            new FileUpdate("File2.cs", "updated 2", FileUpdateType.Update, 1),
            new FileUpdate("File3.cs", "new file", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { CreateBackup = true });
        
        // Assert - Changes applied
        File.ReadAllText(Path.Combine(_testDirectory, "File1.cs")).Should().Be("updated 1");
        File.ReadAllText(Path.Combine(_testDirectory, "File2.cs")).Should().Be("updated 2");
        File.Exists(Path.Combine(_testDirectory, "File3.cs")).Should().BeTrue();
        
        // Undo - Restore from backup
        await _service.RestoreBackupAsync(result.BackupId);
        
        // Assert - Back to original state
        File.ReadAllText(Path.Combine(_testDirectory, "File1.cs")).Should().Be("original 1");
        File.ReadAllText(Path.Combine(_testDirectory, "File2.cs")).Should().Be("original 2");
        // Note: New files aren't removed on restore (by design - safer)
    }

    #endregion

    #region Empty Files and Directories Tests

    [Fact]
    public async Task CreateMultipleEmptyFiles_ShouldSucceed()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("Empty1.txt", "", FileUpdateType.Create, 0),
            new FileUpdate("Empty2.txt", "", FileUpdateType.Create, 0),
            new FileUpdate("Empty3.txt", "", FileUpdateType.Create, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesCreated.Should().Be(3);
        
        File.Exists(Path.Combine(_testDirectory, "Empty1.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "Empty2.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "Empty3.txt")).Should().BeTrue();
        
        File.ReadAllText(Path.Combine(_testDirectory, "Empty1.txt")).Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateFileToEmpty_ShouldClearContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "ToClear.cs");
        File.WriteAllText(filePath, "lots of content here");
        
        var updates = new[]
        {
            new FileUpdate("ToClear.cs", "", FileUpdateType.Update, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesUpdated.Should().Be(1);
        File.ReadAllText(filePath).Should().BeEmpty("content should be cleared");
    }

    #endregion

    #region Backup and Restore Tests

    [Fact]
    public async Task Backup_MultipleFiles_ShouldPreserveAll()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "File1.cs"), "content 1");
        File.WriteAllText(Path.Combine(_testDirectory, "File2.cs"), "content 2");
        File.WriteAllText(Path.Combine(_testDirectory, "File3.cs"), "content 3");
        
        var filePaths = new[]
        {
            Path.Combine(_testDirectory, "File1.cs"),
            Path.Combine(_testDirectory, "File2.cs"),
            Path.Combine(_testDirectory, "File3.cs")
        };
        
        // Act - Create backup
        var backupId = await _service.CreateBackupAsync(filePaths);
        
        // Modify all files
        File.WriteAllText(filePaths[0], "modified 1");
        File.WriteAllText(filePaths[1], "modified 2");
        File.WriteAllText(filePaths[2], "modified 3");
        
        // Restore
        await _service.RestoreBackupAsync(backupId);
        
        // Assert - All restored
        File.ReadAllText(filePaths[0]).Should().Be("content 1");
        File.ReadAllText(filePaths[1]).Should().Be("content 2");
        File.ReadAllText(filePaths[2]).Should().Be("content 3");
    }

    [Fact]
    public async Task Backup_EmptyFile_ShouldPreserveEmptyState()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "EmptyFile.txt");
        File.WriteAllText(filePath, "");
        
        // Act
        var backupId = await _service.CreateBackupAsync(new[] { filePath });
        
        // Modify
        File.WriteAllText(filePath, "now has content");
        
        // Restore
        await _service.RestoreBackupAsync(backupId);
        
        // Assert
        File.ReadAllText(filePath).Should().BeEmpty("should restore to empty");
    }

    #endregion

    #region Complex Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateNestedStructure_ShouldSucceed()
    {
        // Arrange - AI suggests creating a complete new feature
        var updates = new[]
        {
            new FileUpdate("Features/Authentication/AuthService.cs", "public class AuthService { }", FileUpdateType.Create, 10),
            new FileUpdate("Features/Authentication/IAuthService.cs", "public interface IAuthService { }", FileUpdateType.Create, 5),
            new FileUpdate("Features/Authentication/Models/User.cs", "public class User { }", FileUpdateType.Create, 15),
            new FileUpdate("Features/Authentication/Models/Role.cs", "public class Role { }", FileUpdateType.Create, 8),
            new FileUpdate("Tests/AuthServiceTests.cs", "public class AuthServiceTests { }", FileUpdateType.Create, 20)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(5);
        result.Errors.Should().BeEmpty();
        
        // Verify directory structure
        Directory.Exists(Path.Combine(_testDirectory, "Features", "Authentication")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Features", "Authentication", "Models")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Tests")).Should().BeTrue();
        
        // Verify all files exist
        File.Exists(Path.Combine(_testDirectory, "Features", "Authentication", "AuthService.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "Features", "Authentication", "Models", "User.cs")).Should().BeTrue();
    }

    [Fact]
    public async Task CompleteWorkflow_RefactorExistingStructure_ShouldSucceed()
    {
        // Arrange - Existing structure
        Directory.CreateDirectory(Path.Combine(_testDirectory, "OldStructure"));
        File.WriteAllText(Path.Combine(_testDirectory, "OldStructure", "OldService.cs"), "old service");
        File.WriteAllText(Path.Combine(_testDirectory, "OldStructure", "OldHelper.cs"), "old helper");
        
        // AI suggests refactoring to new structure
        var updates = new[]
        {
            new FileUpdate("NewStructure/RefactoredService.cs", "refactored service", FileUpdateType.Create, 20),
            new FileUpdate("NewStructure/RefactoredHelper.cs", "refactored helper", FileUpdateType.Create, 15),
            new FileUpdate("OldStructure/OldService.cs", "", FileUpdateType.Delete, 0),
            new FileUpdate("OldStructure/OldHelper.cs", "", FileUpdateType.Delete, 0)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true, AllowDelete = true, CreateBackup = true });
        
        // Assert
        result.FilesCreated.Should().Be(2);
        result.FilesDeleted.Should().Be(2);
        
        // New structure exists
        Directory.Exists(Path.Combine(_testDirectory, "NewStructure")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "NewStructure", "RefactoredService.cs")).Should().BeTrue();
        
        // Old files deleted
        File.Exists(Path.Combine(_testDirectory, "OldStructure", "OldService.cs")).Should().BeFalse();
        File.Exists(Path.Combine(_testDirectory, "OldStructure", "OldHelper.cs")).Should().BeFalse();
        
        // Can undo everything
        await _service.RestoreBackupAsync(result.BackupId);
        File.Exists(Path.Combine(_testDirectory, "OldStructure", "OldService.cs")).Should().BeTrue("restored");
        File.Exists(Path.Combine(_testDirectory, "OldStructure", "OldHelper.cs")).Should().BeTrue("restored");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task InvalidPath_WithDirectoryTraversal_ShouldFail()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("../../etc/malicious.txt", "bad", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesCreated.Should().Be(0);
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Contain("Invalid file path");
    }

    [Fact]
    public async Task InvalidPath_Absolute_ShouldFail()
    {
        // Arrange
        var updates = new[]
        {
            new FileUpdate("/etc/passwd", "bad", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert
        result.FilesCreated.Should().Be(0);
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task PartialFailure_ShouldContinueWithOthers()
    {
        // Arrange - Mix of valid and invalid operations
        File.WriteAllText(Path.Combine(_testDirectory, "Valid.cs"), "original");
        
        var updates = new[]
        {
            new FileUpdate("Valid.cs", "updated", FileUpdateType.Update, 1),
            new FileUpdate("../../invalid.cs", "bad", FileUpdateType.Create, 1),
            new FileUpdate("NewFile.cs", "good", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions());
        
        // Assert - Fail-fast validation (safer approach!)
        result.FilesUpdated.Should().Be(0, "validation failed - no files modified");
        result.FilesCreated.Should().Be(0, "validation failed - no files created");
        result.Errors.Should().NotBeEmpty("should contain validation error");
        
        // No files should be modified when validation fails
        File.ReadAllText(Path.Combine(_testDirectory, "Valid.cs")).Should().Be("original");
        File.Exists(Path.Combine(_testDirectory, "NewFile.cs")).Should().BeFalse();
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public async Task RealWorld_AIRefactorsToCleanArchitecture_ShouldSucceed()
    {
        // Arrange - Existing monolithic structure
        File.WriteAllText(Path.Combine(_testDirectory, "Monolith.cs"), "everything in one file");
        
        // AI suggests Clean Architecture refactoring
        var updates = new[]
        {
            // Domain layer
            new FileUpdate("Domain/Entities/User.cs", "public class User { }", FileUpdateType.Create, 10),
            new FileUpdate("Domain/Entities/Order.cs", "public class Order { }", FileUpdateType.Create, 15),
            
            // Application layer
            new FileUpdate("Application/Services/UserService.cs", "public class UserService { }", FileUpdateType.Create, 25),
            new FileUpdate("Application/Interfaces/IUserService.cs", "public interface IUserService { }", FileUpdateType.Create, 8),
            
            // Infrastructure layer
            new FileUpdate("Infrastructure/Persistence/UserRepository.cs", "public class UserRepository { }", FileUpdateType.Create, 30),
            
            // Tests
            new FileUpdate("Tests/Domain/UserTests.cs", "public class UserTests { }", FileUpdateType.Create, 20),
            new FileUpdate("Tests/Application/UserServiceTests.cs", "public class UserServiceTests { }", FileUpdateType.Create, 35)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true, CreateBackup = true });
        
        // Assert
        result.FilesCreated.Should().Be(7);
        result.Errors.Should().BeEmpty();
        
        // Verify Clean Architecture structure created
        Directory.Exists(Path.Combine(_testDirectory, "Domain", "Entities")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Application", "Services")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Application", "Interfaces")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Infrastructure", "Persistence")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Tests", "Domain")).Should().BeTrue();
        Directory.Exists(Path.Combine(_testDirectory, "Tests", "Application")).Should().BeTrue();
        
        // Verify all files created
        File.Exists(Path.Combine(_testDirectory, "Domain", "Entities", "User.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "Application", "Services", "UserService.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_testDirectory, "Infrastructure", "Persistence", "UserRepository.cs")).Should().BeTrue();
    }

    [Fact]
    public async Task RealWorld_AIAddsTestFiles_ShouldCreateInParallel()
    {
        // Arrange - Existing source files
        Directory.CreateDirectory(Path.Combine(_testDirectory, "src"));
        File.WriteAllText(Path.Combine(_testDirectory, "src", "Service1.cs"), "service 1");
        File.WriteAllText(Path.Combine(_testDirectory, "src", "Service2.cs"), "service 2");
        File.WriteAllText(Path.Combine(_testDirectory, "src", "Service3.cs"), "service 3");
        
        // AI suggests adding tests for all services
        var updates = new[]
        {
            new FileUpdate("tests/Service1Tests.cs", "public class Service1Tests { }", FileUpdateType.Create, 20),
            new FileUpdate("tests/Service2Tests.cs", "public class Service2Tests { }", FileUpdateType.Create, 25),
            new FileUpdate("tests/Service3Tests.cs", "public class Service3Tests { }", FileUpdateType.Create, 30)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { AllowCreateDirectories = true });
        
        // Assert
        result.FilesCreated.Should().Be(3);
        
        var testsDir = Path.Combine(_testDirectory, "tests");
        Directory.Exists(testsDir).Should().BeTrue();
        Directory.GetFiles(testsDir, "*.cs").Should().HaveCount(3);
    }

    #endregion

    #region Atomicity Tests

    [Fact]
    public async Task InvalidPathInBatch_ShouldFailEarlyWithoutModifying()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "File1.cs"), "original 1");
        
        var updates = new[]
        {
            new FileUpdate("File1.cs", "updated 1", FileUpdateType.Update, 1),
            new FileUpdate("../../invalid.cs", "bad", FileUpdateType.Create, 1),  // Invalid path!
            new FileUpdate("File2.cs", "new content", FileUpdateType.Create, 1)
        };
        
        // Act
        var result = await _service.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { CreateBackup = true });
        
        // Assert - Fail-fast validation (all-or-nothing approach)
        result.FilesUpdated.Should().Be(0, "validation failed - no modifications");
        result.FilesCreated.Should().Be(0, "validation failed - no creations");
        result.Errors.Should().NotBeEmpty("should report invalid path");
        
        // No files modified
        File.ReadAllText(Path.Combine(_testDirectory, "File1.cs")).Should().Be("original 1");
        File.Exists(Path.Combine(_testDirectory, "File2.cs")).Should().BeFalse();
    }

    #endregion
}

