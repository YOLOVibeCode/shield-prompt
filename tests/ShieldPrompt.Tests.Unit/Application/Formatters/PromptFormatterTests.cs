using FluentAssertions;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Tests.Unit.Application.Formatters;

public class PromptFormatterTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<FileNode> _testFiles;

    public PromptFormatterTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPromptTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        // Create test files
        var file1 = Path.Combine(_testDirectory, "Program.cs");
        var file2 = Path.Combine(_testDirectory, "User.cs");
        File.WriteAllText(file1, "public class Program { }");
        File.WriteAllText(file2, "public class User { }");

        _testFiles =
        [
            new FileNode(file1, "Program.cs", false),
            new FileNode(file2, "User.cs", false)
        ];
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    [Fact]
    public void PlainTextFormatter_WithFiles_CreatesPlainText()
    {
        // Arrange
        var formatter = new PlainTextFormatter();

        // Act
        var result = formatter.Format(_testFiles);

        // Assert
        result.Should().Contain("=== FILE:");
        result.Should().Contain("Program.cs");
        result.Should().Contain("User.cs");
        result.Should().Contain("public class Program");
        result.Should().Contain("public class User");
    }

    [Fact]
    public void MarkdownFormatter_WithFiles_CreatesMarkdown()
    {
        // Arrange
        var formatter = new MarkdownFormatter();

        // Act
        var result = formatter.Format(_testFiles);

        // Assert
        result.Should().Contain("# Project Context");
        result.Should().Contain("## Files");
        result.Should().Contain("### `Program.cs`");
        result.Should().Contain("```");
        result.Should().Contain("public class Program");
    }

    [Fact]
    public void XmlFormatter_WithFiles_CreatesValidXml()
    {
        // Arrange
        var formatter = new XmlFormatter();

        // Act
        var result = formatter.Format(_testFiles);

        // Assert
        result.Should().Contain("<project_context>");
        result.Should().Contain("<files>");
        result.Should().Contain("<file path=\"");
        result.Should().Contain("Program.cs");
        result.Should().Contain("<![CDATA[");
        result.Should().Contain("</project_context>");
    }

    [Fact]
    public void PlainTextFormatter_WithTaskDescription_IncludesTask()
    {
        // Arrange
        var formatter = new PlainTextFormatter();
        var task = "Refactor this code to use async/await";

        // Act
        var result = formatter.Format(_testFiles, task);

        // Assert
        result.Should().Contain(task);
    }

    [Fact]
    public void MarkdownFormatter_WithTaskDescription_IncludesTask()
    {
        // Arrange
        var formatter = new MarkdownFormatter();
        var task = "Add error handling";

        // Act
        var result = formatter.Format(_testFiles, task);

        // Assert
        result.Should().Contain("## Task");
        result.Should().Contain(task);
    }

    [Fact]
    public void XmlFormatter_WithTaskDescription_IncludesTask()
    {
        // Arrange
        var formatter = new XmlFormatter();
        var task = "Optimize performance";

        // Act
        var result = formatter.Format(_testFiles, task);

        // Assert
        result.Should().Contain("<description>");
        result.Should().Contain(task);
        result.Should().Contain("</description>");
    }

    [Fact]
    public void PlainTextFormatter_HasCorrectName()
    {
        // Arrange
        var formatter = new PlainTextFormatter();

        // Act & Assert
        formatter.FormatName.Should().Be("Plain Text");
    }

    [Fact]
    public void MarkdownFormatter_HasCorrectName()
    {
        // Arrange
        var formatter = new MarkdownFormatter();

        // Act & Assert
        formatter.FormatName.Should().Be("Markdown");
    }

    [Fact]
    public void XmlFormatter_HasCorrectName()
    {
        // Arrange
        var formatter = new XmlFormatter();

        // Act & Assert
        formatter.FormatName.Should().Be("XML");
    }
}

