using FluentAssertions;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class FileNodeTests
{
    [Fact]
    public void FileNode_WithFile_CreatesCorrectly()
    {
        // Arrange
        var path = "/test/file.cs";
        var name = "file.cs";
        var isDirectory = false;

        // Act
        var node = new FileNode(path, name, isDirectory);

        // Assert
        node.Path.Should().Be(path);
        node.Name.Should().Be(name);
        node.IsDirectory.Should().BeFalse();
        node.IsSelected.Should().BeFalse(); // Default
        node.Children.Should().BeEmpty();
    }

    [Fact]
    public void FileNode_WithDirectory_CreatesCorrectly()
    {
        // Arrange
        var path = "/test/src";
        var name = "src";
        var isDirectory = true;

        // Act
        var node = new FileNode(path, name, isDirectory);

        // Assert
        node.Path.Should().Be(path);
        node.Name.Should().Be(name);
        node.IsDirectory.Should().BeTrue();
        node.Children.Should().NotBeNull();
    }

    [Fact]
    public void AddChild_WithValidChild_AddsToChildren()
    {
        // Arrange
        var parent = new FileNode("/test", "test", true);
        var child = new FileNode("/test/file.cs", "file.cs", false);

        // Act
        parent.AddChild(child);

        // Assert
        parent.Children.Should().ContainSingle();
        parent.Children.First().Should().Be(child);
    }

    [Theory]
    [InlineData(".cs")]
    [InlineData(".txt")]
    [InlineData(".md")]
    public void Extension_WithFile_ReturnsCorrectExtension(string expectedExt)
    {
        // Arrange
        var node = new FileNode($"/test/file{expectedExt}", $"file{expectedExt}", false);

        // Act
        var extension = node.Extension;

        // Assert
        extension.Should().Be(expectedExt);
    }

    [Fact]
    public void Extension_WithDirectory_ReturnsEmpty()
    {
        // Arrange
        var node = new FileNode("/test/folder", "folder", true);

        // Act
        var extension = node.Extension;

        // Assert
        extension.Should().BeEmpty();
    }

    [Fact]
    public void IsSelected_SetTrue_UpdatesProperty()
    {
        // Arrange
        var node = new FileNode("/test/file.cs", "file.cs", false);

        // Act
        node.IsSelected = true;

        // Assert
        node.IsSelected.Should().BeTrue();
    }
}

