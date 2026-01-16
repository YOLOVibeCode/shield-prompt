using FluentAssertions;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Tests.Unit.Application.Formatters;

/// <summary>
/// Tests for HybridXmlMarkdownFormatter.
/// Ensures prompts are generated correctly with proper response instructions.
/// </summary>
public class HybridXmlMarkdownFormatterTests
{
    private readonly HybridXmlMarkdownFormatter _sut;
    
    public HybridXmlMarkdownFormatterTests()
    {
        _sut = new HybridXmlMarkdownFormatter();
    }
    
    [Fact]
    public void FormatName_ReturnsCorrectName()
    {
        // Act & Assert
        _sut.FormatName.Should().Be("Hybrid XML-in-Markdown");
    }
    
    [Fact]
    public void FormatType_ReturnsHybridXmlMarkdown()
    {
        // Act & Assert
        _sut.FormatType.Should().Be(ResponseFormat.HybridXmlMarkdown);
    }
    
    [Fact]
    public void SupportsPartialUpdates_ReturnsTrue()
    {
        // Act & Assert
        _sut.SupportsPartialUpdates.Should().BeTrue();
    }
    
    [Fact]
    public void LlmAdherenceRate_IsHighQuality()
    {
        // Act & Assert
        _sut.LlmAdherenceRate.Should().BeGreaterThanOrEqualTo(0.90);
    }
    
    [Fact]
    public void GeneratePrompt_WithSimpleContext_ContainsAllSections()
    {
        // Arrange
        var role = new Role(
            Id: "debug",
            Name: "Debug Analyst",
            Icon: "🐛",
            Description: "Finds bugs",
            SystemPrompt: "You are a debug expert.",
            Tone: "Analytical",
            Style: "Detailed",
            Priorities: new[] { "Correctness" },
            Expertise: new[] { "Debugging" });
        
        var file = new FileNode("/Users/admin/test.cs", "test.cs", false);
        file.Content = "public class Test { }";
        
        var context = new PromptContext(
            SessionId: "test_session_123",
            SelectedRole: role,
            CustomInstructions: "Focus on null checks",
            SelectedFiles: new[] { file },
            BaseDirectory: "/Users/admin",
            TargetModel: "GPT-4",
            IsSanitized: false,
            TotalTokens: 100);
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert
        prompt.Should().Contain("# ShieldPrompt Analysis Request");
        prompt.Should().Contain("Session ID:");
        prompt.Should().Contain("test_session_123");
        prompt.Should().Contain("Role:");
        prompt.Should().Contain("Debug Analyst");
        prompt.Should().Contain("## 📋 Your Task");
        prompt.Should().Contain("Focus on null checks");
        prompt.Should().Contain("## 📁 Files Included");
        prompt.Should().Contain("test.cs");
        prompt.Should().Contain("public class Test { }");
        prompt.Should().Contain("⚠️ CRITICAL: OUTPUT FORMAT"); // Action-only format
        prompt.Should().Contain("You MUST respond with ONLY a valid XML block");
        prompt.Should().Contain("<code_changes>");
        prompt.Should().Contain("<file_operation>CREATE|UPDATE|DELETE</file_operation>");
        prompt.Should().Contain("<![CDATA[");
        prompt.Should().Contain("START YOUR RESPONSE WITH: <code_changes");
    }
    
    [Fact]
    public void GeneratePrompt_WithMultipleFiles_ListsAllFiles()
    {
        // Arrange
        var role = new Role(
            Id: "test",
            Name: "Test Role",
            Icon: "🎯",
            Description: "Test",
            SystemPrompt: "Test prompt",
            Tone: "Professional",
            Style: "Brief",
            Priorities: new[] { "Quality" },
            Expertise: new[] { "Testing" });
        
        var files = new[]
        {
            new FileNode("/Users/admin/file1.cs", "file1.cs", false) { Content = "class One { }" },
            new FileNode("/Users/admin/file2.cs", "file2.cs", false) { Content = "class Two { }" },
            new FileNode("/Users/admin/file3.cs", "file3.cs", false) { Content = "class Three { }" }
        };
        
        var context = new PromptContext(
            SessionId: "multi_file_test",
            SelectedRole: role,
            CustomInstructions: "Review all",
            SelectedFiles: files,
            BaseDirectory: "/Users/admin",
            TargetModel: "GPT-4",
            IsSanitized: false,
            TotalTokens: 300);
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert
        prompt.Should().Contain("file1.cs");
        prompt.Should().Contain("file2.cs");
        prompt.Should().Contain("file3.cs");
        prompt.Should().Contain("class One");
        prompt.Should().Contain("class Two");
        prompt.Should().Contain("class Three");
        prompt.Should().Contain("3 files");
    }
    
    [Fact]
    public void GeneratePrompt_WhenSanitized_IndicatesSanitization()
    {
        // Arrange
        var role = new Role(
            Id: "test",
            Name: "Test",
            Icon: "🎯",
            Description: "Test",
            SystemPrompt: "Test",
            Tone: "Professional",
            Style: "Brief",
            Priorities: new[] { "Quality" },
            Expertise: new[] { "Testing" });
        
        var file = new FileNode("/test.cs", "test.cs", false) { Content = "test" };
        
        var context = new PromptContext(
            SessionId: "test",
            SelectedRole: role,
            CustomInstructions: "Test",
            SelectedFiles: new[] { file },
            BaseDirectory: "/",
            TargetModel: "GPT-4",
            IsSanitized: true,  // SANITIZED
            TotalTokens: 100);
        
        // Act
        var prompt = _sut.GeneratePrompt(context);
        
        // Assert
        prompt.Should().Contain("Sanitization:");
        prompt.Should().Contain("Enabled");
        prompt.Should().Contain("✅");
    }
    
    [Fact]
    public void EstimateTokenOverhead_ReturnsReasonableOverhead()
    {
        // Arrange
        var baseTokens = 1000;
        
        // Act
        var totalEstimate = _sut.EstimateTokenOverhead(baseTokens);
        
        // Assert
        totalEstimate.Should().BeGreaterThan(baseTokens); // Has overhead
        totalEstimate.Should().BeLessThan(baseTokens * 2); // But not too much
    }
    
    [Fact]
    public void GenerateResponseExample_ContainsValidStructure()
    {
        // Act
        var example = _sut.GenerateResponseExample();
        
        // Assert - Action-only format (NO prose analysis)
        example.Should().Contain("<code_changes>");
        example.Should().Contain("<file_operation>UPDATE</file_operation>");
        example.Should().Contain("<file_operation>CREATE</file_operation>");
        example.Should().Contain("<file_operation>DELETE</file_operation>");
        example.Should().Contain("<file_summary>");
        example.Should().Contain("<![CDATA[");
        example.Should().NotContain("# Analysis"); // Action-only = no prose
    }
}

