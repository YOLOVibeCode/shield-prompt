using FluentAssertions;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Unit tests for FileOperationViewModel.
/// Validates file operation UI representation and state management.
/// </summary>
public class FileOperationViewModelTests
{
    [Fact]
    public void Constructor_WithValidOperation_InitializesProperties()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Program.cs",
            Content: "// Updated code",
            Reason: "Added null check");
        
        // Act
        var vm = new FileOperationViewModel(operation);
        
        // Assert
        vm.Operation.Should().Be(operation);
        vm.IsSelected.Should().BeTrue(); // Default to selected
        vm.Status.Should().Be(OperationStatus.Pending);
        vm.WarningMessage.Should().BeNull();
    }
    
    [Fact]
    public void OperationType_ReturnsCorrectType()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Create,
            Path: "src/NewFile.cs",
            Content: "// New file",
            Reason: "Added feature");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.OperationType.Should().Be(FileOperationType.Create);
    }
    
    [Fact]
    public void FilePath_ReturnsCorrectPath()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Delete,
            Path: "src/OldFile.cs",
            Content: null,
            Reason: "Deprecated");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.FilePath.Should().Be("src/OldFile.cs");
    }
    
    [Fact]
    public void Reason_ReturnsCorrectReason()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Test.cs",
            Content: "code",
            Reason: "Fixed bug");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.Reason.Should().Be("Fixed bug");
    }
    
    [Fact]
    public void IsPartialUpdate_WithLineNumbers_ReturnsTrue()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Service.cs",
            Content: "// Partial",
            Reason: "Updated method",
            StartLine: 10,
            EndLine: 20);
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.IsPartialUpdate.Should().BeTrue();
    }
    
    [Fact]
    public void IsPartialUpdate_WithoutLineNumbers_ReturnsFalse()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Service.cs",
            Content: "// Full file",
            Reason: "Complete rewrite");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.IsPartialUpdate.Should().BeFalse();
    }
    
    [Fact]
    public void IsDestructive_WithDeleteOperation_ReturnsTrue()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Delete,
            Path: "src/Old.cs",
            Content: null,
            Reason: "Removed");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.IsDestructive.Should().BeTrue();
    }
    
    [Fact]
    public void IsDestructive_WithCreateOperation_ReturnsFalse()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Create,
            Path: "src/New.cs",
            Content: "code",
            Reason: "Added");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.IsDestructive.Should().BeFalse();
    }
    
    [Fact]
    public void LineRangeText_WithPartialUpdate_ReturnsFormattedRange()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Test.cs",
            Content: "code",
            Reason: "Updated",
            StartLine: 5,
            EndLine: 10);
        var vm = new FileOperationViewModel(operation);
        
        // Act
        var rangeText = vm.LineRangeText;
        
        // Assert
        rangeText.Should().Be("Lines 5-10");
    }
    
    [Fact]
    public void LineRangeText_WithoutLineNumbers_ReturnsFullFile()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Test.cs",
            Content: "code",
            Reason: "Updated");
        var vm = new FileOperationViewModel(operation);
        
        // Act
        var rangeText = vm.LineRangeText;
        
        // Assert
        rangeText.Should().Be("Full file");
    }
    
    [Fact]
    public void ImpactText_WithPartialUpdate_ReturnsLineCount()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Update,
            Path: "src/Test.cs",
            Content: "code",
            Reason: "Updated",
            StartLine: 10,
            EndLine: 15);
        var vm = new FileOperationViewModel(operation);
        
        // Act
        var impact = vm.ImpactText;
        
        // Assert
        impact.Should().Contain("6 lines"); // 15-10+1 = 6
    }
    
    [Fact]
    public void ImpactText_WithNewFile_ReturnsNewFileMessage()
    {
        // Arrange
        var content = string.Join("\n", Enumerable.Repeat("line", 50));
        var operation = new FileOperation(
            Type: FileOperationType.Create,
            Path: "src/New.cs",
            Content: content,
            Reason: "Created");
        var vm = new FileOperationViewModel(operation);
        
        // Act
        var impact = vm.ImpactText;
        
        // Assert
        impact.Should().Contain("New file");
        impact.Should().Contain("50 lines");
    }
    
    [Fact]
    public void ImpactText_WithDeleteOperation_ReturnsDeleteMessage()
    {
        // Arrange
        var operation = new FileOperation(
            Type: FileOperationType.Delete,
            Path: "src/Old.cs",
            Content: null,
            Reason: "Removed");
        var vm = new FileOperationViewModel(operation);
        
        // Act
        var impact = vm.ImpactText;
        
        // Assert
        impact.Should().Be("File deletion");
    }
    
    [Fact]
    public void OperationIcon_ReturnsCorrectIconForEachType()
    {
        // Arrange & Act & Assert
        var updateOp = new FileOperationViewModel(new FileOperation(FileOperationType.Update, "test", "code", "reason"));
        updateOp.OperationIcon.Should().Be("✏️");
        
        var createOp = new FileOperationViewModel(new FileOperation(FileOperationType.Create, "test", "code", "reason"));
        createOp.OperationIcon.Should().Be("➕");
        
        var deleteOp = new FileOperationViewModel(new FileOperation(FileOperationType.Delete, "test", null, "reason"));
        deleteOp.OperationIcon.Should().Be("🗑️");
        
        var partialOp = new FileOperationViewModel(new FileOperation(FileOperationType.PartialUpdate, "test", "code", "reason", 1, 5));
        partialOp.OperationIcon.Should().Be("📝");
    }
    
    [Fact]
    public void StatusIcon_ReturnsCorrectIconForEachStatus()
    {
        // Arrange
        var operation = new FileOperation(FileOperationType.Update, "test", "code", "reason");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.Status = OperationStatus.Pending;
        vm.StatusIcon.Should().Be("⏳");
        
        vm.Status = OperationStatus.Applied;
        vm.StatusIcon.Should().Be("✅");
        
        vm.Status = OperationStatus.Failed;
        vm.StatusIcon.Should().Be("❌");
        
        vm.Status = OperationStatus.Skipped;
        vm.StatusIcon.Should().Be("⏭️");
        
        vm.Status = OperationStatus.Warning;
        vm.StatusIcon.Should().Be("⚠️");
    }
    
    [Fact]
    public void HasWarning_WithWarningMessage_ReturnsTrue()
    {
        // Arrange
        var operation = new FileOperation(FileOperationType.Create, "test.cs", "code", "reason");
        var vm = new FileOperationViewModel(operation)
        {
            WarningMessage = "File already exists"
        };
        
        // Act & Assert
        vm.HasWarning.Should().BeTrue();
    }
    
    [Fact]
    public void HasWarning_WithoutWarningMessage_ReturnsFalse()
    {
        // Arrange
        var operation = new FileOperation(FileOperationType.Create, "test.cs", "code", "reason");
        var vm = new FileOperationViewModel(operation);
        
        // Act & Assert
        vm.HasWarning.Should().BeFalse();
    }
    
    [Fact]
    public void ToggleSelection_ChangesIsSelectedState()
    {
        // Arrange
        var operation = new FileOperation(FileOperationType.Update, "test", "code", "reason");
        var vm = new FileOperationViewModel(operation);
        var initialState = vm.IsSelected;
        
        // Act
        vm.ToggleSelection();
        
        // Assert
        vm.IsSelected.Should().Be(!initialState);
    }
}

