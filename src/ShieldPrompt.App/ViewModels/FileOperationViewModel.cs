using CommunityToolkit.Mvvm.ComponentModel;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using System;
using System.Linq;

namespace ShieldPrompt.App.ViewModels;

/// <summary>
/// ViewModel representing a single file operation from an LLM response.
/// Provides UI-friendly properties and state management.
/// </summary>
public partial class FileOperationViewModel : ObservableObject
{
    [ObservableProperty]
    private FileOperation _operation;
    
    [ObservableProperty]
    private bool _isSelected;
    
    [ObservableProperty]
    private OperationStatus _status;
    
    [ObservableProperty]
    private string? _warningMessage;
    
    public FileOperationViewModel(FileOperation operation)
    {
        _operation = operation;
        _isSelected = true; // Default to selected
        _status = OperationStatus.Pending;
    }
    
    /// <summary>
    /// Gets the type of operation.
    /// </summary>
    public FileOperationType OperationType => Operation.Type;
    
    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string FilePath => Operation.Path;
    
    /// <summary>
    /// Gets the reason for this operation.
    /// </summary>
    public string Reason => Operation.Reason;
    
    /// <summary>
    /// Gets whether this is a partial update (has line numbers).
    /// </summary>
    public bool IsPartialUpdate => Operation.StartLine.HasValue && Operation.EndLine.HasValue;
    
    /// <summary>
    /// Gets whether this operation is destructive (delete or full update).
    /// </summary>
    public bool IsDestructive => Operation.IsDestructive;
    
    /// <summary>
    /// Gets the line range text (e.g., "Lines 10-20" or "Full file").
    /// </summary>
    public string LineRangeText
    {
        get
        {
            if (IsPartialUpdate)
                return $"Lines {Operation.StartLine}-{Operation.EndLine}";
            return "Full file";
        }
    }
    
    /// <summary>
    /// Gets a human-readable impact description.
    /// </summary>
    public string ImpactText
    {
        get
        {
            if (Operation.Type == FileOperationType.Delete)
                return "File deletion";
            
            if (Operation.Type == FileOperationType.Create)
                return $"New file ({CountLines(Operation.Content)} lines)";
            
            // Check if it's a partial update (has line numbers)
            if (IsPartialUpdate)
                return $"Partial update ({Operation.EndLine!.Value - Operation.StartLine!.Value + 1} lines)";
            
            // Full file replacement
            return $"Full file replacement ({CountLines(Operation.Content)} lines)";
        }
    }
    
    /// <summary>
    /// Gets the icon for this operation type.
    /// </summary>
    public string OperationIcon => Operation.Type switch
    {
        FileOperationType.Update => "✏️",
        FileOperationType.PartialUpdate => "📝",
        FileOperationType.Create => "➕",
        FileOperationType.Delete => "🗑️",
        _ => "❓"
    };
    
    /// <summary>
    /// Gets the icon for the current status.
    /// </summary>
    public string StatusIcon => Status switch
    {
        OperationStatus.Pending => "⏳",
        OperationStatus.Applied => "✅",
        OperationStatus.Failed => "❌",
        OperationStatus.Skipped => "⏭️",
        OperationStatus.Warning => "⚠️",
        _ => "❓"
    };
    
    /// <summary>
    /// Gets whether this operation has a warning.
    /// </summary>
    public bool HasWarning => !string.IsNullOrWhiteSpace(WarningMessage);
    
    /// <summary>
    /// Toggles the selection state of this operation.
    /// </summary>
    public void ToggleSelection()
    {
        IsSelected = !IsSelected;
    }
    
    private static int CountLines(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return 0;
        return content.Count(c => c == '\n') + 1;
    }
}

/// <summary>
/// Status of a file operation in the UI.
/// </summary>
public enum OperationStatus
{
    Pending,
    Applied,
    Failed,
    Skipped,
    Warning
}

