using FluentAssertions;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Tests.Unit.Application.PromptTemplates;

/// <summary>
/// Integration tests for focus area functionality.
/// Tests the full workflow from template → focus selection → prompt composition.
/// </summary>
public class FocusAreaIntegrationTests
{
    private readonly PromptComposer _composer;
    
    public FocusAreaIntegrationTests()
    {
        var tokenService = new TokenCountingService();
        var xmlBuilder = new XmlPromptBuilder();
        _composer = new PromptComposer(tokenService, xmlBuilder);
    }
    
    [Fact]
    public void Compose_WithSelectedFocusAreas_IncludesThemInPrompt()
    {
        // Arrange
        var template = new PromptTemplate(
            Id: "test",
            Name: "Test",
            Icon: "🧪",
            Description: "Test template",
            SystemPrompt: "You are a tester.",
            FocusOptions: new[] { "Security", "Performance", "Style" });
        
        var files = new List<FileNode>
        {
            new FileNode("test.cs", "test.cs", false) { Content = "public class Test {}" }
        };
        
        var options = new PromptOptions(
            CustomInstructions: null,
            SelectedFocusAreas: new[] { "Security", "Performance" },
            IncludeFilePaths: true,
            IncludeLineNumbers: false);
        
        // Act
        var result = _composer.Compose(template, files, options);
        
        // Assert
        result.FullPrompt.Should().Contain("**Focus Areas:**");
        result.FullPrompt.Should().Contain("- Security");
        result.FullPrompt.Should().Contain("- Performance");
        result.FullPrompt.Should().NotContain("- Style");
    }
    
    [Fact]
    public void Compose_WithNoFocusAreasSelected_OmitsFocusSection()
    {
        // Arrange
        var template = new PromptTemplate(
            Id: "test",
            Name: "Test",
            Icon: "🧪",
            Description: "Test template",
            SystemPrompt: "You are a tester.",
            FocusOptions: new[] { "Security", "Performance" });
        
        var files = new List<FileNode>
        {
            new FileNode("test.cs", "test.cs", false) { Content = "public class Test {}" }
        };
        
        var options = new PromptOptions(
            CustomInstructions: null,
            SelectedFocusAreas: null,
            IncludeFilePaths: true,
            IncludeLineNumbers: false);
        
        // Act
        var result = _composer.Compose(template, files, options);
        
        // Assert
        result.FullPrompt.Should().NotContain("**Focus Areas:**");
    }
    
    [Fact]
    public void Compose_WithAllFocusAreasSelected_IncludesAll()
    {
        // Arrange
        var template = new PromptTemplate(
            Id: "code-review",
            Name: "Code Review",
            Icon: "🔍",
            Description: "Code review",
            SystemPrompt: "You are an expert code reviewer.",
            FocusOptions: new[] { "Security", "Performance", "Style", "Best Practices" });
        
        var files = new List<FileNode>
        {
            new FileNode("app.cs", "app.cs", false) { Content = "public class App {}" }
        };
        
        var options = new PromptOptions(
            CustomInstructions: "Look for bugs",
            SelectedFocusAreas: new[] { "Security", "Performance", "Style", "Best Practices" },
            IncludeFilePaths: true,
            IncludeLineNumbers: false);
        
        // Act
        var result = _composer.Compose(template, files, options);
        
        // Assert
        result.FullPrompt.Should().Contain("- Security");
        result.FullPrompt.Should().Contain("- Performance");
        result.FullPrompt.Should().Contain("- Style");
        result.FullPrompt.Should().Contain("- Best Practices");
    }
    
    [Theory]
    [InlineData("Security")]
    [InlineData("Performance")]
    [InlineData("Style")]
    public void Compose_WithSingleFocusArea_IncludesOnlyThatArea(string focusArea)
    {
        // Arrange
        var template = new PromptTemplate(
            Id: "test",
            Name: "Test",
            Icon: "🧪",
            Description: "Test",
            SystemPrompt: "You are a tester.",
            FocusOptions: new[] { "Security", "Performance", "Style" });
        
        var files = new List<FileNode>
        {
            new FileNode("test.cs", "test.cs", false) { Content = "class Test {}" }
        };
        
        var options = new PromptOptions(
            SelectedFocusAreas: new[] { focusArea });
        
        // Act
        var result = _composer.Compose(template, files, options);
        
        // Assert
        result.FullPrompt.Should().Contain($"- {focusArea}");
        
        // Verify other areas not included
        var otherAreas = new[] { "Security", "Performance", "Style" }
            .Where(a => a != focusArea);
        
        foreach (var other in otherAreas)
        {
            result.FullPrompt.Should().NotContain($"- {other}");
        }
    }
}

