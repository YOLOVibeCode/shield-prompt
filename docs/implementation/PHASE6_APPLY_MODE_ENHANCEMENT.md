# Phase 6: Apply Mode Enhancement Implementation Specification

**Phase ID:** PHASE-6  
**Priority:** P0 (CRITICAL)  
**Estimated Effort:** 5-6 days  
**Prerequisites:** Phase 1, Phase 3  
**Status:** PENDING

---

## 1. Executive Summary

Apply Mode Enhancement is the **most critical phase** of v2.0. It transforms ShieldPrompt from a prompt generator into a complete AI-assisted development tool by enabling users to **automatically apply LLM-generated file changes** with full preview, conflict resolution, and undo support.

---

## 2. Feature Requirements

### 2.1 Core Features

| Feature | Description | Priority |
|---------|-------------|----------|
| Response Parser | Parse structured LLM responses | P0 |
| Change Preview | Visual diff of proposed changes | P0 |
| Apply Changes | Execute file operations | P0 |
| Undo Support | Full undo of applied changes | P0 |
| Conflict Resolution | Handle merge conflicts | P1 |
| Partial Apply | Apply only selected changes | P1 |
| Backup System | Auto-backup before changes | P0 |

### 2.2 Supported Operations

| Operation | Description | Safety Level |
|-----------|-------------|--------------|
| CREATE | Create new file | Low risk |
| UPDATE | Replace entire file | Medium risk |
| PARTIAL_UPDATE | Update specific lines | Medium risk |
| DELETE | Delete file | High risk |
| RENAME | Rename/move file | Medium risk |

### 2.3 Response Format (From Architect Spec)

```xml
<shieldprompt version="1.0">
  <file op="UPDATE" path="src/App.cs" reason="Refactor to async">
<![CDATA[
// Full file content here
]]>
  </file>
  <file op="CREATE" path="src/NewService.cs" reason="Extract service">
<![CDATA[
// New file content
]]>
  </file>
  <file op="DELETE" path="src/OldFile.cs" reason="No longer needed"/>
</shieldprompt>
```

---

## 3. Domain Model Specification

### 3.1 FileOperation Record (EXISTS - VERIFY)

**File:** `src/ShieldPrompt.Domain/Records/FileOperation.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Represents a file operation to be applied.
/// </summary>
public record FileOperation(
    FileOperationType Type,
    string FilePath,
    string? Content,
    string? Reason,
    int? StartLine = null,
    int? EndLine = null,
    string? OriginalPath = null);  // For RENAME

public enum FileOperationType
{
    Create,
    Update,
    PartialUpdate,
    Delete,
    Rename
}
```

### 3.2 ApplyResult Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/ApplyResult.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Result of applying file operations.
/// </summary>
public record ApplyResult(
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<AppliedOperation> Operations,
    IReadOnlyList<string> Errors,
    string BackupId);

public record AppliedOperation(
    FileOperation Operation,
    bool Success,
    string? Error,
    string? BackupPath);
```

### 3.3 ConflictInfo Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/ConflictInfo.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Information about a file conflict.
/// </summary>
public record ConflictInfo(
    string FilePath,
    ConflictType Type,
    string? CurrentContent,
    string? ProposedContent,
    string? OriginalContent);

public enum ConflictType
{
    FileModified,       // File changed since prompt was generated
    FileDeleted,        // File was deleted
    FileCreatedExists,  // CREATE but file already exists
    MergeConflict       // Partial update conflicts
}
```

---

## 4. Interface Specifications (ISP-Compliant)

### 4.1 IStructuredResponseParser (EXISTS - VERIFY)

**File:** `src/ShieldPrompt.Application/Interfaces/IStructuredResponseParser.cs`

**ISP Compliance:** 3 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Parses structured LLM responses into file operations.
/// </summary>
public interface IStructuredResponseParser
{
    /// <summary>
    /// Parses an LLM response into file operations.
    /// </summary>
    Task<ParsedResponse> ParseAsync(string response, CancellationToken ct = default);
    
