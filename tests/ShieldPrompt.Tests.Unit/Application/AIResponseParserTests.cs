using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application;

/// <summary>
/// Tests for AI Response Parser.
/// Ensures we can extract file operations from ChatGPT/Claude responses.
/// </summary>
public class AIResponseParserTests
{
    private readonly IAIResponseParser _parser;
    private readonly List<FileNode> _originalFiles;

    public AIResponseParserTests()
    {
        _parser = new AIResponseParser();
        
        _originalFiles = new List<FileNode>
        {
            new FileNode("/project/src/Program.cs", "Program.cs", false),
            new FileNode("/project/src/Database.cs", "Database.cs", false)
        };
    }

    [Fact]
    public void Parse_EmptyResponse_ShouldReturnNoUpdates()
    {
        // Act
        var result = _parser.Parse("", _originalFiles);
        
        // Assert
        result.Updates.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ChatGPTFormat_WithFileHeaders_ShouldExtractFile()
    {
        // Arrange
        var chatGptResponse = @"
Here's the updated code:

**Program.cs**
```csharp
public class Program
{
    public static void Main()
    {
        Console.WriteLine(""Hello"");
    }
}
```
";
        
        // Act
        var result = _parser.Parse(chatGptResponse, _originalFiles);
        
        // Assert
        result.Updates.Should().ContainSingle();
        result.Updates[0].FilePath.Should().Be("Program.cs");
        result.Updates[0].Type.Should().Be(FileUpdateType.Update);
        result.Updates[0].Content.Should().Contain("public class Program");
    }

    [Fact]
    public void Parse_MultipleFiles_ShouldExtractAll()
    {
        // Arrange
        var response = @"
**Program.cs**
```csharp
public class Program { }
```

**Database.cs**
```csharp
public class Database { }
```
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert
        result.Updates.Should().HaveCount(2);
        result.Updates.Should().Contain(u => u.FilePath == "Program.cs");
        result.Updates.Should().Contain(u => u.FilePath == "Database.cs");
    }

    [Fact]
    public void Parse_WithCommentStyle_ShouldExtractFilePath()
    {
        // Arrange
        var response = @"
```csharp
// File: src/Program.cs
public class Program { }
```
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert
        result.Updates.Should().ContainSingle();
        result.Updates[0].FilePath.Should().Be("src/Program.cs");
    }

    [Fact]
    public void Parse_NoFileMarkers_ShouldMapToFirstOriginalFile()
    {
        // Arrange - AI returns code block without file name
        var response = @"
```csharp
public class Program { }
```
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert - Should map to first original file
        result.Updates.Should().ContainSingle();
        result.Updates[0].FilePath.Should().Be("Program.cs");
    }

    [Fact]
    public void Parse_NewFile_ShouldDetectAsCreate()
    {
        // Arrange
        var response = @"
**NewConfig.cs** (new file)
```csharp
public class Config { }
```
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert
        result.Updates.Should().ContainSingle();
        result.Updates[0].FilePath.Should().Be("NewConfig.cs");
        result.Updates[0].Type.Should().Be(FileUpdateType.Create);
    }

    [Fact]
    public void Parse_ClaudeFormat_ShouldExtractFiles()
    {
        // Arrange
        var claudeResponse = @"
I'll update these files:

`Program.cs`:
```csharp
public class Program { }
```

`Database.cs`:
```csharp
public class Database { }
```
";
        
        // Act
        var result = _parser.Parse(claudeResponse, _originalFiles);
        
        // Assert
        result.Updates.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_WithExplanationText_ShouldExtractOnlyCode()
    {
        // Arrange
        var response = @"
I've made the following improvements:
1. Added error handling
2. Improved performance

**Program.cs**
```csharp
public class Program { }
```

This approach is better because...
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert
        result.Updates.Should().ContainSingle();
        result.Updates[0].Content.Should().NotContain("I've made");
        result.Updates[0].Content.Should().NotContain("This approach");
    }

    [Fact]
    public void Parse_EstimatesLinesChanged_FromContent()
    {
        // Arrange
        var response = @"
**Program.cs**
```csharp
line 1
line 2
line 3
```
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert
        result.Updates[0].EstimatedLinesChanged.Should().Be(3);
    }

    [Fact]
    public void Parse_MultipleCodeBlocksNoHeaders_ShouldMapInOrder()
    {
        // Arrange
        var response = @"
```csharp
// First file
public class Program { }
```

```csharp
// Second file
public class Database { }
```
";
        
        // Act
        var result = _parser.Parse(response, _originalFiles);
        
        // Assert - Should map to original files in order
        result.Updates.Should().HaveCount(2);
        result.Updates[0].FilePath.Should().Be("Program.cs");
        result.Updates[1].FilePath.Should().Be("Database.cs");
    }
}

