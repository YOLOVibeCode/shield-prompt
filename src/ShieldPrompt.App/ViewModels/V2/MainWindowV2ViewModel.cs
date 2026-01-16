using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using ModelProfile = ShieldPrompt.Domain.Entities.ModelProfile;
using ModelProfiles = ShieldPrompt.Domain.Entities.ModelProfiles;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Sanitization.Interfaces;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// MainWindow ViewModel for ShieldPrompt v2.0.
/// Completely new UI while reusing existing business logic via DI.
/// </summary>
public partial class MainWindowV2ViewModel : ObservableObject
{
    // === REUSED: Existing Services (from Sanitization and Application layers) ===
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly ITokenCountingService _tokenService;
    private readonly IPromptComposer _promptComposer;
    private readonly IFileAggregationService _fileService;
    private readonly IClipboardService _clipboardService;
    private readonly IRoleRepository _roleRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IPromptTemplateRepository _templateRepository;
    private readonly IMappingSession _mappingSession;
    private readonly IGitRepositoryService? _gitService;
    private readonly ILayoutStateRepository? _layoutRepository;

    // === State ===
    private SanitizationResult? _lastSanitizationResult;
    private readonly DateTime _sessionStartTime = DateTime.Now;
    private bool _isLoadingLayout;

    /// <summary>
    /// StatusBar ViewModel for the status bar control.
    /// </summary>
    public StatusBarViewModel StatusBar { get; }

    /// <summary>
    /// ApplyDashboard ViewModel for processing LLM responses.
    /// </summary>
    public ApplyDashboardViewModel ApplyDashboard { get; }

    // === Observable Properties ===

    // Workspace & Files - Multi-root support
    [ObservableProperty]
    private Workspace? _currentWorkspace;

    [ObservableProperty]
    private ObservableCollection<Workspace> _recentWorkspaces = new();

    /// <summary>
    /// Collection of workspace roots (multiple folders can be added).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<WorkspaceRootViewModel> _workspaceRoots = new();

    /// <summary>
    /// The primary workspace root where file changes will be applied.
    /// </summary>
    [ObservableProperty]
    private WorkspaceRootViewModel? _primaryWorkspaceRoot;

    [ObservableProperty]
    private ObservableCollection<FileNodeViewModel> _fileTree = new();

    [ObservableProperty]
    private ObservableCollection<FileNodeViewModel> _filteredFileTree = new();

    [ObservableProperty]
    private string _fileSearchQuery = string.Empty;

    // Roles
    [ObservableProperty]
    private ObservableCollection<Role> _availableRoles = new();

    [ObservableProperty]
    private Role? _selectedRole;

    // Templates
    [ObservableProperty]
    private ObservableCollection<PromptTemplate> _availableTemplates = new();

    [ObservableProperty]
    private PromptTemplate? _selectedTemplate;

    // Models
    [ObservableProperty]
    private ObservableCollection<ModelProfile> _availableModels = new();

    [ObservableProperty]
    private ModelProfile? _selectedModel;

    // Preview
    [ObservableProperty]
    private string _livePreviewContent = string.Empty;

    [ObservableProperty]
    private string _customInstructions = string.Empty;

    [ObservableProperty]
    private string _previewTokenCount = string.Empty;

    // Sanitization
    [ObservableProperty]
    private bool _sanitizationEnabled = true;

    [ObservableProperty]
    private bool _hasSanitizedValues;

    [ObservableProperty]
    private string _sanitizedCountText = string.Empty;

    // Status
    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private string _selectedFilesText = "0 files selected";

    [ObservableProperty]
    private string _totalTokensText = "0 tokens";

    [ObservableProperty]
    private string _contextUsageText = "0 / 0";

    [ObservableProperty]
    private double _contextUsagePercent;

    [ObservableProperty]
    private IBrush _contextUsageColor = Brushes.Green;

    [ObservableProperty]
    private string _selectedModelName = string.Empty;

    [ObservableProperty]
    private string _sessionDuration = "Session: 0m";

    // Response Dashboard
    [ObservableProperty]
    private bool _responseDashboardVisible;

    [ObservableProperty]
    private string _responseText = string.Empty;

    // Layout & Panels
    [ObservableProperty]
    private double _fileTreeWidth = LayoutDefaults.FileTreeWidth;

    [ObservableProperty]
    private double _previewHeight = 0.5; // 50% of right panel (ratio)

    [ObservableProperty]
    private bool _isFileTreeCollapsed;

