using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Services;

namespace ShieldPrompt.Tests.Unit.Sanitization;

public class PatternRegistryTests
{
    [Fact]
    public void AddPattern_WithValidPattern_AddsToRegistry()
    {
        // Arrange
        var sut = new PatternRegistry();
        var pattern = new Pattern("Test", @"\d+", PatternCategory.Custom);

        // Act
        sut.AddPattern(pattern);

        // Assert
        sut.GetPattern("Test").Should().Be(pattern);
    }

    [Fact]
    public void GetPatterns_WithNoFilter_ReturnsAll()
    {
        // Arrange
        var sut = new PatternRegistry();
        sut.AddPattern(new Pattern("DB", @"db", PatternCategory.Database));
        sut.AddPattern(new Pattern("IP", @"\d+", PatternCategory.IPAddress));

        // Act
        var result = sut.GetPatterns();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetPatterns_WithCategoryFilter_ReturnsOnlyThatCategory()
    {
        // Arrange
        var sut = new PatternRegistry();
        sut.AddPattern(new Pattern("DB", @"db", PatternCategory.Database));
        sut.AddPattern(new Pattern("IP", @"\d+", PatternCategory.IPAddress));
        sut.AddPattern(new Pattern("SSN", @"\d{3}-\d{2}-\d{4}", PatternCategory.SSN));

        // Act
        var result = sut.GetPatterns(PatternCategory.Database);

        // Assert
        result.Should().ContainSingle();
        result.First().Category.Should().Be(PatternCategory.Database);
    }

    [Fact]
    public void GetPattern_WithNonExistent_ReturnsNull()
    {
        // Arrange
        var sut = new PatternRegistry();

        // Act
        var result = sut.GetPattern("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RemovePattern_WithExisting_RemovesAndReturnsTrue()
    {
        // Arrange
        var sut = new PatternRegistry();
        sut.AddPattern(new Pattern("Test", @"\d+", PatternCategory.Custom));

        // Act
        var result = sut.RemovePattern("Test");

        // Assert
        result.Should().BeTrue();
        sut.GetPattern("Test").Should().BeNull();
    }

    [Fact]
    public void RemovePattern_WithNonExistent_ReturnsFalse()
    {
        // Arrange
        var sut = new PatternRegistry();

        // Act
        var result = sut.RemovePattern("NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AddPattern_WithDuplicateName_Overwrites()
    {
        // Arrange
        var sut = new PatternRegistry();
        var pattern1 = new Pattern("Test", @"\d+", PatternCategory.Custom);
        var pattern2 = new Pattern("Test", @"[a-z]+", PatternCategory.Custom);

        // Act
        sut.AddPattern(pattern1);
        sut.AddPattern(pattern2);

        // Assert
        var stored = sut.GetPattern("Test");
        stored.Should().Be(pattern2); // Latest wins
    }

    [Fact]
    public void GetPatterns_OnlyReturnsEnabledPatterns()
    {
        // Arrange
        var sut = new PatternRegistry();
        sut.AddPattern(new Pattern("Enabled", @"test", PatternCategory.Custom) { IsEnabled = true });
        sut.AddPattern(new Pattern("Disabled", @"test2", PatternCategory.Custom) { IsEnabled = false });

        // Act
        var result = sut.GetPatterns();

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("Enabled");
    }
}

