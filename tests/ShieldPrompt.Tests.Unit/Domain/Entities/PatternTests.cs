using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class PatternTests
{
    [Fact]
    public void Pattern_WithValidInputs_CreatesCorrectly()
    {
        // Arrange
        var name = "Database Names";
        var regex = @"(prod|dev).*db";
        var category = PatternCategory.Database;

        // Act
        var pattern = new Pattern(name, regex, category);

        // Assert
        pattern.Name.Should().Be(name);
        pattern.RegexPattern.Should().Be(regex);
        pattern.Category.Should().Be(category);
        pattern.IsEnabled.Should().BeTrue(); // Default
        pattern.Priority.Should().Be(100); // Default
    }

    [Fact]
    public void Pattern_WithNullName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new Pattern(null!, @"test", PatternCategory.Database);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Pattern_WithNullRegex_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new Pattern("Test", null!, PatternCategory.Database);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("regexPattern");
    }

    [Fact]
    public void Pattern_WithEmptyName_ThrowsArgumentException()
    {
        // Act
        Action act = () => new Pattern(string.Empty, @"test", PatternCategory.Database);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Pattern_WithCustomPriority_SetsPriority()
    {
        // Arrange
        var pattern = new Pattern("Test", @"test", PatternCategory.Custom)
        {
            Priority = 50
        };

        // Act & Assert
        pattern.Priority.Should().Be(50);
    }

    [Fact]
    public void Pattern_CanBeDisabled()
    {
        // Arrange
        var pattern = new Pattern("Test", @"test", PatternCategory.Database)
        {
            IsEnabled = false
        };

        // Act & Assert
        pattern.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void CreateRegex_WithValidPattern_ReturnsCompiledRegex()
    {
        // Arrange
        var pattern = new Pattern("Numbers", @"\d+", PatternCategory.Custom);

        // Act
        var regex = pattern.CreateRegex();

        // Assert
        regex.Should().NotBeNull();
        regex.IsMatch("123").Should().BeTrue();
        regex.IsMatch("abc").Should().BeFalse();
    }

    [Fact]
    public void CreateRegex_WithInvalidPattern_ThrowsArgumentException()
    {
        // Arrange - Invalid regex pattern
        var pattern = new Pattern("Bad", @"[invalid(", PatternCategory.Custom);

        // Act
        Action act = () => pattern.CreateRegex();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Matches_WithMatchingContent_ReturnsTrue()
    {
        // Arrange
        var pattern = new Pattern("Database", @"(?i)productiondb", PatternCategory.Database);

        // Act
        var result = pattern.Matches("Connect to ProductionDB");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_WithNonMatchingContent_ReturnsFalse()
    {
        // Arrange
        var pattern = new Pattern("Database", @"(?i)productiondb", PatternCategory.Database);

        // Act
        var result = pattern.Matches("Connect to localhost");

        // Assert
        result.Should().BeFalse();
    }
}

