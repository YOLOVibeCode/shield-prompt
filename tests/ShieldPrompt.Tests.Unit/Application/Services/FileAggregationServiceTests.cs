using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class FileAggregationServiceTests : IDisposable
{
    private readonly IFileAggregationService _sut;
    private readonly string _testDirectory;

    public FileAggregationServiceTests()
    {
        _sut = new FileAggregationService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPromptTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    [Fact]
    public async Task LoadDirectoryAsync_WithValidPath_ReturnsTree()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "test.cs");
        await File.WriteAllTextAsync(file1, "public class Test { }");

        // Act
        var result = await _sut.LoadDirectoryAsync(_testDirectory);

        // Assert
        result.Should().NotBeNull();
        result.IsDirectory.Should().BeTrue();
        result.Children.Should().ContainSingle();
        result.Children.First().Name.Should().Be("test.cs");
    }

    [Fact]
    public async Task LoadDirectoryAsync_WithNestedFolders_ReturnsHierarchy()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "src");
        Directory.CreateDirectory(subDir);
        await File.WriteAllTextAsync(Path.Combine(subDir, "Program.cs"), "class Program { }");

        // Act
        var result = await _sut.LoadDirectoryAsync(_testDirectory);

        // Assert
        result.Children.Should().ContainSingle(n => n.Name == "src");
        var srcNode = result.Children.First(n => n.Name == "src");
        srcNode.IsDirectory.Should().BeTrue();
        srcNode.Children.Should().ContainSingle(n => n.Name == "Program.cs");
    }

    [Fact]
    public async Task LoadDirectoryAsync_WithNodeModules_ExcludesFolder()
    {
        // Arrange
        var nodeModules = Path.Combine(_testDirectory, "node_modules");
        Directory.CreateDirectory(nodeModules);
        await File.WriteAllTextAsync(Path.Combine(nodeModules, "package.json"), "{}");

        // Act
        var result = await _sut.LoadDirectoryAsync(_testDirectory);

        // Assert
        result.Children.Should().NotContain(n => n.Name == "node_modules");
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".exe")]
    [InlineData(".dll")]
    public void IsBinaryFile_WithBinaryExtension_ReturnsTrue(string extension)
    {
        // Arrange
        var path = $"/test/file{extension}";

        // Act
        var result = _sut.IsBinaryFile(path);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(".cs")]
    [InlineData(".txt")]
    [InlineData(".md")]
    [InlineData(".json")]
    public void IsBinaryFile_WithTextExtension_ReturnsFalse(string extension)
    {
        // Arrange
        var path = $"/test/file{extension}";

        // Act
        var result = _sut.IsBinaryFile(path);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("node_modules")]
    [InlineData(".git")]
    [InlineData("bin")]
    [InlineData("obj")]
    public void IsExcluded_WithExcludedFolder_ReturnsTrue(string folderName)
    {
        // Arrange
        var path = Path.Combine("/test", folderName, "file.txt");

        // Act
        var result = _sut.IsExcluded(path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExcluded_WithNormalPath_ReturnsFalse()
    {
        // Arrange
        var path = "/test/src/Program.cs";

        // Act
        var result = _sut.IsExcluded(path);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AggregateContentsAsync_WithThreeFiles_ConcatenatesAll()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "file1.txt");
        var file2 = Path.Combine(_testDirectory, "file2.txt");
        var file3 = Path.Combine(_testDirectory, "file3.txt");

        await File.WriteAllTextAsync(file1, "Content 1");
        await File.WriteAllTextAsync(file2, "Content 2");
        await File.WriteAllTextAsync(file3, "Content 3");

        var nodes = new[]
        {
            new FileNode(file1, "file1.txt", false),
            new FileNode(file2, "file2.txt", false),
            new FileNode(file3, "file3.txt", false)
        };

        // Act
        var result = await _sut.AggregateContentsAsync(nodes);

        // Assert
        result.Should().Contain("Content 1");
        result.Should().Contain("Content 2");
        result.Should().Contain("Content 3");
        result.Should().Contain("file1.txt");
        result.Should().Contain("file2.txt");
        result.Should().Contain("file3.txt");
    }

    [Fact]
    public async Task AggregateContentsAsync_WithEmptyList_ReturnsEmpty()
    {
        // Arrange
        var nodes = Array.Empty<FileNode>();

        // Act
        var result = await _sut.AggregateContentsAsync(nodes);

        // Assert
        result.Should().BeEmpty();
    }
}

