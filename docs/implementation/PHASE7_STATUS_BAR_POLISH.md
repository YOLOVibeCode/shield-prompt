# Phase 7: Status Bar & Polish Implementation Specification

**Phase ID:** PHASE-7  
**Priority:** P1 (Polish)  
**Estimated Effort:** 2-3 days  
**Prerequisites:** Phase 1, Phase 4  
**Status:** PENDING

---

## 1. Executive Summary

Phase 7 focuses on the status bar implementation and general UI polish. The status bar provides contextual feedback, progress indicators, and quick stats to improve user experience.

---

## 2. Feature Requirements

### 2.1 Core Features

| Feature | Description | Priority |
|---------|-------------|----------|
| Status Messages | Display operation status | P0 |
| Progress Indicator | Show long-running operation progress | P0 |
| File Count | Display selected/total files | P0 |
| Token Count | Display selected/limit tokens | P0 |
| Git Branch | Display current git branch | P1 |
| Session Timer | Display session duration | P2 |
| Model Indicator | Show current AI model | P1 |

### 2.2 Status Message Types

| Type | Icon | Color | Example |
|------|------|-------|---------|
| Info | ℹ️ | White | "Ready" |
| Success | ✅ | Green | "Copied to clipboard!" |
| Warning | ⚠️ | Yellow | "Large file included" |
| Error | ❌ | Red | "Error loading file" |
| Progress | 🔄 | Blue | "Loading... 45%" |

---

## 3. Domain Model Specification

### 3.1 StatusInfo Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/StatusInfo.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Represents the current status bar state.
/// </summary>
public record StatusInfo(
    string Message,
    StatusSeverity Severity,
    bool IsLoading,
    ProgressInfo? Progress = null);
```

### 3.2 ProgressInfo Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/ProgressInfo.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Progress information for long-running operations.
/// </summary>
public record ProgressInfo(
    int Current,
    int Total,
    string? Description = null)
{
    public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
    public bool IsIndeterminate => Total <= 0;
}
```

### 3.3 StatusSeverity Enum (NEW)

**File:** `src/ShieldPrompt.Domain/Enums/StatusSeverity.cs`

```csharp
namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Severity level for status messages.
/// </summary>
public enum StatusSeverity
{
    Info,
    Success,
    Warning,
    Error
}
```

---

## 4. Interface Specifications (ISP-Compliant)

### 4.1 IStatusMessageReporter (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IStatusMessageReporter.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Reports status messages to the UI.
/// Follows ISP - message reporting only.
/// </summary>
public interface IStatusMessageReporter
{
    /// <summary>
    /// Reports an informational message.
    /// </summary>
    void ReportInfo(string message);
    
    /// <summary>
    /// Reports a success message.
    /// </summary>
    void ReportSuccess(string message);
    
    /// <summary>
    /// Reports a warning message.
    /// </summary>
    void ReportWarning(string message);
    
    /// <summary>
    /// Reports an error message.
    /// </summary>
    void ReportError(string message);
}
```

### 4.2 IProgressReporter (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IProgressReporter.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Reports progress for long-running operations.
/// Follows ISP - progress reporting only.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Gets the current status.
    /// </summary>
    StatusInfo CurrentStatus { get; }
    
    /// <summary>
    /// Event fired when status changes.
    /// </summary>
    event EventHandler<StatusInfo>? StatusChanged;
    
    /// <summary>
    /// Reports progress for an operation.
    /// </summary>
    void ReportProgress(int current, int total, string? description = null);
    
    /// <summary>
    /// Clears the current progress.
    /// </summary>
    void ClearProgress();
}
```

### 4.3 IStatusBarService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IStatusBarService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Provides aggregated status bar data.
/// Follows ISP - status aggregation only.
/// </summary>
public interface IStatusBarService
{
    /// <summary>
    /// Gets the current file selection summary.
    /// </summary>
    FileSelectionSummary GetFileSelectionSummary();
    
    /// <summary>
    /// Gets the current token usage summary.
    /// </summary>
    TokenUsageSummary GetTokenUsageSummary(int contextLimit);
    
    /// <summary>
    /// Gets the session information.
    /// </summary>
    SessionInfo GetSessionInfo();
    
    /// <summary>
    /// Refreshes all status bar data.
    /// </summary>
    void Refresh();
}

public record FileSelectionSummary(int SelectedCount, int TotalCount);
public record TokenUsageSummary(int UsedTokens, int ContextLimit, double Percentage);
public record SessionInfo(TimeSpan Duration, DateTime StartTime);
```

---

## 5. Service Implementations

### 5.1 StatusReporter

**File:** `src/ShieldPrompt.Application/Services/StatusReporter.cs`

```csharp
namespace ShieldPrompt.Application.Services;