    [ObservableProperty]
    private bool _isPreviewCollapsed;

    public MainWindowV2ViewModel(
        ISanitizationEngine sanitizationEngine,
        IDesanitizationEngine desanitizationEngine,
        ITokenCountingService tokenService,
        IPromptComposer promptComposer,
        IFileAggregationService fileService,
        IClipboardService clipboardService,
        IRoleRepository roleRepository,
        IWorkspaceRepository workspaceRepository,
        IPromptTemplateRepository templateRepository,
        IMappingSession mappingSession,
        StatusBarViewModel statusBar,
        ApplyDashboardViewModel applyDashboard,
        IGitRepositoryService? gitService = null,
        ILayoutStateRepository? layoutRepository = null)
    {
        _sanitizationEngine = sanitizationEngine;
        _desanitizationEngine = desanitizationEngine;
        _tokenService = tokenService;
        _promptComposer = promptComposer;
        _fileService = fileService;
        _clipboardService = clipboardService;
        _roleRepository = roleRepository;
        _workspaceRepository = workspaceRepository;
        _templateRepository = templateRepository;
        _mappingSession = mappingSession;
        _gitService = gitService;
        _layoutRepository = layoutRepository;
        StatusBar = statusBar;
        ApplyDashboard = applyDashboard;

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadRolesAsync();
        await LoadTemplatesAsync();
        await LoadWorkspacesAsync();
        await LoadLayoutStateAsync();
        LoadModels();
        StartSessionTimer();
    }

    private async Task LoadLayoutStateAsync()
    {
        if (_layoutRepository == null) return;

        try
        {
            _isLoadingLayout = true;
            var state = await _layoutRepository.LoadLayoutStateAsync();
            if (state != null)
            {
                FileTreeWidth = state.FileTreeWidth;
                IsFileTreeCollapsed = state.IsFileTreeCollapsed;
                IsPreviewCollapsed = state.IsPreviewCollapsed;
            }
        }
        finally
        {
            _isLoadingLayout = false;
        }
    }

    private async Task SaveLayoutStateAsync()
    {
        if (_layoutRepository == null || _isLoadingLayout) return;

        var state = new LayoutState(
            FileTreeWidth,
            PreviewHeight,
            IsFileTreeCollapsed,
            false, // PromptBuilder not separate in V2
            IsPreviewCollapsed);

        await _layoutRepository.SaveLayoutStateAsync(state);
    }

    private Task LoadRolesAsync()
    {
        var roles = _roleRepository.GetAllRoles();
        AvailableRoles = new ObservableCollection<Role>(roles);
        SelectedRole = _roleRepository.GetDefault();
        return Task.CompletedTask;
    }

    private Task LoadTemplatesAsync()
    {
        var templates = _templateRepository.GetAllTemplates();
        AvailableTemplates = new ObservableCollection<PromptTemplate>(templates);
        SelectedTemplate = templates.FirstOrDefault();
        return Task.CompletedTask;
    }

    private async Task LoadWorkspacesAsync()
    {
        var workspaces = await _workspaceRepository.GetAllAsync();
        RecentWorkspaces = new ObservableCollection<Workspace>(workspaces);
    }

    private void LoadModels()
    {
        AvailableModels = new ObservableCollection<ModelProfile>
        {
            ModelProfiles.GPT4o,
            ModelProfiles.GPT4oMini,
            ModelProfiles.Claude35Sonnet,
            ModelProfiles.Claude3Opus,
            ModelProfiles.Gemini25Pro
        };
        SelectedModel = AvailableModels.First();
        SelectedModelName = SelectedModel.DisplayName;
        StatusBar.UpdateModel(SelectedModel.DisplayName);
    }

