using FluentAssertions;
using ShieldPrompt.Infrastructure.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class FileBackupServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _backupDir;
    private readonly FileBackupService _sut;

    public FileBackupServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ShieldPromptBackupTests_{Guid.NewGuid()}");
        _backupDir = Path.Combine(_testDir, "backups");
        Directory.CreateDirectory(_testDir);
        _sut = new FileBackupService(_backupDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    #region CreateBackupAsync Tests

    [Fact]
    public async Task CreateBackupAsync_ReturnsBackupId()
    {
        var file = Path.Combine(_testDir, "test.cs");
        await File.WriteAllTextAsync(file, "content");

        var backupId = await _sut.CreateBackupAsync(new[] { file });

        backupId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateBackupAsync_CopiesFiles()
    {
        var file = Path.Combine(_testDir, "test.cs");
        await File.WriteAllTextAsync(file, "content to backup");

        var backupId = await _sut.CreateBackupAsync(new[] { file });

        // Backup should contain the file
        var backups = await _sut.ListBackupsAsync();
        backups.Should().Contain(b => b.Id == backupId && b.FileCount == 1);
    }

    [Fact]
    public async Task CreateBackupAsync_WithMultipleFiles_BacksUpAll()
    {
        var file1 = Path.Combine(_testDir, "file1.cs");
        var file2 = Path.Combine(_testDir, "file2.cs");
        await File.WriteAllTextAsync(file1, "content1");
        await File.WriteAllTextAsync(file2, "content2");

        var backupId = await _sut.CreateBackupAsync(new[] { file1, file2 });

        var backups = await _sut.ListBackupsAsync();
        backups.Should().Contain(b => b.Id == backupId && b.FileCount == 2);
    }

    [Fact]
    public async Task CreateBackupAsync_WithNonexistentFiles_SkipsThose()
    {
        var existingFile = Path.Combine(_testDir, "exists.cs");
        await File.WriteAllTextAsync(existingFile, "content");

        var backupId = await _sut.CreateBackupAsync(new[] { existingFile, Path.Combine(_testDir, "nonexistent.cs") });

        var backups = await _sut.ListBackupsAsync();
        backups.Should().Contain(b => b.Id == backupId && b.FileCount == 1);
    }

    #endregion

    #region RestoreBackupAsync Tests

    [Fact]
    public async Task RestoreBackupAsync_RestoresFileContent()
    {
        var file = Path.Combine(_testDir, "test.cs");
        await File.WriteAllTextAsync(file, "original content");

        var backupId = await _sut.CreateBackupAsync(new[] { file });

        // Modify the file
        await File.WriteAllTextAsync(file, "modified content");

        // Restore
        var result = await _sut.RestoreBackupAsync(backupId);

        result.Should().BeTrue();
        var restoredContent = await File.ReadAllTextAsync(file);
        restoredContent.Should().Be("original content");
    }

    [Fact]
    public async Task RestoreBackupAsync_WithInvalidId_ReturnsFalse()
    {
        var result = await _sut.RestoreBackupAsync("nonexistent-backup");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreBackupAsync_RestoresDeletedFiles()
    {
        var file = Path.Combine(_testDir, "test.cs");
        await File.WriteAllTextAsync(file, "content");

        var backupId = await _sut.CreateBackupAsync(new[] { file });

        // Delete the file
        File.Delete(file);

        // Restore
        await _sut.RestoreBackupAsync(backupId);

        File.Exists(file).Should().BeTrue();
        var content = await File.ReadAllTextAsync(file);
        content.Should().Be("content");
    }

    #endregion

    #region DeleteBackupAsync Tests

    [Fact]
    public async Task DeleteBackupAsync_RemovesBackup()
    {
        var file = Path.Combine(_testDir, "test.cs");
        await File.WriteAllTextAsync(file, "content");

        var backupId = await _sut.CreateBackupAsync(new[] { file });

        await _sut.DeleteBackupAsync(backupId);

        var backups = await _sut.ListBackupsAsync();
        backups.Should().NotContain(b => b.Id == backupId);
    }

    [Fact]
    public async Task DeleteBackupAsync_WithInvalidId_DoesNotThrow()
    {
        var act = () => _sut.DeleteBackupAsync("nonexistent");

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ListBackupsAsync Tests

    [Fact]
    public async Task ListBackupsAsync_WhenEmpty_ReturnsEmptyList()
    {
        var backups = await _sut.ListBackupsAsync();

        backups.Should().BeEmpty();
    }

    [Fact]
    public async Task ListBackupsAsync_ReturnsAllBackups()
    {
        var file1 = Path.Combine(_testDir, "file1.cs");
        var file2 = Path.Combine(_testDir, "file2.cs");
        await File.WriteAllTextAsync(file1, "content1");
        await File.WriteAllTextAsync(file2, "content2");

        var backup1 = await _sut.CreateBackupAsync(new[] { file1 });
        var backup2 = await _sut.CreateBackupAsync(new[] { file2 });

        var backups = await _sut.ListBackupsAsync();

        backups.Should().HaveCount(2);
        backups.Select(b => b.Id).Should().Contain(backup1);
        backups.Select(b => b.Id).Should().Contain(backup2);
    }

    [Fact]
    public async Task ListBackupsAsync_OrdersNewestFirst()
    {
        var file = Path.Combine(_testDir, "test.cs");
        await File.WriteAllTextAsync(file, "content");

        var backup1 = await _sut.CreateBackupAsync(new[] { file });
        await Task.Delay(10); // Ensure different timestamps
        var backup2 = await _sut.CreateBackupAsync(new[] { file });

        var backups = await _sut.ListBackupsAsync();

        backups[0].Id.Should().Be(backup2);
        backups[1].Id.Should().Be(backup1);
    }

    #endregion
}
