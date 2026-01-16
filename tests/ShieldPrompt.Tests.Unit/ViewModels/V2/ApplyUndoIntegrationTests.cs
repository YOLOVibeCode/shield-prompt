using FluentAssertions;
using NSubstitute;
using ShieldPrompt.App.ViewModels.V2;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

/// <summary>
/// Integration tests for Apply/Undo workflow.
/// Tests the complete flow: Parse → Apply → Undo
/// Following the architect's recommendations for file write verification.
/// </summary>
public class ApplyUndoIntegrationTests : IDisposable
{
    private readonly string _testWorkspaceRoot;
    private readonly ApplyDashboardViewModel _sut;

    public ApplyUndoIntegrationTests()
    {
        // Create temporary test workspace
        _testWorkspaceRoot = Path.Combine(Path.GetTempPath(), $"ShieldPromptTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testWorkspaceRoot);

        // Use real implementations for integration testing
        var actionFactory = new ShieldPrompt.Application.Factories.FileActionFactory();
        var undoRedoManager = new ShieldPrompt.Application.Services.UndoRedoManager();
        var diffService = new ShieldPrompt.Application.Services.DiffService();
        var parser = new ShieldPrompt.Application.Parsers.StructuredResponseParser();

        // Mock status reporter
        var statusReporter = Substitute.For<IStatusMessageReporter>();

        _sut = new ApplyDashboardViewModel(
            parser,
            actionFactory,
            undoRedoManager,
            diffService,
            statusReporter);

        _sut.SetWorkspaceRoot(_testWorkspaceRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testWorkspaceRoot))
        {
            Directory.Delete(_testWorkspaceRoot, recursive: true);
        }
    }

    #region Apply Tests

    [Fact]
    public async Task ApplySelected_WithCreateOperation_WritesFileToDisc()
    {
        // Arrange
        var response = @"<code_changes>
            <changed_file>
                <file_path>test.md</file_path>
                <file_summary>Test file</file_summary>
                <file_operation>CREATE</file_operation>
                <file_code><![CDATA[# Test File
This is a test.]]></file_code>
            </changed_file>
        </code_changes>";

        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;

        // Act
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Assert
        var expectedPath = Path.Combine(_testWorkspaceRoot, "test.md");
        File.Exists(expectedPath).Should().BeTrue("file should be written to disk");

        var content = await File.ReadAllTextAsync(expectedPath);
        content.Should().Contain("# Test File");
        content.Should().Contain("This is a test.");
    }

    [Fact]
    public async Task ApplySelected_AfterSuccess_MarksOperationAsApplied()
    {
        // Arrange
        var response = BuildSimpleCreateResponse("applied-test.txt", "content");
        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        var operation = _sut.Operations.First();
        operation.IsSelected = true;
        operation.ApplyStatus.Should().Be(ApplyStatus.Pending, "initially pending");

        // Act
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Assert
        operation.ApplyStatus.Should().Be(ApplyStatus.Applied, "should be marked as applied");
    }

    [Fact]
    public async Task ApplySelected_WithNestedPath_CreatesDirectory()
    {
        // Arrange
        var response = BuildSimpleCreateResponse("src/nested/file.cs", "public class Test {}");
        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;

        // Act
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Assert
        var expectedPath = Path.Combine(_testWorkspaceRoot, "src", "nested", "file.cs");
        File.Exists(expectedPath).Should().BeTrue("should create nested directories");
    }

    [Fact]
    public async Task ApplyAll_WithMultipleOperations_AppliesAll()
    {
        // Arrange
        var response = @"<code_changes>
            <changed_file>
                <file_path>file1.txt</file_path>
                <file_summary>First file</file_summary>
                <file_operation>CREATE</file_operation>
                <file_code><![CDATA[Content 1]]></file_code>
            </changed_file>
            <changed_file>
                <file_path>file2.txt</file_path>
                <file_summary>Second file</file_summary>
                <file_operation>CREATE</file_operation>
                <file_code><![CDATA[Content 2]]></file_code>
            </changed_file>
        </code_changes>";

        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);

        // Act
        await _sut.ApplyAllCommand.ExecuteAsync(null);