    /// <summary>
    /// Validates a response format without parsing.
    /// </summary>
    bool IsValidFormat(string response);
    
    /// <summary>
    /// Gets the expected format for prompt instructions.
    /// </summary>
    string GetFormatInstructions();
}
```

### 4.2 IFileApplyService (NEW/ENHANCED)

**File:** `src/ShieldPrompt.Application/Interfaces/IFileApplyService.cs`

**ISP Compliance:** 5 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for applying file operations from LLM responses.
/// Follows ISP - apply operations only.
/// </summary>
public interface IFileApplyService
{
    /// <summary>
    /// Previews operations without applying.
    /// </summary>
    Task<ApplyPreview> PreviewAsync(
        IEnumerable<FileOperation> operations, 
        string workspaceRoot,
        CancellationToken ct = default);
    
    /// <summary>
    /// Applies operations with backup.
    /// </summary>
    Task<ApplyResult> ApplyAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default);
    
    /// <summary>
    /// Applies selected operations only.
    /// </summary>
    Task<ApplyResult> ApplySelectiveAsync(
        IEnumerable<FileOperation> operations,
        IEnumerable<string> selectedPaths,
        string workspaceRoot,
        CancellationToken ct = default);
    
    /// <summary>
    /// Undoes the last apply operation.
    /// </summary>
    Task<bool> UndoAsync(string backupId, CancellationToken ct = default);
    
    /// <summary>
    /// Checks for conflicts before apply.
    /// </summary>
    Task<IReadOnlyList<ConflictInfo>> CheckConflictsAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default);
}

public record ApplyPreview(
    IReadOnlyList<FileOperationPreview> Previews,
    int TotalFiles,
    int CreatedCount,
    int UpdatedCount,
    int DeletedCount,
    IReadOnlyList<string> Warnings);

public record FileOperationPreview(
    FileOperation Operation,
    string? CurrentContent,
    string? ProposedContent,
    IReadOnlyList<DiffLine>? Diff,
    bool HasConflict);
```

