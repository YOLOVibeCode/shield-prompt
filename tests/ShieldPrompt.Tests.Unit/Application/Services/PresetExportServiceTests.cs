using FluentAssertions;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class PresetExportServiceTests
{
    private readonly PresetExportService _sut;

    public PresetExportServiceTests()
    {
        _sut = new PresetExportService();
    }

    #region ExportToJson Tests

    [Fact]
    public void ExportToJson_ReturnsValidJson()
    {
        var preset = PromptPreset.Create("Test Preset");

        var json = _sut.ExportToJson(preset);

        json.Should().NotBeNullOrEmpty();
        json.Should().StartWith("{");
        json.Should().EndWith("}");
    }

    [Fact]
    public void ExportToJson_ContainsAllProperties()
    {
        var preset = PromptPreset.Create("My Preset") with
        {
            Description = "A test preset",
            Icon = "🔧",
            CustomInstructions = "Review carefully",
            RoleId = "security_expert",
            ExplicitFilePaths = new[] { "/file1.cs", "/file2.cs" }
        };

        var json = _sut.ExportToJson(preset);

        json.Should().Contain("My Preset");
        json.Should().Contain("A test preset");
        // Icon may be JSON-escaped (🔧 -> \uD83D\uDD27), so check for presence in either form
        json.Should().Match(j => j.Contains("🔧") || j.Contains("\\uD83D\\uDD27"));
        json.Should().Contain("Review carefully");
        json.Should().Contain("security_expert");
        json.Should().Contain("/file1.cs");
    }

    [Fact]
    public void ExportToJson_ExcludesUsageStatistics()
    {
        var preset = PromptPreset.Create("Test") with
        {
            UsageCount = 100,
            LastUsed = DateTime.UtcNow
        };

        var json = _sut.ExportToJson(preset);

        // UsageCount and LastUsed should not be exported for sharing
        json.Should().NotContain("\"UsageCount\"");
        json.Should().NotContain("\"LastUsed\"");
    }

    #endregion

    #region ImportFromJson Tests

    [Fact]
    public void ImportFromJson_ReturnsPreset()
    {
        var original = PromptPreset.Create("Test Preset") with
        {
            Description = "Imported preset",
            CustomInstructions = "Test instructions"
        };
        var json = _sut.ExportToJson(original);

        var imported = _sut.ImportFromJson(json);

        imported.Should().NotBeNull();
        imported!.Name.Should().Be("Test Preset");
        imported.Description.Should().Be("Imported preset");
        imported.CustomInstructions.Should().Be("Test instructions");
    }

    [Fact]
    public void ImportFromJson_GeneratesNewId()
    {
        var original = PromptPreset.Create("Test");
        var json = _sut.ExportToJson(original);

        var imported = _sut.ImportFromJson(json);

        imported.Should().NotBeNull();
        imported!.Id.Should().NotBe(original.Id);
    }

    [Fact]
    public void ImportFromJson_SetsIsBuiltInToFalse()
    {
        var original = PromptPreset.Create("Test") with { IsBuiltIn = true };
        var json = _sut.ExportToJson(original);

        var imported = _sut.ImportFromJson(json);

        imported.Should().NotBeNull();
        imported!.IsBuiltIn.Should().BeFalse();
    }

    [Fact]
    public void ImportFromJson_ResetsUsageStatistics()
    {
        var original = PromptPreset.Create("Test") with { UsageCount = 100 };
        var json = _sut.ExportToJson(original);

        var imported = _sut.ImportFromJson(json);

        imported.Should().NotBeNull();
        imported!.UsageCount.Should().Be(0);
    }

    [Fact]
    public void ImportFromJson_WithInvalidJson_ReturnsNull()
    {
        var invalidJson = "{ invalid json }";

        var result = _sut.ImportFromJson(invalidJson);

        result.Should().BeNull();
    }

    [Fact]
    public void ImportFromJson_WithEmptyJson_ReturnsNull()
    {
        var result = _sut.ImportFromJson("");

        result.Should().BeNull();
    }

    [Fact]
    public void ImportFromJson_WithMissingName_ReturnsNull()
    {
        var json = "{ \"Description\": \"No name\" }";

        var result = _sut.ImportFromJson(json);

        result.Should().BeNull();
    }

    [Fact]
    public void ImportFromJson_PreservesFilePatterns()
    {
        var original = PromptPreset.Create("Test") with
        {
            FilePatterns = new[] { "**/*.cs", "**/*.md" }
        };
        var json = _sut.ExportToJson(original);

        var imported = _sut.ImportFromJson(json);

        imported.Should().NotBeNull();
        imported!.FilePatterns.Should().BeEquivalentTo("**/*.cs", "**/*.md");
    }

    [Fact]
    public void ImportFromJson_PreservesExplicitFilePaths()
    {
        var original = PromptPreset.Create("Test") with
        {
            ExplicitFilePaths = new[] { "/src/app.cs", "/tests/tests.cs" }
        };
        var json = _sut.ExportToJson(original);

        var imported = _sut.ImportFromJson(json);

        imported.Should().NotBeNull();
        imported!.ExplicitFilePaths.Should().BeEquivalentTo("/src/app.cs", "/tests/tests.cs");
    }

    #endregion
}
