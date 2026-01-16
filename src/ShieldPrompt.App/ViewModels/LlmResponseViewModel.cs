using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShieldPrompt.App.ViewModels;

/// <summary>
/// ViewModel for the LLM Response Dashboard tab.
/// Handles parsing, displaying, and applying file operations from LLM responses.
/// </summary>
public partial class LlmResponseViewModel : ObservableObject
{
    private readonly IStructuredResponseParser _parser;
    private readonly IFileWriterService _fileWriter;
    private readonly IUndoRedoManager _undoManager;
    private readonly IClipboardService _clipboard;
    
    [ObservableProperty]
    private string _responseText = string.Empty;
    
    [ObservableProperty]
    private ParseResult? _parseResult;
    
    [ObservableProperty]
    private ResponseStatistics _statistics = ResponseStatistics.Empty;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    public ObservableCollection<FileOperationViewModel> Operations { get; } = new();
    
    public LlmResponseViewModel(
        IStructuredResponseParser parser,
        IFileWriterService fileWriter,
        IUndoRedoManager undoManager,
        IClipboardService clipboard)
    {
        _parser = parser;
        _fileWriter = fileWriter;
        _undoManager = undoManager;
        _clipboard = clipboard;
    }
    
    /// <summary>
    /// Gets the number of selected operations.
    /// </summary>
    public int SelectedOperationCount => Operations.Count(op => op.IsSelected);
    
    /// <summary>
    /// Gets whether there are any operations to display.
    /// </summary>
    public bool HasOperations => Operations.Count > 0;
    
    [RelayCommand]
    private async Task PasteFromClipboardAsync(CancellationToken ct = default)
    {
        try
        {
            var clipboardText = await _clipboard.GetTextAsync(ct);
            
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                StatusMessage = "⚠️ Clipboard is empty.";
                return;
            }
            
            ResponseText = clipboardText;
            StatusMessage = "✅ Pasted from clipboard. Parsing...";
            
            // Auto-parse after paste
            await ParseResponseAsync(ct);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Failed to paste from clipboard: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task ParseResponseAsync(CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ResponseText))
            {
                StatusMessage = "⚠️ Response text is empty. Please paste an LLM response first.";
                return;
            }
            
            StatusMessage = "🔍 Parsing response...";
            
            // Parse the response (auto-detect format)
            var result = await _parser.ParseAsync(ResponseText, ResponseFormat.Auto, ct);
            
            ParseResult = result;
            
            if (!result.Success)
            {
                StatusMessage = $"❌ Failed to parse response. {string.Join(", ", result.Warnings.Select(w => w.Message))}";
                return;
            }
            
            // Populate operations
            Operations.Clear();
            foreach (var operation in result.Operations)
            {
                Operations.Add(new FileOperationViewModel(operation));
            }
            
            // Update statistics
            Statistics = CalculateStatistics(result);
            
            OnPropertyChanged(nameof(SelectedOperationCount));
            OnPropertyChanged(nameof(HasOperations));
            
            StatusMessage = result.Warnings.Any()
                ? $"✅ Parsed {result.Operations.Count} operations with {result.Warnings.Count} warnings."
                : $"✅ Parsed {result.Operations.Count} operations successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Parse error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task ApplyAllChangesAsync(CancellationToken ct = default)
    {
        try
        {
            var selectedOps = Operations.Where(op => op.IsSelected).ToList();
            
            if (selectedOps.Count == 0)
            {
                StatusMessage = "⚠️ No operations selected. Please select at least one operation to apply.";
                return;
            }
            
            StatusMessage = $"⚡ Applying {selectedOps.Count} operations...";
            
            // Convert FileOperation to FileUpdate
            var fileUpdates = selectedOps.Select(ConvertToFileUpdate).ToList();
            
            // Apply all operations as a batch
            var result = await _fileWriter.ApplyUpdatesAsync(
                fileUpdates,
                Environment.CurrentDirectory, // TODO: Get actual base directory from context
                new FileWriteOptions 
                { 
                    CreateBackup = true,
                    AllowCreateDirectories = true,
                    AllowDelete = true 
                },
                ct);
            
            // Update operation statuses based on result
            var totalSuccess = result.FilesCreated + result.FilesUpdated + result.FilesDeleted;
            
            if (result.Errors.Any())
            {
                // Mark some as failed (we don't know which exactly, so mark proportionally)
                foreach (var opVm in selectedOps.Take(result.Errors.Count))
                {
                    opVm.Status = OperationStatus.Failed;
                }
                foreach (var opVm in selectedOps.Skip(result.Errors.Count))
                {
                    opVm.Status = OperationStatus.Applied;
                }
                
                StatusMessage = $"⚠️ Applied {totalSuccess} operations with {result.Errors.Count} errors.";
            }
            else
            {
                // All succeeded
                foreach (var opVm in selectedOps)
                {
                    opVm.Status = OperationStatus.Applied;
                }
                
                StatusMessage = $"✅ Applied {totalSuccess} operations successfully! Press Ctrl+Z to undo.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Failed to apply changes: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void ClearDashboard()
    {
        ResponseText = string.Empty;
        ParseResult = null;
        Operations.Clear();
        Statistics = ResponseStatistics.Empty;
        StatusMessage = string.Empty;
        
        OnPropertyChanged(nameof(SelectedOperationCount));
        OnPropertyChanged(nameof(HasOperations));
    }
    
    private static FileUpdate ConvertToFileUpdate(FileOperationViewModel opVm)
    {
        var type = opVm.Operation.Type switch
        {
            FileOperationType.Create => FileUpdateType.Create,
            FileOperationType.Delete => FileUpdateType.Delete,
            _ => FileUpdateType.Update
        };
        
        var estimatedLines = opVm.Operation.Content?.Count(c => c == '\n') + 1 ?? 0;
        
        return new FileUpdate(
            FilePath: opVm.FilePath,
            Content: opVm.Operation.Content ?? string.Empty,
            Type: type,
            EstimatedLinesChanged: estimatedLines);
    }
    
    private static ResponseStatistics CalculateStatistics(ParseResult parseResult)
    {
        var updateCount = parseResult.Operations.Count(op => op.Type == FileOperationType.Update || op.Type == FileOperationType.PartialUpdate);
        var createCount = parseResult.Operations.Count(op => op.Type == FileOperationType.Create);
        var deleteCount = parseResult.Operations.Count(op => op.Type == FileOperationType.Delete);
        
        var estimatedLines = parseResult.Operations
            .Where(op => op.Content != null)
            .Sum(op => op.Content!.Count(c => c == '\n') + 1);
        
        return new ResponseStatistics(
            TotalOperations: parseResult.Operations.Count,
            UpdateCount: updateCount,
            CreateCount: createCount,
            DeleteCount: deleteCount,
            WarningCount: parseResult.Warnings.Count,
            EstimatedLinesAffected: estimatedLines,
            DetectedFormat: parseResult.DetectedFormat,
            ParsedAt: DateTime.UtcNow);
    }
}