/// <summary>
/// Unified status reporter implementing both message and progress reporting.
/// </summary>
public class StatusReporter : IStatusMessageReporter, IProgressReporter
{
    private StatusInfo _currentStatus = new("Ready", StatusSeverity.Info, false);
    
    public StatusInfo CurrentStatus => _currentStatus;
    
    public event EventHandler<StatusInfo>? StatusChanged;

    public void ReportInfo(string message)
    {
        UpdateStatus(new StatusInfo(message, StatusSeverity.Info, false));
    }

    public void ReportSuccess(string message)
    {
        UpdateStatus(new StatusInfo(message, StatusSeverity.Success, false));
    }

    public void ReportWarning(string message)
    {
        UpdateStatus(new StatusInfo(message, StatusSeverity.Warning, false));
    }

    public void ReportError(string message)
    {
        UpdateStatus(new StatusInfo(message, StatusSeverity.Error, false));
    }

    public void ReportProgress(int current, int total, string? description = null)
    {
        var progress = new ProgressInfo(current, total, description);
        var message = description ?? $"Processing... {progress.Percentage:F0}%";
        UpdateStatus(new StatusInfo(message, StatusSeverity.Info, true, progress));
    }

    public void ClearProgress()
    {
        UpdateStatus(new StatusInfo("Ready", StatusSeverity.Info, false));
    }

    private void UpdateStatus(StatusInfo status)
    {
        _currentStatus = status;
        StatusChanged?.Invoke(this, status);
    }
}
```

### 5.2 StatusBarService

**File:** `src/ShieldPrompt.Application/Services/StatusBarService.cs`

```csharp
namespace ShieldPrompt.Application.Services;

public class StatusBarService(
    ISessionManager sessionManager,
    ITokenCountingService tokenService) : IStatusBarService
{
    private readonly DateTime _sessionStart = DateTime.UtcNow;
    private int _selectedCount;
    private int _totalCount;
    private int _usedTokens;

    public FileSelectionSummary GetFileSelectionSummary()
    {
        return new FileSelectionSummary(_selectedCount, _totalCount);
    }

    public TokenUsageSummary GetTokenUsageSummary(int contextLimit)
    {
        var percentage = contextLimit > 0 ? (double)_usedTokens / contextLimit * 100 : 0;
        return new TokenUsageSummary(_usedTokens, contextLimit, percentage);
    }

    public SessionInfo GetSessionInfo()
    {
        var duration = DateTime.UtcNow - _sessionStart;
        return new SessionInfo(duration, _sessionStart);
    }

    public void Refresh()
    {
        // Called by ViewModel to update cached values
        var session = sessionManager.ActiveSession;
        if (session != null)
        {
            _selectedCount = session.SelectedFilePaths.Count;
            // Calculate tokens from selected files
        }
    }
}
```

---

## 6. ViewModel Specification

### 6.1 StatusBarViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/StatusBarViewModel.cs`

```csharp
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
    private readonly IGitRepositoryService _gitService;
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
        IGitRepositoryService gitService)
    {
        _progressReporter = progressReporter;
        _statusBarService = statusBarService;
        _gitService = gitService;

        _progressReporter.StatusChanged += OnStatusChanged;

        // Session timer
        _sessionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _sessionTimer.Tick += OnSessionTimerTick;
        _sessionTimer.Start();
    }

    // Computed properties for UI
    public string FileCountDisplay => $"{SelectedFileCount}/{TotalFileCount} files";
    
    public string TokenDisplay => $"{UsedTokens:N0} / {ContextLimit:N0}";
    
    public string BranchDisplay => ShowGitBranch ? $"⎇ {CurrentBranch}" : string.Empty;
    
    public string SessionDisplay => $"Session: {SessionDuration:h\\:mm}";

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

    public void UpdateFileCount(int selected, int total)
    {
        SelectedFileCount = selected;
        TotalFileCount = total;
        OnPropertyChanged(nameof(FileCountDisplay));
    }

    public void UpdateTokenUsage(int used, int limit)
    {
        UsedTokens = used;
        ContextLimit = limit;
        TokenPercentage = limit > 0 ? (double)used / limit * 100 : 0;
        OnPropertyChanged(nameof(TokenDisplay));
        OnPropertyChanged(nameof(TokenProgressColor));
    }

    public async Task UpdateGitInfoAsync(string workspacePath)
    {
        var branch = _gitService.GetCurrentBranch(workspacePath);
        if (branch != null)
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
    }

    private void OnSessionTimerTick(object? sender, EventArgs e)
    {
        var info = _statusBarService.GetSessionInfo();
        SessionDuration = info.Duration;
        OnPropertyChanged(nameof(SessionDisplay));
    }
}
```

