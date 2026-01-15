using Xunit;
using FluentAssertions;
using ShieldPrompt.Infrastructure.Services;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using NSubstitute;

namespace ShieldPrompt.Tests.Unit.Application.PromptTemplates;

/// <summary>
/// Tests for YamlPromptTemplateRepository.
/// Phase 2 of Wizard UI implementation - TDD approach.
/// </summary>
public class PromptTemplateRepositoryTests
{
    [Fact]
    public void GetAllTemplates_ReturnsFallbackTemplates_WhenYamlMissing()
    {
        // Arrange
        var sut = new YamlPromptTemplateRepository("/nonexistent/path.yaml");

        // Act
        var templates = sut.GetAllTemplates();

        // Assert
        templates.Should().NotBeEmpty("fallback templates should always be available");
        templates.Should().Contain(t => t.Id == "code_review");
        templates.Should().Contain(t => t.Id == "debug");
        templates.Should().Contain(t => t.Id == "explain");
        templates.Should().Contain(t => t.Id == "generic");
    }

    [Fact]
    public void GetById_ReturnsCorrectTemplate()
    {
        // Arrange
        var sut = new YamlPromptTemplateRepository("/nonexistent/path.yaml");

        // Act
        var template = sut.GetById("code_review");

        // Assert
        template.Should().NotBeNull();
        template!.Name.Should().Be("Code Review");
        template.Icon.Should().Be("🔍");
        template.SystemPrompt.Should().Contain("code reviewer");
    }

    [Fact]
    public void GetById_ReturnsNull_WhenTemplateNotFound()
    {
        // Arrange
        var sut = new YamlPromptTemplateRepository("/nonexistent/path.yaml");

        // Act
        var template = sut.GetById("nonexistent_template");

        // Assert
        template.Should().BeNull();
    }

    [Fact]
    public void GetAllTemplates_CachesResults()
    {
        // Arrange
        var sut = new YamlPromptTemplateRepository("/nonexistent/path.yaml");

        // Act
        var templates1 = sut.GetAllTemplates();
        var templates2 = sut.GetAllTemplates();

        // Assert
        templates1.Should().BeSameAs(templates2, "templates should be cached");
    }

    [Fact]
    public void FallbackTemplates_IncludeRequiredFields()
    {
        // Arrange
        var sut = new YamlPromptTemplateRepository("/nonexistent/path.yaml");

        // Act
        var templates = sut.GetAllTemplates();

        // Assert
        foreach (var template in templates)
        {
            template.Id.Should().NotBeNullOrWhiteSpace();
            template.Name.Should().NotBeNullOrWhiteSpace();
            template.Icon.Should().NotBeNullOrWhiteSpace();
            template.Description.Should().NotBeNullOrWhiteSpace();
            template.FocusOptions.Should().NotBeNull();
        }
    }

    [Fact]
    public void DebugTemplate_RequiresCustomInput()
    {
        // Arrange
        var sut = new YamlPromptTemplateRepository("/nonexistent/path.yaml");

        // Act
        var template = sut.GetById("debug");

        // Assert
        template.Should().NotBeNull();
        template!.RequiresCustomInput.Should().BeTrue("debug template needs user to describe the issue");
        template.Placeholder.Should().NotBeNullOrWhiteSpace();
    }
}

/// <summary>
/// Tests for PromptComposer service.
/// Phase 2 of Wizard UI implementation - TDD approach.
/// </summary>
public class PromptComposerTests
{
    private readonly PromptComposer _sut;
    private readonly ITokenCountingService _tokenCountingService;

    public PromptComposerTests()
    {
        _tokenCountingService = Substitute.For<ITokenCountingService>();
        _tokenCountingService.CountTokens(Arg.Any<string>()).Returns(1000);
        
        _sut = new PromptComposer(_tokenCountingService);
    }

    [Fact]
    public void Compose_IncludesSystemPrompt_FromTemplate()
    {
        // Arrange
        var template = new PromptTemplate(
            "test", "Test", "🧪", "Test template",
            "You are a test assistant.",
            Array.Empty<string>());
        var files = Array.Empty<FileNode>();
        var options = new PromptOptions();

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.SystemPrompt.Should().Be("You are a test assistant.");
        result.FullPrompt.Should().Contain("You are a test assistant.");
    }

    [Fact]
    public void Compose_SubstitutesCustomInstructions()
    {
        // Arrange
        var template = new PromptTemplate(
            "test", "Test", "🧪", "Test template",
            "Issue: {custom_instructions}",
            Array.Empty<string>());
        var files = Array.Empty<FileNode>();
        var options = new PromptOptions(CustomInstructions: "App crashes on startup");

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.SystemPrompt.Should().Contain("App crashes on startup");
    }

