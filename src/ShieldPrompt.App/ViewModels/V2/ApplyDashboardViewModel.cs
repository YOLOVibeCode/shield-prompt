using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for the Apply Dashboard - processes LLM responses and applies file operations.
/// Uses action system for reliable undo/redo support.
/// </summary>
public partial class ApplyDashboardViewModel : ObservableObject
{
    private readonly IStructuredResponseParser _parser;
    private readonly IFileActionFactory _actionFactory;
    private readonly IUndoRedoManager _undoRedoManager;
    private readonly IDiffService _diffService;
    private readonly IStatusMessageReporter _statusReporter;
    private string _workspaceRoot = string.Empty;

    [ObservableProperty]
    private string _responseText = string.Empty;

    [ObservableProperty]
    private bool _isParsing;

    [ObservableProperty]
    private bool _isApplying;

    [ObservableProperty]
    private bool _hasParsedOperations;

    [ObservableProperty]
    private string _parseStatusText = string.Empty;

    [ObservableProperty]
    private string _analysisText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FileOperationViewModel> _operations = new();

    [ObservableProperty]
    private FileOperationViewModel? _selectedOperation;

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private ApplyResult? _lastApplyResult;

    public ApplyDashboardViewModel(
        IStructuredResponseParser parser,
        IFileActionFactory actionFactory,
        IUndoRedoManager undoRedoManager,
        IDiffService diffService,
        IStatusMessageReporter statusReporter)
    {
        _parser = parser;
        _actionFactory = actionFactory;
        _undoRedoManager = undoRedoManager;
        _diffService = diffService;
        _statusReporter = statusReporter;

        _undoRedoManager.StateChanged += (s, e) =>
        {
            CanUndo = _undoRedoManager.CanUndo;
            CanRedo = _undoRedoManager.CanRedo;
        };
        CanUndo = _undoRedoManager.CanUndo; // Initial state
        CanRedo = _undoRedoManager.CanRedo; // Initial state
    }

    /// <summary>
    /// Sets the workspace root for file operations.
    /// </summary>
    public void SetWorkspaceRoot(string workspaceRoot)
    {
        _workspaceRoot = workspaceRoot;
    }

