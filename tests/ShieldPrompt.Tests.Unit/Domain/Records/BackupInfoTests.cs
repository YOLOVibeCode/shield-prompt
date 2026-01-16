using FluentAssertions;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Records;

public class BackupInfoTests
{
    [Fact]
    public void HasFiles_WhenFileCountGreaterThanZero_ReturnsTrue()
    {
        var backup = new BackupInfo("id-1", DateTime.UtcNow, 3, new[] { "file1.cs", "file2.cs", "file3.cs" });

        backup.HasFiles.Should().BeTrue();
    }

    [Fact]
    public void HasFiles_WhenFileCountIsZero_ReturnsFalse()
    {
        var backup = new BackupInfo("id-1", DateTime.UtcNow, 0, Array.Empty<string>());

        backup.HasFiles.Should().BeFalse();
    }

    [Fact]
    public void Age_ReturnsTimeSinceCreation()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-30);
        var backup = new BackupInfo("id-1", createdAt, 1, new[] { "file.cs" });

        backup.Age.TotalMinutes.Should().BeGreaterThanOrEqualTo(29);
        backup.Age.TotalMinutes.Should().BeLessThan(35);
    }
}
