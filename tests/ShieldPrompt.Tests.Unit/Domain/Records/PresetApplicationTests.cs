using FluentAssertions;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Records;

public class PresetApplicationTests
{
    [Fact]
    public void HasSelections_WhenFilesSelected_ReturnsTrue()
    {
        var result = new PresetApplication(5, 0, Array.Empty<string>());

        result.HasSelections.Should().BeTrue();
    }

    [Fact]
    public void HasSelections_WhenNoFilesSelected_ReturnsFalse()
    {
        var result = new PresetApplication(0, 0, Array.Empty<string>());

        result.HasSelections.Should().BeFalse();
    }

    [Fact]
    public void HasWarnings_WhenFilesNotFound_ReturnsTrue()
    {
        var result = new PresetApplication(0, 2, Array.Empty<string>());

        result.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public void HasWarnings_WhenWarningsExist_ReturnsTrue()
    {
        var result = new PresetApplication(5, 0, new[] { "Some warning" });

        result.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public void HasWarnings_WhenNoIssues_ReturnsFalse()
    {
        var result = new PresetApplication(5, 0, Array.Empty<string>());

        result.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void Empty_HasNoSelectionsOrWarnings()
    {
        var result = PresetApplication.Empty;

        result.FilesSelected.Should().Be(0);
        result.FilesNotFound.Should().Be(0);
        result.Warnings.Should().BeEmpty();
        result.HasSelections.Should().BeFalse();
        result.HasWarnings.Should().BeFalse();
    }
}