---

## 7. View Specification

### 7.1 StatusBar.axaml (NEW/ENHANCE)

**File:** `src/ShieldPrompt.App/Views/V2/Controls/StatusBar.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:ShieldPrompt.App.ViewModels.V2"
             x:DataType="vm:StatusBarViewModel"
             x:Class="ShieldPrompt.App.Views.V2.Controls.StatusBar">
    
    <Border Background="#181825" Padding="8,4">
        <Grid ColumnDefinitions="Auto,*,Auto,Auto,Auto,Auto,Auto">
            
            <!-- Status Message -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8">
                <TextBlock Text="{Binding StatusIcon}" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding StatusMessage}" 
                           VerticalAlignment="Center"
                           Foreground="{Binding StatusSeverity, Converter={StaticResource SeverityToColorConverter}}"/>
            </StackPanel>
            
            <!-- Progress Bar (when loading) -->
            <ProgressBar Grid.Column="1"
                         Value="{Binding ProgressPercentage}"
                         Maximum="100"
                         Height="4"
                         Margin="16,0"
                         IsVisible="{Binding ShowProgress}"
                         VerticalAlignment="Center"/>
            
            <!-- File Count -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4" Margin="0,0,16,0">
                <TextBlock Text="📁" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding FileCountDisplay}" 
                           Foreground="#cdd6f4"
                           VerticalAlignment="Center"/>
            </StackPanel>
            
            <!-- Git Branch -->
            <TextBlock Grid.Column="3"
                       Text="{Binding BranchDisplay}"
                       Foreground="#89b4fa"
                       IsVisible="{Binding ShowGitBranch}"
                       Margin="0,0,16,0"
                       VerticalAlignment="Center"/>
            
            <!-- Token Usage with Progress -->
            <Grid Grid.Column="4" ColumnDefinitions="Auto,Auto" Margin="0,0,16,0">
                <TextBlock Grid.Column="0"
                           Text="{Binding TokenDisplay}"
                           Foreground="#cdd6f4"
                           VerticalAlignment="Center"
                           Margin="0,0,8,0"/>
                <ProgressBar Grid.Column="1"
                             Value="{Binding TokenPercentage}"
                             Maximum="100"
                             Width="80"
                             Height="6"
                             Foreground="{Binding TokenProgressColor}"
                             VerticalAlignment="Center"/>
            </Grid>
            
            <!-- Current Model -->
            <Border Grid.Column="5"
                    Background="#313244"
                    CornerRadius="4"
                    Padding="8,2"
                    Margin="0,0,16,0">
                <TextBlock Text="{Binding CurrentModel}"
                           Foreground="#a6e3a1"
                           FontSize="11"
                           VerticalAlignment="Center"/>
            </Border>
            
            <!-- Session Timer -->
            <TextBlock Grid.Column="6"
                       Text="{Binding SessionDisplay}"
                       Foreground="#6c7086"
                       FontSize="11"
                       VerticalAlignment="Center"/>
        </Grid>
    </Border>
</UserControl>
```

---

## 8. Test Specifications (TDD)

### 8.1 StatusReporter Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Application/Services/StatusReporterTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Application.Services;

public class StatusReporterTests
{
    private readonly StatusReporter _sut;

    public StatusReporterTests()
    {
        _sut = new StatusReporter();
    }

    [Fact]
    public void ReportInfo_SetsCorrectSeverity()
    {
        _sut.ReportInfo("Test message");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Info);
        _sut.CurrentStatus.Message.Should().Be("Test message");
    }

    [Fact]
    public void ReportSuccess_SetsCorrectSeverity()
    {
        _sut.ReportSuccess("Done!");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Success);
    }

    [Fact]
    public void ReportWarning_SetsCorrectSeverity()
    {
        _sut.ReportWarning("Careful!");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Warning);
    }

    [Fact]
    public void ReportError_SetsCorrectSeverity()
    {
        _sut.ReportError("Failed!");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Error);
    }

    [Fact]
    public void ReportProgress_CalculatesPercentage()
    {
        _sut.ReportProgress(50, 100, "Loading...");

        _sut.CurrentStatus.Progress.Should().NotBeNull();
        _sut.CurrentStatus.Progress!.Percentage.Should().Be(50);
        _sut.CurrentStatus.IsLoading.Should().BeTrue();
    }

    [Fact]
    public void ClearProgress_ResetsToReady()
    {
        _sut.ReportProgress(50, 100);
        _sut.ClearProgress();

        _sut.CurrentStatus.Message.Should().Be("Ready");
        _sut.CurrentStatus.IsLoading.Should().BeFalse();
        _sut.CurrentStatus.Progress.Should().BeNull();
    }

    [Fact]
    public void StatusChanged_FiresOnUpdate()
    {
        StatusInfo? received = null;
        _sut.StatusChanged += (s, e) => received = e;

        _sut.ReportSuccess("Test");

        received.Should().NotBeNull();
        received!.Severity.Should().Be(StatusSeverity.Success);
    }
}
```

### 8.2 StatusBarViewModel Tests

**File:** `tests/ShieldPrompt.Tests.Unit/ViewModels/V2/StatusBarViewModelTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

