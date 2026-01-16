using ShieldPrompt.App.ViewModels.V2;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

public class WorkspaceRootViewModelTests
{
    [Theory]
    [InlineData("/Users/admin/Projects/MyRepo", "MyRepo")]
    [InlineData("/Users/admin/Projects/MyRepo/", "MyRepo")]
    [InlineData("C:\\Users\\Admin\\Projects\\MyRepo", "MyRepo")]
    [InlineData("C:\\Users\\Admin\\Projects\\MyRepo\\", "MyRepo")]
    [InlineData("/home/user/code", "code")]
    public void Constructor_SetsNameFromPath(string path, string expectedName)
    {
        // Arrange
        var rootNode = new FileNode(path, "root", true);

        // Act
        var vm = new WorkspaceRootViewModel(rootNode);

        // Assert
        Assert.Equal(expectedName, vm.Name);
        Assert.Equal(path, vm.Path);
    }

    [Fact]
    public void Constructor_FirstWorkspace_IsPrimaryByDefault()
    {
        // Arrange
        var rootNode = new FileNode("/test/path", "test", true);

        // Act
        var vm = new WorkspaceRootViewModel(rootNode, isPrimary: true);

        // Assert
        Assert.True(vm.IsPrimary);
        Assert.Equal("⭐", vm.Icon);
    }

    [Fact]
    public void Constructor_NonPrimaryWorkspace_HasFolderIcon()
    {
        // Arrange
        var rootNode = new FileNode("/test/path", "test", true);

        // Act
        var vm = new WorkspaceRootViewModel(rootNode, isPrimary: false);

        // Assert
        Assert.False(vm.IsPrimary);
        Assert.Equal("📁", vm.Icon);
    }

    [Fact]
    public void SetPrimary_ChangesIconAndBackground()
    {
        // Arrange
        var rootNode = new FileNode("/test/path", "test", true);
        var vm = new WorkspaceRootViewModel(rootNode, isPrimary: false);

        // Act
        vm.IsPrimary = true;

        // Assert
        Assert.Equal("⭐", vm.Icon);
        Assert.Equal("#2d3748", vm.BackgroundColor);
    }

    [Fact]
    public void Children_AreCreatedFromRootNodeChildren()
    {
        // Arrange
        var rootNode = new FileNode("/test/path", "root", true);
        var child1 = new FileNode("/test/path/file1.cs", "file1.cs", false);
        var child2 = new FileNode("/test/path/file2.cs", "file2.cs", false);
        rootNode.AddChild(child1);
        rootNode.AddChild(child2);

        // Act
        var vm = new WorkspaceRootViewModel(rootNode);

        // Assert
        Assert.Equal(2, vm.Children.Count);
        Assert.Equal("file1.cs", vm.Children[0].Name);
        Assert.Equal("file2.cs", vm.Children[1].Name);
    }

    [Fact]
    public void GetTotalFileCount_ReturnsCorrectCount()
    {
        // Arrange
        var rootNode = new FileNode("/test/path", "root", true);
        var subDir = new FileNode("/test/path/subdir", "subdir", true);
        var file1 = new FileNode("/test/path/file1.cs", "file1.cs", false);
        var file2 = new FileNode("/test/path/subdir/file2.cs", "file2.cs", false);
        var file3 = new FileNode("/test/path/subdir/file3.cs", "file3.cs", false);

        subDir.AddChild(file2);
        subDir.AddChild(file3);
        rootNode.AddChild(file1);
        rootNode.AddChild(subDir);

        // Act
        var vm = new WorkspaceRootViewModel(rootNode);
        var count = vm.GetTotalFileCount();

        // Assert
        Assert.Equal(3, count);
    }
}
