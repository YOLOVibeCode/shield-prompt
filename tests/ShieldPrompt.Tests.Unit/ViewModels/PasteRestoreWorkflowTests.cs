using FluentAssertions;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Patterns;
using ShieldPrompt.Sanitization.Services;
using Xunit;
using Xunit.Abstractions;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Tests for the complete paste & restore workflow.
/// Ensures desanitization, preview, and restoration work correctly.
/// </summary>
public class PasteRestoreWorkflowTests
{
    private readonly ISanitizationEngine _sanitizer;
    private readonly IDesanitizationEngine _desanitizer;
    private readonly IMappingSession _session;
    private readonly ITestOutputHelper _output;

    public PasteRestoreWorkflowTests(ITestOutputHelper output)
    {
        _output = output;
        
        var registry = new PatternRegistry();
        foreach (var pattern in BuiltInPatterns.GetAll())
        {
            registry.AddPattern(pattern);
        }
        
        var aliasGenerator = new AliasGenerator();
        _session = new MappingSession();
        _sanitizer = new SanitizationEngine(registry, _session, aliasGenerator);
        _desanitizer = new DesanitizationEngine();
    }

    [Fact]
    public void PasteRestore_EmptyContent_ShouldHandleGracefully()
    {
        // Arrange
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        
        // Act
        vm.PastedContent = "";
        
        // Assert
        vm.RestoredContent.Should().BeEmpty();
        vm.AliasCount.Should().Be(0);
        vm.DetectedAliases.Should().BeEmpty();
    }

    [Fact]
    public void PasteRestore_ContentWithNoAliases_ShouldReturnUnchanged()
    {
        // Arrange
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        var content = "This is plain code with no aliases";
        
        // Act
        vm.PastedContent = content;
        
        // Assert
        vm.RestoredContent.Should().Be(content, "no aliases to restore");
        vm.AliasCount.Should().Be(0);
        vm.DetectedAliases.Should().BeEmpty();
    }

    [Fact]
    public void PasteRestore_RoundTrip_ShouldRestoreOriginalValues()
    {
        // Arrange - Original code with secrets
        var originalCode = @"
var db = ""ProductionDB"";
var ip = ""192.168.1.50"";
var key = ""AKIAIOSFODNN7EXAMPLE"";
";
        
        // Step 1: Sanitize (simulates copy action)
        var sanitized = _sanitizer.Sanitize(originalCode, new SanitizationOptions());
        
        _output.WriteLine("Original:");
        _output.WriteLine(originalCode);
        _output.WriteLine("");
        _output.WriteLine("Sanitized:");
        _output.WriteLine(sanitized.SanitizedContent);
        
        // Simulate AI modifying the code (but keeping aliases)
        var aiResponse = sanitized.SanitizedContent.Replace("var", "string");
        
        _output.WriteLine("");
        _output.WriteLine("AI Response (with aliases):");
        _output.WriteLine(aiResponse);
        
        // Step 2: User pastes AI response into ShieldPrompt
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        vm.PastedContent = aiResponse;
        
        // Assert - Aliases detected
        vm.DetectedAliases.Should().NotBeEmpty("AI response contains aliases");
        vm.AliasCount.Should().BeGreaterThan(0);
        
        // Assert - Values restored
        vm.RestoredContent.Should().Contain("ProductionDB", "DATABASE alias should be restored");
        vm.RestoredContent.Should().Contain("192.168.1.50", "IP_ADDRESS alias should be restored");
        vm.RestoredContent.Should().Contain("AKIAIOSFODNN7", "AWS_KEY alias should be restored");
        
        _output.WriteLine("");
        _output.WriteLine("Restored:");
        _output.WriteLine(vm.RestoredContent);
    }

    [Fact]
    public void PasteRestore_MultipleOccurrences_ShouldCountCorrectly()
    {
        // Arrange - Sanitize content with repeated values
        var originalCode = @"
Connect to ProductionDB
Backup to ProductionDB
Query ProductionDB
";
        
        var sanitized = _sanitizer.Sanitize(originalCode, new SanitizationOptions());
        
        // Act - Paste the sanitized content
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        vm.PastedContent = sanitized.SanitizedContent;
        
        // Assert - Should detect DATABASE alias occurred 3 times
        var dbAlias = vm.DetectedAliases.FirstOrDefault(a => a.Original == "ProductionDB");
        dbAlias.Should().NotBeNull("ProductionDB was sanitized");
        dbAlias!.Occurrences.Should().Be(3, "ProductionDB appears 3 times");
    }

    [Fact]
    public void PasteRestore_MixedAliases_ShouldDetectAll()
    {
        // Arrange - Multiple different secrets
        var originalCode = @"
Database: ProductionDB
Server: 192.168.1.50
AWS Key: AKIAIOSFODNN7EXAMPLE
SSN: 123-45-6789
Password: MySecret123
";
        
        var sanitized = _sanitizer.Sanitize(originalCode, new SanitizationOptions());
        
        // Act
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        vm.PastedContent = sanitized.SanitizedContent;
        
        // Assert
        vm.AliasCount.Should().BeGreaterThanOrEqualTo(3, "at least 3 different values");
        vm.DetectedAliases.Should().Contain(a => a.Original == "ProductionDB");
        vm.DetectedAliases.Should().Contain(a => a.Original == "192.168.1.50");
        vm.DetectedAliases.Should().Contain(a => a.Original == "123-45-6789");
        // Note: Password pattern might match "Password: MySecret123" as whole string or just "MySecret123"
        vm.DetectedAliases.Should().Contain(a => a.Original.Contains("MySecret") || a.Original.Contains("Password:"));
    }

