using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Services;

namespace ShieldPrompt.Tests.Unit.Sanitization;

public class AliasGeneratorTests
{
    [Fact]
    public void Generate_ForDatabase_ReturnsDATABASE_0()
    {
        // Arrange
        var sut = new AliasGenerator();

        // Act
        var result = sut.Generate(PatternCategory.Database);

        // Assert
        result.Should().Be("DATABASE_0");
    }

    [Fact]
    public void Generate_CalledTwice_IncrementsCounter()
    {
        // Arrange
        var sut = new AliasGenerator();

        // Act
        var first = sut.Generate(PatternCategory.Database);
        var second = sut.Generate(PatternCategory.Database);

        // Assert
        first.Should().Be("DATABASE_0");
        second.Should().Be("DATABASE_1");
    }

    [Fact]
    public void Generate_DifferentCategories_UseDifferentCounters()
    {
        // Arrange
        var sut = new AliasGenerator();

        // Act
        var db = sut.Generate(PatternCategory.Database);
        var ip = sut.Generate(PatternCategory.IPAddress);
        var db2 = sut.Generate(PatternCategory.Database);

        // Assert
        db.Should().Be("DATABASE_0");
        ip.Should().Be("IP_ADDRESS_0");
        db2.Should().Be("DATABASE_1");
    }

    [Theory]
    [InlineData(PatternCategory.Server, "SERVER_0")]
    [InlineData(PatternCategory.IPAddress, "IP_ADDRESS_0")]
    [InlineData(PatternCategory.Hostname, "HOSTNAME_0")]
    [InlineData(PatternCategory.SSN, "SSN_0")]
    [InlineData(PatternCategory.CreditCard, "CREDIT_CARD_0")]
    [InlineData(PatternCategory.AWSKey, "AWS_KEY_0")]
    [InlineData(PatternCategory.GitHubToken, "GITHUB_TOKEN_0")]
    [InlineData(PatternCategory.OpenAIKey, "OPENAI_KEY_0")]
    [InlineData(PatternCategory.PrivateKey, "PRIVATE_KEY_0")]
    [InlineData(PatternCategory.Password, "PASSWORD_0")]
    [InlineData(PatternCategory.BearerToken, "BEARER_TOKEN_0")]
    [InlineData(PatternCategory.Email, "EMAIL_0")]
    [InlineData(PatternCategory.Phone, "PHONE_0")]
    public void Generate_ForCategory_ReturnsCorrectPrefix(PatternCategory category, string expected)
    {
        // Arrange
        var sut = new AliasGenerator();

        // Act
        var result = sut.Generate(category);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Reset_AfterGenerating_ResetsCounters()
    {
        // Arrange
        var sut = new AliasGenerator();
        sut.Generate(PatternCategory.Database); // DATABASE_0
        sut.Generate(PatternCategory.Database); // DATABASE_1

        // Act
        sut.Reset();
        var result = sut.Generate(PatternCategory.Database);

        // Assert
        result.Should().Be("DATABASE_0"); // Counter reset
    }
}

