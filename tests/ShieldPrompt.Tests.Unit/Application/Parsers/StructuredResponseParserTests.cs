using FluentAssertions;
using ShieldPrompt.Application.Parsers;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Tests.Unit.Application.Parsers;

/// <summary>
/// Tests for StructuredResponseParser.
/// Ensures LLM responses are parsed correctly across all formats.
/// </summary>
public class StructuredResponseParserTests
{
    private readonly StructuredResponseParser _sut;
    
    public StructuredResponseParserTests()
    {
        _sut = new StructuredResponseParser();
    }
    
    [Fact]
    public async Task ParseAsync_WithValidHybridXmlMarkdown_ExtractsOperations()
    {
        // Arrange
        var llmResponse = @"# Analysis

I found 2 issues:
1. Missing null check
2. No error handling

## Proposed Changes

<code_changes>
  <changed_file>
    <file_path>src/Program.cs</file_path>
    <file_summary>Added null check</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(""Hello"");
    }
}
    ]]></file_code>
  </changed_file>
  
  <changed_file>
    <file_path>src/Helper.cs</file_path>
    <file_summary>New helper class</file_summary>
    <file_operation>CREATE</file_operation>
    <file_code><![CDATA[
public class Helper { }
    ]]></file_code>
  </changed_file>
  
  <changed_file>
    <file_path>src/OldFile.cs</file_path>
    <file_summary>Deprecated</file_summary>
    <file_operation>DELETE</file_operation>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Analysis.Should().Contain("I found 2 issues");
        result.Operations.Should().HaveCount(3);
        
        // Check UPDATE operation
        var updateOp = result.Operations[0];
        updateOp.Type.Should().Be(FileOperationType.Update);
        updateOp.Path.Should().Be("src/Program.cs");
        updateOp.Reason.Should().Be("Added null check");
        updateOp.Content.Should().Contain("class Program");
        updateOp.IsValid().Should().BeTrue();
        
        // Check CREATE operation
        var createOp = result.Operations[1];
        createOp.Type.Should().Be(FileOperationType.Create);
        createOp.Path.Should().Be("src/Helper.cs");
        createOp.Reason.Should().Be("New helper class");
        createOp.Content.Should().Contain("public class Helper");
        
        // Check DELETE operation
        var deleteOp = result.Operations[2];
        deleteOp.Type.Should().Be(FileOperationType.Delete);
        deleteOp.Path.Should().Be("src/OldFile.cs");
        deleteOp.Reason.Should().Be("Deprecated");
        deleteOp.Content.Should().BeNull();
    }
    
    [Fact]
    public async Task ParseAsync_WithPartialUpdate_ExtractsLineNumbers()
    {
        // Arrange
        var llmResponse = @"# Analysis
Fixed bug in line 10.

<code_changes>
  <changed_file>
    <file_path>src/Service.cs</file_path>
    <file_summary>Fixed bug</file_summary>
    <file_operation>PARTIAL_UPDATE</file_operation>
    <start_line>7</start_line>
    <end_line>10</end_line>
    <file_code><![CDATA[
    public void DoSomething()
    {
        // Fixed code
    }
    ]]></file_code>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(1);
        
        var op = result.Operations[0];
        op.Type.Should().Be(FileOperationType.PartialUpdate);
        op.StartLine.Should().Be(7);
        op.EndLine.Should().Be(10);
        op.IsValid().Should().BeTrue();
    }
    
    [Fact]
    public async Task ParseAsync_WithInvalidXml_ReturnsFailure()
    {
        // Arrange
        var llmResponse = @"# Analysis
Some text

<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[
    code here
    <!-- Missing closing CDATA and changed_file tag
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.Operations.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ParseAsync_WithMissingRequiredAttribute_AddsWarning()
    {
        // Arrange
        var llmResponse = @"<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code]]></file_code>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.HybridXmlMarkdown);
        
        // Assert - Missing 'file_summary' element should add warning but still parse
        result.Success.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Message.Contains("file_summary"));
    }
    
    [Fact]
    public async Task ParseAsync_WithEmptyResponse_ReturnsEmptyOperations()
    {
        // Arrange
        var llmResponse = "Just some analysis text, no operations.";
        
        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Analysis.Should().Be(llmResponse);
        result.Operations.Should().BeEmpty();
    }
    
    [Fact]
    public void CanParse_WithValidHybridXmlMarkdown_ReturnsTrue()
    {
        // Arrange
        var content = @"Some text
<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>test</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code]]></file_code>
  </changed_file>
