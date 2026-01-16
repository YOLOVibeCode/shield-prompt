using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class FileSearchServiceTests
{
    private readonly IFileSearchService _sut;

    public FileSearchServiceTests()
    {
        _sut = new FileSearchService();
    }

    private FileNode CreateTestFileTree()
    {
        var root = new FileNode("/root", "root", true);
        var src = new FileNode("/root/src", "src", true);
        var app = new FileNode("/root/src/App.cs", "App.cs", false);
        var user = new FileNode("/root/src/User.cs", "User.cs", false);
        var readme = new FileNode("/root/README.md", "README.md", false);
        var config = new FileNode("/root/config.json", "config.json", false);
        
        src.AddChild(app);
        src.AddChild(user);
        root.AddChild(src);
        root.AddChild(readme);
        root.AddChild(config);
        
        return root;
    }

    [Fact]
    public void SearchByPattern_WithGlobPattern_FindsMatchingFiles()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.SearchByPattern(root, "**/*.cs", isRegex: false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Name == "App.cs");
        result.Should().Contain(f => f.Name == "User.cs");
    }

    [Fact]
    public void SearchByPattern_WithRegexPattern_FindsMatchingFiles()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.SearchByPattern(root, @".*\.cs$", isRegex: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Name == "App.cs");
        result.Should().Contain(f => f.Name == "User.cs");
    }

    [Fact]
    public void SearchByPattern_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.SearchByPattern(root, "**/*.py", isRegex: false);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchByContentAsync_WithQuery_FindsFilesContainingText()
    {
        // Arrange
        var root = CreateTestFileTree();
        // Set content for files
        var appFile = root.Children.First(c => c.Name == "src").Children.First(c => c.Name == "App.cs");
        appFile.Content = "public class App { }";

        // Act
        var result = await _sut.SearchByContentAsync(root, "class App", CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("App.cs");
    }

    [Fact]
    public async Task SearchByContentAsync_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var root = CreateTestFileTree();
        var appFile = root.Children.First(c => c.Name == "src").Children.First(c => c.Name == "App.cs");
        appFile.Content = "public class App { }";

        // Act
        var result = await _sut.SearchByContentAsync(root, "nonexistent text", CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FilterByExtension_WithMultipleExtensions_FiltersCorrectly()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.FilterByExtension(root, new[] { ".cs", ".md" });

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(f => f.Name == "App.cs");
        result.Should().Contain(f => f.Name == "User.cs");
        result.Should().Contain(f => f.Name == "README.md");
    }

    [Fact]
    public void FilterByExtension_WithSingleExtension_FiltersCorrectly()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.FilterByExtension(root, new[] { ".json" });

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("config.json");
    }

    [Fact]
    public void FilterByName_WithMatchingQuery_FindsFiles()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.FilterByName(root, "App");

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("App.cs");
    }

    [Fact]
    public void FilterByName_IsCaseInsensitive()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.FilterByName(root, "app");

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("App.cs");
    }

    [Fact]
    public void FilterByName_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var root = CreateTestFileTree();

        // Act
        var result = _sut.FilterByName(root, "nonexistent");

        // Assert
        result.Should().BeEmpty();
    }
}

