using FluentAssertions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Patterns;
using ShieldPrompt.Sanitization.Services;

namespace ShieldPrompt.Tests.Unit.Sanitization;

public class SanitizationEngineTests : IDisposable
{
    private readonly ISanitizationEngine _sut;
    private readonly IPatternRegistry _patternRegistry;
    private readonly IMappingSession _session;

    public SanitizationEngineTests()
    {
        _patternRegistry = new PatternRegistry();
        _session = new MappingSession();
        
        // Add all built-in patterns
        foreach (var pattern in BuiltInPatterns.GetAll())
        {
            _patternRegistry.AddPattern(pattern);
        }

        _sut = new SanitizationEngine(_patternRegistry, _session, new AliasGenerator());
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    [Fact]
    public void Sanitize_WithDatabaseName_ReplacesWithAlias()
    {
        // Arrange
        var content = "Connect to ProductionDB for data";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeTrue();
        result.SanitizedContent.Should().Contain("DATABASE_0");
        result.SanitizedContent.Should().NotContain("ProductionDB");
        result.Matches.Should().ContainSingle()
            .Which.Original.Should().Be("ProductionDB");
    }

    [Fact]
    public void Sanitize_WithPrivateIP_ReplacesWithAlias()
    {
        // Arrange
        var content = "Connect to 192.168.1.50 on port 3306";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeTrue();
        result.SanitizedContent.Should().Contain("IP_ADDRESS_0");
        result.SanitizedContent.Should().NotContain("192.168.1.50");
    }

    [Fact]
    public void Sanitize_WithMultipleMatches_ReplacesAll()
    {
        // Arrange
        var content = "ProductionDB at 192.168.1.50 and staging-mysql at 10.0.0.5";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeTrue();
        result.TotalMatches.Should().BeGreaterThanOrEqualTo(3); // 2 DBs + 2 IPs
        result.SanitizedContent.Should().NotContain("ProductionDB");
        result.SanitizedContent.Should().NotContain("192.168.1.50");
        result.SanitizedContent.Should().NotContain("staging-mysql");
        result.SanitizedContent.Should().NotContain("10.0.0.5");
    }

    [Fact]
    public void Sanitize_WithSameValueTwice_UsesSameAlias()
    {
        // Arrange
        var content = "ProductionDB here and ProductionDB there";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeTrue();
        var allAliases = result.Matches.Select(m => m.Alias).Distinct();
        allAliases.Should().ContainSingle(); // Same alias used
    }

    [Fact]
    public void Sanitize_WithNoSensitiveData_ReturnsUnchanged()
    {
        // Arrange
        var content = "This is just normal text with nothing sensitive";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeFalse();
        result.SanitizedContent.Should().Be(content);
        result.Matches.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_WithSSN_ReplacesWithAlias()
    {
        // Arrange
        var content = "SSN: 123-45-6789 for verification";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeTrue();
        result.SanitizedContent.Should().Contain("SSN_0");
        result.SanitizedContent.Should().NotContain("123-45-6789");
    }

    [Fact]
    public void Sanitize_WithAPIKey_ReplacesWithAlias()
    {
        // Arrange
        var content = "Use this key: AKIAIOSFODNN7EXAMPLE for AWS";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeTrue();
        result.SanitizedContent.Should().Contain("AWS_KEY_0");
        result.SanitizedContent.Should().NotContain("AKIAIOSFODNN7EXAMPLE");
    }

    [Fact]
    public void Sanitize_WithPIIDisabled_DoesNotSanitizePII()
    {
        // Arrange
        var content = "SSN: 123-45-6789 and ProductionDB";
        var options = new SanitizationOptions { EnablePII = false };

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.SanitizedContent.Should().Contain("123-45-6789"); // PII not sanitized
        result.SanitizedContent.Should().Contain("DATABASE_0"); // Infrastructure still sanitized
    }

    [Fact]
    public void Sanitize_WithInfrastructureDisabled_DoesNotSanitizeInfrastructure()
    {
        // Arrange
        var content = "SSN: 123-45-6789 and ProductionDB";
        var options = new SanitizationOptions { EnableInfrastructure = false };

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.SanitizedContent.Should().Contain("ProductionDB"); // Infrastructure not sanitized
        result.SanitizedContent.Should().Contain("SSN_0"); // PII still sanitized
    }

    [Fact]
    public void Sanitize_StoresMappingInSession()
    {
        // Arrange
        var content = "ProductionDB at 192.168.1.50";
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        _session.GetOriginal("DATABASE_0").Should().Be("ProductionDB");
        _session.GetOriginal("IP_ADDRESS_0").Should().Be("192.168.1.50");
    }

    [Fact]
    public void Sanitize_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var content = string.Empty;
        var options = new SanitizationOptions();

        // Act
        var result = _sut.Sanitize(content, options);

        // Assert
        result.WasSanitized.Should().BeFalse();
        result.SanitizedContent.Should().BeEmpty();
        result.Matches.Should().BeEmpty();
    }
}

