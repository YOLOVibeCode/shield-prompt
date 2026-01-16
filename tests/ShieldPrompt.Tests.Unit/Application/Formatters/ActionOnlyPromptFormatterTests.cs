using FluentAssertions;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Formatters;

/// <summary>
/// Tests for action-only prompt mode with strict LLM instructions.
/// Validates architect's specification for cross-LLM compatibility.
/// </summary>
public class ActionOnlyPromptFormatterTests
{
    private readonly HybridXmlMarkdownFormatter _sut = new();
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_StartsWithTaskSection()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert - Updated to match VALIDATED PROMPT TEMPLATE
        prompt.Should().StartWith("# ShieldPrompt Analysis Request");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_ContainsCriticalWarning()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert
        prompt.Should().Contain("⚠️ CRITICAL");
        prompt.Should().Contain("OUTPUT FORMAT");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_ShowsExactExpectedFormat()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert
        prompt.Should().Contain("<code_changes>");
        prompt.Should().Contain("<file_operation>");
        prompt.Should().Contain("<![CDATA[");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_IncludesNegativeExamples()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert - Updated to match VALIDATED PROMPT TEMPLATE wording
        prompt.Should().Contain("Do NOT include");
        prompt.Should().Contain("Do NOT explain");
        prompt.Should().Contain("xml fences");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_EndsWithStartDirective()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert
        prompt.Should().Contain("START YOUR RESPONSE WITH: <code_changes");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_DefinesEmptyResponseCase()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert - Updated to match exact wording
        prompt.Should().Contain("If no changes needed, return:");
        prompt.Should().Contain("<code_changes/>");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_FormatInstructionsUnder200Tokens()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Extract format section (after "⚠️ CRITICAL")
        var formatSection = prompt.Substring(prompt.IndexOf("⚠️ CRITICAL"));
        var tokenCount = EstimateTokenCount(formatSection);
        
        // Assert
        tokenCount.Should().BeLessThan(300, "format instructions should be concise for LLM to process");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_RepeatsFormatConstraintTwice()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert - "ONLY" should appear at least twice for emphasis (architect spec)
        var onlyCount = System.Text.RegularExpressions.Regex.Matches(prompt, "ONLY", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        onlyCount.Should().BeGreaterThanOrEqualTo(2, "constraint should be repeated for emphasis per architect spec");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_IncludesFileContentInCodeBlocks()
    {
        // Arrange
        var context = CreateContextWithFiles();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert - Updated to match VALIDATED PROMPT TEMPLATE
        prompt.Should().Contain("## 📁 Files Included");
        prompt.Should().Contain("### ✅ `test.cs`");
        prompt.Should().Contain("```csharp");
        prompt.Should().Contain("// test content");
    }
    
    [Fact]
    public void GeneratePrompt_ActionOnlyMode_NoAnalysisPrompting()
    {
        // Arrange
        var context = CreateMinimalContext();
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert - Should NOT ask LLM to provide prose analysis (action-only!)
        // Note: "Analysis Request" in title is OK - it's the document name, not a prompt to analyze
        prompt.Should().NotContain("explain your reasoning");
        prompt.Should().NotContain("describe the issues");
        prompt.Should().NotContain("provide analysis");
    }
    
    private PromptContext CreateMinimalContext()
    {
        var role = new Role(
            Id: "test",
            Name: "Test Engineer",
            Icon: "🧪",
            SystemPrompt: "You fix code.",
            Description: "Test role",
            Tone: "Professional",
            Style: "Concise",
            Priorities: new List<string>(),
            Expertise: new List<string>());
        
        return new PromptContext(
            SessionId: "test-123",
            SelectedRole: role,
            CustomInstructions: "Fix the bug",
            SelectedFiles: new List<FileNode>(),
            BaseDirectory: "/test",
            TargetModel: "gpt-4",
            IsSanitized: false,
            TotalTokens: 0);
    }
    
    private PromptContext CreateContextWithFiles()
    {
        var context = CreateMinimalContext();
        var file = new FileNode("test.cs", "test.cs", false)
        {
            Content = "// test content"
        };
        var files = new List<FileNode> { file };
        
        return context with { SelectedFiles = files };
    }
    
    private int EstimateTokenCount(string text)
    {
        // Rough estimate: ~4 characters per token
        return text.Length / 4;
    }
}

