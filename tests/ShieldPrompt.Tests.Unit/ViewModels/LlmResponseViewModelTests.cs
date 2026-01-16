using FluentAssertions;
using NSubstitute;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Interfaces;
using System.Collections.ObjectModel;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Unit tests for LlmResponseViewModel.
/// Validates LLM response parsing, dashboard state, and file operations.
/// </summary>
public class LlmResponseViewModelTests
{
    private readonly IStructuredResponseParser _mockParser;
    private readonly IFileWriterService _mockFileWriter;
    private readonly IUndoRedoManager _mockUndoManager;
    private readonly IClipboardService _mockClipboard;
    private readonly LlmResponseViewModel _sut;
    
    public LlmResponseViewModelTests()
    {
        _mockParser = Substitute.For<IStructuredResponseParser>();
        _mockFileWriter = Substitute.For<IFileWriterService>();
        _mockUndoManager = Substitute.For<IUndoRedoManager>();
        _mockClipboard = Substitute.For<IClipboardService>();
        
        _sut = new LlmResponseViewModel(
            _mockParser,
            _mockFileWriter,
            _mockUndoManager,
            _mockClipboard);
    }
    
    [Fact]
    public void Constructor_InitializesWithEmptyState()
    {
        // Assert
        _sut.ResponseText.Should().BeEmpty();
        _sut.ParseResult.Should().BeNull();
        _sut.Operations.Should().BeEmpty();
        _sut.Statistics.IsEmpty.Should().BeTrue();
        _sut.StatusMessage.Should().BeEmpty();
    }
    
    [Fact]
    public async Task PasteFromClipboardAsync_WithValidContent_UpdatesResponseText()
    {
        // Arrange
        const string clipboardContent = "# Analysis\nSome analysis\n```xml\n<code_changes></code_changes>\n```";
        _mockClipboard.GetTextAsync(Arg.Any<CancellationToken>()).Returns(clipboardContent);
        
        // Act
        await _sut.PasteFromClipboardCommand.ExecuteAsync(null);
        
        // Assert
        _sut.ResponseText.Should().Be(clipboardContent);
    }
    
    [Fact]
    public async Task PasteFromClipboardAsync_WithEmptyClipboard_ShowsErrorMessage()
    {
        // Arrange
        _mockClipboard.GetTextAsync(Arg.Any<CancellationToken>()).Returns(string.Empty);
        
        // Act
        await _sut.PasteFromClipboardCommand.ExecuteAsync(null);
        
        // Assert
        _sut.StatusMessage.Should().Contain("Clipboard is empty");
    }
    
    [Fact]
    public async Task ParseResponseAsync_WithValidResponse_PopulatesOperations()
    {
        // Arrange
        var operations = new List<FileOperation>
        {
            new(FileOperationType.Update, "src/Test.cs", "code", "Updated"),
            new(FileOperationType.Create, "src/New.cs", "new code", "Created")
        };
        
        var parseResult = new ParseResult(
            Success: true,
            Analysis: "Some analysis",
            Operations: operations,
            Warnings: new List<ParseWarning>(),
            DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        
        _mockParser.ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>())
            .Returns(parseResult);
        
        _sut.ResponseText = "valid response";
        
        // Act
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        
        // Assert
        _sut.ParseResult.Should().Be(parseResult);
        _sut.Operations.Should().HaveCount(2);
        _sut.Statistics.TotalOperations.Should().Be(2);
        _sut.Statistics.UpdateCount.Should().Be(1);
        _sut.Statistics.CreateCount.Should().Be(1);
    }
    