    /// <summary>
    /// Parses the response text and extracts operations.
    /// </summary>
    [RelayCommand]
    private async Task ParseResponseAsync()
    {
        if (string.IsNullOrWhiteSpace(ResponseText))
        {
            ParseStatusText = "No response text to parse";
            return;
        }

        IsParsing = true;
        ParseStatusText = "Parsing response...";
        _statusReporter.ReportInfo("Parsing LLM response...");

        try
        {
            var result = await _parser.ParseAsync(ResponseText);

            if (result.Success && result.Operations.Count > 0)
            {
                Operations.Clear();
                foreach (var op in result.Operations)
                {
                    var vm = new FileOperationViewModel(op, _diffService);
                    vm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(FileOperationViewModel.IsSelected))
                            UpdateSelectedCount();
                    };
                    Operations.Add(vm);
                }

                AnalysisText = result.Analysis ?? string.Empty;
                TotalCount = Operations.Count;
                UpdateSelectedCount();
                HasParsedOperations = true;

                var warnings = result.Warnings.Count > 0
                    ? $" ({result.Warnings.Count} warnings)"
                    : "";
                ParseStatusText = $"Found {result.Operations.Count} operations{warnings}";
                _statusReporter.ReportSuccess($"Parsed {result.Operations.Count} operations");
            }
            else
            {
                ParseStatusText = result.ErrorMessage ?? "No operations found in response";
                HasParsedOperations = false;
                _statusReporter.ReportWarning("No operations found");
            }
        }
        catch (Exception ex)
        {
            ParseStatusText = $"Parse error: {ex.Message}";
            HasParsedOperations = false;
            _statusReporter.ReportError($"Parse failed: {ex.Message}");
        }
        finally
        {
            IsParsing = false;
        }
    }

    /// <summary>
    /// Applies all selected operations using the action system.
    /// </summary>
    [RelayCommand]
    private async Task ApplySelectedAsync()
    {
        if (string.IsNullOrEmpty(_workspaceRoot))
        {
            _statusReporter.ReportError("No workspace selected");
            return;
        }

        var selectedVMs = Operations.Where(o => o.IsSelected && o.ApplyStatus != ApplyStatus.Applied).ToList();
        if (selectedVMs.Count == 0)
        {
            _statusReporter.ReportWarning("No operations selected");
            return;
        }

        IsApplying = true;
        _statusReporter.ReportInfo($"Applying {selectedVMs.Count} operations...");

        int successCount = 0;
        int failureCount = 0;

        try
        {
            foreach (var vm in selectedVMs)
            {
                try
                {
                    // 1. Create action from operation
                    var action = await _actionFactory.CreateAsync(vm.Operation, _workspaceRoot);

                    // 2. Wrap in adapter for UndoRedoManager
                    var adapter = new Application.Actions.FileActionAdapter(action);

                    // 3. Execute via UndoRedoManager (automatically records for undo)
                    await _undoRedoManager.ExecuteAsync(adapter);

                    // 4. Update status
                    vm.ApplyStatus = ApplyStatus.Applied;
                    vm.AppliedAt = DateTime.UtcNow;
                    vm.ErrorMessage = null;
                    successCount++;
                }
                catch (Exception ex)
                {
                    vm.ApplyStatus = ApplyStatus.Failed;
                    vm.ErrorMessage = ex.Message;
                    failureCount++;
                }
            }

            // Update CanUndo based on undo manager
            CanUndo = _undoRedoManager.CanUndo;

            if (failureCount == 0)
            {
                _statusReporter.ReportSuccess($"Applied {successCount} operations");
            }
            else
            {
                _statusReporter.ReportWarning($"Applied {successCount}/{selectedVMs.Count} ({failureCount} failed)");
            }
        }
        catch (Exception ex)
        {
            _statusReporter.ReportError($"Apply failed: {ex.Message}");
        }
        finally
        {
            IsApplying = false;
        }
    }

    /// <summary>
    /// Applies all operations.
    /// </summary>
    [RelayCommand]
    private async Task ApplyAllAsync()
    {
        SelectAll();
        await ApplySelectedAsync();
    }

    /// <summary>
    /// Undoes the last apply operation using the action system.
    /// </summary>
    [RelayCommand]
    private async Task UndoAsync()
    {
        if (!_undoRedoManager.CanUndo)
        {
            _statusReporter.ReportWarning("Nothing to undo");
            return;
        }

        _statusReporter.ReportInfo("Undoing last apply...");

        try
        {
            await _undoRedoManager.UndoAsync();

            // Find and update the last applied operation's status
            var lastApplied = Operations.LastOrDefault(o => o.ApplyStatus == ApplyStatus.Applied);
            if (lastApplied != null)
            {
                lastApplied.ApplyStatus = ApplyStatus.Pending;
                lastApplied.AppliedAt = null;
                lastApplied.ErrorMessage = null;
            }

            CanUndo = _undoRedoManager.CanUndo;
            CanRedo = _undoRedoManager.CanRedo;
            _statusReporter.ReportSuccess("Undo successful");
        }
        catch (Exception ex)
        {
            _statusReporter.ReportError($"Undo failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Redoes the last undone operation.
    /// </summary>
    [RelayCommand]
    private async Task RedoAsync()
    {
        if (!_undoRedoManager.CanRedo)
        {
            _statusReporter.ReportWarning("Nothing to redo");
            return;
        }

        _statusReporter.ReportInfo("Redoing last operation...");

        try
        {
            await _undoRedoManager.RedoAsync();

            // Update operation statuses after redo
            // For simplicity, mark all pending operations that were previously applied as applied again
            // Note: This is a simplified approach. A more robust solution would track specific operations.
            var pendingOps = Operations.Where(o => o.ApplyStatus == ApplyStatus.Pending).ToList();
            foreach (var op in pendingOps.Take(1)) // Redo the most recent
            {
                op.ApplyStatus = ApplyStatus.Applied;
                op.AppliedAt = DateTime.UtcNow;
            }

            CanUndo = _undoRedoManager.CanUndo;
            CanRedo = _undoRedoManager.CanRedo;
            _statusReporter.ReportSuccess("Redo successful");
        }
        catch (Exception ex)
        {
            _statusReporter.ReportError($"Redo failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Selects all operations.
    /// </summary>
    [RelayCommand]
    private void SelectAll()
    {
        foreach (var op in Operations)
            op.IsSelected = true;
        UpdateSelectedCount();
    }

    /// <summary>
    /// Deselects all operations.
    /// </summary>
    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var op in Operations)
            op.IsSelected = false;
        UpdateSelectedCount();
    }

    /// <summary>
    /// Clears all parsed operations and resets the dashboard.
    /// </summary>
    [RelayCommand]
    public void Clear()
    {
        ResponseText = string.Empty;
        Operations.Clear();
        AnalysisText = string.Empty;
        ParseStatusText = string.Empty;
        HasParsedOperations = false;
        SelectedCount = 0;
        TotalCount = 0;
        CanUndo = _undoRedoManager.CanUndo;
        CanRedo = _undoRedoManager.CanRedo;
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = Operations.Count(o => o.IsSelected);
    }

    // Summary properties for UI
    public int CreateCount => Operations.Count(o => o.OperationType == FileOperationType.Create);
    public int UpdateCount => Operations.Count(o => o.OperationType == FileOperationType.Update || o.OperationType == FileOperationType.PartialUpdate);
    public int DeleteCount => Operations.Count(o => o.OperationType == FileOperationType.Delete);
}

/// <summary>
/// Apply status for individual operations.
/// </summary>
public enum ApplyStatus
{
    Pending,
    Applied,
    Failed
}

/// <summary>
/// ViewModel for a single file operation.
/// </summary>
public partial class FileOperationViewModel : ObservableObject
{
    private readonly IDiffService _diffService;

    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private ApplyStatus _applyStatus = ApplyStatus.Pending;

    [ObservableProperty]
    private DateTime? _appliedAt;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private IReadOnlyList<DiffLine>? _diffLines;

    public FileOperation Operation { get; }

    public FileOperationViewModel(FileOperation operation, IDiffService diffService)
    {
        Operation = operation;
        _diffService = diffService;
    }

    public string Path => Operation.Path;
    public FileOperationType OperationType => Operation.Type;
    public string Reason => Operation.Reason;
    public string? Content => Operation.Content;
    public bool IsDestructive => Operation.IsDestructive;

    /// <summary>
    /// Icon for the operation type.
    /// </summary>
    public string TypeIcon => OperationType switch
    {
        FileOperationType.Create => "➕",
        FileOperationType.Update => "✏️",
        FileOperationType.PartialUpdate => "📝",
        FileOperationType.Delete => "🗑️",
        FileOperationType.Rename => "📛",
        _ => "📄"
    };

    /// <summary>
    /// Display name for the operation type.
    /// </summary>
    public string TypeName => OperationType switch
    {
        FileOperationType.Create => "CREATE",
        FileOperationType.Update => "UPDATE",
        FileOperationType.PartialUpdate => "PARTIAL",
        FileOperationType.Delete => "DELETE",
        FileOperationType.Rename => "RENAME",
        _ => "UNKNOWN"
    };

    /// <summary>
    /// Color for the operation type.
    /// </summary>
    public IBrush TypeColor => OperationType switch
    {
        FileOperationType.Create => Brushes.LightGreen,
        FileOperationType.Update => Brushes.Orange,
        FileOperationType.PartialUpdate => Brushes.Yellow,
        FileOperationType.Delete => Brushes.Red,
        FileOperationType.Rename => Brushes.Cyan,
        _ => Brushes.Gray
    };

    /// <summary>
    /// Status icon.
    /// </summary>
    public string StatusIcon => ApplyStatus switch
    {
        ApplyStatus.Applied => "✅",
        ApplyStatus.Failed => "❌",
        _ => ""
    };

    /// <summary>
    /// File name without path.
    /// </summary>
    public string FileName => System.IO.Path.GetFileName(Path);

    /// <summary>
    /// Directory path.
    /// </summary>
    public string Directory => System.IO.Path.GetDirectoryName(Path) ?? string.Empty;

    /// <summary>
    /// Computes diff from current content.
    /// </summary>
    public void ComputeDiff(string currentContent)
    {
        if (Content != null)
        {
            DiffLines = _diffService.ComputeDiff(currentContent, Content);
        }
    }
}
