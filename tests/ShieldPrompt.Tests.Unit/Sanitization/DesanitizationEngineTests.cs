using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Patterns;
using ShieldPrompt.Sanitization.Services;

namespace ShieldPrompt.Tests.Unit.Sanitization;

public class DesanitizationEngineTests : IDisposable
{
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly IPatternRegistry _patternRegistry;
    private readonly IMappingSession _session;

    public DesanitizationEngineTests()
    {
        _patternRegistry = new PatternRegistry();
        _session = new MappingSession();
        
        foreach (var pattern in BuiltInPatterns.GetAll())
        {
            _patternRegistry.AddPattern(pattern);
        }

        _sanitizationEngine = new SanitizationEngine(_patternRegistry, _session, new AliasGenerator());
        _desanitizationEngine = new DesanitizationEngine();
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    [Fact]
    public void Desanitize_WithDATABASE_0_RestoresOriginal()
    {
        // Arrange
        _session.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);
        var content = "Connect to DATABASE_0 for data";

        // Act
        var result = _desanitizationEngine.Desanitize(content, _session);

        // Assert
        result.Should().Be("Connect to ProductionDB for data");
    }

    [Fact]
    public void Desanitize_WithMultipleAliases_RestoresAll()
    {
        // Arrange
        _session.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);
        _session.AddMapping("192.168.1.50", "IP_ADDRESS_0", PatternCategory.IPAddress);
        var content = "DATABASE_0 at IP_ADDRESS_0";

        // Act
        var result = _desanitizationEngine.Desanitize(content, _session);

        // Assert
        result.Should().Be("ProductionDB at 192.168.1.50");
    }

    [Fact]
    public void Desanitize_WithNoAliases_ReturnsUnchanged()
    {
        // Arrange
        var content = "This has no aliases";

        // Act
        var result = _desanitizationEngine.Desanitize(content, _session);

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void Desanitize_WithUnknownAlias_LeavesUnchanged()
    {
        // Arrange
        var content = "Unknown UNKNOWN_ALIAS_0 here";

        // Act
        var result = _desanitizationEngine.Desanitize(content, _session);

        // Assert
        result.Should().Be(content); // Alias not in session, left as-is
    }

    [Fact]
    public void RoundTrip_SanitizeAndDesanitize_RestoresOriginal()
    {
        // Arrange
        var original = "ProductionDB at 192.168.1.50 with SSN 123-45-6789";
        var options = new SanitizationOptions();

        // Act
        var sanitized = _sanitizationEngine.Sanitize(original, options);
        var restored = _desanitizationEngine.Desanitize(sanitized.SanitizedContent, _session);

        // Assert
        restored.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_WithComplexContent_PreservesStructure()
    {
        // Arrange
        var original = @"
public class DbConfig
{
    private string _server = ""ProductionDB"";
    private string _ip = ""192.168.1.50"";
    private string _apiKey = ""AKIAIOSFODNN7EXAMPLE"";
}";
        var options = new SanitizationOptions();

        // Act
        var sanitized = _sanitizationEngine.Sanitize(original, options);
        var restored = _desanitizationEngine.Desanitize(sanitized.SanitizedContent, _session);

        // Assert
        restored.Should().Be(original);
        sanitized.SanitizedContent.Should().NotContain("ProductionDB");
        sanitized.SanitizedContent.Should().NotContain("192.168.1.50");
        sanitized.SanitizedContent.Should().NotContain("AKIAIOSFODNN7EXAMPLE");
    }

    [Fact]
    public void Desanitize_WithSameAliasTwice_RestoresBoth()
    {
        // Arrange
        _session.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);
        var content = "DATABASE_0 here and DATABASE_0 there";

        // Act
        var result = _desanitizationEngine.Desanitize(content, _session);

        // Assert
        result.Should().Be("ProductionDB here and ProductionDB there");
    }
}

