using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    private readonly IAIResponseParser _aiParser;
    private readonly IFileWriterService _fileWriter;
    private readonly IPromptTemplateRepository _templateRepository;
    private readonly IPromptComposer _promptComposer;
    private readonly ILayoutStateRepository _layoutRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICustomRoleRepository _customRoleRepository;
    private readonly IFormatMetadataRepository _formatMetadataRepository;

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

    [ObservableProperty]
    private bool _showToolbar = true;

    [ObservableProperty]
    private bool _showStatusBar = true;

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

    [ObservableProperty]
    private FormatMetadata? _selectedFormatMetadata;

    [ObservableProperty]
    private PromptTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _customInstructions = string.Empty;

    // AI Role selection
    public ObservableCollection<Role> AvailableRoles { get; } = new();
    
    [ObservableProperty]
    private Role? _selectedRole;

    // Focus Areas for selected template
    public ObservableCollection<FocusAreaItem> AvailableFocusAreas { get; } = new();

    [ObservableProperty]
    private bool _hasFocusAreas;

    [ObservableProperty]
    private string _livePreview = "Select files and a template to see the preview...";

    [ObservableProperty]
    private int _previewTokenCount;

    [ObservableProperty]
    private bool _showTokenWarning;

    // Layout State Properties
    [ObservableProperty]
    private double _fileTreeWidth = LayoutDefaults.FileTreeWidth;

    [ObservableProperty]
    private double _promptBuilderHeightRatio = LayoutDefaults.PromptBuilderHeight;

    [ObservableProperty]
    private bool _isFileTreeCollapsed;

    [ObservableProperty]
    private bool _isPromptBuilderCollapsed;

    [ObservableProperty]
    private bool _isPreviewCollapsed;
    
    [ObservableProperty]
    private bool _isCopyFlashActive;
    
    public RoleEditorViewModel RoleEditorViewModel { get; }
    public OutputFormatSettingsViewModel OutputFormatSettingsViewModel { get; }
    public LlmResponseViewModel LlmResponseViewModel { get; }
    
    public MainWindowViewModel(
        IFileAggregationService fileAggregationService,
        ITokenCountingService tokenCountingService,
        ISanitizationEngine sanitizationEngine,
        IDesanitizationEngine desanitizationEngine,
        IMappingSession session,
        ISettingsRepository settingsRepository,
        IUndoRedoManager undoRedoManager,
        IAIResponseParser aiParser,
        IFileWriterService fileWriter,
        IPromptTemplateRepository templateRepository,
        IPromptComposer promptComposer,
        ILayoutStateRepository layoutRepository,
        IRoleRepository roleRepository,
        ICustomRoleRepository customRoleRepository,
        IFormatMetadataRepository formatMetadataRepository,
        RoleEditorViewModel roleEditorViewModel,
        OutputFormatSettingsViewModel outputFormatSettingsViewModel,
        LlmResponseViewModel llmResponseViewModel)
    {
        _fileAggregationService = fileAggregationService;
        _tokenCountingService = tokenCountingService;
        _sanitizationEngine = sanitizationEngine;
        _desanitizationEngine = desanitizationEngine;
        _session = session;
        _settingsRepository = settingsRepository;
        _undoRedoManager = undoRedoManager;
        _aiParser = aiParser;
        _fileWriter = fileWriter;
        _templateRepository = templateRepository;
        _promptComposer = promptComposer;
        _layoutRepository = layoutRepository;
        _roleRepository = roleRepository;
        _customRoleRepository = customRoleRepository;
        _formatMetadataRepository = formatMetadataRepository;
        RoleEditorViewModel = roleEditorViewModel;
        OutputFormatSettingsViewModel = outputFormatSettingsViewModel;
        LlmResponseViewModel = llmResponseViewModel;
        
        // Subscribe to undo/redo state changes
        _undoRedoManager.StateChanged += OnUndoRedoStateChanged;
        
        // Subscribe to property changes for live counter updates
        PropertyChanged += OnViewModelPropertyChanged;

        // Subscribe to layout property changes for persistence
        PropertyChanged += OnLayoutPropertyChanged;

        // Initialize formatters
        AvailableFormatters = new ObservableCollection<IPromptFormatter>
        {
            new ShieldPrompt.Application.Formatters.PlainTextFormatter(),
            new ShieldPrompt.Application.Formatters.MarkdownFormatter(),
            new ShieldPrompt.Application.Formatters.XmlFormatter()
        };
        _selectedFormatter = AvailableFormatters[1]; // Default to Markdown

        // Initialize templates
        foreach (var template in _templateRepository.GetAllTemplates())
        {
            AvailableTemplates.Add(template);
        }
        // Default to Code Review template
        _selectedTemplate = AvailableTemplates.FirstOrDefault(t => t.Id == "code_review") 
                           ?? AvailableTemplates.FirstOrDefault();

        // Initialize roles (built-in + custom)
        foreach (var role in _roleRepository.GetAllRoles())
        {
            AvailableRoles.Add(role);
        }
        foreach (var role in _customRoleRepository.GetCustomRoles())
        {
            AvailableRoles.Add(role);
        }
        // Default to Software Engineer role
        _selectedRole = _roleRepository.GetDefault();

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

            // Load saved layout state
            var savedState = await _layoutRepository.LoadLayoutStateAsync();
            if (savedState != null)
            {
                FileTreeWidth = savedState.FileTreeWidth;
                PromptBuilderHeightRatio = savedState.PromptBuilderHeight;
                IsFileTreeCollapsed = savedState.IsFileTreeCollapsed;
                IsPromptBuilderCollapsed = savedState.IsPromptBuilderCollapsed;
                IsPreviewCollapsed = savedState.IsPreviewCollapsed;
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
        // Check if action requires confirmation
        var action = _undoRedoManager.PeekUndo();
        if (action?.RequiresConfirmation == true)
        {
            // Show confirmation dialog (simplified for now - will enhance with proper dialog)
            var message = action.ConfirmationMessage ?? "Are you sure you want to undo this action?";
            StatusText = $"⚠️ Confirmation needed: {action.Description}";
            
            // For now, we'll undo with a warning message
            // TODO: Add proper confirmation dialog in future version
            StatusText = "⚠️ Undoing AI file changes...";
        }
        
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
    
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RootNodeViewModel) && RootNodeViewModel != null)
        {
            // Subscribe to all file node selection changes for live counter updates
            SubscribeToFileSelectionChanges(RootNodeViewModel);
            UpdateSelectedFileCount();
            UpdateEstimatedTokenCount();
            UpdateLivePreview(); // Update preview when files change
        }
        
        // Update preview when template or custom instructions change
        if (e.PropertyName == nameof(SelectedTemplate) || 
            e.PropertyName == nameof(CustomInstructions))
        {
            UpdateLivePreview();
        }
    }
    
    private void SubscribeToFileSelectionChanges(FileNodeViewModel node)
    {
        node.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FileNodeViewModel.IsSelected))
            {
                UpdateSelectedFileCount();
                UpdateEstimatedTokenCount();
                UpdateLivePreview(); // Update preview when selection changes
            }
        };
        
        foreach (var child in node.Children)
        {
            SubscribeToFileSelectionChanges(child);
        }
    }
    
    private void UpdateSelectedFileCount()
    {
        if (RootNodeViewModel == null)
        {
            SelectedFileCount = 0;
            return;
        }
        
        SelectedFileCount = CountSelectedFiles(RootNodeViewModel);
    }
    
    private int CountSelectedFiles(FileNodeViewModel node)
    {
        var count = (!node.IsDirectory && node.IsSelected) ? 1 : 0;
        foreach (var child in node.Children)
        {
            count += CountSelectedFiles(child);
        }
        return count;
    }
    
    private void UpdateEstimatedTokenCount()
    {
        if (RootNodeViewModel == null)
        {
            TotalTokens = 0;
            return;
        }
        
        // Sum per-file token counts for selected files (live estimate)
        TotalTokens = SumSelectedTokens(RootNodeViewModel);
    }
    
    private int SumSelectedTokens(FileNodeViewModel node)
    {
        var sum = (!node.IsDirectory && node.IsSelected) ? node.TokenCount : 0;
        foreach (var child in node.Children)
        {
            sum += SumSelectedTokens(child);
        }
        return sum;
    }

    private void UpdateLivePreview()
    {
        try
        {
            // Don't update if no template selected
            if (SelectedTemplate == null)
            {
                LivePreview = "Select a template from the toolbar to see the preview...";
                PreviewTokenCount = 0;
                ShowTokenWarning = false;
                return;
            }

            // Get selected files
            var selectedFiles = RootNodeViewModel != null 
                ? GetSelectedFilesFromViewModel(RootNodeViewModel).ToList()
                : new List<FileNode>();

            if (!selectedFiles.Any())
            {
                LivePreview = $"{SelectedTemplate.Icon} {SelectedTemplate.Name} ready. Select files to see preview...";
                PreviewTokenCount = 0;
                ShowTokenWarning = false;
                return;
            }

            // Load file content if not already loaded (lazy load for preview)
            foreach (var file in selectedFiles)
            {
                if (string.IsNullOrEmpty(file.Content) && File.Exists(file.Path))
                {
                    try
                    {
                        file.Content = File.ReadAllText(file.Path);
                    }
                    catch
                    {
                        file.Content = "[Error loading file]";
                    }
                }
            }

            // Compose the prompt
            var selectedFocusAreas = AvailableFocusAreas
                .Where(fa => fa.IsSelected)
                .Select(fa => fa.Name)
                .ToList();
            
            var options = new PromptOptions(
                CustomInstructions: string.IsNullOrWhiteSpace(CustomInstructions) ? null : CustomInstructions,
                SelectedFocusAreas: selectedFocusAreas.Count > 0 ? selectedFocusAreas : null,
                SelectedRole: SelectedRole,
                IncludeFilePaths: true,
                IncludeLineNumbers: false);

            var composed = _promptComposer.Compose(SelectedTemplate, selectedFiles, options);

            // Update preview
            LivePreview = composed.FullPrompt;
            PreviewTokenCount = composed.EstimatedTokens;

            // Show warning if over 80% of model limit
            var limit = SelectedModel?.ContextLimit ?? 128000;
            ShowTokenWarning = PreviewTokenCount > (limit * 0.8);
        }
        catch (Exception ex)
        {
            LivePreview = $"Error generating preview: {ex.Message}";
            PreviewTokenCount = 0;
            ShowTokenWarning = false;
        }
    }

    public ObservableCollection<ModelProfile> AvailableModels { get; } = 
        new(ModelProfiles.All);

    public ObservableCollection<IPromptFormatter> AvailableFormatters { get; }

    public ObservableCollection<PromptTemplate> AvailableTemplates { get; } = new();

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
            
            // Auto-expand the root node so files are visible
            RootNodeViewModel.IsExpanded = true;
            
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
            // Get selected files for context
            var selectedFiles = RootNodeViewModel != null 
                ? GetSelectedFilesFromViewModel(RootNodeViewModel).ToList()
                : new List<FileNode>();
            
            var baseDir = RootNode?.Path ?? Environment.CurrentDirectory;
            
            var dialog = new ShieldPrompt.App.Views.PasteRestoreDialog
            {
                DataContext = new PasteRestoreViewModel(
                    _desanitizationEngine, 
                    _session,
                    _aiParser,
                    _fileWriter,
                    selectedFiles,
                    baseDir,
                    _undoRedoManager)
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

        if (SelectedTemplate == null)
        {
            StatusText = "⚠️ Please select a template first";
            return;
        }

        try
        {
            StatusText = "Composing prompt...";
            
            var selectedFiles = RootNodeViewModel != null 
                ? GetSelectedFilesFromViewModel(RootNodeViewModel).ToList()
                : new List<FileNode>();

            if (selectedFiles.Count == 0)
            {
                StatusText = "No files selected";
                return;
            }

            // Compose the prompt using the selected template
            var selectedFocusAreas = AvailableFocusAreas
                .Where(fa => fa.IsSelected)
                .Select(fa => fa.Name)
                .ToList();
            
            var options = new PromptOptions(
                CustomInstructions: string.IsNullOrWhiteSpace(CustomInstructions) ? null : CustomInstructions,
                SelectedFocusAreas: selectedFocusAreas.Count > 0 ? selectedFocusAreas : null,
                SelectedRole: SelectedRole,
                IncludeFilePaths: true,
                IncludeLineNumbers: false);

            var composed = _promptComposer.Compose(SelectedTemplate, selectedFiles, options);
            
            // 🔐 SANITIZE - Replace sensitive data with aliases
            StatusText = "🔐 Sanitizing sensitive data...";
            var sanitizationResult = _sanitizationEngine.Sanitize(composed.FullPrompt, new SanitizationOptions());
            
            // Copy sanitized content to clipboard
            await ClipboardService.SetTextAsync(sanitizationResult.SanitizedContent);
            
            // Update counts
            TotalTokens = composed.EstimatedTokens;
            SanitizedValueCount = sanitizationResult.TotalMatches;
            
            if (sanitizationResult.WasSanitized)
            {
                StatusText = $"✅ Copied with {SelectedTemplate.Icon} {SelectedTemplate.Name} - 🔐 {SanitizedValueCount} values sanitized - {TotalTokens:N0} tokens";
            }
            else
            {
                StatusText = $"✅ Copied with {SelectedTemplate.Icon} {SelectedTemplate.Name} - {TotalTokens:N0} tokens (no sensitive data)";
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

    // View Control Commands
    [RelayCommand]
    private void ToggleShieldPreview()
    {
        ShowSanitizationPreview = !ShowSanitizationPreview;
    }

    [RelayCommand]
    private void ToggleStatusBar()
    {
        ShowStatusBar = !ShowStatusBar;
    }

    [RelayCommand]
    private async Task CopyLivePreview()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(LivePreview))
            {
                StatusText = "⚠️ No content to copy";
                return;
            }

            // Copy to clipboard
            await TextCopy.ClipboardService.SetTextAsync(LivePreview);
            
            // Show flash animation
            IsCopyFlashActive = true;
            
            // Update status message
            var tokenInfo = PreviewTokenCount > 0 ? $" ({PreviewTokenCount:N0} tokens)" : "";
            StatusText = $"📋 Copied to clipboard{tokenInfo}";
            
            // Auto-dismiss flash after 2 seconds
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                IsCopyFlashActive = false;
            });
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Copy failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleToolbar()
    {
        ShowToolbar = !ShowToolbar;
    }

    // File Menu Commands
    [RelayCommand]
    private void Exit()
    {
        // Dispose session before exit
        _session?.Dispose();
        Environment.Exit(0);
    }

    [RelayCommand]
    private void SelectAll()
    {
        if (RootNodeViewModel != null)
        {
            SelectAllNodes(RootNodeViewModel, true);
            StatusText = "All files selected";
        }
    }

    [RelayCommand]
    private void DeselectAll()
    {
        if (RootNodeViewModel != null)
        {
            SelectAllNodes(RootNodeViewModel, false);
            StatusText = "All files deselected";
        }
    }

    private void SelectAllNodes(FileNodeViewModel node, bool selected)
    {
        if (!node.IsDirectory)
        {
            node.IsSelected = selected;
        }
        foreach (var child in node.Children)
        {
            SelectAllNodes(child, selected);
        }
    }

    // Tools Menu Commands
    [RelayCommand]
    private async Task LoadTutorialProjectAsync()
    {
        // Try to find tutorial project in samples/
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var tutorialPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "samples", "tutorial-project");
        tutorialPath = Path.GetFullPath(tutorialPath);

        if (Directory.Exists(tutorialPath))
        {
            await LoadFolderAsync(tutorialPath);
            StatusText = "📚 Tutorial project loaded! Follow samples/tutorial-project/README_TUTORIAL.md";
        }
        else
        {
            StatusText = "⚠️ Tutorial not found. Clone from GitHub to get samples/tutorial-project/";
        }
    }

    [RelayCommand]
    private void ClearSession()
    {
        _session.Clear();
        SanitizedValueCount = 0;
        ShowSanitizationPreview = false;
        StatusText = "🧹 Session cleared - all mappings removed";
    }

    [RelayCommand]
    private async Task ShowDiagnosticsAsync()
    {
        var info = $@"ShieldPrompt Diagnostics
===================================

Version: 1.0.3
.NET: {Environment.Version}
OS: {Environment.OSVersion}
Architecture: {RuntimeInformation.ProcessArchitecture}
Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB

Current Session:
- Mappings: {_session.GetAllMappings().Count()}
- Created: {_session.CreatedAt:yyyy-MM-dd HH:mm:ss}
- Expires: {_session.ExpiresAt:yyyy-MM-dd HH:mm:ss}

Files Loaded: {RootNode?.Children.Count ?? 0} items
Files Selected: {SelectedFileCount}
Total Tokens: {TotalTokens:N0}
Sanitized Values: {SanitizedValueCount}

Undo/Redo:
- Can Undo: {CanUndo}
- Can Redo: {CanRedo}
- Undo: {UndoDescription ?? "N/A"}
- Redo: {RedoDescription ?? "N/A"}
";

        await ClipboardService.SetTextAsync(info);
        StatusText = "📋 Diagnostics copied to clipboard";
    }

    [RelayCommand]
    private async Task ViewLogsAsync()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShieldPrompt", "logs");

        if (Directory.Exists(logPath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = logPath,
                UseShellExecute = true
            });
        }
        else
        {
            StatusText = "ℹ️ No logs directory found (logging not enabled)";
        }
    }

    // Help Menu Commands
    [RelayCommand]
    private void OpenUserGuide()
    {
        OpenUrl("https://github.com/YOLOVibeCode/shield-prompt/blob/main/docs/USER_GUIDE.md");
    }

    [RelayCommand]
    private void OpenTutorial()
    {
        OpenUrl("https://github.com/YOLOVibeCode/shield-prompt/blob/main/samples/tutorial-project/README_TUTORIAL.md");
    }

    [RelayCommand]
    private void ShowKeyboardShortcuts()
    {
        var shortcuts = @"Keyboard Shortcuts
==================

File:
Ctrl+O     Open Folder
F5         Refresh  
Ctrl+C     Copy to Clipboard
Ctrl+V     Paste & Restore

Edit:
Ctrl+Z     Undo
Ctrl+Y     Redo
Ctrl+A     Select All Files
Ctrl+D     Deselect All Files

View:
Ctrl+Shift+S   Toggle Shield Preview
Ctrl+Plus      Increase Font Size
Ctrl+Minus     Decrease Font Size
Ctrl+0         Reset Font Size

Tools:
Ctrl+T     Load Tutorial Project

Help:
F1         User Guide
";
        ClipboardService.SetText(shortcuts);
        StatusText = "⌨️ Shortcuts copied to clipboard";
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        OpenUrl("https://github.com/YOLOVibeCode/shield-prompt");
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        StatusText = "Checking for updates...";
        // Future: implement update check
        await Task.Delay(500);
        StatusText = "ℹ️ You're running v1.1.2 - Check GitHub releases for latest version";
    }

    [RelayCommand]
    private void ShowAboutDialog()
    {
        var about = @"ShieldPrompt v1.1.2
====================

Secure AI Prompt Generation
Enterprise-Grade Data Protection

© 2026 YOLOVibeCode
MIT License (Free for commercial use)

Technology:
- .NET 10.0
- Avalonia UI 11.3
- TiktokenSharp 1.2.0

Tests: 186/186 passing ✅

GitHub: https://github.com/YOLOVibeCode/shield-prompt
";
        ClipboardService.SetText(about);
        StatusText = "ℹ️ About info copied to clipboard";
    }

    // Font Size Commands
    [RelayCommand]
    private void IncreaseFontSize()
    {
        // Future: implement font size control
        StatusText = "Font size increased (feature coming soon)";
    }

    [RelayCommand]
    private void DecreaseFontSize()
    {
        // Future: implement font size control
        StatusText = "Font size decreased (feature coming soon)";
    }

    [RelayCommand]
    private void ResetFontSize()
    {
        // Future: implement font size control
        StatusText = "Font size reset to default (feature coming soon)";
    }

    private void OpenUrl(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            ClipboardService.SetText(url);
            StatusText = $"📋 URL copied to clipboard: {url}";
        }
    }

    // Layout State Management

    private System.Timers.Timer? _layoutSaveTimer;

    private void OnLayoutPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FileTreeWidth) or nameof(PromptBuilderHeightRatio) 
            or nameof(IsFileTreeCollapsed) or nameof(IsPromptBuilderCollapsed) or nameof(IsPreviewCollapsed))
        {
            // Debounce save to avoid excessive file I/O
            _layoutSaveTimer?.Stop();
            _layoutSaveTimer?.Dispose();
            _layoutSaveTimer = new System.Timers.Timer(500); // 500ms debounce
            _layoutSaveTimer.Elapsed += async (s, args) =>
            {
                await SaveLayoutStateAsync();
                _layoutSaveTimer?.Dispose();
            };
            _layoutSaveTimer.AutoReset = false;
            _layoutSaveTimer.Start();
        }
    }

    private async Task SaveLayoutStateAsync()
    {
        var state = new LayoutState(
            FileTreeWidth,
            PromptBuilderHeightRatio,
            IsFileTreeCollapsed,
            IsPromptBuilderCollapsed,
            IsPreviewCollapsed);

        await _layoutRepository.SaveLayoutStateAsync(state);
    }

    partial void OnFileTreeWidthChanged(double value)
    {
        // Enforce minimum width
        if (value < 200)
        {
            FileTreeWidth = 200;
        }
    }

    partial void OnPromptBuilderHeightRatioChanged(double value)
    {
        // Enforce bounds (20% to 80%)
        if (value < 0.2)
        {
            PromptBuilderHeightRatio = 0.2;
        }
        else if (value > 0.8)
        {
            PromptBuilderHeightRatio = 0.8;
        }
    }

    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        // Update focus areas when template changes
        AvailableFocusAreas.Clear();
        
        if (value?.FocusOptions != null && value.FocusOptions.Count > 0)
        {
            foreach (var focusArea in value.FocusOptions)
            {
                var item = new FocusAreaItem { Name = focusArea, IsSelected = false };
                item.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(FocusAreaItem.IsSelected))
                    {
                        UpdateLivePreview();
                    }
                };
                AvailableFocusAreas.Add(item);
            }
            HasFocusAreas = true;
        }
        else
        {
            HasFocusAreas = false;
        }
        
        UpdateLivePreview();
    }

    partial void OnSelectedRoleChanged(Role? value)
    {
        // Regenerate live preview when role changes
        UpdateLivePreview();
    }

    partial void OnSelectedFormatterChanged(IPromptFormatter value)
    {
        // Update format metadata when formatter changes
        if (value != null)
        {
            var formatterId = value.GetType().Name.Replace("Formatter", "").ToLowerInvariant();
            SelectedFormatMetadata = _formatMetadataRepository.GetById(formatterId);
        }
        else
        {
            SelectedFormatMetadata = null;
        }
    }

    [RelayCommand]
    private async Task ResetLayout()
    {
        FileTreeWidth = LayoutDefaults.FileTreeWidth;
        PromptBuilderHeightRatio = LayoutDefaults.PromptBuilderHeight;
        IsFileTreeCollapsed = LayoutDefaults.IsFileTreeCollapsed;
        IsPromptBuilderCollapsed = LayoutDefaults.IsPromptBuilderCollapsed;
        IsPreviewCollapsed = LayoutDefaults.IsPreviewCollapsed;

        await _layoutRepository.ResetToDefaultAsync();
        StatusText = "✅ Layout reset to default";
    }
}

/// <summary>
/// View model for a selectable focus area checkbox.
/// </summary>
public partial class FocusAreaItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}

public record SanitizationPreviewItem(
    string Icon,
    string Original,
    string Alias,
    string Category);