        // Assert
        File.Exists(Path.Combine(_testWorkspaceRoot, "file1.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_testWorkspaceRoot, "file2.txt")).Should().BeTrue();
        _sut.Operations.All(op => op.ApplyStatus == ApplyStatus.Applied).Should().BeTrue("all should be applied");
    }

    #endregion

    #region Undo Tests

    [Fact]
    public async Task Undo_AfterCreateOperation_DeletesFile()
    {
        // Arrange
        var response = BuildSimpleCreateResponse("undo-test.txt", "test content");
        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        var filePath = Path.Combine(_testWorkspaceRoot, "undo-test.txt");
        File.Exists(filePath).Should().BeTrue("file should exist after apply");

        // Act
        await _sut.UndoCommand.ExecuteAsync(null);

        // Assert
        File.Exists(filePath).Should().BeFalse("file should be deleted after undo");
    }

    [Fact]
    public async Task Undo_AfterApply_MarksOperationAsPending()
    {
        // Arrange
        var response = BuildSimpleCreateResponse("status-test.txt", "content");
        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        var operation = _sut.Operations.First();
        operation.IsSelected = true;
        await _sut.ApplySelectedCommand.ExecuteAsync(null);
        operation.ApplyStatus.Should().Be(ApplyStatus.Applied);

        // Act
        await _sut.UndoCommand.ExecuteAsync(null);

        // Assert
        operation.ApplyStatus.Should().Be(ApplyStatus.Pending, "should revert to pending state");
    }

    [Fact]
    public async Task CanUndo_BeforeAnyApply_IsFalse()
    {
        // Assert
        _sut.CanUndo.Should().BeFalse("no actions to undo");
    }

    [Fact]
    public async Task CanUndo_AfterApply_IsTrue()
    {
        // Arrange
        var response = BuildSimpleCreateResponse("can-undo-test.txt", "content");
        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Assert
        _sut.CanUndo.Should().BeTrue("should have action to undo");
    }

    #endregion

    #region UPDATE Operation Tests

    [Fact]
    public async Task ApplySelected_WithUpdateOperation_ModifiesExistingFile()
    {
        // Arrange
        var filePath = Path.Combine(_testWorkspaceRoot, "update-test.txt");
        await File.WriteAllTextAsync(filePath, "Original content");

        var response = @"<code_changes>
            <changed_file>
                <file_path>update-test.txt</file_path>
                <file_summary>Updated content</file_summary>
                <file_operation>UPDATE</file_operation>
                <file_code><![CDATA[Updated content]]></file_code>
            </changed_file>
        </code_changes>";

        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;

        // Act
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Be("Updated content");
    }

    [Fact]
    public async Task Undo_AfterUpdate_RestoresOriginalContent()
    {
        // Arrange
        var filePath = Path.Combine(_testWorkspaceRoot, "restore-test.txt");
        var originalContent = "Original data";
        await File.WriteAllTextAsync(filePath, originalContent);

        var response = BuildSimpleUpdateResponse("restore-test.txt", "Modified data");
        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Act
        await _sut.UndoCommand.ExecuteAsync(null);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Be(originalContent, "undo should restore original content");
    }

    #endregion

    #region DELETE Operation Tests

    [Fact]
    public async Task ApplySelected_WithDeleteOperation_RemovesFile()
    {
        // Arrange
        var filePath = Path.Combine(_testWorkspaceRoot, "delete-test.txt");
        await File.WriteAllTextAsync(filePath, "To be deleted");

        var response = @"<code_changes>
            <changed_file>
                <file_path>delete-test.txt</file_path>
                <file_summary>Removed obsolete file</file_summary>
                <file_operation>DELETE</file_operation>
            </changed_file>
        </code_changes>";

        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;

        // Act
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Assert
        File.Exists(filePath).Should().BeFalse("file should be deleted");
    }

    [Fact]
    public async Task Undo_AfterDelete_RestoresFile()
    {
        // Arrange
        var filePath = Path.Combine(_testWorkspaceRoot, "restore-deleted.txt");
        var originalContent = "Important data";
        await File.WriteAllTextAsync(filePath, originalContent);

        var response = @"<code_changes>
            <changed_file>
                <file_path>restore-deleted.txt</file_path>
                <file_summary>Removed file</file_summary>
                <file_operation>DELETE</file_operation>
            </changed_file>
        </code_changes>";

        _sut.ResponseText = response;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        _sut.Operations.First().IsSelected = true;
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        // Act
        await _sut.UndoCommand.ExecuteAsync(null);

        // Assert
        File.Exists(filePath).Should().BeTrue("file should be restored");
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Be(originalContent, "restored content should match original");
    }

    #endregion

    #region Redo Tests

    [Fact]
    public async Task Redo_AfterUndo_RecreatesFile()
    {
        // Arrange - Create a file via XML
        var filePath = "redo_test.txt";
        var content = "Content for redo test.";
        var xmlResponse = BuildSimpleCreateResponse(filePath, content);

        _sut.ResponseText = xmlResponse;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        await _sut.ApplySelectedCommand.ExecuteAsync(null);

        var fullPath = Path.Combine(_testWorkspaceRoot, filePath);
        File.Exists(fullPath).Should().BeTrue("file should be created");

        // Undo
        await _sut.UndoCommand.ExecuteAsync(null);
        File.Exists(fullPath).Should().BeFalse("file should be deleted after undo");

        // Act - Redo
        await _sut.RedoCommand.ExecuteAsync(null);

        // Assert
        File.Exists(fullPath).Should().BeTrue("file should be recreated after redo");
        (await File.ReadAllTextAsync(fullPath)).Should().Be(content);
    }

    [Fact]
    public async Task CanRedo_UpdatesCorrectly()
    {
        // Initial state
        _sut.CanRedo.Should().BeFalse("should not be able to redo initially");

        // Create file
        var xmlResponse = BuildSimpleCreateResponse("test.txt", "content");
        _sut.ResponseText = xmlResponse;
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        await _sut.ApplySelectedCommand.ExecuteAsync(null);
        _sut.CanRedo.Should().BeFalse("should not be able to redo after apply");

        // Undo
        await _sut.UndoCommand.ExecuteAsync(null);
        _sut.CanRedo.Should().BeTrue("should be able to redo after undo");

        // Redo
        await _sut.RedoCommand.ExecuteAsync(null);
        _sut.CanRedo.Should().BeFalse("should not be able to redo after redo");
    }

    #endregion

    #region Helper Methods

    private static string BuildSimpleCreateResponse(string path, string content)
    {
        return $@"<code_changes>
            <changed_file>
                <file_path>{path}</file_path>
                <file_summary>Test file</file_summary>
                <file_operation>CREATE</file_operation>
                <file_code><![CDATA[{content}]]></file_code>
            </changed_file>
        </code_changes>";
    }

    private static string BuildSimpleUpdateResponse(string path, string content)
    {
        return $@"<code_changes>
            <changed_file>
                <file_path>{path}</file_path>
                <file_summary>Updated file</file_summary>
                <file_operation>UPDATE</file_operation>
                <file_code><![CDATA[{content}]]></file_code>
            </changed_file>
        </code_changes>";
    }

    #endregion
}