### 4.3 IBackupService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IBackupService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing file backups.
/// Follows ISP - backup operations only.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates a backup of files.
    /// </summary>
    Task<string> CreateBackupAsync(
        IEnumerable<string> filePaths, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Restores files from a backup.
    /// </summary>
    Task<bool> RestoreBackupAsync(string backupId, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a backup.
    /// </summary>
    Task DeleteBackupAsync(string backupId, CancellationToken ct = default);
    
    /// <summary>
    /// Lists available backups.
    /// </summary>
    Task<IReadOnlyList<BackupInfo>> ListBackupsAsync(CancellationToken ct = default);
}

public record BackupInfo(
    string Id,
    DateTime CreatedAt,
    int FileCount,
    IReadOnlyList<string> FilePaths);
```

### 4.4 IDiffService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IDiffService.cs`

**ISP Compliance:** 2 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for computing file diffs.
/// Follows ISP - diff operations only.
/// </summary>
public interface IDiffService
{
    /// <summary>
    /// Computes diff between two strings.
    /// </summary>
    IReadOnlyList<DiffLine> ComputeDiff(string original, string modified);
    
    /// <summary>
    /// Generates unified diff format.
    /// </summary>
    string GenerateUnifiedDiff(string original, string modified, string filePath);
}

public record DiffLine(
    DiffLineType Type,
    int? OldLineNumber,
    int? NewLineNumber,
    string Content);

public enum DiffLineType
{
    Context,
    Added,
    Removed,
    Modified
}
```

---

## 5. Service Implementations

### 5.1 FileApplyService

**File:** `src/ShieldPrompt.Application/Services/FileApplyService.cs`

```csharp
namespace ShieldPrompt.Application.Services;

public class FileApplyService(
    IBackupService backupService,
    IDiffService diffService,
    IFileSystemService fileSystem) : IFileApplyService
{
    public async Task<ApplyPreview> PreviewAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var previews = new List<FileOperationPreview>();
        var warnings = new List<string>();
        int created = 0, updated = 0, deleted = 0;

        foreach (var op in operations)
        {
            var fullPath = Path.Combine(workspaceRoot, op.FilePath);
            var currentContent = File.Exists(fullPath) 
                ? await File.ReadAllTextAsync(fullPath, ct) 
                : null;
            
            var diff = op.Type switch
            {
                FileOperationType.Update when currentContent != null =>
                    diffService.ComputeDiff(currentContent, op.Content ?? ""),
                FileOperationType.Create => null,
                FileOperationType.Delete => null,
                _ => null
            };

            var hasConflict = await HasConflictAsync(op, fullPath, ct);
            if (hasConflict)
                warnings.Add($"Conflict detected: {op.FilePath}");

            previews.Add(new FileOperationPreview(
                op, currentContent, op.Content, diff, hasConflict));

            switch (op.Type)
            {
                case FileOperationType.Create: created++; break;
                case FileOperationType.Update: updated++; break;
                case FileOperationType.Delete: deleted++; break;
            }
        }

        return new ApplyPreview(
            previews, previews.Count, created, updated, deleted, warnings);
    }

    public async Task<ApplyResult> ApplyAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var opList = operations.ToList();
        
        // Create backup first
        var filesToBackup = opList
            .Where(op => op.Type != FileOperationType.Create)
            .Select(op => Path.Combine(workspaceRoot, op.FilePath))
            .Where(File.Exists);
        
        var backupId = await backupService.CreateBackupAsync(filesToBackup, ct);

        var results = new List<AppliedOperation>();
        var errors = new List<string>();

        foreach (var op in opList)
        {
            try
            {
                await ApplyOperationAsync(op, workspaceRoot, ct);
                results.Add(new AppliedOperation(op, true, null, null));
            }
            catch (Exception ex)
            {
                errors.Add($"{op.FilePath}: {ex.Message}");
                results.Add(new AppliedOperation(op, false, ex.Message, null));
            }
        }

        return new ApplyResult(
            SuccessCount: results.Count(r => r.Success),
            FailureCount: results.Count(r => !r.Success),
            Operations: results,
            Errors: errors,
            BackupId: backupId);
    }

    public async Task<ApplyResult> ApplySelectiveAsync(
        IEnumerable<FileOperation> operations,
        IEnumerable<string> selectedPaths,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var selectedSet = selectedPaths.ToHashSet();
        var filtered = operations.Where(op => selectedSet.Contains(op.FilePath));
        return await ApplyAsync(filtered, workspaceRoot, ct);
    }

    public async Task<bool> UndoAsync(string backupId, CancellationToken ct = default)
    {
        return await backupService.RestoreBackupAsync(backupId, ct);
    }

    public async Task<IReadOnlyList<ConflictInfo>> CheckConflictsAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var conflicts = new List<ConflictInfo>();

        foreach (var op in operations)
        {
            var fullPath = Path.Combine(workspaceRoot, op.FilePath);
            var exists = File.Exists(fullPath);

            if (op.Type == FileOperationType.Create && exists)
            {
                conflicts.Add(new ConflictInfo(
                    op.FilePath,
                    ConflictType.FileCreatedExists,
                    await File.ReadAllTextAsync(fullPath, ct),
                    op.Content,
                    null));
            }
            else if (op.Type == FileOperationType.Delete && !exists)
            {
                conflicts.Add(new ConflictInfo(
                    op.FilePath,
                    ConflictType.FileDeleted,
                    null, null, null));
            }
        }

        return conflicts;
    }

    private async Task ApplyOperationAsync(
        FileOperation op, 
        string workspaceRoot, 
        CancellationToken ct)
    {
        var fullPath = Path.Combine(workspaceRoot, op.FilePath);
        var directory = Path.GetDirectoryName(fullPath);

        switch (op.Type)
        {
            case FileOperationType.Create:
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                await File.WriteAllTextAsync(fullPath, op.Content ?? "", ct);
                break;

            case FileOperationType.Update:
                await File.WriteAllTextAsync(fullPath, op.Content ?? "", ct);
                break;

            case FileOperationType.PartialUpdate:
                await ApplyPartialUpdateAsync(fullPath, op, ct);
                break;

            case FileOperationType.Delete:
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                break;

            case FileOperationType.Rename:
                if (op.OriginalPath != null)
                {
                    var sourcePath = Path.Combine(workspaceRoot, op.OriginalPath);
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    File.Move(sourcePath, fullPath);
                }
                break;
        }
    }

    private async Task ApplyPartialUpdateAsync(
        string fullPath, 
        FileOperation op, 
        CancellationToken ct)
    {
        if (op.StartLine == null || op.EndLine == null || op.Content == null)
            throw new InvalidOperationException("Partial update requires StartLine, EndLine, and Content");

        var lines = (await File.ReadAllLinesAsync(fullPath, ct)).ToList();
        var newLines = op.Content.Split('\n');
        
        // Remove old lines and insert new
        var removeCount = op.EndLine.Value - op.StartLine.Value + 1;
        lines.RemoveRange(op.StartLine.Value - 1, removeCount);
        lines.InsertRange(op.StartLine.Value - 1, newLines);
        
        await File.WriteAllLinesAsync(fullPath, lines, ct);
    }

    private async Task<bool> HasConflictAsync(
        FileOperation op, 
        string fullPath, 
        CancellationToken ct)
    {
        var exists = File.Exists(fullPath);
        
        return op.Type switch
        {
            FileOperationType.Create => exists,
            FileOperationType.Delete => !exists,
            _ => false
        };
    }
}
```

---

## 6. ViewModel Specification

### 6.1 ApplyDashboardViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/ApplyDashboardViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for the apply changes dashboard.
/// This is the main UI for reviewing and applying LLM-generated changes.
/// </summary>
public partial class ApplyDashboardViewModel : ObservableObject
{
    private readonly IStructuredResponseParser _parser;
    private readonly IFileApplyService _applyService;
    private readonly IUndoRedoManager _undoManager;
    
    [ObservableProperty]
    private string _responseText = string.Empty;
    
    [ObservableProperty]
    private ParsedResponse? _parsedResponse;
    
    [ObservableProperty]
    private ApplyPreview? _preview;
    
    [ObservableProperty]
    private ObservableCollection<FileOperationViewModel> _operations = new();
    
    [ObservableProperty]
    private FileOperationViewModel? _selectedOperation;
    
    [ObservableProperty]
    private bool _isParsing;
    
    [ObservableProperty]
    private bool _isApplying;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _hasConflicts;
    
    [ObservableProperty]
    private string _workspaceRoot = string.Empty;
    
    // Statistics
    [ObservableProperty]
    private int _totalOperations;
    
    [ObservableProperty]
    private int _selectedOperations;
    
    [ObservableProperty]
    private int _createCount;
    
    [ObservableProperty]
    private int _updateCount;
    
    [ObservableProperty]
    private int _deleteCount;

    public ApplyDashboardViewModel(
        IStructuredResponseParser parser,
        IFileApplyService applyService,
        IUndoRedoManager undoManager)
    {
        _parser = parser;
        _applyService = applyService;
        _undoManager = undoManager;
    }

    [RelayCommand]
    private async Task ParseResponseAsync()
    {
        if (string.IsNullOrWhiteSpace(ResponseText)) return;

        IsParsing = true;
        StatusMessage = "Parsing response...";

        try
        {
            ParsedResponse = await _parser.ParseAsync(ResponseText);
            
            Operations.Clear();
            foreach (var op in ParsedResponse.Operations)
            {
                Operations.Add(new FileOperationViewModel(op) { IsSelected = true });
            }

            await GeneratePreviewAsync();
            UpdateStatistics();
            StatusMessage = $"Parsed {Operations.Count} operations";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Parse error: {ex.Message}";
        }
        finally
        {
            IsParsing = false;
        }
    }

    [RelayCommand]
    private async Task GeneratePreviewAsync()
    {
        if (ParsedResponse == null || string.IsNullOrEmpty(WorkspaceRoot)) return;

        Preview = await _applyService.PreviewAsync(
            ParsedResponse.Operations, 
            WorkspaceRoot);

        HasConflicts = Preview.Warnings.Any();
        
        // Update operation ViewModels with diff info
        foreach (var preview in Preview.Previews)
        {
            var opVm = Operations.FirstOrDefault(o => o.FilePath == preview.Operation.FilePath);
            if (opVm != null)
            {
                opVm.Diff = preview.Diff;
                opVm.HasConflict = preview.HasConflict;
                opVm.CurrentContent = preview.CurrentContent;
            }
        }
    }

    [RelayCommand]
    private async Task ApplyAllAsync()
    {
        if (ParsedResponse == null) return;

        IsApplying = true;
        StatusMessage = "Applying changes...";

        try
        {
            var selectedOps = Operations
                .Where(o => o.IsSelected)
                .Select(o => o.Operation);

            var result = await _applyService.ApplyAsync(selectedOps, WorkspaceRoot);
            
            // Register undo action
            _undoManager.RegisterAction(new ApplyChangesUndoAction(
                _applyService, 
                result.BackupId,
                result.SuccessCount));

            StatusMessage = $"Applied {result.SuccessCount} changes. {result.FailureCount} failed.";
            
            if (result.FailureCount > 0)
            {
                // Show errors
                foreach (var error in result.Errors)
                {
                    StatusMessage += $"\n{error}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Apply error: {ex.Message}";
        }
        finally
        {
            IsApplying = false;
        }
    }

    [RelayCommand]
    private async Task ApplySelectedAsync()
    {
        var selectedPaths = Operations
            .Where(o => o.IsSelected)
            .Select(o => o.FilePath);
        
        if (!selectedPaths.Any())
        {
            StatusMessage = "No operations selected";
            return;
        }

        IsApplying = true;
        try
        {
            var result = await _applyService.ApplySelectiveAsync(
                ParsedResponse!.Operations,
                selectedPaths,
                WorkspaceRoot);
            
            StatusMessage = $"Applied {result.SuccessCount} selected changes";
        }
        finally
        {
            IsApplying = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var op in Operations)
            op.IsSelected = true;
        UpdateStatistics();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var op in Operations)
            op.IsSelected = false;
        UpdateStatistics();
    }

    [RelayCommand]
    private void ToggleSelection(FileOperationViewModel op)
    {
        op.IsSelected = !op.IsSelected;
        UpdateStatistics();
    }

    private void UpdateStatistics()
    {
        TotalOperations = Operations.Count;
        SelectedOperations = Operations.Count(o => o.IsSelected);
        CreateCount = Operations.Count(o => o.Type == FileOperationType.Create);
        UpdateCount = Operations.Count(o => o.Type == FileOperationType.Update || 
                                            o.Type == FileOperationType.PartialUpdate);
        DeleteCount = Operations.Count(o => o.Type == FileOperationType.Delete);
    }
}
```

