using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services.UndoRedo;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Sanitization.Interfaces;
using TextCopy;

namespace ShieldPrompt.App.ViewModels;

public partial class PasteRestoreViewModel : ViewModelBase
{
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly IMappingSession _session;
    private readonly IAIResponseParser _aiParser;
    private readonly IFileWriterService _fileWriter;
    private readonly IEnumerable<FileNode> _originalFiles;
    private readonly string _baseDirectory;
    private readonly IUndoRedoManager? _undoRedoManager;

    [ObservableProperty]
    private string _pastedContent = string.Empty;

    [ObservableProperty]
    private string _restoredContent = string.Empty;

    [ObservableProperty]
    private int _aliasCount;

    [ObservableProperty]
    private string _previewSummary = string.Empty;

    [ObservableProperty]
    private bool _canApplyToFiles;

    [ObservableProperty]
    private string _applyButtonText = "Apply to Files";

    [ObservableProperty]
    private bool _isApplying;

    public PasteRestoreViewModel(
        IDesanitizationEngine desanitizationEngine,
        IMappingSession session,
        IAIResponseParser aiParser,
        IFileWriterService fileWriter,
        IEnumerable<FileNode> originalFiles,
        string baseDirectory,
        IUndoRedoManager? undoRedoManager = null)
    {
        _desanitizationEngine = desanitizationEngine;
        _session = session;
        _aiParser = aiParser;
        _fileWriter = fileWriter;
        _originalFiles = originalFiles;
        _baseDirectory = baseDirectory;
        _undoRedoManager = undoRedoManager;
    }

    public ObservableCollection<AliasMapping> DetectedAliases { get; } = new();
    public ObservableCollection<string> ChangeSummary { get; } = new();
    public ObservableCollection<FileUpdatePreview> FileUpdates { get; } = new();

    [RelayCommand]
    private async Task PasteFromClipboardAsync()
    {
        var clipboardText = await ClipboardService.GetTextAsync();
        if (!string.IsNullOrEmpty(clipboardText))
        {
            PastedContent = clipboardText;
            RestoreAliases();
        }
    }

    [RelayCommand]
    private void RestoreAliases()
    {
        if (string.IsNullOrEmpty(PastedContent))
            return;

        // Restore using session mappings
        RestoredContent = _desanitizationEngine.Desanitize(PastedContent, _session);

        // Detect which aliases were found
        DetectedAliases.Clear();
        ChangeSummary.Clear();

        foreach (var (alias, original) in _session.GetAllMappings())
        {
            if (PastedContent.Contains(alias))
            {
                var count = CountOccurrences(PastedContent, alias);
                DetectedAliases.Add(new AliasMapping(alias, original, count));
                ChangeSummary.Add($"Will replace {count}x: {alias} → {original}");
            }
        }

        AliasCount = DetectedAliases.Count;
        PreviewSummary = AliasCount > 0
            ? $"🔓 Ready to restore {AliasCount} sensitive values from AI response"
            : "ℹ️ No aliases detected in pasted content";
    }

    private static int CountOccurrences(string content, string value)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(value))
            return 0;

        var count = 0;
        var index = 0;
        while ((index = content.IndexOf(value, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    [RelayCommand]
    private async Task CopyRestoredAsync()
    {
        await ClipboardService.SetTextAsync(RestoredContent);
    }

    [RelayCommand(CanExecute = nameof(CanApplyToFiles))]
    private async Task ApplyToFilesAsync()
    {
        if (string.IsNullOrEmpty(RestoredContent))
            return;

        try
        {
            IsApplying = true;
            PreviewSummary = "Parsing AI response...";

            // Parse AI response to extract file operations
            var parsed = _aiParser.Parse(RestoredContent, _originalFiles);

            if (parsed.Updates.Count == 0)
            {
                PreviewSummary = "⚠️ No file updates detected in AI response";
                return;
            }

            PreviewSummary = $"Creating backup of {parsed.Updates.Count} file(s)...";

            // Apply file updates
            var result = await _fileWriter.ApplyUpdatesAsync(
                parsed.Updates,
                _baseDirectory,
                new FileWriteOptions
                {
                    CreateBackup = true,
                    AllowCreateDirectories = true,
                    AllowDelete = false
                });

            // Update UI with results
            if (result.Errors.Count > 0)
            {
                PreviewSummary = $"⚠️ Partial success: {string.Join(", ", result.Errors)}";
            }
            else
            {
                var summary = new List<string>();
                if (result.FilesUpdated > 0) summary.Add($"{result.FilesUpdated} updated");
                if (result.FilesCreated > 0) summary.Add($"{result.FilesCreated} created");
                if (result.FilesDeleted > 0) summary.Add($"{result.FilesDeleted} deleted");

                PreviewSummary = $"✅ Files {string.Join(", ", summary)}! (Backup: {result.BackupId})";
                
                // Add to undo stack with confirmation requirement
                if (_undoRedoManager != null && !string.IsNullOrEmpty(result.BackupId))
                {
                    var undoAction = new FileUpdateAction(result.BackupId, result, _fileWriter);
                    await _undoRedoManager.ExecuteAsync(undoAction);
                }
            }

            // Update file preview list
            FileUpdates.Clear();
            foreach (var update in parsed.Updates)
            {
                FileUpdates.Add(new FileUpdatePreview(
                    update.FilePath,
                    Path.GetFileName(update.FilePath),
                    update.Type.ToString(),
                    update.EstimatedLinesChanged));
            }
        }
        catch (Exception ex)
        {
            PreviewSummary = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsApplying = false;
        }
    }

    partial void OnPastedContentChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            RestoreAliases();
            UpdateFilePreview();
        }
    }

    private void UpdateFilePreview()
    {
        if (string.IsNullOrEmpty(RestoredContent))
        {
            CanApplyToFiles = false;
            FileUpdates.Clear();
            return;
        }

        // Parse to see what files would be updated
        var parsed = _aiParser.Parse(RestoredContent, _originalFiles);
        
        FileUpdates.Clear();
        foreach (var update in parsed.Updates)
        {
            FileUpdates.Add(new FileUpdatePreview(
                update.FilePath,
                Path.GetFileName(update.FilePath),
                update.Type.ToString(),
                update.EstimatedLinesChanged));
        }

        CanApplyToFiles = FileUpdates.Count > 0;
        ApplyButtonText = FileUpdates.Count > 0 
            ? $"Apply to {FileUpdates.Count} File(s)" 
            : "Apply to Files";
    }
}

public record AliasMapping(string Alias, string Original, int Occurrences = 1);

public record FileUpdatePreview(
    string FilePath,
    string FileName,
    string Type,
    int LinesChanged);