public class StatusBarViewModelTests
{
    private readonly IProgressReporter _progressReporter;
    private readonly IStatusBarService _statusBarService;
    private readonly IGitRepositoryService _gitService;
    private readonly StatusBarViewModel _sut;

    public StatusBarViewModelTests()
    {
        _progressReporter = Substitute.For<IProgressReporter>();
        _statusBarService = Substitute.For<IStatusBarService>();
        _gitService = Substitute.For<IGitRepositoryService>();
        _sut = new StatusBarViewModel(_progressReporter, _statusBarService, _gitService);
    }

    [Fact]
    public void UpdateFileCount_UpdatesProperties()
    {
        _sut.UpdateFileCount(5, 20);

        _sut.SelectedFileCount.Should().Be(5);
        _sut.TotalFileCount.Should().Be(20);
        _sut.FileCountDisplay.Should().Be("5/20 files");
    }

    [Fact]
    public void UpdateTokenUsage_CalculatesPercentage()
    {
        _sut.UpdateTokenUsage(64000, 128000);

        _sut.UsedTokens.Should().Be(64000);
        _sut.TokenPercentage.Should().Be(50);
    }

    [Theory]
    [InlineData(25, "LightGreen")]
    [InlineData(60, "Yellow")]
    [InlineData(85, "Orange")]
    [InlineData(98, "Red")]
    public void TokenProgressColor_BasedOnPercentage(double percentage, string expectedColor)
    {
        _sut.UpdateTokenUsage((int)(percentage * 1280), 128000);

        var colorName = _sut.TokenProgressColor.ToString();
        colorName.Should().Contain(expectedColor);
    }

    [Fact]
    public async Task UpdateGitInfoAsync_WithGitRepo_ShowsBranch()
    {
        _gitService.GetCurrentBranch("/repo").Returns("main");

        await _sut.UpdateGitInfoAsync("/repo");

        _sut.CurrentBranch.Should().Be("main");
        _sut.ShowGitBranch.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateGitInfoAsync_WithoutGitRepo_HidesBranch()
    {
        _gitService.GetCurrentBranch("/not-repo").Returns((string?)null);

        await _sut.UpdateGitInfoAsync("/not-repo");

        _sut.ShowGitBranch.Should().BeFalse();
    }
}
```

---

## 9. Implementation Checklist

### 9.1 Domain Layer

- [ ] Create `StatusInfo` record
- [ ] Create `ProgressInfo` record
- [ ] Create `StatusSeverity` enum
- [ ] Write unit tests
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~StatusInfo"`

### 9.2 Application Layer

- [ ] Create `IStatusMessageReporter` interface
- [ ] Create `IProgressReporter` interface
- [ ] Create `IStatusBarService` interface
- [ ] Implement `StatusReporter`
- [ ] Implement `StatusBarService`
- [ ] Write unit tests (TDD)
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~StatusReporter"`

### 9.3 Presentation Layer

- [ ] Create `StatusBarViewModel`
- [ ] Create/Update `StatusBar.axaml`
- [ ] Add severity to color converter
- [ ] Write ViewModel tests (TDD)
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~StatusBarViewModel"`

### 9.4 Integration

- [ ] Register services in DI
- [ ] Wire status bar to MainWindowV2
- [ ] Connect to file selection events
- [ ] Connect to token counting events
- [ ] Connect to git service
- [ ] End-to-end testing

---

## 10. Acceptance Criteria

| Criterion | Verification |
|-----------|--------------|
| Status messages display correctly | Manual test |
| Progress bar shows during loading | Manual test |
| File count updates on selection | Manual test |
| Token count updates on selection | Manual test |
| Git branch shows when in repo | Manual test |
| Token color changes at thresholds | Manual test |
| Session timer updates | Manual test |
| All unit tests pass | `dotnet test` |

---

## 11. Security Considerations

| Risk | Mitigation |
|------|------------|
| No sensitive data in status | Messages are generic, no file contents |
| Error messages don't leak paths | Use generic error descriptions |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial specification |