### 6.2 FileOperationViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/FileOperationViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for a single file operation in the apply dashboard.
/// </summary>
public partial class FileOperationViewModel : ObservableObject
{
    private readonly FileOperation _operation;
    
    [ObservableProperty]
    private bool _isSelected = true;
    
    [ObservableProperty]
    private bool _hasConflict;
    
    [ObservableProperty]
    private IReadOnlyList<DiffLine>? _diff;
    
    [ObservableProperty]
    private string? _currentContent;
    
    [ObservableProperty]
    private bool _isExpanded;

    public FileOperationViewModel(FileOperation operation)
    {
        _operation = operation;
    }

    public FileOperation Operation => _operation;
    public string FilePath => _operation.FilePath;
    public FileOperationType Type => _operation.Type;
    public string? Content => _operation.Content;
    public string? Reason => _operation.Reason;

    public string TypeIcon => Type switch
    {
        FileOperationType.Create => "➕",
        FileOperationType.Update => "📝",
        FileOperationType.PartialUpdate => "✏️",
        FileOperationType.Delete => "🗑️",
        FileOperationType.Rename => "📋",
        _ => "❓"
    };

    public string TypeText => Type switch
    {
        FileOperationType.Create => "CREATE",
        FileOperationType.Update => "UPDATE",
        FileOperationType.PartialUpdate => "PATCH",
        FileOperationType.Delete => "DELETE",
        FileOperationType.Rename => "RENAME",
        _ => "UNKNOWN"
    };

