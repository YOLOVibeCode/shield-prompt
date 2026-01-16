using FluentAssertions;
using ShieldPrompt.App.ViewModels.V2;
using ShieldPrompt.Domain.Entities;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

/// <summary>
/// TDD tests for FileNodeViewModel cascading selection behavior.
/// 
/// Use Cases:
/// 1. Select folder → cascades down to all children
/// 2. Deselect folder → cascades down to all children
/// 3. Select all children → parent auto-selects
/// 4. Select some children → parent shows indeterminate
/// 5. Deselect all children → parent auto-deselects
/// </summary>
public class FileNodeViewModelTests
{
    #region Helper Methods

    private static FileNode CreateTestTree()
    {
        // Create tree:
        // root/
        //   ├── src/
        //   │   ├── App.cs
        //   │   ├── User.cs
        //   │   └── Services/
        //   │       └── AuthService.cs
        //   ├── tests/
        //   │   └── AppTests.cs
        //   └── README.md

        var root = new FileNode("/root", "root", true);

        var src = new FileNode("/root/src", "src", true);
        src.AddChild(new FileNode("/root/src/App.cs", "App.cs", false) { Content = "class App {}" });
        src.AddChild(new FileNode("/root/src/User.cs", "User.cs", false) { Content = "class User {}" });

        var services = new FileNode("/root/src/Services", "Services", true);
        services.AddChild(new FileNode("/root/src/Services/AuthService.cs", "AuthService.cs", false) { Content = "class Auth {}" });
        src.AddChild(services);

        var tests = new FileNode("/root/tests", "tests", true);
        tests.AddChild(new FileNode("/root/tests/AppTests.cs", "AppTests.cs", false) { Content = "class Tests {}" });

        root.AddChild(src);
        root.AddChild(tests);
        root.AddChild(new FileNode("/root/README.md", "README.md", false) { Content = "# README" });

        return root;
    }

    #endregion

    #region Use Case 1: Select Folder → Cascades Down

    [Fact]
    public void SelectFolder_CascadesDownToAllFiles()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");

        // Act - Select the src folder
        srcFolder.IsChecked = true;

        // Assert - All files in src should be selected
        srcFolder.Children.First(c => c.Name == "App.cs").IsChecked.Should().BeTrue();
        srcFolder.Children.First(c => c.Name == "User.cs").IsChecked.Should().BeTrue();
        
