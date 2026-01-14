using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Presentation.ViewModels;
using ShieldPrompt.Sanitization.Interfaces;
using TextCopy;

namespace ShieldPrompt.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFileAggregationService _fileAggregationService;
    private readonly ITokenCountingService _tokenCountingService;
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly IMappingSession _session;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUndoRedoManager _undoRedoManager;

    [ObservableProperty]
    private FileNode? _rootNode;

    [ObservableProperty]
    private FileNodeViewModel? _rootNodeViewModel;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _showSanitizationPreview;

    [ObservableProperty]
    private string _sanitizationPreview = string.Empty;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private string? _undoDescription;

    [ObservableProperty]
    private string? _redoDescription;

    public ObservableCollection<SanitizationPreviewItem> SanitizationItems { get; } = new();

    [ObservableProperty]
    private int _totalTokens;

    [ObservableProperty]
    private int _selectedFileCount;

    [ObservableProperty]
    private int _sanitizedValueCount;

    [ObservableProperty]
    private ModelProfile _selectedModel = ModelProfiles.GPT4o;

    [ObservableProperty]
    private IPromptFormatter _selectedFormatter;

    public MainWindowViewModel(
        IFileAggregationService fileAggregationService,
        ITokenCountingService tokenCountingService,
        ISanitizationEngine sanitizationEngine,
        IDesanitizationEngine desanitizationEngine,
        IMappingSession session,
        ISettingsRepository settingsRepository,
        IUndoRedoManager undoRedoManager)
    {
        _fileAggregationService = fileAggregationService;
        _tokenCountingService = tokenCountingService;
        _sanitizationEngine = sanitizationEngine;
        _desanitizationEngine = desanitizationEngine;
        _session = session;
        _settingsRepository = settingsRepository;
        _undoRedoManager = undoRedoManager;

        // Subscribe to undo/redo state changes
        _undoRedoManager.StateChanged += OnUndoRedoStateChanged;

        // Initialize formatters
        AvailableFormatters = new ObservableCollection<IPromptFormatter>
        {
            new ShieldPrompt.Application.Formatters.PlainTextFormatter(),
            new ShieldPrompt.Application.Formatters.MarkdownFormatter(),
            new ShieldPrompt.Application.Formatters.XmlFormatter()
        };
        _selectedFormatter = AvailableFormatters[1]; // Default to Markdown

        // Load settings and restore previous state
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var settings = await _settingsRepository.LoadAsync();

            // Restore last model
            if (!string.IsNullOrEmpty(settings.LastModelName))
            {
                var model = AvailableModels.FirstOrDefault(m => m.Name == settings.LastModelName);
                if (model != null)
                    SelectedModel = model;
            }

            // Restore last format
            if (!string.IsNullOrEmpty(settings.LastFormatName))
            {
                var formatter = AvailableFormatters.FirstOrDefault(f => f.FormatName == settings.LastFormatName);
                if (formatter != null)
                    SelectedFormatter = formatter;
            }

            // Auto-open last folder
            if (!string.IsNullOrEmpty(settings.LastFolderPath) && Directory.Exists(settings.LastFolderPath))
            {
                await LoadFolderAsync(settings.LastFolderPath, settings.LastSelectedFiles);
            }
        }
        catch
        {
            // Silently ignore - app still usable with defaults
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var selectedPaths = RootNodeViewModel != null
                ? GetSelectedFilePaths(RootNodeViewModel).ToList()
                : new List<string>();

            var settings = new AppSettings
            {
                LastFolderPath = RootNode?.Path,
                LastFormatName = SelectedFormatter?.FormatName,
                LastModelName = SelectedModel?.Name,
                LastSelectedFiles = selectedPaths
            };

            await _settingsRepository.SaveAsync(settings);
        }
        catch
        {
            // Silently ignore save failures
        }
    }

    private static IEnumerable<string> GetSelectedFilePaths(FileNodeViewModel node)
    {
        if (!node.IsDirectory && node.IsSelected)
        {
            yield return node.Path;
        }

        foreach (var child in node.Children)
        {
            foreach (var path in GetSelectedFilePaths(child))
            {
                yield return path;
            }
        }
    }

    private static void RestoreFileSelection(FileNodeViewModel node, IReadOnlyList<string> selectedPaths)
    {
        if (!node.IsDirectory && selectedPaths.Contains(node.Path))
        {
            node.IsSelected = true;
        }

        foreach (var child in node.Children)
        {
            RestoreFileSelection(child, selectedPaths);
        }
    }

    private void UpdateSanitizationPreview(SanitizationResult result)
    {
        SanitizationItems.Clear();

        if (result.Matches.Count == 0)
        {
            SanitizationPreview = "✅ No sensitive data detected - safe to share as-is!";
            ShowSanitizationPreview = false;
            return;
        }

        // Group matches by category for better display
        var grouped = result.Matches
            .GroupBy(m => m.Category)
            .OrderByDescending(g => g.Count());

        foreach (var group in grouped)
        {
            foreach (var match in group.Take(5)) // Show max 5 per category
            {
                SanitizationItems.Add(new SanitizationPreviewItem(
                    GetCategoryIcon(match.Category),
                    match.Original,
                    match.Alias,
                    match.Category.ToString()
                ));
            }

            if (group.Count() > 5)
            {
                SanitizationItems.Add(new SanitizationPreviewItem(
                    "⋯",
                    $"... and {group.Count() - 5} more {group.Key} values",
                    "",
                    ""
                ));
            }
        }

        SanitizationPreview = $"🛡️ Protected {result.TotalMatches} sensitive values";
        ShowSanitizationPreview = true;
    }

    private static string GetCategoryIcon(PatternCategory category) => category switch
    {
        PatternCategory.Database => "🗄️",
        PatternCategory.Server => "🖥️",
        PatternCategory.IPAddress => "🌐",
        PatternCategory.Hostname => "🏠",
        PatternCategory.ConnectionString => "🔌",
        PatternCategory.FilePath => "📁",
        PatternCategory.SSN => "🆔",
        PatternCategory.CreditCard => "💳",
        PatternCategory.APIKey or PatternCategory.AWSKey or PatternCategory.GitHubToken or 
        PatternCategory.OpenAIKey or PatternCategory.AnthropicKey => "🔑",
        PatternCategory.PrivateKey => "🔐",
        PatternCategory.Password => "🔒",
        PatternCategory.BearerToken => "🎫",
        _ => "🛡️"
    };

    [RelayCommand]
    private void ToggleSanitizationPreview()
    {
        ShowSanitizationPreview = !ShowSanitizationPreview;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private async Task UndoAsync()
    {
        await _undoRedoManager.UndoAsync();
        StatusText = $"↶ Undone: {_undoRedoManager.RedoDescription ?? "action"}";
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private async Task RedoAsync()
    {
        await _undoRedoManager.RedoAsync();
        StatusText = $"↷ Redone: {_undoRedoManager.UndoDescription ?? "action"}";
    }

    private void OnUndoRedoStateChanged(object? sender, EventArgs e)
    {
        CanUndo = _undoRedoManager.CanUndo;
        CanRedo = _undoRedoManager.CanRedo;
        UndoDescription = _undoRedoManager.UndoDescription;
        RedoDescription = _undoRedoManager.RedoDescription;
        
        // Notify commands to update their CanExecute state
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<ModelProfile> AvailableModels { get; } = 
        new(ModelProfiles.All);

    public ObservableCollection<IPromptFormatter> AvailableFormatters { get; }

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        try
        {
            // Open folder picker dialog
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new Avalonia.Controls.OpenFolderDialog
                {
                    Title = "Select folder to analyze"
                };

                var selectedPath = await dialog.ShowAsync(desktop.MainWindow!);
                
                if (string.IsNullOrEmpty(selectedPath))
                    return; // User cancelled

                StatusText = "Loading directory...";
                
                await LoadFolderAsync(selectedPath);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Error: {ex.Message}";
        }
    }

    private async Task LoadFolderAsync(string folderPath, IReadOnlyList<string>? selectedFiles = null)
    {
        IsLoading = true;
        try
        {
            RootNode = await _fileAggregationService.LoadDirectoryAsync(folderPath);
            RootNodeViewModel = new FileNodeViewModel(RootNode);
            
            // Restore previous file selection if provided
            if (selectedFiles != null && selectedFiles.Count > 0)
            {
                RestoreFileSelection(RootNodeViewModel, selectedFiles);
            }
            
            // Calculate token counts for files (async in background)
            _ = Task.Run(async () => await UpdateTokenCountsAsync(RootNodeViewModel));
            
            StatusText = $"✅ Loaded {CountFiles(RootNodeViewModel)} files from {Path.GetFileName(folderPath)}";

            // Save settings
            await SaveSettingsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenPasteRestoreDialogAsync()
    {
        try
        {
            var dialog = new ShieldPrompt.App.Views.PasteRestoreDialog
            {
                DataContext = new PasteRestoreViewModel(_desanitizationEngine, _session)
            };

            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow!);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Error: {ex.Message}";
        }
    }

    private async Task UpdateTokenCountsAsync(FileNodeViewModel node)
    {
        if (!node.IsDirectory)
        {
            var count = await _tokenCountingService.CountFileTokensAsync(new FileNode(node.Path, node.Name, false));
            node.TokenCount = count.Count;
        }

        foreach (var child in node.Children)
        {
            await UpdateTokenCountsAsync(child);
        }
    }

    private static int CountFiles(FileNodeViewModel node)
    {
        var count = node.IsDirectory ? 0 : 1;
        foreach (var child in node.Children)
        {
            count += CountFiles(child);
        }
        return count;
    }

    [RelayCommand]
    private async Task CopyToClipboardAsync()
    {
        if (RootNode == null)
        {
            StatusText = "No files loaded";
            return;
        }

        try
        {
            StatusText = "Aggregating files...";
            
            var selectedFiles = RootNodeViewModel != null 
                ? GetSelectedFilesFromViewModel(RootNodeViewModel).ToList()
                : new List<FileNode>();
            SelectedFileCount = selectedFiles.Count;

            if (selectedFiles.Count == 0)
            {
                StatusText = "No files selected";
                return;
            }

            // Format using selected formatter
            var formatted = SelectedFormatter.Format(selectedFiles);
            
            // 🔐 SANITIZE - Replace sensitive data with aliases
            StatusText = "🔐 Sanitizing sensitive data...";
            var sanitizationResult = _sanitizationEngine.Sanitize(formatted, new SanitizationOptions());
            
            // Update preview with what was sanitized
            UpdateSanitizationPreview(sanitizationResult);
            
            // Count tokens on sanitized content
            TotalTokens = _tokenCountingService.CountTokens(sanitizationResult.SanitizedContent);
            SanitizedValueCount = sanitizationResult.TotalMatches;
            
            // Copy sanitized content to clipboard
            await ClipboardService.SetTextAsync(sanitizationResult.SanitizedContent);
            
            if (sanitizationResult.WasSanitized)
            {
                StatusText = $"✅ Copied {SelectedFileCount} files - 🔐 {SanitizedValueCount} values sanitized - {TotalTokens:N0} tokens";
            }
            else
            {
                StatusText = $"✅ Copied {SelectedFileCount} files - {TotalTokens:N0} tokens (no sensitive data found)";
            }

            // Save settings after successful copy
            await SaveSettingsAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Error: {ex.Message}";
        }
    }

    private static IEnumerable<FileNode> GetSelectedFiles(FileNode node)
    {
        if (!node.IsDirectory && node.IsSelected)
        {
            yield return node;
        }

        foreach (var child in node.Children)
        {
            foreach (var selectedChild in GetSelectedFiles(child))
            {
                yield return selectedChild;
            }
        }
    }

    private static IEnumerable<FileNode> GetSelectedFilesFromViewModel(FileNodeViewModel node)
    {
        if (!node.IsDirectory && node.IsSelected)
        {
            yield return new FileNode(node.Path, node.Name, false);
        }

        foreach (var child in node.Children)
        {
            foreach (var selectedChild in GetSelectedFilesFromViewModel(child))
            {
                yield return selectedChild;
            }
        }
    }
}

public record SanitizationPreviewItem(
    string Icon,
    string Original,
    string Alias,
    string Category);