</code_changes>";
        
        // Act
        var canParse = _sut.CanParse(content, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        canParse.Should().BeTrue();
    }
    
    [Fact]
    public void CanParse_WithoutShieldPromptTag_ReturnsFalse()
    {
        // Arrange
        var content = "Just some regular markdown text";
        
        // Act
        var canParse = _sut.CanParse(content, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        canParse.Should().BeFalse();
    }
    
    [Fact]
    public void DetectFormats_WithHybridContent_ReturnsHybridFirst()
    {
        // Arrange
        var content = @"# Analysis
<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>test</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code]]></file_code>
  </changed_file>
</code_changes>";
        
        // Act
        var formats = _sut.DetectFormats(content);
        
        // Assert
        formats.Should().NotBeEmpty();
        formats.First().Should().Be(ResponseFormat.HybridXmlMarkdown);
    }
    
    [Fact]
    public async Task ParseAsync_WithMultipleFilesSameOperation_ParsesAll()
    {
        // Arrange
        var llmResponse = @"<code_changes>
  <changed_file>
    <file_path>file1.cs</file_path>
    <file_summary>Fix 1</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code1]]></file_code>
  </changed_file>
  <changed_file>
    <file_path>file2.cs</file_path>
    <file_summary>Fix 2</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code2]]></file_code>
  </changed_file>
  <changed_file>
    <file_path>file3.cs</file_path>
    <file_summary>Fix 3</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code3]]></file_code>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.HybridXmlMarkdown);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(3);
        result.Operations.Select(o => o.Path).Should().BeEquivalentTo(new[] 
        { 
            "file1.cs", 
            "file2.cs", 
            "file3.cs" 
        });
    }
    
    [Fact]
    public async Task ParseAsync_WithAutoFormat_DetectsCorrectFormat()
    {
        // Arrange
        var llmResponse = @"<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>test</file_summary>
    <file_operation>CREATE</file_operation>
    <file_code><![CDATA[code]]></file_code>
  </changed_file>
</code_changes>";

        // Act - Using Auto detection
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.DetectedFormat.Should().Be(ResponseFormat.HybridXmlMarkdown);
        result.Operations.Should().HaveCount(1);
    }
    
    // ===== EDGE CASE TESTS: Markdown Fences =====
    
    [Fact]
    public async Task ParseAsync_WithMarkdownXmlFences_ExtractsXmlCorrectly()
    {
        // Arrange - Browser ChatGPT/Claude often wraps XML in markdown
        var llmResponse = @"Sure! Here are the changes:

```xml
<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>Fixed bug</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[code here]]></file_code>
  </changed_file>
</code_changes>
```

Let me know if you need anything else!";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(1);
        result.Operations[0].Path.Should().Be("test.cs");
        result.Analysis.Should().Contain("Sure! Here are the changes:");
    }
    
    [Fact]
    public async Task ParseAsync_WithMultipleMarkdownFences_ExtractsFirstXmlBlock()
    {
        // Arrange - LLM might show examples before actual changes
        var llmResponse = @"Here's an example format:
```xml
<example>dummy</example>
```

And here are your actual changes:

```xml
<code_changes>
  <changed_file>
    <file_path>real.cs</file_path>
    <file_summary>Real change</file_summary>
    <file_operation>CREATE</file_operation>
    <file_code><![CDATA[real code]]></file_code>
  </changed_file>
</code_changes>
```";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(1);
        result.Operations[0].Path.Should().Be("real.cs");
    }
    
    // ===== EDGE CASE TESTS: HTML Entities & Escaping =====
    
    [Fact]
    public async Task ParseAsync_WithHtmlEntities_DecodesCorrectly()
    {
        // Arrange - LLMs sometimes HTML-encode content
        var llmResponse = @"<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>Fixed &lt;bug&gt;</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[
if (value &gt; 10 && value &lt; 20)
{
    result = &quot;valid&quot;;
}
    ]]></file_code>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations[0].Reason.Should().Contain("<bug>");
        result.Operations[0].Content.Should().Contain("value > 10 && value < 20");
        result.Operations[0].Content.Should().Contain("\"valid\"");
    }
    
    // ===== EDGE CASE TESTS: Whitespace & Formatting =====
    
    [Fact]
    public async Task ParseAsync_WithExtraWhitespace_NormalizesCorrectly()
    {
        // Arrange - LLMs sometimes add extra whitespace
        var llmResponse = @"


<code_changes>
  
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>test</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[
    
    
code with blank lines


    ]]></file_code>
  </changed_file>
  
