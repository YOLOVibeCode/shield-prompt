using FluentAssertions;
using ShieldPrompt.Sanitization.Patterns;

namespace ShieldPrompt.Tests.Unit.Sanitization;

public class BuiltInPatternsTests
{
    [Theory]
    [InlineData("ProductionDB")]
    [InlineData("staging-mysql")]
    [InlineData("dev_postgres")]
    [InlineData("test-mongodb")]
    [InlineData("UAT_SQL")]
    public void ServerDatabaseNames_WithValidNames_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.ServerDatabaseNames;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("192.168.1.50")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    public void PrivateIPAddresses_WithPrivateIPs_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.PrivateIPAddresses;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("8.8.8.8")] // Public IP
    [InlineData("1.1.1.1")]
    public void PrivateIPAddresses_WithPublicIPs_DoesNotMatch(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.PrivateIPAddresses;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Server=prod-sql;")]
    [InlineData("Data Source=localhost;")]
    [InlineData("HOST=db.internal;")]
    public void ConnectionStrings_WithValidStrings_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.ConnectionStrings;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(@"C:\Users\Admin\Documents\secret.txt")]
    [InlineData(@"D:\Projects\MyApp\config.json")]
    [InlineData(@"\\fileserver\share\data\file.csv")]
    public void WindowsFilePaths_WithValidPaths_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.WindowsFilePaths;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("db-prod-01.internal.company.com")]
    [InlineData("api.corp.mycompany.io")]
    [InlineData("server.local.network")]
    public void InternalHostnames_WithInternalDomains_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.InternalHostnames;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("123-45-6789")]
    [InlineData("123 45 6789")]
    [InlineData("123456789")]
    public void SocialSecurityNumbers_WithValidFormats_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.SocialSecurityNumbers;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("4111 1111 1111 1111")] // Visa test card
    [InlineData("5500-0000-0000-0004")] // MasterCard
    [InlineData("3782 822463 10005")] // Amex
    public void CreditCards_WithValidFormats_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.CreditCards;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("AKIAIOSFODNN7EXAMPLE")]
    [InlineData("AKIAJG7L5WE12345678X")]
    public void AWSKeys_WithValidKeys_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.AWSKeys;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("ghp_1234567890123456789012345678901234567890")]
    [InlineData("gho_1234567890123456789012345678901234567890")]
    public void GitHubTokens_WithValidTokens_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.GitHubTokens;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OpenAIKeys_WithValidKey_Matches()
    {
        // Arrange
        var pattern = BuiltInPatterns.OpenAIKeys;
        var testKey = "sk-" + new string('a', 48);

        // Act
        var result = pattern.Matches(testKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AnthropicKeys_WithValidKey_Matches()
    {
        // Arrange
        var pattern = BuiltInPatterns.AnthropicKeys;
        var testKey = "sk-ant-" + new string('a', 88);

        // Act
        var result = pattern.Matches(testKey);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("-----BEGIN RSA PRIVATE KEY-----")]
    [InlineData("-----BEGIN EC PRIVATE KEY-----")]
    [InlineData("-----BEGIN OPENSSH PRIVATE KEY-----")]
    [InlineData("-----BEGIN PRIVATE KEY-----")]
    public void PrivateKeys_WithPEMHeaders_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.PrivateKeys;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("password = \"MySecret123\"")]
    [InlineData("apiKey: 'abc123def456'")]
    [InlineData("AUTH_TOKEN=secretvalue123")]
    public void PasswordsInCode_WithVariousFormats_Matches(string input)
    {
        // Arrange
        var pattern = BuiltInPatterns.PasswordsInCode;

        // Act
        var result = pattern.Matches(input);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void JWTTokens_WithValidJWT_Matches()
    {
        // Arrange
        var pattern = BuiltInPatterns.JWTTokens;
        var testJWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

        // Act
        var result = pattern.Matches(testJWT);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAll_Returns14Patterns()
    {
        // Act
        var patterns = BuiltInPatterns.GetAll().ToList();

        // Assert
        patterns.Should().HaveCount(14);
        patterns.Should().OnlyHaveUniqueItems(p => p.Name);
    }

    [Fact]
    public void GetAll_PatternsHaveValidPriorities()
    {
        // Act
        var patterns = BuiltInPatterns.GetAll();

        // Assert
        patterns.Should().AllSatisfy(p => p.Priority.Should().BeGreaterThan(0));
    }

    [Fact]
    public void GetAll_PatternsAreEnabledByDefault()
    {
        // Act
        var patterns = BuiltInPatterns.GetAll();

        // Assert
        patterns.Should().AllSatisfy(p => p.IsEnabled.Should().BeTrue());
    }
}

