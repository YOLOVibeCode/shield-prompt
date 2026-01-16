using FluentAssertions;
using NSubstitute;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class PresetServiceTests
{
    private readonly IPresetRepository _repository;
    private readonly PresetService _sut;

    public PresetServiceTests()
    {
        _repository = Substitute.For<IPresetRepository>();
        _sut = new PresetService(_repository);
    }

    #region CreateFromSession Tests

    [Fact]
    public void CreateFromSession_SetsPresetName()
    {
        var session = PromptSession.CreateNew("ws-1", "Test Session");

        var preset = _sut.CreateFromSession(session, "My Preset");

        preset.Name.Should().Be("My Preset");
    }

    [Fact]
    public void CreateFromSession_CapturesSelectedFilePaths()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            SelectedFilePaths = new[] { "/file1.cs", "/file2.cs" }
        };

        var preset = _sut.CreateFromSession(session, "Test Preset");

        preset.ExplicitFilePaths.Should().BeEquivalentTo("/file1.cs", "/file2.cs");
    }

    [Fact]
    public void CreateFromSession_CapturesCustomInstructions()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            CustomInstructions = "Review for security vulnerabilities"
        };

        var preset = _sut.CreateFromSession(session, "Security Review");

        preset.CustomInstructions.Should().Be("Review for security vulnerabilities");
    }

    [Fact]
    public void CreateFromSession_CapturesRoleId()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            SelectedRoleId = "security_expert"
        };

        var preset = _sut.CreateFromSession(session, "Test Preset");

        preset.RoleId.Should().Be("security_expert");
    }

    [Fact]
    public void CreateFromSession_CapturesModelId()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            SelectedModelId = "gpt-4o"
        };

        var preset = _sut.CreateFromSession(session, "Test Preset");

        preset.ModelId.Should().Be("gpt-4o");
    }

    [Fact]
    public void CreateFromSession_SetsWorkspaceIdFromSession()
    {
        var session = PromptSession.CreateNew("ws-123", "Test");

        var preset = _sut.CreateFromSession(session, "Test Preset");

        preset.WorkspaceId.Should().Be("ws-123");
        preset.Scope.Should().Be(PresetScope.Workspace);
    }

    #endregion

    #region ApplyToSessionAsync Tests

    [Fact]
    public async Task ApplyToSessionAsync_SelectsExistingFiles()
    {
        var preset = PromptPreset.Create("Test") with
        {
            ExplicitFilePaths = new[] { "/root/file1.cs" }
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        result.FilesSelected.Should().Be(1);
        result.FilesNotFound.Should().Be(0);
    }

    [Fact]
    public async Task ApplyToSessionAsync_ReportsMissingFiles()
    {
        var preset = PromptPreset.Create("Test") with
        {
            ExplicitFilePaths = new[] { "/nonexistent/file.cs" }
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        result.FilesNotFound.Should().Be(1);
        result.Warnings.Should().Contain(w => w.Contains("not found"));
    }

    [Fact]
    public async Task ApplyToSessionAsync_MatchesFilePatterns()
    {
        var preset = PromptPreset.Create("Test") with
        {
            FilePatterns = new[] { "*.cs" }
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        result.FilesSelected.Should().BeGreaterThanOrEqualTo(2); // file1.cs and file2.cs
    }

    [Fact]
    public async Task ApplyToSessionAsync_WithGlobPatterns_MatchesRecursively()
    {
        var preset = PromptPreset.Create("Test") with
        {
            FilePatterns = new[] { "**/*.cs" }
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        result.FilesSelected.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ApplyToSessionAsync_DeduplicatesFiles()
    {
        var preset = PromptPreset.Create("Test") with
        {
            ExplicitFilePaths = new[] { "/root/file1.cs" },
            FilePatterns = new[] { "*.cs" }  // Matches all .cs files by name
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        // file1.cs should only be counted once (explicit + pattern match)
        // Pattern *.cs matches: file1.cs, file2.cs, nested.cs (by name)
        result.FilesSelected.Should().Be(3);
    }

    #endregion

    #region RecordUsageAsync Tests

    [Fact]
    public async Task RecordUsageAsync_IncrementsUsageCount()
    {
        var preset = PromptPreset.Create("Test") with { Id = "preset-1", UsageCount = 5 };
        _repository.GetByIdAsync("preset-1", Arg.Any<CancellationToken>()).Returns(preset);

        await _sut.RecordUsageAsync("preset-1");

        await _repository.Received(1).SaveAsync(
            Arg.Is<PromptPreset>(p => p.UsageCount == 6),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordUsageAsync_UpdatesLastUsed()
    {
        var preset = PromptPreset.Create("Test") with
        {
            Id = "preset-1",
            LastUsed = DateTime.UtcNow.AddDays(-7)
        };
        _repository.GetByIdAsync("preset-1", Arg.Any<CancellationToken>()).Returns(preset);
        var beforeCall = DateTime.UtcNow;

        await _sut.RecordUsageAsync("preset-1");

        await _repository.Received(1).SaveAsync(
            Arg.Is<PromptPreset>(p => p.LastUsed >= beforeCall),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordUsageAsync_WithNonexistentPreset_DoesNotThrow()
    {
        _repository.GetByIdAsync("nonexistent", Arg.Any<CancellationToken>()).Returns((PromptPreset?)null);

        var act = () => _sut.RecordUsageAsync("nonexistent");

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetPinnedPresetsAsync Tests

    [Fact]
    public async Task GetPinnedPresetsAsync_ReturnsPinnedGlobalPresets()
    {
        var globalPresets = new[]
        {
            PromptPreset.Create("Pinned") with { IsPinned = true },
            PromptPreset.Create("Not Pinned") with { IsPinned = false }
        };
        _repository.GetGlobalPresetsAsync(Arg.Any<CancellationToken>()).Returns(globalPresets);

        var result = await _sut.GetPinnedPresetsAsync(null);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Pinned");
    }

    [Fact]
    public async Task GetPinnedPresetsAsync_IncludesWorkspacePresets()
    {
        var globalPresets = new[]
        {
            PromptPreset.Create("Global Pinned") with { IsPinned = true }
        };
        var workspacePresets = new[]
        {
            PromptPreset.Create("Workspace Pinned") with { IsPinned = true }
        };
        _repository.GetGlobalPresetsAsync(Arg.Any<CancellationToken>()).Returns(globalPresets);
        _repository.GetWorkspacePresetsAsync("ws-1", Arg.Any<CancellationToken>()).Returns(workspacePresets);

        var result = await _sut.GetPinnedPresetsAsync("ws-1");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPinnedPresetsAsync_OrdersByUsageCount()
    {
        var globalPresets = new[]
        {
            PromptPreset.Create("Less Used") with { IsPinned = true, UsageCount = 1 },
            PromptPreset.Create("Most Used") with { IsPinned = true, UsageCount = 10 }
        };
        _repository.GetGlobalPresetsAsync(Arg.Any<CancellationToken>()).Returns(globalPresets);

        var result = await _sut.GetPinnedPresetsAsync(null);

        result[0].Name.Should().Be("Most Used");
        result[1].Name.Should().Be("Less Used");
    }

    #endregion

    #region Helper Methods

    private static FileNode CreateTestFileTree()
    {
        var root = new FileNode("/root", "root", true);
        root.AddChild(new FileNode("/root/file1.cs", "file1.cs", false));
        root.AddChild(new FileNode("/root/file2.cs", "file2.cs", false));
        root.AddChild(new FileNode("/root/readme.md", "readme.md", false));

        var subdir = new FileNode("/root/subdir", "subdir", true);
        subdir.AddChild(new FileNode("/root/subdir/nested.cs", "nested.cs", false));
        root.AddChild(subdir);

        return root;
    }

    #endregion
}
