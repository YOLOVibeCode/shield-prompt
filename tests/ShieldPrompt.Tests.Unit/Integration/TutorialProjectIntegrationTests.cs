using FluentAssertions;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Patterns;
using ShieldPrompt.Sanitization.Services;
using Xunit;
using Xunit.Abstractions;

namespace ShieldPrompt.Tests.Unit.Integration;

/// <summary>
/// Integration tests to verify the tutorial project works as expected with the real app.
/// This ensures the tutorial experience matches documentation!
/// </summary>
public class TutorialProjectIntegrationTests
{
    private readonly IFileAggregationService _fileAggregation;
    private readonly ITokenCountingService _tokenCounting;
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly IPatternRegistry _patternRegistry;
    private readonly IMappingSession _mappingSession;
    private readonly IAliasGenerator _aliasGenerator;
    private readonly string _tutorialPath;
    private readonly ITestOutputHelper _output;

    public TutorialProjectIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Setup real services (no mocks - this is integration test!)
        _fileAggregation = new FileAggregationService();
        _tokenCounting = new TokenCountingService();
        
        _patternRegistry = new PatternRegistry();
        // Register all built-in patterns
        foreach (var pattern in BuiltInPatterns.GetAll())
        {
            _patternRegistry.AddPattern(pattern);
        }
        
        _aliasGenerator = new AliasGenerator();
        _mappingSession = new MappingSession();
        _sanitizationEngine = new SanitizationEngine(
            _patternRegistry,
            _mappingSession,
            _aliasGenerator);
        
