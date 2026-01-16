using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class PromptPresetTests
{
    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var preset1 = PromptPreset.Create("Test 1");
        var preset2 = PromptPreset.Create("Test 2");

        preset1.Id.Should().NotBeNullOrEmpty();
        preset2.Id.Should().NotBeNullOrEmpty();
        preset1.Id.Should().NotBe(preset2.Id);
    }

    [Fact]
    public void Create_SetsName()
    {
        var preset = PromptPreset.Create("My Preset");

        preset.Name.Should().Be("My Preset");
    }

    [Fact]
    public void Create_WithoutWorkspaceId_SetsGlobalScope()
    {
        var preset = PromptPreset.Create("Global Preset");

        preset.Scope.Should().Be(PresetScope.Global);
        preset.WorkspaceId.Should().BeNull();
    }

    [Fact]
    public void Create_WithWorkspaceId_SetsWorkspaceScope()
    {
        var preset = PromptPreset.Create("Workspace Preset", "ws-123");

        preset.Scope.Should().Be(PresetScope.Workspace);
        preset.WorkspaceId.Should().Be("ws-123");
    }

    [Fact]
    public void IsValid_WithRequiredProperties_ReturnsTrue()
    {
        var preset = PromptPreset.Create("Valid Preset");

        preset.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyId_ReturnsFalse()
    {
        var preset = new PromptPreset { Id = "", Name = "Test" };

        preset.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyName_ReturnsFalse()
    {
        var preset = new PromptPreset { Id = "123", Name = "" };

        preset.IsValid().Should().BeFalse();
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var preset = PromptPreset.Create("Test");

        preset.Description.Should().BeEmpty();
        preset.Icon.Should().Be("📋");
        preset.UsageCount.Should().Be(0);
        preset.FilePatterns.Should().BeEmpty();
        preset.ExplicitFilePaths.Should().BeEmpty();
        preset.CustomInstructions.Should().BeEmpty();
        preset.RoleId.Should().BeNull();
        preset.ModelId.Should().BeNull();
        preset.IsPinned.Should().BeFalse();
        preset.IsBuiltIn.Should().BeFalse();
    }

    [Fact]
    public void With_CanUpdateProperties()
    {
        var preset = PromptPreset.Create("Original");

        var updated = preset with
        {
            Name = "Updated",
            UsageCount = 5,
            IsPinned = true
        };

        updated.Name.Should().Be("Updated");
        updated.UsageCount.Should().Be(5);
        updated.IsPinned.Should().BeTrue();
        // Original should remain unchanged (immutability)
        preset.Name.Should().Be("Original");
    }
}
