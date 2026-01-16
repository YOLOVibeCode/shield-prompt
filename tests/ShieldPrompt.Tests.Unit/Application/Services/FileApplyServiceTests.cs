using FluentAssertions;
using NSubstitute;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class FileApplyServiceTests : IDisposable
{
    private readonly IBackupService _backupService;
    private readonly IDiffService _diffService;
    private readonly FileApplyService _sut;
    private readonly string _tempDir;

    public FileApplyServiceTests()
    {
        _backupService = Substitute.For<IBackupService>();
        _diffService = Substitute.For<IDiffService>();
        _sut = new FileApplyService(_backupService, _diffService);
        _tempDir = Path.Combine(Path.GetTempPath(), $"ShieldPromptApplyTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    #region PreviewAsync Tests

    [Fact]
    public async Task PreviewAsync_WithUpdateOperation_ReturnsPreviewWithDiff()
    {
        var existingFile = Path.Combine(_tempDir, "test.cs");
        await File.WriteAllTextAsync(existingFile, "original content");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Update, "test.cs", "new content", "Refactor")
        };

        _diffService.ComputeDiff("original content", "new content")
            .Returns(new[] { new DiffLine(DiffLineType.Modified, 1, 1, "changed") });

        var preview = await _sut.PreviewAsync(operations, _tempDir);

        preview.TotalFiles.Should().Be(1);
        preview.UpdatedCount.Should().Be(1);
        preview.Previews[0].Diff.Should().NotBeNull();
    }

    [Fact]
    public async Task PreviewAsync_WithCreateOperation_SetsCorrectCounts()
    {
        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "new.cs", "content", "New file")
        };

        var preview = await _sut.PreviewAsync(operations, _tempDir);

        preview.CreatedCount.Should().Be(1);
        preview.UpdatedCount.Should().Be(0);
        preview.DeletedCount.Should().Be(0);
    }

    [Fact]
    public async Task PreviewAsync_WithDeleteOperation_SetsCorrectCounts()
    {
        var existingFile = Path.Combine(_tempDir, "delete.cs");
        await File.WriteAllTextAsync(existingFile, "content");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Delete, "delete.cs", null, "Removing obsolete file")
        };

        var preview = await _sut.PreviewAsync(operations, _tempDir);

        preview.DeletedCount.Should().Be(1);
    }

    [Fact]
    public async Task PreviewAsync_DetectsConflicts()
    {
        var existingFile = Path.Combine(_tempDir, "exists.cs");
        await File.WriteAllTextAsync(existingFile, "content");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "exists.cs", "new content", "Create")
        };

        var preview = await _sut.PreviewAsync(operations, _tempDir);

        preview.Previews[0].HasConflict.Should().BeTrue();
        preview.Warnings.Should().NotBeEmpty();
    }

    #endregion

    #region ApplyAsync Tests

    [Fact]
    public async Task ApplyAsync_WithCreateOperation_CreatesFile()
    {
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "new.cs", "new file content", "Create new file")
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.SuccessCount.Should().Be(1);
        File.Exists(Path.Combine(_tempDir, "new.cs")).Should().BeTrue();
        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "new.cs"));
        content.Should().Be("new file content");
    }

    [Fact]
    public async Task ApplyAsync_WithUpdateOperation_UpdatesFile()
    {
        var existingFile = Path.Combine(_tempDir, "existing.cs");
        await File.WriteAllTextAsync(existingFile, "old content");

        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Update, "existing.cs", "new content", "Update file")
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.SuccessCount.Should().Be(1);
        var content = await File.ReadAllTextAsync(existingFile);
        content.Should().Be("new content");
    }

    [Fact]
    public async Task ApplyAsync_WithDeleteOperation_DeletesFile()
    {
        var fileToDelete = Path.Combine(_tempDir, "delete-me.cs");
        await File.WriteAllTextAsync(fileToDelete, "to be deleted");

        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Delete, "delete-me.cs", null, "Remove obsolete")
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.SuccessCount.Should().Be(1);
        File.Exists(fileToDelete).Should().BeFalse();
    }

    [Fact]
    public async Task ApplyAsync_WithRenameOperation_RenamesFile()
    {
        var originalFile = Path.Combine(_tempDir, "old-name.cs");
        await File.WriteAllTextAsync(originalFile, "content");

        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Rename, "new-name.cs", null, "Rename file", OriginalPath: "old-name.cs")
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.SuccessCount.Should().Be(1);
        File.Exists(originalFile).Should().BeFalse();
        File.Exists(Path.Combine(_tempDir, "new-name.cs")).Should().BeTrue();
    }

    [Fact]
    public async Task ApplyAsync_CreatesBackupBeforeApplying()
    {
        var existingFile = Path.Combine(_tempDir, "existing.cs");
        await File.WriteAllTextAsync(existingFile, "existing content");

        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Update, "existing.cs", "updated", "Update")
        };

        await _sut.ApplyAsync(operations, _tempDir);

        await _backupService.Received(1).CreateBackupAsync(
            Arg.Is<IEnumerable<string>>(paths => paths.Any(p => p.Contains("existing.cs"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_CreatesSubdirectories()
    {
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "src/nested/deep/new.cs", "content", "Create nested file")
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.SuccessCount.Should().Be(1);
        File.Exists(Path.Combine(_tempDir, "src", "nested", "deep", "new.cs")).Should().BeTrue();
    }

    [Fact]
    public async Task ApplyAsync_ReturnsBackupId()
    {
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "new.cs", "content", "Create")
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.BackupId.Should().Be("backup-123");
    }

    #endregion

    #region ApplySelectiveAsync Tests

    [Fact]
    public async Task ApplySelectiveAsync_OnlyAppliesSelectedPaths()
    {
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "file1.cs", "content1", "Create file 1"),
            new FileOperation(FileOperationType.Create, "file2.cs", "content2", "Create file 2"),
            new FileOperation(FileOperationType.Create, "file3.cs", "content3", "Create file 3")
        };

        var result = await _sut.ApplySelectiveAsync(operations, new[] { "file1.cs", "file3.cs" }, _tempDir);

        result.SuccessCount.Should().Be(2);
        File.Exists(Path.Combine(_tempDir, "file1.cs")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDir, "file2.cs")).Should().BeFalse();
        File.Exists(Path.Combine(_tempDir, "file3.cs")).Should().BeTrue();
    }

    #endregion

    #region UndoAsync Tests

    [Fact]
    public async Task UndoAsync_RestoresFromBackup()
    {
        _backupService.RestoreBackupAsync("backup-123", Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.UndoAsync("backup-123");

        result.Should().BeTrue();
        await _backupService.Received(1).RestoreBackupAsync("backup-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UndoAsync_ReturnsFalseOnFailure()
    {
        _backupService.RestoreBackupAsync("backup-123", Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.UndoAsync("backup-123");

        result.Should().BeFalse();
    }

    #endregion

    #region CheckConflictsAsync Tests

    [Fact]
    public async Task CheckConflictsAsync_DetectsCreateConflict()
    {
        var existingFile = Path.Combine(_tempDir, "exists.cs");
        await File.WriteAllTextAsync(existingFile, "existing");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "exists.cs", "new content", "Create")
        };

        var conflicts = await _sut.CheckConflictsAsync(operations, _tempDir);

        conflicts.Should().HaveCount(1);
        conflicts[0].Type.Should().Be(ConflictType.FileCreatedExists);
    }

    [Fact]
    public async Task CheckConflictsAsync_DetectsDeleteConflict()
    {
        var operations = new[]
        {
            new FileOperation(FileOperationType.Delete, "nonexistent.cs", null, "Delete")
        };

        var conflicts = await _sut.CheckConflictsAsync(operations, _tempDir);

        conflicts.Should().HaveCount(1);
        conflicts[0].Type.Should().Be(ConflictType.FileDeleted);
    }

    [Fact]
    public async Task CheckConflictsAsync_NoConflictForValidOperations()
    {
        var existingFile = Path.Combine(_tempDir, "exists.cs");
        await File.WriteAllTextAsync(existingFile, "content");

        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "new.cs", "content", "Create"),
            new FileOperation(FileOperationType.Update, "exists.cs", "new content", "Update")
        };

        var conflicts = await _sut.CheckConflictsAsync(operations, _tempDir);

        conflicts.Should().BeEmpty();
    }

    #endregion

    #region PartialUpdate Tests

    [Fact]
    public async Task ApplyAsync_WithPartialUpdate_UpdatesSpecificLines()
    {
        var existingFile = Path.Combine(_tempDir, "test.cs");
        await File.WriteAllTextAsync(existingFile, "line1\nline2\nline3\nline4\nline5");

        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");

        var operations = new[]
        {
            new FileOperation(FileOperationType.PartialUpdate, "test.cs", "replaced2\nreplaced3", "Update lines 2-3", StartLine: 2, EndLine: 3)
        };

        var result = await _sut.ApplyAsync(operations, _tempDir);

        result.SuccessCount.Should().Be(1);
        var content = await File.ReadAllTextAsync(existingFile);
        content.Should().Contain("line1");
        content.Should().Contain("replaced2");
        content.Should().Contain("replaced3");
        content.Should().Contain("line4");
    }

    #endregion
}
