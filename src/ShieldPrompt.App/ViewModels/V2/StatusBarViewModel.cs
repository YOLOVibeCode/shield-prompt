using System;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for the status bar.
///
/// MVVM Compliance:
/// - NO business logic
/// - Delegates to services
/// - Only exposes properties for binding
/// </summary>
public partial class StatusBarViewModel : ObservableObject
{
    private readonly IProgressReporter _progressReporter;
    private readonly IStatusBarService _statusBarService;
    private readonly IGitRepositoryService? _gitService;
    private readonly DispatcherTimer _sessionTimer;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private StatusSeverity _statusSeverity = StatusSeverity.Info;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private bool _showProgress;

    [ObservableProperty]
    private int _selectedFileCount;

    [ObservableProperty]
    private int _totalFileCount;

    [ObservableProperty]
    private int _usedTokens;

    [ObservableProperty]
    private int _contextLimit = 128000;

    [ObservableProperty]
    private double _tokenPercentage;

    [ObservableProperty]
    private string _currentBranch = string.Empty;

    [ObservableProperty]
    private bool _showGitBranch;

    [ObservableProperty]
    private string _currentModel = "GPT-4o";

    [ObservableProperty]
    private TimeSpan _sessionDuration;

    public StatusBarViewModel(
        IProgressReporter progressReporter,
        IStatusBarService statusBarService,
        IGitRepositoryService? gitService = null)
    {
        _progressReporter = progressReporter;
        _statusBarService = statusBarService;
        _gitService = gitService;

        _progressReporter.StatusChanged += OnStatusChanged;

        // Session timer - updates every minute
        _sessionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _sessionTimer.Tick += OnSessionTimerTick;
        _sessionTimer.Start();

        // Initialize session duration
        UpdateSessionDuration();
    }

    // Computed properties for UI
    public string FileCountDisplay => $"{SelectedFileCount}/{TotalFileCount} files";

    public string TokenDisplay => $"{UsedTokens:N0} / {ContextLimit:N0}";

    public string BranchDisplay => ShowGitBranch ? $"⎇ {CurrentBranch}" : string.Empty;

    public string SessionDisplay => SessionDuration.TotalHours >= 1
        ? $"Session: {(int)SessionDuration.TotalHours}:{SessionDuration.Minutes:D2}"
        : $"Session: {SessionDuration.Minutes}m";

    public string StatusIcon => StatusSeverity switch
    {
        StatusSeverity.Success => "✅",
        StatusSeverity.Warning => "⚠️",
        StatusSeverity.Error => "❌",
        _ => IsLoading ? "🔄" : "ℹ️"
    };

    public IBrush TokenProgressColor => TokenPercentage switch
    {
        < 50 => Brushes.LightGreen,
        < 80 => Brushes.Yellow,
        < 95 => Brushes.Orange,
        _ => Brushes.Red
    };

    public IBrush StatusColor => StatusSeverity switch
    {
        StatusSeverity.Success => Brushes.LightGreen,
        StatusSeverity.Warning => Brushes.Yellow,
        StatusSeverity.Error => Brushes.Red,
        _ => Brushes.White
    };

    /// <summary>
    /// Updates file selection counts in the status bar.
    /// </summary>
    public void UpdateFileCount(int selected, int total)
    {
        SelectedFileCount = selected;
        TotalFileCount = total;
        _statusBarService.UpdateFileSelection(selected, total);
        OnPropertyChanged(nameof(FileCountDisplay));
    }

    /// <summary>
    /// Updates token usage in the status bar.
    /// </summary>
    public void UpdateTokenUsage(int used, int limit)
    {
        UsedTokens = used;
        ContextLimit = limit;
        TokenPercentage = limit > 0 ? (double)used / limit * 100 : 0;
        _statusBarService.UpdateTokenUsage(used);
        OnPropertyChanged(nameof(TokenDisplay));
        OnPropertyChanged(nameof(TokenProgressColor));
    }

    /// <summary>
    /// Updates the current model display.
    /// </summary>
    public void UpdateModel(string modelName)
    {
        CurrentModel = modelName;
    }

    /// <summary>
    /// Updates git branch information for the given workspace path.
    /// </summary>
    public void UpdateGitInfo(string workspacePath)
    {
        if (_gitService == null)
        {
            ShowGitBranch = false;
            return;
        }

        var branch = _gitService.GetCurrentBranch(workspacePath);
        if (!string.IsNullOrEmpty(branch))
        {
            CurrentBranch = branch;
            ShowGitBranch = true;
        }
        else
        {
            ShowGitBranch = false;
        }
        OnPropertyChanged(nameof(BranchDisplay));
    }

    /// <summary>
    /// Reports an info status message.
    /// </summary>
    public void ReportInfo(string message)
    {
        if (_progressReporter is IStatusMessageReporter reporter)
        {
            reporter.ReportInfo(message);
        }
    }

    /// <summary>
    /// Reports a success status message.
    /// </summary>
    public void ReportSuccess(string message)
    {
        if (_progressReporter is IStatusMessageReporter reporter)
        {
            reporter.ReportSuccess(message);
        }
    }

    /// <summary>
    /// Reports a warning status message.
    /// </summary>
    public void ReportWarning(string message)
    {
        if (_progressReporter is IStatusMessageReporter reporter)
        {
            reporter.ReportWarning(message);
        }
    }

    /// <summary>
    /// Reports an error status message.
    /// </summary>
    public void ReportError(string message)
    {
        if (_progressReporter is IStatusMessageReporter reporter)
        {
            reporter.ReportError(message);
        }
    }

    private void OnStatusChanged(object? sender, StatusInfo status)
    {
        StatusMessage = status.Message;
        StatusSeverity = status.Severity;
        IsLoading = status.IsLoading;

        if (status.Progress != null)
        {
            ShowProgress = true;
            ProgressPercentage = status.Progress.Percentage;
        }
        else
        {
            ShowProgress = false;
        }

        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(StatusColor));
    }

    private void OnSessionTimerTick(object? sender, EventArgs e)
    {
        UpdateSessionDuration();
    }

    private void UpdateSessionDuration()
    {
        var info = _statusBarService.GetSessionInfo();
        SessionDuration = info.Duration;
        OnPropertyChanged(nameof(SessionDisplay));
    }
}
