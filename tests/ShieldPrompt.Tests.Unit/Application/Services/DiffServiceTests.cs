using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class DiffServiceTests
{
    private readonly DiffService _sut;

    public DiffServiceTests()
    {
        _sut = new DiffService();
    }

    #region ComputeDiff Tests

    [Fact]
    public void ComputeDiff_WithIdenticalContent_ReturnsContextLines()
    {
        var content = "line1\nline2\nline3";

        var diff = _sut.ComputeDiff(content, content);

        diff.Should().OnlyContain(d => d.Type == DiffLineType.Context);
    }

    [Fact]
    public void ComputeDiff_WithAddedLine_ReturnsAddedType()
    {
        var original = "line1\nline3";
        var modified = "line1\nline2\nline3";

        var diff = _sut.ComputeDiff(original, modified);

        diff.Should().Contain(d => d.Type == DiffLineType.Added && d.Content == "line2");
    }

    [Fact]
    public void ComputeDiff_WithRemovedLine_ReturnsRemovedType()
    {
        var original = "line1\nline2\nline3";
        var modified = "line1\nline3";

        var diff = _sut.ComputeDiff(original, modified);

        diff.Should().Contain(d => d.Type == DiffLineType.Removed && d.Content == "line2");
    }

    [Fact]
    public void ComputeDiff_WithModifiedLine_ReturnsRemovedAndAdded()
    {
        var original = "line1\nold\nline3";
        var modified = "line1\nnew\nline3";

        var diff = _sut.ComputeDiff(original, modified);

        diff.Should().Contain(d => d.Type == DiffLineType.Removed && d.Content == "old");
        diff.Should().Contain(d => d.Type == DiffLineType.Added && d.Content == "new");
    }

    [Fact]
    public void ComputeDiff_SetsLineNumbers()
    {
        var original = "line1\nline2\nline3";
        var modified = "line1\nmodified\nline3";

        var diff = _sut.ComputeDiff(original, modified);

        diff.Should().Contain(d => d.OldLineNumber == 2 && d.Type == DiffLineType.Removed);
        diff.Should().Contain(d => d.NewLineNumber == 2 && d.Type == DiffLineType.Added);
    }

    [Fact]
    public void ComputeDiff_WithEmptyOriginal_ReturnsAllAdded()
    {
        var original = "";
        var modified = "line1\nline2";

        var diff = _sut.ComputeDiff(original, modified);

        diff.Should().OnlyContain(d => d.Type == DiffLineType.Added);
    }

    [Fact]
    public void ComputeDiff_WithEmptyModified_ReturnsAllRemoved()
    {
        var original = "line1\nline2";
        var modified = "";

        var diff = _sut.ComputeDiff(original, modified);

        diff.Should().OnlyContain(d => d.Type == DiffLineType.Removed);
    }

    #endregion

    #region GenerateUnifiedDiff Tests

    [Fact]
    public void GenerateUnifiedDiff_IncludesFileHeader()
    {
        var original = "line1";
        var modified = "line2";

        var diff = _sut.GenerateUnifiedDiff(original, modified, "src/test.cs");

        diff.Should().Contain("--- a/src/test.cs");
        diff.Should().Contain("+++ b/src/test.cs");
    }

    [Fact]
    public void GenerateUnifiedDiff_UsesCorrectPrefixes()
    {
        var original = "old line";
        var modified = "new line";

        var diff = _sut.GenerateUnifiedDiff(original, modified, "test.cs");

        diff.Should().Contain("-old line");
        diff.Should().Contain("+new line");
    }

    [Fact]
    public void GenerateUnifiedDiff_IncludesContextLines()
    {
        var original = "context\nold\ncontext";
        var modified = "context\nnew\ncontext";

        var diff = _sut.GenerateUnifiedDiff(original, modified, "test.cs");

        diff.Should().Contain(" context");
    }

    #endregion
}
