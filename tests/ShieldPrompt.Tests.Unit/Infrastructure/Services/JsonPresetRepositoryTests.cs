using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Infrastructure.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class JsonPresetRepositoryTests : IDisposable
{
    private readonly string _testDir;
    private readonly JsonPresetRepository _sut;

    public JsonPresetRepositoryTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ShieldPromptTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
        _sut = new JsonPresetRepository(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    #region GetGlobalPresetsAsync Tests

    [Fact]
    public async Task GetGlobalPresetsAsync_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _sut.GetGlobalPresetsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGlobalPresetsAsync_ReturnsOnlyGlobalPresets()
    {
        var globalPreset = PromptPreset.Create("Global") with { Scope = PresetScope.Global };
        var workspacePreset = PromptPreset.Create("Workspace", "ws-1");

        await _sut.SaveAsync(globalPreset);
        await _sut.SaveAsync(workspacePreset);

        var result = await _sut.GetGlobalPresetsAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Global");
    }

    #endregion

    #region GetWorkspacePresetsAsync Tests

    [Fact]
    public async Task GetWorkspacePresetsAsync_ReturnsPresetsForWorkspace()
    {
        var ws1Preset = PromptPreset.Create("WS1 Preset", "ws-1");
        var ws2Preset = PromptPreset.Create("WS2 Preset", "ws-2");

        await _sut.SaveAsync(ws1Preset);
        await _sut.SaveAsync(ws2Preset);

        var result = await _sut.GetWorkspacePresetsAsync("ws-1");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("WS1 Preset");
    }

    [Fact]
    public async Task GetWorkspacePresetsAsync_DoesNotIncludeGlobalPresets()
    {
        var globalPreset = PromptPreset.Create("Global") with { Scope = PresetScope.Global };
        var workspacePreset = PromptPreset.Create("Workspace", "ws-1");

        await _sut.SaveAsync(globalPreset);
        await _sut.SaveAsync(workspacePreset);

        var result = await _sut.GetWorkspacePresetsAsync("ws-1");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Workspace");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingPreset_ReturnsPreset()
    {
        var preset = PromptPreset.Create("Test");
        await _sut.SaveAsync(preset);

        var result = await _sut.GetByIdAsync(preset.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync("nonexistent-id");

        result.Should().BeNull();
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_CreatesNewPreset()
    {
        var preset = PromptPreset.Create("New Preset");

        await _sut.SaveAsync(preset);

        var retrieved = await _sut.GetByIdAsync(preset.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("New Preset");
    }

    [Fact]
    public async Task SaveAsync_UpdatesExistingPreset()
    {
        var preset = PromptPreset.Create("Original");
        await _sut.SaveAsync(preset);

        var updated = preset with { Name = "Updated" };
        await _sut.SaveAsync(updated);

        var retrieved = await _sut.GetByIdAsync(preset.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task SaveAsync_PersistsAllProperties()
    {
        var preset = PromptPreset.Create("Full Preset") with
        {
            Description = "Test description",
            Icon = "🔧",
            CustomInstructions = "Custom",
            RoleId = "role-1",
            ModelId = "model-1",
            FilePatterns = new[] { "**/*.cs" },
            ExplicitFilePaths = new[] { "/file.cs" },
            IsPinned = true
        };

        await _sut.SaveAsync(preset);

        var retrieved = await _sut.GetByIdAsync(preset.Id);
        retrieved.Should().BeEquivalentTo(preset);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesPreset()
    {
        var preset = PromptPreset.Create("To Delete");
        await _sut.SaveAsync(preset);

        await _sut.DeleteAsync(preset.Id);

        var result = await _sut.GetByIdAsync(preset.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentId_DoesNotThrow()
    {
        var act = () => _sut.DeleteAsync("nonexistent-id");

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Persistence Tests

    [Fact]
    public async Task Presets_PersistAcrossInstances()
    {
        var preset = PromptPreset.Create("Persistent");
        await _sut.SaveAsync(preset);

        // Create new repository instance pointing to same directory
        var newRepository = new JsonPresetRepository(_testDir);
        var result = await newRepository.GetByIdAsync(preset.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Persistent");
    }

    #endregion
}