        var servicesFolder = srcFolder.Children.First(c => c.Name == "Services");
        servicesFolder.IsChecked.Should().BeTrue();
        servicesFolder.Children.First(c => c.Name == "AuthService.cs").IsChecked.Should().BeTrue();
    }

    [Fact]
    public void SelectFolder_CascadesDownToNestedFolders()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");

        // Act
        srcFolder.IsChecked = true;

        // Assert - Nested Services folder should also be selected
        var servicesFolder = srcFolder.Children.First(c => c.Name == "Services");
        servicesFolder.IsChecked.Should().BeTrue();
    }

    [Fact]
    public void SelectRootFolder_SelectsEntireTree()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);

        // Act - Select root
        vm.IsChecked = true;

        // Assert - Everything selected
        var allFiles = vm.GetAllFiles().ToList();
        allFiles.Should().HaveCount(5); // App.cs, User.cs, AuthService.cs, AppTests.cs, README.md
        allFiles.Should().OnlyContain(f => f.IsChecked == true);
    }

    #endregion

    #region Use Case 2: Deselect Folder → Cascades Down

    [Fact]
    public void DeselectFolder_CascadesDownToAllFiles()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");
        
        // First select everything
        srcFolder.IsChecked = true;
        srcFolder.Children.First(c => c.Name == "App.cs").IsChecked.Should().BeTrue(); // Verify selected

        // Act - Deselect the src folder
        srcFolder.IsChecked = false;

        // Assert - All files in src should be deselected
        srcFolder.Children.First(c => c.Name == "App.cs").IsChecked.Should().BeFalse();
        srcFolder.Children.First(c => c.Name == "User.cs").IsChecked.Should().BeFalse();
        
        var servicesFolder = srcFolder.Children.First(c => c.Name == "Services");
        servicesFolder.IsChecked.Should().BeFalse();
        servicesFolder.Children.First(c => c.Name == "AuthService.cs").IsChecked.Should().BeFalse();
    }

    [Fact]
    public void DeselectRootFolder_DeselectsEntireTree()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        vm.IsChecked = true; // Select all first

        // Act - Deselect root
        vm.IsChecked = false;

        // Assert - Everything deselected
        var allFiles = vm.GetAllFiles().ToList();
        allFiles.Should().OnlyContain(f => f.IsChecked == false);
    }

    #endregion

    #region Use Case 3: Select All Children → Parent Auto-Selects

    [Fact]
    public void SelectAllChildren_ParentBecomesSelected()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var testsFolder = vm.Children.First(c => c.Name == "tests");
        var testFile = testsFolder.Children.First(c => c.Name == "AppTests.cs");

        // Act - Select the only child file
        testFile.IsChecked = true;

        // Assert - Parent folder should become fully selected
        testsFolder.IsChecked.Should().BeTrue();
    }

    [Fact]
    public void SelectAllChildrenInFolder_ParentBecomesSelected()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");

        // Act - Select all children manually
        foreach (var child in srcFolder.Children)
        {
            if (child.IsDirectory)
            {
                // Select all nested children
                foreach (var nested in child.Children)
                {
                    nested.IsChecked = true;
                }
            }
            else
            {
                child.IsChecked = true;
            }
        }

        // Assert - src folder should be selected
        srcFolder.IsChecked.Should().BeTrue();
    }

    #endregion

    #region Use Case 4: Select Some Children → Parent Shows Indeterminate

    [Fact]
    public void SelectSomeChildren_ParentBecomesIndeterminate()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");

        // Act - Select only one child
        srcFolder.Children.First(c => c.Name == "App.cs").IsChecked = true;

        // Assert - src folder should be indeterminate (null)
        srcFolder.IsChecked.Should().BeNull();
    }

    [Fact]
    public void SelectSomeChildrenAtRoot_RootBecomesIndeterminate()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);

        // Act - Select only README.md
        vm.Children.First(c => c.Name == "README.md").IsChecked = true;

        // Assert - Root should be indeterminate
        vm.IsChecked.Should().BeNull();
    }

    [Fact]
    public void IndeterminateStatePropagatesUpward()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");
        var servicesFolder = srcFolder.Children.First(c => c.Name == "Services");

        // Act - Select only AuthService.cs deep in the tree
        servicesFolder.Children.First(c => c.Name == "AuthService.cs").IsChecked = true;

        // Assert - Both parent folders should be indeterminate
        servicesFolder.IsChecked.Should().BeTrue(); // Only child, so true
        srcFolder.IsChecked.Should().BeNull(); // Some children selected
        vm.IsChecked.Should().BeNull(); // Some children selected
    }

    #endregion

    #region Use Case 5: Deselect All Children → Parent Auto-Deselects

    [Fact]
    public void DeselectAllChildren_ParentBecomesDeselected()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var testsFolder = vm.Children.First(c => c.Name == "tests");
        var testFile = testsFolder.Children.First(c => c.Name == "AppTests.cs");

        // Select first
        testFile.IsChecked = true;
        testsFolder.IsChecked.Should().BeTrue();

        // Act - Deselect the only child
        testFile.IsChecked = false;

        // Assert - Parent should become deselected
        testsFolder.IsChecked.Should().BeFalse();
    }

    [Fact]
    public void DeselectLastChild_ParentBecomesDeselected()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);
        var srcFolder = vm.Children.First(c => c.Name == "src");

        // Select all first
        srcFolder.IsChecked = true;

        // Act - Deselect each file one by one
        srcFolder.Children.First(c => c.Name == "App.cs").IsChecked = false;
        srcFolder.IsChecked.Should().BeNull(); // Now indeterminate

        srcFolder.Children.First(c => c.Name == "User.cs").IsChecked = false;
        var servicesFolder = srcFolder.Children.First(c => c.Name == "Services");
        servicesFolder.Children.First(c => c.Name == "AuthService.cs").IsChecked = false;

        // Assert - src folder should be fully deselected
        servicesFolder.IsChecked.Should().BeFalse();
        srcFolder.IsChecked.Should().BeFalse();
    }

    #endregion

    #region GetSelectedFiles

    [Fact]
    public void GetSelectedFiles_ReturnsOnlySelectedFiles()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);

        // Select specific files
        var srcFolder = vm.Children.First(c => c.Name == "src");
        srcFolder.Children.First(c => c.Name == "App.cs").IsChecked = true;
        vm.Children.First(c => c.Name == "README.md").IsChecked = true;

        // Act
        var selectedFiles = vm.GetSelectedFiles().ToList();

        // Assert
        selectedFiles.Should().HaveCount(2);
        selectedFiles.Should().Contain(f => f.Name == "App.cs");
        selectedFiles.Should().Contain(f => f.Name == "README.md");
    }

    [Fact]
    public void GetSelectedFiles_ExcludesDirectories()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);

        // Select a folder
        var srcFolder = vm.Children.First(c => c.Name == "src");
        srcFolder.IsChecked = true;

        // Act
        var selectedFiles = vm.GetSelectedFiles().ToList();

        // Assert - Should include files, not the src directory itself
        selectedFiles.Should().HaveCount(3); // App.cs, User.cs, AuthService.cs
        selectedFiles.Should().NotContain(f => f.IsDirectory);
    }

    #endregion

    #region IsSelected Property

    [Fact]
    public void IsSelected_ReturnsTrueOnlyForCheckedFiles()
    {
        // Arrange
        var root = CreateTestTree();
        var vm = new FileNodeViewModel(root);

        // Select a folder (which is checked but not "selected")
        var srcFolder = vm.Children.First(c => c.Name == "src");
        srcFolder.IsChecked = true;

        // Assert - Folder is checked but IsSelected should be false (it's a directory)
        srcFolder.IsSelected.Should().BeFalse();
        
        // Files should have IsSelected = true
        srcFolder.Children.First(c => c.Name == "App.cs").IsSelected.Should().BeTrue();
    }

    #endregion

    #region Initial State

    [Fact]
    public void Constructor_SetsInitialStateFromModel()
    {
        // Arrange
        var model = new FileNode("/test", "test", false) { IsSelected = true };

        // Act
        var vm = new FileNodeViewModel(model);

        // Assert
        vm.IsChecked.Should().BeTrue();
    }

    [Fact]
    public void Constructor_DefaultsToUnchecked()
    {
        // Arrange
        var model = new FileNode("/test", "test", false);

        // Act
        var vm = new FileNodeViewModel(model);

        // Assert
        vm.IsChecked.Should().BeFalse();
    }

    #endregion
}