</code_changes>


";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(1);
        result.Operations[0].Content.Should().NotBeNull();
    }
    
    // ===== EDGE CASE TESTS: Path Security =====
    
    [Theory]
    [InlineData("../../../etc/passwd", "Relative path traversal")]
    [InlineData("/etc/passwd", "Absolute path outside workspace")]
    [InlineData("C:\\Windows\\System32\\config", "Windows system path")]
    [InlineData("src/../../outside/file.cs", "Sneaky traversal")]
    public async Task ParseAsync_WithDangerousPath_AddsSecurityWarning(string dangerousPath, string reason)
    {
        // Arrange
        var llmResponse = $@"<code_changes>
  <changed_file>
    <file_path>{dangerousPath}</file_path>
    <file_summary>test</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[malicious]]></file_code>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert - Should parse but add security warning (case-insensitive check)
        result.Success.Should().BeTrue();
        result.Warnings.Should().Contain(w => 
            w.Message.Contains("security", StringComparison.OrdinalIgnoreCase) || 
            w.Message.Contains("path traversal", StringComparison.OrdinalIgnoreCase), 
            $"because {reason} should trigger security warning");
    }
    
    // ===== EDGE CASE TESTS: Empty/No-Op Responses =====
    
    [Fact]
    public async Task ParseAsync_WithEmptyShieldPromptTag_ReturnsNoOperations()
    {
        // Arrange - LLM indicates no changes needed
        var llmResponse = @"I analyzed your code and found no issues.

<code_changes/>

Everything looks good!";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().BeEmpty();
        result.Analysis.Should().Contain("I analyzed your code");
    }
    
    [Fact]
    public async Task ParseAsync_WithSelfClosingFileTag_HandlesCorrectly()
    {
        // Arrange
        var llmResponse = @"<code_changes>
  <changed_file>
    <file_path>old.cs</file_path>
    <file_summary>Removed</file_summary>
    <file_operation>DELETE</file_operation>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(1);
        result.Operations[0].Type.Should().Be(FileOperationType.Delete);
        result.Operations[0].Content.Should().BeNull();
    }
    
    // ===== EDGE CASE TESTS: Malformed/Truncated =====
    
    [Fact]
    public async Task ParseAsync_WithTruncatedXml_ReturnsFailureWithContext()
    {
        // Arrange - LLM hit token limit mid-response
        var llmResponse = @"<code_changes>
  <changed_file>
    <file_path>test.cs</file_path>
    <file_summary>Fix</file_summary>
    <file_operation>UPDATE</file_operation>
    <file_code><![CDATA[
partial code that got cut off because token lim";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unclosed");
    }
    
    [Fact]
    public async Task ParseAsync_WithMixedCaseAttributes_ParsesCaseInsensitive()
    {
        // Arrange - LLMs might vary case
        var llmResponse = @"<code_changes>
  <changed_file>
    <FILE_PATH>test.cs</FILE_PATH>
    <FILE_SUMMARY>Fixed</FILE_SUMMARY>
    <FILE_OPERATION>Update</FILE_OPERATION>
    <FILE_CODE><![CDATA[code]]></FILE_CODE>
  </changed_file>
</code_changes>";

        // Act
        var result = await _sut.ParseAsync(llmResponse, ResponseFormat.Auto);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Operations.Should().HaveCount(1);
        result.Operations[0].Type.Should().Be(FileOperationType.Update);
    }
}