    [Fact]
    public void PasteRestore_SimulatedChatGPTResponse_ShouldWork()
    {
        // Arrange - Create a realistic ChatGPT interaction
        var userCode = @"
public class Config
{
    private string _db = ""ProductionDB"";
    private string _ip = ""192.168.1.50"";
}
";
        
        // Step 1: Sanitize (user copies)
        var sanitized = _sanitizer.Sanitize(userCode, new SanitizationOptions());
        
        // Step 2: Simulate ChatGPT improving the code (with aliases)
        var chatGptResponse = @"
Here's the improved version:

```csharp
public class Config
{
    // Better: Use configuration instead of hardcoded values
    private readonly string _db = Configuration[""Database""] ?? ""DATABASE_0"";
    private readonly string _ip = Configuration[""ServerIP""] ?? ""IP_ADDRESS_0"";
    
    public Config()
    {
        ValidateConfiguration();
    }
    
    private void ValidateConfiguration()
    {
        if (_db == ""DATABASE_0"" || _ip == ""IP_ADDRESS_0"")
            throw new InvalidOperationException(""Configuration not loaded"");
    }
}
```

This approach:
- Uses dependency injection
- Validates configuration
- Follows best practices
";
        
        // Step 3: User pastes ChatGPT response
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        vm.PastedContent = chatGptResponse;
        
        // Assert - Aliases detected in ChatGPT's response
        vm.AliasCount.Should().BeGreaterThan(0, "ChatGPT used the aliases");
        
        // Assert - Restored content has real values
        vm.RestoredContent.Should().Contain("ProductionDB");
        vm.RestoredContent.Should().Contain("192.168.1.50");
        vm.RestoredContent.Should().NotContain("DATABASE_0", "should be fully restored");
        vm.RestoredContent.Should().NotContain("IP_ADDRESS_0", "should be fully restored");
        
        _output.WriteLine("ChatGPT Response:");
        _output.WriteLine(chatGptResponse);
        _output.WriteLine("");
        _output.WriteLine("Restored Content:");
        _output.WriteLine(vm.RestoredContent);
        _output.WriteLine("");
        _output.WriteLine($"Detected {vm.AliasCount} aliases");
        foreach (var alias in vm.DetectedAliases)
        {
            _output.WriteLine($"  {alias.Alias} → {alias.Original} ({alias.Occurrences}x)");
        }
    }

    [Fact]
    public async Task PasteRestore_CopyRestored_ShouldCopyToClipboard()
    {
        // Arrange
        var originalCode = "DB: ProductionDB";
        var sanitized = _sanitizer.Sanitize(originalCode, new SanitizationOptions());
        
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        vm.PastedContent = sanitized.SanitizedContent;
        
        // Act
        await vm.CopyRestoredCommand.ExecuteAsync(null);
        
        // Assert
        var clipboardContent = await TextCopy.ClipboardService.GetTextAsync();
        clipboardContent.Should().Contain("ProductionDB", "restored content should be in clipboard");
    }

    [Fact]
    public void PasteRestore_WithUnknownAliases_ShouldLeaveUnchanged()
    {
        // Arrange
        var contentWithFakeAlias = "Connect to DATABASE_99 and IP_ADDRESS_999";
        
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        
        // Act
        vm.PastedContent = contentWithFakeAlias;
        
        // Assert - Unknown aliases should be left as-is (safe default)
        vm.RestoredContent.Should().Contain("DATABASE_99", "unknown alias should remain");
        vm.RestoredContent.Should().Contain("IP_ADDRESS_999", "unknown alias should remain");
        vm.DetectedAliases.Should().BeEmpty("no known aliases in content");
    }

    [Fact]
    public void PasteRestore_ChangeSummary_ShouldDescribeAllChanges()
    {
        // Arrange
        var originalCode = "DB: ProductionDB, IP: 192.168.1.50";
        var sanitized = _sanitizer.Sanitize(originalCode, new SanitizationOptions());
        
        var parser = new ShieldPrompt.Application.Services.AIResponseParser();
        var writer = new ShieldPrompt.Application.Services.FileWriterService();
        var vm = new PasteRestoreViewModel(_desanitizer, _session, parser, writer, Array.Empty<FileNode>(), "/test");
        
        // Act
        vm.PastedContent = sanitized.SanitizedContent;
        
        // Assert
        vm.ChangeSummary.Should().NotBeEmpty("should describe changes");
        vm.ChangeSummary.Should().Contain(s => s.Contains("ProductionDB"));
        vm.ChangeSummary.Should().Contain(s => s.Contains("192.168.1.50"));
        
        _output.WriteLine("Change Summary:");
        foreach (var change in vm.ChangeSummary)
        {
            _output.WriteLine($"  {change}");
        }
    }
}