    [Fact]
    public void Compose_IncludesFileContent()
    {
        // Arrange
        var template = new PromptTemplate(
            "test", "Test", "🧪", "Test",
            "Review this:",
            Array.Empty<string>());
        var files = new[]
        {
            new FileNode("/test/App.cs", "App.cs", false) { Content = "public class App { }" }
        };
        var options = new PromptOptions();

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.UserContent.Should().Contain("App.cs");
        result.UserContent.Should().Contain("public class App");
        result.FullPrompt.Should().Contain("```csharp");
    }

    [Fact]
    public void Compose_IncludesFilePaths_WhenOptionEnabled()
    {
        // Arrange
        var template = new PromptTemplate("test", "Test", "🧪", "Test", "", Array.Empty<string>());
        var files = new[]
        {
            new FileNode("/project/src/App.cs", "App.cs", false) { Content = "code" }
        };
        var options = new PromptOptions(IncludeFilePaths: true);

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.UserContent.Should().Contain("/project/src/App.cs");
    }

    [Fact]
    public void Compose_ExcludesFilePaths_WhenOptionDisabled()
    {
        // Arrange
        var template = new PromptTemplate("test", "Test", "🧪", "Test", "", Array.Empty<string>());
        var files = new[]
        {
            new FileNode("/project/src/App.cs", "App.cs", false) { Content = "code" }
        };
        var options = new PromptOptions(IncludeFilePaths: false);

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.UserContent.Should().Contain("App.cs");
        result.UserContent.Should().NotContain("/project/src/");
    }

    [Fact]
    public void Compose_IncludesFocusAreas_WhenSelected()
    {
        // Arrange
        var template = new PromptTemplate(
            "test", "Test", "🧪", "Test", "Review:",
            new[] { "Security", "Performance" });
        var files = Array.Empty<FileNode>();
        var options = new PromptOptions(
            SelectedFocusAreas: new[] { "Security", "Performance" });

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.FullPrompt.Should().Contain("Security");
        result.FullPrompt.Should().Contain("Performance");
        result.FullPrompt.Should().Contain("Focus Areas");
    }

    [Fact]
    public void Compose_AddsWarning_WhenCustomInputRequired_ButNotProvided()
    {
        // Arrange
        var template = new PromptTemplate(
            "debug", "Debug", "🐛", "Debug template",
            "{custom_instructions}",
            Array.Empty<string>(),
            RequiresCustomInput: true,
            Placeholder: "Describe issue...");
        var files = Array.Empty<FileNode>();
        var options = new PromptOptions(); // No custom instructions

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("requires custom instructions"));
    }

    [Fact]
    public void Compose_AddsWarning_WhenNoFilesSelected()
    {
        // Arrange
        var template = new PromptTemplate("test", "Test", "🧪", "Test", "Review:", Array.Empty<string>());
        var files = Array.Empty<FileNode>();
        var options = new PromptOptions();

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("No files selected"));
    }

    [Fact]
    public void Compose_CountsTokens()
    {
        // Arrange
        var template = new PromptTemplate("test", "Test", "🧪", "Test", "Review:", Array.Empty<string>());
        var files = new[]
        {
            new FileNode("/test/App.cs", "App.cs", false) { Content = "public class App { }" }
        };
        var options = new PromptOptions();
        _tokenCountingService.CountTokens(Arg.Any<string>()).Returns(2500);

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.EstimatedTokens.Should().Be(2500);
        _tokenCountingService.Received(1).CountTokens(Arg.Any<string>());
    }

    [Fact]
    public void Compose_DetectsLanguageFromExtension()
    {
        // Arrange
        var template = new PromptTemplate("test", "Test", "🧪", "Test", "", Array.Empty<string>());
        var files = new[]
        {
            new FileNode("/test/App.ts", "App.ts", false) { Content = "const x = 1;" }
        };
        var options = new PromptOptions();

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.UserContent.Should().Contain("```typescript");
    }

    [Fact]
    public void Compose_SubstitutesFileCount()
    {
        // Arrange
        var template = new PromptTemplate(
            "test", "Test", "🧪", "Test",
            "Reviewing {file_count} files",
            Array.Empty<string>());
        var files = new[]
        {
            new FileNode("/test/A.cs", "A.cs", false) { Content = "a" },
            new FileNode("/test/B.cs", "B.cs", false) { Content = "b" },
            new FileNode("/test/C.cs", "C.cs", false) { Content = "c" }
        };
        var options = new PromptOptions();

        // Act
        var result = _sut.Compose(template, files, options);

        // Assert
        result.SystemPrompt.Should().Contain("Reviewing 3 files");
    }
}