    public IBrush TypeColor => Type switch
    {
        FileOperationType.Create => Brushes.LightGreen,
        FileOperationType.Update => Brushes.Yellow,
        FileOperationType.PartialUpdate => Brushes.Orange,
        FileOperationType.Delete => Brushes.Red,
        FileOperationType.Rename => Brushes.LightBlue,
        _ => Brushes.Gray
    };

    public int AddedLines => Diff?.Count(d => d.Type == DiffLineType.Added) ?? 0;
    public int RemovedLines => Diff?.Count(d => d.Type == DiffLineType.Removed) ?? 0;
    
    public string DiffSummary => $"+{AddedLines} -{RemovedLines}";
}
```

---

## 7. View Specification

### 7.1 ApplyDashboard.axaml (NEW)

**File:** `src/ShieldPrompt.App/Views/V2/ApplyDashboard.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:ShieldPrompt.App.ViewModels.V2"
             x:DataType="vm:ApplyDashboardViewModel"
             x:Class="ShieldPrompt.App.Views.V2.ApplyDashboard">
    
    <Grid RowDefinitions="Auto,*,Auto">
        
        <!-- Header with Stats -->
        <Border Grid.Row="0" Background="#181825" Padding="16">
            <Grid ColumnDefinitions="*,Auto">
                <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="24">
                    <StackPanel>
                        <TextBlock Text="Total" Foreground="#6c7086" FontSize="11"/>
                        <TextBlock Text="{Binding TotalOperations}" FontSize="24" FontWeight="Bold"/>
                    </StackPanel>
                    <StackPanel>
                        <TextBlock Text="Create" Foreground="#a6e3a1" FontSize="11"/>
                        <TextBlock Text="{Binding CreateCount}" FontSize="24" Foreground="#a6e3a1"/>
                    </StackPanel>
                    <StackPanel>
                        <TextBlock Text="Update" Foreground="#f9e2af" FontSize="11"/>
                        <TextBlock Text="{Binding UpdateCount}" FontSize="24" Foreground="#f9e2af"/>
                    </StackPanel>
                    <StackPanel>
                        <TextBlock Text="Delete" Foreground="#f38ba8" FontSize="11"/>
                        <TextBlock Text="{Binding DeleteCount}" FontSize="24" Foreground="#f38ba8"/>
                    </StackPanel>
                </StackPanel>
                
                <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                    <Button Content="Select All" Command="{Binding SelectAllCommand}"/>
                    <Button Content="Deselect All" Command="{Binding DeselectAllCommand}"/>
                </StackPanel>
            </Grid>
        </Border>
        
        <!-- Operations List -->
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding Operations}"
                 SelectedItem="{Binding SelectedOperation}"
                 Background="#1e1e2e">
            <ListBox.ItemTemplate>
                <DataTemplate DataType="vm:FileOperationViewModel">
                    <Border Padding="12" Margin="4"
                            Background="#313244" CornerRadius="6"
                            BorderBrush="{Binding TypeColor}"
                            BorderThickness="2,0,0,0">
                        <Grid RowDefinitions="Auto,Auto,Auto">
                            
                            <!-- Header -->
                            <Grid Grid.Row="0" ColumnDefinitions="Auto,Auto,*,Auto">
                                <CheckBox Grid.Column="0"
                                          IsChecked="{Binding IsSelected}"
                                          Margin="0,0,12,0"/>
                                <Border Grid.Column="1"
                                        Background="{Binding TypeColor}"
                                        CornerRadius="4"
                                        Padding="8,4">
                                    <TextBlock Text="{Binding TypeText}"
                                               FontWeight="Bold"
                                               FontSize="10"/>
                                </Border>
                                <TextBlock Grid.Column="2"
                                           Text="{Binding FilePath}"
                                           FontWeight="SemiBold"
                                           Margin="12,0"/>
                                <TextBlock Grid.Column="3"
                                           Text="{Binding DiffSummary}"
                                           Foreground="#6c7086"/>
                            </Grid>
                            
                            <!-- Reason -->
                            <TextBlock Grid.Row="1"
                                       Text="{Binding Reason}"
                                       Foreground="#6c7086"
                                       FontSize="12"
                                       Margin="32,8,0,0"
                                       IsVisible="{Binding Reason, Converter={x:Static StringConverters.IsNotNullOrEmpty}}"/>
                            
                            <!-- Conflict Warning -->
                            <Border Grid.Row="2"
                                    Background="#f38ba820"
                                    CornerRadius="4"
                                    Padding="8"
                                    Margin="32,8,0,0"
                                    IsVisible="{Binding HasConflict}">
                                <TextBlock Text="⚠️ Conflict detected - file may have changed"
                                           Foreground="#f38ba8"/>
                            </Border>
                        </Grid>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        
        <!-- Actions -->
        <Border Grid.Row="2" Background="#181825" Padding="16">
            <Grid ColumnDefinitions="*,Auto">
                <TextBlock Grid.Column="0"
                           Text="{Binding StatusMessage}"
                           VerticalAlignment="Center"
                           Foreground="#a6e3a1"/>
                <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="12">
                    <Button Content="Apply Selected"
                            Command="{Binding ApplySelectedCommand}"
                            IsEnabled="{Binding !IsApplying}"/>
                    <Button Content="Apply All"
                            Command="{Binding ApplyAllCommand}"
                            Classes="primary"
                            IsEnabled="{Binding !IsApplying}"/>
                </StackPanel>
            </Grid>
        </Border>
    </Grid>