    private void StartSessionTimer()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                var duration = DateTime.Now - _sessionStartTime;
                SessionDuration = $"Session: {(int)duration.TotalHours}h {duration.Minutes}m";
            }
        });
    }

    // === Commands ===

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        try
        {
            // Open folder picker dialog
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var dialog = new Avalonia.Controls.OpenFolderDialog
                {
                    Title = "Select folder to analyze"
                };

                var selectedPath = await dialog.ShowAsync(desktop.MainWindow!);
#pragma warning restore CS0618

                if (string.IsNullOrEmpty(selectedPath))
                {
                    StatusText = "Cancelled";
                    return; // User cancelled
                }

                await LoadFolderAsync(selectedPath);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"❌ Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Adds a folder as a workspace root. Multiple roots can be added.
    /// The first root added becomes the primary workspace.
    /// </summary>
    public async Task LoadFolderAsync(string path)
    {
        StatusText = "Loading...";
        try
        {
            // Check if this folder is already added
            if (WorkspaceRoots.Any(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                StatusText = "Folder already added";
                StatusBar.ReportWarning("This folder is already in the workspace");
                return;
            }

            var rootNode = await _fileService.LoadDirectoryAsync(path);

            // Determine if this should be the primary workspace
            var isPrimary = WorkspaceRoots.Count == 0;
            var workspaceRoot = new WorkspaceRootViewModel(rootNode, isPrimary);

            // Subscribe to selection changes for all children
            foreach (var child in workspaceRoot.Children)
            {
                SubscribeToSelectionChanges(child);
            }

            // Add to workspace roots
            WorkspaceRoots.Add(workspaceRoot);

            // Set as primary if first
            if (isPrimary)
            {
                PrimaryWorkspaceRoot = workspaceRoot;
            }

            // Rebuild the flat file tree for display
            RebuildFileTree();

            // Create or update workspace entry
            var workspace = await _workspaceRepository.GetByPathAsync(path) ?? new Workspace
            {
                Name = Path.GetFileName(path) ?? path,
                RootPath = path
            };
            workspace = workspace with { LastOpened = DateTime.UtcNow };
            await _workspaceRepository.SaveAsync(workspace);
            CurrentWorkspace = workspace;

            // Update git branch info for primary workspace
            if (_gitService != null && PrimaryWorkspaceRoot != null)
            {
                StatusBar.UpdateGitInfo(PrimaryWorkspaceRoot.Path);
            }

            // Set primary workspace root for apply operations
            if (PrimaryWorkspaceRoot != null)
            {
                ApplyDashboard.SetWorkspaceRoot(PrimaryWorkspaceRoot.Path);
            }

            UpdateTokenCounts();
            UpdateLivePreview();
            StatusText = "Ready";
            StatusBar.ReportSuccess($"Added: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            StatusBar.ReportError(ex.Message);
        }
    }

    /// <summary>
    /// Sets a workspace root as the primary (where changes will be applied).
    /// </summary>
    [RelayCommand]
    private void SetPrimaryWorkspace(WorkspaceRootViewModel workspace)
    {
        foreach (var root in WorkspaceRoots)
        {
            root.IsPrimary = root == workspace;
        }
        PrimaryWorkspaceRoot = workspace;

        // Update apply dashboard with new primary
        ApplyDashboard.SetWorkspaceRoot(workspace.Path);

        // Update git info for new primary
        if (_gitService != null)
        {
            StatusBar.UpdateGitInfo(workspace.Path);
        }

        StatusBar.ReportInfo($"Primary workspace: {workspace.Name}");
    }

    /// <summary>
    /// Removes a workspace root from the collection.
    /// </summary>
    [RelayCommand]
    private void RemoveWorkspaceRoot(WorkspaceRootViewModel workspace)
    {
        var wasPrimary = workspace.IsPrimary;
        WorkspaceRoots.Remove(workspace);

        // If we removed the primary, set a new one
        if (wasPrimary && WorkspaceRoots.Count > 0)
        {
            SetPrimaryWorkspace(WorkspaceRoots[0]);
        }
        else if (WorkspaceRoots.Count == 0)
        {
            PrimaryWorkspaceRoot = null;
        }

        RebuildFileTree();
        UpdateTokenCounts();
        UpdateLivePreview();
        StatusBar.ReportInfo($"Removed: {workspace.Name}");
    }

    /// <summary>
    /// Clears all workspace roots.
    /// </summary>
    [RelayCommand]
    private void ClearAllWorkspaces()
    {
        WorkspaceRoots.Clear();
        PrimaryWorkspaceRoot = null;
        FileTree.Clear();
        FilteredFileTree.Clear();
        UpdateTokenCounts();
        UpdateLivePreview();
        StatusBar.ReportInfo("All workspaces cleared");
    }

    /// <summary>
    /// Rebuilds the flat file tree from all workspace roots.
    /// </summary>
    private void RebuildFileTree()
    {
        FileTree.Clear();

        // Add children from all workspace roots to the flat tree
        foreach (var root in WorkspaceRoots)
        {
            foreach (var child in root.Children)
            {
                FileTree.Add(child);
            }
        }

        FilteredFileTree = new ObservableCollection<FileNodeViewModel>(FileTree);
    }

    /// <summary>
    /// Subscribes to IsSelected changes on a FileNodeViewModel and its children.
    /// When selection changes, updates token counts and live preview.
    /// </summary>
    private void SubscribeToSelectionChanges(FileNodeViewModel node)
    {
        node.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(FileNodeViewModel.IsSelected))
            {
                // Debounce updates to avoid excessive recalculation during cascading changes
                UpdateTokenCounts();
                UpdateLivePreview();
            }
        };

        // Recursively subscribe to children
        foreach (var child in node.Children)
        {
            SubscribeToSelectionChanges(child);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (CurrentWorkspace != null)
        {
            await LoadFolderAsync(CurrentWorkspace.RootPath);
        }
    }

    [RelayCommand]
    private async Task CopyToClipboardAsync()
    {
        StatusText = "Copying...";
        StatusBar.ReportInfo("Copying to clipboard...");
        try
        {
            var selectedFiles = GetSelectedFiles().ToList();
            if (!selectedFiles.Any())
            {
                StatusText = "No files selected";
                StatusBar.ReportWarning("No files selected");
                return;
            }

            if (SelectedTemplate == null || SelectedRole == null)
            {
                StatusText = "Select a template and role first";
                StatusBar.ReportWarning("Select a template and role first");
                return;
            }

            // Compose prompt using existing service
            var options = new PromptOptions(
                CustomInstructions: CustomInstructions,
                IncludeLineNumbers: false,
                IncludeFilePaths: true,
                SelectedRole: SelectedRole,
                SelectedFocusAreas: null);

            var result = _promptComposer.Compose(SelectedTemplate, selectedFiles, options);
            var content = result.FullPrompt;

            // Sanitize if enabled (using existing engine)
            if (SanitizationEnabled)
            {
                var sanitizationResult = _sanitizationEngine.Sanitize(content, new SanitizationOptions());
                content = sanitizationResult.SanitizedContent;
                UpdateSanitizationStatus(sanitizationResult);
            }

            await _clipboardService.SetTextAsync(content);
            StatusText = "✅ Copied to clipboard!";
            StatusBar.ReportSuccess("Copied to clipboard!");
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            StatusBar.ReportError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task CopyPreviewAsync()
    {
        if (!string.IsNullOrEmpty(LivePreviewContent))
        {
            await _clipboardService.SetTextAsync(LivePreviewContent);
            StatusText = "✅ Preview copied!";
        }
    }

    [RelayCommand]
    private async Task PasteResponseAsync()
    {
        var clipboardContent = await _clipboardService.GetTextAsync();
        if (!string.IsNullOrEmpty(clipboardContent))
        {
            ResponseText = clipboardContent;
            ApplyDashboard.ResponseText = clipboardContent;
            ResponseDashboardVisible = true;
            StatusBar.ReportInfo("Response pasted - ready to parse");
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        StatusText = "Settings...";
    }

    [RelayCommand]
    private void CloseApplyDashboard()
    {
        ResponseDashboardVisible = false;
        ApplyDashboard.Clear();
        StatusBar.ReportInfo("Ready");
    }

    [RelayCommand]
    private void ToggleFileTree()
    {
        IsFileTreeCollapsed = !IsFileTreeCollapsed;
    }

    [RelayCommand]
    private void TogglePreview()
    {
        IsPreviewCollapsed = !IsPreviewCollapsed;
    }

    [RelayCommand]
    private async Task ResetLayoutAsync()
    {
        FileTreeWidth = LayoutDefaults.FileTreeWidth;
        PreviewHeight = 0.5;
        IsFileTreeCollapsed = false;
        IsPreviewCollapsed = false;

        if (_layoutRepository != null)
        {
            await _layoutRepository.ResetToDefaultAsync();
        }

        StatusBar.ReportInfo("Layout reset to default");
    }

    // === Public Methods for Updates ===

    public void UpdateTokenCounts()
    {
        var selectedFiles = GetSelectedFiles().ToList();
        var totalTokens = 0;
        var totalFiles = CountTotalFiles();

        foreach (var file in selectedFiles)
        {
            if (!string.IsNullOrEmpty(file.Content))
            {
                totalTokens += _tokenService.CountTokens(file.Content);
            }
        }

        SelectedFilesText = $"{selectedFiles.Count} file{(selectedFiles.Count != 1 ? "s" : "")} selected";
        TotalTokensText = $"{totalTokens:N0} tokens";

        // Update StatusBar with file count
        StatusBar.UpdateFileCount(selectedFiles.Count, totalFiles);

        if (SelectedModel != null)
        {
            var limit = SelectedModel.ContextLimit;
            ContextUsageText = $"{totalTokens:N0} / {limit:N0}";
            ContextUsagePercent = Math.Min(100, (double)totalTokens / limit * 100);

            ContextUsageColor = ContextUsagePercent switch
            {
                < 50 => Brushes.Green,
                < 80 => Brushes.Orange,
                _ => Brushes.Red
            };

            // Update StatusBar with token usage
            StatusBar.UpdateTokenUsage(totalTokens, limit);
        }

        PreviewTokenCount = $"({totalTokens:N0} tokens)";
    }

    private int CountTotalFiles()
    {
        return FileTree.Sum(node => CountFilesRecursive(node));
    }

    private int CountFilesRecursive(FileNodeViewModel node)
    {
        if (!node.IsDirectory)
            return 1;
        return node.Children.Sum(child => CountFilesRecursive(child));
    }

    public void UpdateLivePreview()
    {
        var selectedFiles = GetSelectedFiles().ToList();
        if (!selectedFiles.Any() || SelectedRole == null || SelectedTemplate == null)
        {
            LivePreviewContent = "Select files, a role, and a template to see preview...";
            return;
        }

        try
        {
            var options = new PromptOptions(
                CustomInstructions: CustomInstructions,
                IncludeLineNumbers: false,
                IncludeFilePaths: true,
                SelectedRole: SelectedRole,
                SelectedFocusAreas: null);

            var result = _promptComposer.Compose(SelectedTemplate, selectedFiles, options);
            LivePreviewContent = result.FullPrompt;
        }
        catch (Exception ex)
        {
            LivePreviewContent = $"Error generating preview: {ex.Message}";
        }
    }

    public void UpdateSanitizationStatus(SanitizationResult result)
    {
        _lastSanitizationResult = result;
        HasSanitizedValues = result.TotalMatches > 0;
        SanitizedCountText = result.TotalMatches > 0
            ? $"🔐 {result.TotalMatches} value{(result.TotalMatches != 1 ? "s" : "")} sanitized"
            : string.Empty;
    }

    // === Property Changed Handlers ===

    partial void OnFileSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredFileTree = new ObservableCollection<FileNodeViewModel>(FileTree);
        }
        else
        {
            var filtered = FileTree.Where(f =>
                f.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                f.Children.Any(c => c.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
            FilteredFileTree = new ObservableCollection<FileNodeViewModel>(filtered);
        }
    }

    partial void OnCustomInstructionsChanged(string value)
    {
        UpdateLivePreview();
    }

    partial void OnSelectedRoleChanged(Role? value)
    {
        UpdateLivePreview();
    }

    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        UpdateLivePreview();
    }

    partial void OnSelectedModelChanged(ModelProfile? value)
    {
        if (value != null)
        {
            SelectedModelName = value.DisplayName;
            StatusBar.UpdateModel(value.DisplayName);
            UpdateTokenCounts();
        }
    }

    partial void OnFileTreeWidthChanged(double value)
    {
        _ = SaveLayoutStateAsync();
    }

    partial void OnIsFileTreeCollapsedChanged(bool value)
    {
        _ = SaveLayoutStateAsync();
    }

    partial void OnIsPreviewCollapsedChanged(bool value)
    {
        _ = SaveLayoutStateAsync();
    }

    // === Helpers ===

    /// <summary>
    /// Gets all selected file nodes from the FileNodeViewModels using cascading selection.
    /// Returns the underlying FileNode models for use with business logic services.
    /// </summary>
    private IEnumerable<FileNode> GetSelectedFiles()
    {
        return FileTree
            .SelectMany(vm => vm.GetSelectedFiles())
            .Select(vm => CreateFileNodeFromViewModel(vm));
    }

    private static FileNode CreateFileNodeFromViewModel(FileNodeViewModel vm)
    {
        var node = new FileNode(vm.Path, vm.Name, vm.IsDirectory);

        // Lazy-load file content if not already loaded
        if (string.IsNullOrEmpty(vm.Content) && !vm.IsDirectory && File.Exists(vm.Path))
        {
            try
            {
                node.Content = File.ReadAllText(vm.Path);
            }
            catch (Exception)
            {
                node.Content = "[Error loading file]";
            }
        }
        else
        {
            node.Content = vm.Content;
        }

        return node;
    }
}

// Uses ShieldPrompt.Domain.Entities.ModelProfile and ShieldPrompt.Domain.Entities.ModelProfiles