        // Get tutorial project path (5 levels up from bin/Debug/net10.0)
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _tutorialPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "samples", "tutorial-project");
        _tutorialPath = Path.GetFullPath(_tutorialPath);
    }

    [Fact]
    public void TutorialProject_ShouldExist_AndBeReadable()
    {
        // Verify tutorial project exists
        Directory.Exists(_tutorialPath).Should().BeTrue(
            $"tutorial project should exist at {_tutorialPath}");
        
        // Verify expected files exist
        var programCs = Path.Combine(_tutorialPath, "src", "Program.cs");
        var databaseCs = Path.Combine(_tutorialPath, "src", "DatabaseService.cs");
        var apiClientCs = Path.Combine(_tutorialPath, "src", "ApiClient.cs");
        var configJson = Path.Combine(_tutorialPath, "config", "appsettings.json");
        
        File.Exists(programCs).Should().BeTrue("Program.cs should exist");
        File.Exists(databaseCs).Should().BeTrue("DatabaseService.cs should exist");
        File.Exists(apiClientCs).Should().BeTrue("ApiClient.cs should exist");
        File.Exists(configJson).Should().BeTrue("appsettings.json should exist");
        
        _output.WriteLine($"✅ Tutorial project found at: {_tutorialPath}");
    }

    [Fact]
    public async Task TutorialProject_WhenLoaded_ShouldReturnFileTree()
    {
        // Act
        var result = await _fileAggregation.LoadDirectoryAsync(_tutorialPath);
        
        // Assert
        result.Should().NotBeNull();
        result.Children.Should().NotBeEmpty();
        
        // Should have src and config folders
        var folders = result.Children.Where(n => n.IsDirectory).ToList();
        folders.Should().Contain(n => n.Name == "src");
        folders.Should().Contain(n => n.Name == "config");
        
        _output.WriteLine($"✅ Loaded tutorial with {folders.Count} folders");
    }

    [Fact]
    public async Task TutorialProject_WhenAggregated_ShouldContainExpectedContent()
    {
        // Arrange
        var rootNode = await _fileAggregation.LoadDirectoryAsync(_tutorialPath);
        
        // Select C# files (as tutorial instructs)
        var csFiles = GetAllFiles(rootNode)
            .Where(f => f.Name.EndsWith(".cs"))
            .ToList();
        
        csFiles.Should().HaveCount(3, "tutorial has 3 C# files");
        
        // Act
        var aggregated = await _fileAggregation.AggregateContentsAsync(csFiles);
        
        // Assert
        aggregated.Should().NotBeNullOrEmpty();
        aggregated.Should().Contain("ProductionDB", "should contain fake DB name");
        aggregated.Should().Contain("192.168.1.50", "should contain fake IP");
        aggregated.Should().Contain("AKIAIOSFODNN7EXAMPLE", "should contain fake AWS key");
        aggregated.Should().Contain("123-45-6789", "should contain fake SSN");
        
        _output.WriteLine($"✅ Aggregated {csFiles.Count} files");
        _output.WriteLine($"   Content size: {aggregated.Length:N0} characters");
    }

    [Fact]
    public async Task TutorialProject_TokenCount_ShouldMatchExpectations()
    {
        // Arrange
        var rootNode = await _fileAggregation.LoadDirectoryAsync(_tutorialPath);
        var csFiles = GetAllFiles(rootNode)
            .Where(f => f.Name.EndsWith(".cs"))
            .ToList();
        
        var content = await _fileAggregation.AggregateContentsAsync(csFiles);
        
        // Format as Markdown (tutorial recommendation)
        var formatter = new MarkdownFormatter();
        var formatted = formatter.Format(csFiles, content);
        
        // Act - Count tokens for GPT-4o
        var tokenCount = _tokenCounting.CountTokens(formatted, "cl100k_base");
        
        // Assert
        tokenCount.Should().BeGreaterThan(500, "tutorial should have substantial content");
        tokenCount.Should().BeLessThan(5000, "but not too large");
        
        var exceeds = _tokenCounting.ExceedsLimit(tokenCount, ModelProfiles.GPT4o);
        exceeds.Should().BeFalse("tutorial should fit in GPT-4o context");
        
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine("TUTORIAL PROJECT - TOKEN COUNT");
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine($"Files: {csFiles.Count}");
        _output.WriteLine($"Raw content: {content.Length:N0} chars");
        _output.WriteLine($"Formatted (Markdown): {formatted.Length:N0} chars");
        _output.WriteLine($"Token count: {tokenCount:N0} tokens");
        _output.WriteLine($"Model limit (GPT-4o): {ModelProfiles.GPT4o.ContextLimit:N0} tokens");
        _output.WriteLine($"Exceeds limit: {exceeds}");
    }

    [Fact]
    public async Task TutorialProject_WhenSanitized_ShouldMaskAllFakeSecrets()
    {
        // Arrange
        var rootNode = await _fileAggregation.LoadDirectoryAsync(_tutorialPath);
        var csFiles = GetAllFiles(rootNode)
            .Where(f => f.Name.EndsWith(".cs"))
            .ToList();
        
        var content = await _fileAggregation.AggregateContentsAsync(csFiles);
        
        // Act
        var result = _sanitizationEngine.Sanitize(
            content,
            new SanitizationOptions { EnableInfrastructure = true, EnablePII = true });
        
        // Assert
        result.WasSanitized.Should().BeTrue("tutorial has fake secrets");
        result.Matches.Should().HaveCountGreaterThan(10, "should detect many fake secrets");
        
        // Verify specific secrets were masked
        result.SanitizedContent.Should().NotContain("ProductionDB", "DB name should be masked");
        result.SanitizedContent.Should().Contain("DATABASE_", "should have DB alias");
        
        result.SanitizedContent.Should().NotContain("192.168.1.50", "IP should be masked");
        result.SanitizedContent.Should().Contain("IP_ADDRESS_", "should have IP alias");
        
        result.SanitizedContent.Should().NotContain("AKIAIOSFODNN7EXAMPLE", "AWS key should be masked");
        result.SanitizedContent.Should().Contain("AWS_KEY_", "should have AWS alias");
        
        result.SanitizedContent.Should().NotContain("123-45-6789", "SSN should be masked");
        result.SanitizedContent.Should().Contain("SSN_", "should have SSN alias");
        
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine("TUTORIAL PROJECT - SANITIZATION RESULTS");
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine($"Sensitive values detected: {result.Matches.Count}");
        _output.WriteLine("");
        _output.WriteLine("First 15 matches:");
        foreach (var match in result.Matches.Take(15))
        {
            _output.WriteLine($"  {match.Category,-15} {match.Original,-35} → {match.Alias}");
        }
    }

    [Fact]
    public async Task TutorialProject_FullWorkflow_ShouldMatchDocumentedBehavior()
    {
        // This test simulates the EXACT workflow described in README_TUTORIAL.md
        
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine("TUTORIAL WORKFLOW - INTEGRATION TEST");
        _output.WriteLine("═══════════════════════════════════════════");
        
        // Step 1: Load tutorial project
        _output.WriteLine("Step 1: Loading tutorial project...");
        var rootNode = await _fileAggregation.LoadDirectoryAsync(_tutorialPath);
        rootNode.Should().NotBeNull();
        _output.WriteLine($"  ✅ Loaded from: {_tutorialPath}");
        
        // Step 2: Select files (as tutorial instructs: 3 C# files)
        _output.WriteLine("Step 2: Selecting C# files...");
        var selectedFiles = GetAllFiles(rootNode)
            .Where(f => f.Name.EndsWith(".cs"))
            .ToList();
        
        selectedFiles.Should().HaveCount(3, "tutorial selects 3 C# files");
        _output.WriteLine($"  ✅ Selected {selectedFiles.Count} files");
        
        // Step 3: Aggregate content
        _output.WriteLine("Step 3: Aggregating content...");
        var content = await _fileAggregation.AggregateContentsAsync(selectedFiles);
        content.Should().NotBeNullOrEmpty();
        _output.WriteLine($"  ✅ Aggregated: {content.Length:N0} characters");
        
        // Step 4: Format as Markdown (tutorial recommendation)
        _output.WriteLine("Step 4: Formatting as Markdown...");
        var formatter = new MarkdownFormatter();
        var formatted = formatter.Format(selectedFiles, content);
        _output.WriteLine($"  ✅ Formatted: {formatted.Length:N0} characters");
        
        // Step 5: Count tokens
        _output.WriteLine("Step 5: Counting tokens...");
        var tokenCount = _tokenCounting.CountTokens(formatted, "cl100k_base");
        _output.WriteLine($"  ✅ Token count: {tokenCount:N0} tokens");
        
        // Step 6: Sanitize (happens automatically on copy)
        _output.WriteLine("Step 6: Sanitizing (automatic on copy)...");
        var sanitized = _sanitizationEngine.Sanitize(
            formatted,
            new SanitizationOptions { EnableInfrastructure = true, EnablePII = true });
        
        _output.WriteLine($"  ✅ Sanitized: {sanitized.Matches.Count} secrets masked");
        _output.WriteLine("");
        
        // Verify tutorial expectations
        tokenCount.Should().BeInRange(800, 3000, "tutorial claims ~500+ tokens");
        sanitized.WasSanitized.Should().BeTrue();
        sanitized.Matches.Should().HaveCountGreaterThan(12, "tutorial shows 15+ values");
        
        // Verify specific pattern categories detected
        var categories = sanitized.Matches.Select(m => m.Category).Distinct().ToList();
        _output.WriteLine("");
        _output.WriteLine("Pattern categories detected:");
        foreach (var category in categories.OrderBy(c => c.ToString()))
        {
            var count = sanitized.Matches.Count(m => m.Category == category);
            _output.WriteLine($"  {category}: {count}");
        }
        
        categories.Should().Contain(PatternCategory.Database, "has ProductionDB, CustomerData");
        categories.Should().Contain(PatternCategory.IPAddress, "has 192.168.1.50, 10.0.0.25");
        categories.Should().Contain(PatternCategory.AWSKey, "has AWS keys");
        categories.Should().Contain(PatternCategory.GitHubToken, "has GitHub tokens");
        categories.Should().Contain(PatternCategory.SSN, "has 123-45-6789, 987-65-4321");
        categories.Should().Contain(PatternCategory.Password, "has passwords in code");
        
        _output.WriteLine("");
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine("✅ TUTORIAL WORKFLOW VERIFIED!");
        _output.WriteLine($"   Files: {selectedFiles.Count}");
        _output.WriteLine($"   Tokens: {tokenCount:N0}");
        _output.WriteLine($"   Secrets: {sanitized.Matches.Count}");
        _output.WriteLine("═══════════════════════════════════════════");
    }

    private IEnumerable<FileNode> GetAllFiles(FileNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in GetAllFiles(child))
            {
                yield return descendant;
            }
        }
    }
}