</UserControl>
```

---

## 8. Test Specifications (TDD)

### 8.1 FileApplyService Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Application/Services/FileApplyServiceTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Application.Services;

public class FileApplyServiceTests
{
    private readonly IBackupService _backupService;
    private readonly IDiffService _diffService;
    private readonly IFileSystemService _fileSystem;
    private readonly FileApplyService _sut;
    private readonly string _tempDir;

    public FileApplyServiceTests()
    {
        _backupService = Substitute.For<IBackupService>();
        _diffService = Substitute.For<IDiffService>();
        _fileSystem = Substitute.For<IFileSystemService>();
        _sut = new FileApplyService(_backupService, _diffService, _fileSystem);
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task PreviewAsync_WithUpdateOperation_ReturnsPreviewWithDiff()
    {
        // Arrange
        var existingFile = Path.Combine(_tempDir, "test.cs");
        await File.WriteAllTextAsync(existingFile, "original content");
        
        var operations = new[]
        {
            new FileOperation(FileOperationType.Update, "test.cs", "new content", "Refactor")
        };
        
        _diffService.ComputeDiff("original content", "new content")
            .Returns(new[] { new DiffLine(DiffLineType.Modified, 1, 1, "changed") });

        // Act
        var preview = await _sut.PreviewAsync(operations, _tempDir);

        // Assert
        preview.TotalFiles.Should().Be(1);
        preview.UpdatedCount.Should().Be(1);
        preview.Previews[0].Diff.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyAsync_WithCreateOperation_CreatesFile()
    {
        // Arrange
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");
        
        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "new.cs", "new file content", "Create new file")
        };

        // Act
        var result = await _sut.ApplyAsync(operations, _tempDir);

        // Assert
        result.SuccessCount.Should().Be(1);
        File.Exists(Path.Combine(_tempDir, "new.cs")).Should().BeTrue();
        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "new.cs"));
        content.Should().Be("new file content");
    }

    [Fact]
    public async Task ApplyAsync_WithDeleteOperation_DeletesFile()
    {
        // Arrange
        var fileToDelete = Path.Combine(_tempDir, "delete-me.cs");
        await File.WriteAllTextAsync(fileToDelete, "to be deleted");
        
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");
        
        var operations = new[]
        {
            new FileOperation(FileOperationType.Delete, "delete-me.cs", null, "Remove obsolete")
        };

        // Act
        var result = await _sut.ApplyAsync(operations, _tempDir);

        // Assert
        result.SuccessCount.Should().Be(1);
        File.Exists(fileToDelete).Should().BeFalse();
    }

    [Fact]
    public async Task ApplyAsync_CreatesBackupBeforeApplying()
    {
        // Arrange
        var existingFile = Path.Combine(_tempDir, "existing.cs");
        await File.WriteAllTextAsync(existingFile, "existing content");
        
        _backupService.CreateBackupAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns("backup-123");
        
        var operations = new[]
        {
            new FileOperation(FileOperationType.Update, "existing.cs", "updated", "Update")
        };

        // Act
        await _sut.ApplyAsync(operations, _tempDir);

        // Assert
        await _backupService.Received(1).CreateBackupAsync(
            Arg.Is<IEnumerable<string>>(paths => paths.Any(p => p.Contains("existing.cs"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UndoAsync_RestoresFromBackup()
    {
        // Arrange
        _backupService.RestoreBackupAsync("backup-123", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.UndoAsync("backup-123");

        // Assert
        result.Should().BeTrue();
        await _backupService.Received(1).RestoreBackupAsync("backup-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckConflictsAsync_WithExistingFile_DetectsCreateConflict()
    {
        // Arrange
        var existingFile = Path.Combine(_tempDir, "exists.cs");
        await File.WriteAllTextAsync(existingFile, "existing");
        
        var operations = new[]
        {
            new FileOperation(FileOperationType.Create, "exists.cs", "new content", "Create")
        };

        // Act
        var conflicts = await _sut.CheckConflictsAsync(operations, _tempDir);

        // Assert
        conflicts.Should().HaveCount(1);
        conflicts[0].Type.Should().Be(ConflictType.FileCreatedExists);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}
```

