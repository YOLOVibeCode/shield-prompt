using FluentAssertions;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Records;

public class ConflictInfoTests
{
    [Fact]
    public void Description_ForFileModified_ReturnsCorrectMessage()
    {
        var conflict = new ConflictInfo("src/test.cs", ConflictType.FileModified, "content", "new", "old");

        conflict.Description.Should().Contain("modified");
        conflict.Description.Should().Contain("src/test.cs");
    }

    [Fact]
    public void Description_ForFileDeleted_ReturnsCorrectMessage()
    {
        var conflict = new ConflictInfo("src/test.cs", ConflictType.FileDeleted, null, null, null);

        conflict.Description.Should().Contain("deleted");
        conflict.Description.Should().Contain("src/test.cs");
    }

    [Fact]
    public void Description_ForFileCreatedExists_ReturnsCorrectMessage()
    {
        var conflict = new ConflictInfo("src/new.cs", ConflictType.FileCreatedExists, "existing", "new", null);

        conflict.Description.Should().Contain("already exists");
        conflict.Description.Should().Contain("src/new.cs");
    }

    [Fact]
    public void Description_ForMergeConflict_ReturnsCorrectMessage()
    {
        var conflict = new ConflictInfo("src/test.cs", ConflictType.MergeConflict, "content", "new", null);

        conflict.Description.Should().Contain("Merge conflict");
        conflict.Description.Should().Contain("src/test.cs");
    }
}
