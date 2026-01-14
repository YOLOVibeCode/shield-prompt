using FluentAssertions;
using ShieldPrompt.Domain.ValueObjects;

namespace ShieldPrompt.Tests.Unit.Domain.ValueObjects;

public class TokenCountTests
{
    [Fact]
    public void TokenCount_WithValidCount_CreatesCorrectly()
    {
        // Arrange & Act
        var tokenCount = new TokenCount(100);

        // Assert
        tokenCount.Count.Should().Be(100);
    }

    [Fact]
    public void TokenCount_WithZero_CreatesCorrectly()
    {
        // Arrange & Act
        var tokenCount = new TokenCount(0);

        // Assert
        tokenCount.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TokenCount_WithNegativeValue_ThrowsArgumentException(int invalidCount)
    {
        // Act
        Action act = () => new TokenCount(invalidCount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Add_WithTwoCounts_ReturnsSum()
    {
        // Arrange
        var count1 = new TokenCount(100);
        var count2 = new TokenCount(50);

        // Act
        var result = count1.Add(count2);

        // Assert
        result.Count.Should().Be(150);
    }

    [Fact]
    public void Equals_WithSameCount_ReturnsTrue()
    {
        // Arrange
        var count1 = new TokenCount(100);
        var count2 = new TokenCount(100);

        // Act & Assert
        count1.Equals(count2).Should().BeTrue();
        (count1 == count2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCount_ReturnsFalse()
    {
        // Arrange
        var count1 = new TokenCount(100);
        var count2 = new TokenCount(50);

        // Act & Assert
        count1.Equals(count2).Should().BeFalse();
        (count1 != count2).Should().BeTrue();
    }

    [Fact]
    public void CompareTo_WithSmallerCount_ReturnsPositive()
    {
        // Arrange
        var count1 = new TokenCount(100);
        var count2 = new TokenCount(50);

        // Act
        var result = count1.CompareTo(count2);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithLargerCount_ReturnsNegative()
    {
        // Arrange
        var count1 = new TokenCount(50);
        var count2 = new TokenCount(100);

        // Act
        var result = count1.CompareTo(count2);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void CompareTo_WithEqualCount_ReturnsZero()
    {
        // Arrange
        var count1 = new TokenCount(100);
        var count2 = new TokenCount(100);

        // Act
        var result = count1.CompareTo(count2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ToString_ReturnsCountAsString()
    {
        // Arrange
        var tokenCount = new TokenCount(1234);

        // Act
        var result = tokenCount.ToString();

        // Assert
        result.Should().Be("1234");
    }
}