---

## 9. Implementation Checklist

### 9.1 Domain Layer

- [ ] Verify `FileOperation` record
- [ ] Create `ApplyResult` record
- [ ] Create `ConflictInfo` record
- [ ] Write unit tests

### 9.2 Application Layer

- [ ] Verify `IStructuredResponseParser` interface
- [ ] Create `IFileApplyService` interface
- [ ] Create `IBackupService` interface
- [ ] Create `IDiffService` interface
- [ ] Implement `FileApplyService`
- [ ] Implement `BackupService`
- [ ] Implement `DiffService`
- [ ] Write unit tests (TDD)

### 9.3 Presentation Layer

- [ ] Create `ApplyDashboardViewModel`
- [ ] Create `FileOperationViewModel`
- [ ] Create `ApplyDashboard.axaml`
- [ ] Create `DiffViewer.axaml` (for detailed diff view)
- [ ] Write ViewModel tests (TDD)

### 9.4 Integration

- [ ] Register services in DI
- [ ] Wire to LLM Response tab
- [ ] Implement keyboard shortcuts
- [ ] End-to-end testing

---

## 10. Acceptance Criteria

| Criterion | Verification |
|-----------|--------------|
| Can parse valid LLM response | Unit test |
| Preview shows all operations | Manual test |
| Apply creates/updates/deletes files | Integration test |
| Undo restores original files | Integration test |
| Conflicts are detected and shown | Manual test |
| Selective apply works | Manual test |
| All unit tests pass | `dotnet test` |

---

## 11. Security Considerations

| Risk | Mitigation |
|------|------------|
| Path traversal attacks | Validate all paths are within workspace |
| Arbitrary file deletion | Require explicit confirmation for DELETE |
| Sensitive file modification | Backup before any operation |
| Large file handling | Limit file size, warn user |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial specification |