    [Fact]
    public async Task ParseResponseAsync_WithParseFailure_ShowsErrorMessage()
    {
        // Arrange
        var parseResult = new ParseResult(
            Success: false,
            Analysis: null,
            Operations: new List<FileOperation>(),
            Warnings: new List<ParseWarning> { new("Invalid XML", null, null) },
            DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        
        _mockParser.ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>())
            .Returns(parseResult);
        
        _sut.ResponseText = "invalid response";
        
        // Act
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        
        // Assert
        _sut.StatusMessage.Should().Contain("Failed to parse");
        _sut.Operations.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ParseResponseAsync_WithEmptyResponse_ShowsError()
    {
        // Arrange
        _sut.ResponseText = "";
        
        // Act
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        
        // Assert
        _sut.StatusMessage.Should().Contain("Response text is empty");
    }
    
    [Fact]
    public async Task ApplyAllChangesAsync_WithSelectedOperations_AppliesThemSequentially()
    {
        // Arrange
        var operation1 = new FileOperation(FileOperationType.Update, "test1.cs", "code1", "reason1");
        var operation2 = new FileOperation(FileOperationType.Create, "test2.cs", "code2", "reason2");
        
        var parseResult = new ParseResult(
            Success: true,
            Analysis: "analysis",
            Operations: new List<FileOperation> { operation1, operation2 },
            Warnings: new List<ParseWarning>(),
            DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        
        _mockParser.ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>())
            .Returns(parseResult);
        
        _mockFileWriter.ApplyUpdatesAsync(Arg.Any<IEnumerable<FileUpdate>>(), Arg.Any<string>(), Arg.Any<FileWriteOptions>(), Arg.Any<CancellationToken>())
            .Returns(new FileWriteResult(
                FilesCreated: 1,
                FilesUpdated: 1,
                FilesDeleted: 0,
                BackupId: "backup-123",
                Errors: Array.Empty<string>()));
        
        _sut.ResponseText = "response";
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        
        // Act
        await _sut.ApplyAllChangesCommand.ExecuteAsync(null);
        
        // Assert
        await _mockFileWriter.Received(1).ApplyUpdatesAsync(Arg.Any<IEnumerable<FileUpdate>>(), Arg.Any<string>(), Arg.Any<FileWriteOptions>(), Arg.Any<CancellationToken>());
        _sut.StatusMessage.Should().Contain("Applied 2");
    }
    
    [Fact]
    public async Task ApplyAllChangesAsync_WithNoSelectedOperations_ShowsMessage()
    {
        // Arrange
        var operation = new FileOperation(FileOperationType.Update, "test.cs", "code", "reason");
        var parseResult = new ParseResult(
            Success: true,
            Analysis: "analysis",
            Operations: new List<FileOperation> { operation },
            Warnings: new List<ParseWarning>(),
            DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        
        _mockParser.ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>())
            .Returns(parseResult);
        
        _sut.ResponseText = "response";
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        
        // Deselect all operations
        foreach (var op in _sut.Operations)
        {
            op.IsSelected = false;
        }
        
        // Act
        await _sut.ApplyAllChangesCommand.ExecuteAsync(null);
        
        // Assert
        _sut.StatusMessage.Should().Contain("No operations selected");
    }
    
    [Fact]
    public async Task ApplyAllChangesAsync_WithFileWriterError_MarksOperationAsFailed()
    {
        // Arrange
        var operation = new FileOperation(FileOperationType.Update, "test.cs", "code", "reason");
        var parseResult = new ParseResult(
            Success: true,
            Analysis: "analysis",
            Operations: new List<FileOperation> { operation },
            Warnings: new List<ParseWarning>(),
            DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        
        _mockParser.ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>())
            .Returns(parseResult);
        
        _mockFileWriter.ApplyUpdatesAsync(Arg.Any<IEnumerable<FileUpdate>>(), Arg.Any<string>(), Arg.Any<FileWriteOptions>(), Arg.Any<CancellationToken>())
            .Returns(new FileWriteResult(
                FilesCreated: 0,
                FilesUpdated: 0,
                FilesDeleted: 0,
                BackupId: "backup-123",
                Errors: new[] { "Disk full" }));
        
        _sut.ResponseText = "response";
        await _sut.ParseResponseCommand.ExecuteAsync(null);
        
        // Act
        await _sut.ApplyAllChangesCommand.ExecuteAsync(null);
        
        // Assert
        _sut.Operations[0].Status.Should().Be(OperationStatus.Failed);
        _sut.StatusMessage.Should().Contain("with");
        _sut.StatusMessage.Should().Contain("errors");
    }
    
    [Fact]
    public void ClearDashboard_ResetsAllState()
    {
        // Arrange
        _sut.ResponseText = "some response";
        _sut.Operations.Add(new FileOperationViewModel(
            new FileOperation(FileOperationType.Update, "test.cs", "code", "reason")));
        
        // Act
        _sut.ClearDashboardCommand.Execute(null);
        
        // Assert
        _sut.ResponseText.Should().BeEmpty();
        _sut.ParseResult.Should().BeNull();
        _sut.Operations.Should().BeEmpty();
        _sut.Statistics.IsEmpty.Should().BeTrue();
        _sut.StatusMessage.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ParseResponseAsync_AutomaticallyParsesAfterPaste()
    {
        // Arrange
        const string validResponse = "# Analysis\nSome text\n```xml\n<shieldprompt></shieldprompt>\n```";
        _mockClipboard.GetTextAsync(Arg.Any<CancellationToken>()).Returns(validResponse);
        
        var parseResult = new ParseResult(
            Success: true,
            Analysis: "analysis",
            Operations: new List<FileOperation>(),
            Warnings: new List<ParseWarning>(),
            DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        
        _mockParser.ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>())
            .Returns(parseResult);
        
        // Act
        await _sut.PasteFromClipboardCommand.ExecuteAsync(null);
        
        // Assert - Parse should be called automatically
        await _mockParser.Received(1).ParseAsync(Arg.Any<string>(), Arg.Any<ResponseFormat>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public void SelectedOperationCount_ReturnsCorrectCount()
    {
        // Arrange
        _sut.Operations.Add(new FileOperationViewModel(
            new FileOperation(FileOperationType.Update, "test1.cs", "code", "reason")) { IsSelected = true });
        _sut.Operations.Add(new FileOperationViewModel(
            new FileOperation(FileOperationType.Create, "test2.cs", "code", "reason")) { IsSelected = true });
        _sut.Operations.Add(new FileOperationViewModel(
            new FileOperation(FileOperationType.Delete, "test3.cs", null, "reason")) { IsSelected = false });
        
        // Act
        var count = _sut.SelectedOperationCount;
        
        // Assert
        count.Should().Be(2);
    }
    
    [Fact]
    public void HasOperations_ReturnsTrueWhenOperationsExist()
    {
        // Arrange
        _sut.Operations.Add(new FileOperationViewModel(
            new FileOperation(FileOperationType.Update, "test.cs", "code", "reason")));
        
        // Act & Assert
        _sut.HasOperations.Should().BeTrue();
    }
    
    [Fact]
    public void HasOperations_ReturnsFalseWhenEmpty()
    {
        // Act & Assert
        _sut.HasOperations.Should().BeFalse();
    }
}

