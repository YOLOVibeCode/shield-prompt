# Phase 2: Multi-Tab Prompt Sessions Implementation Specification

**Phase ID:** PHASE-2  
**Priority:** P1 (High Value Feature)  
**Estimated Effort:** 4-5 days  
**Prerequisites:** Phase 1 (Workspace Management)  
**Status:** PENDING

---

## 1. Executive Summary

Multi-Tab Prompt Sessions allow users to work on multiple prompts concurrently within a single workspace. Each tab maintains isolated state (selected files, custom instructions, role) while sharing the workspace context.

---

## 2. Feature Requirements

### 2.1 Core Features

| Feature | Description | Priority |
|---------|-------------|----------|
| Tab Creation | Create new prompt tabs within workspace | P0 |
| Tab Switching | Switch between tabs preserving state | P0 |
| Tab Closing | Close tabs with unsaved changes warning | P0 |
| Tab Reordering | Drag to reorder tabs | P2 |
| Tab Persistence | Restore tabs on app restart | P1 |
| Tab Duplication | Clone a tab with current state | P2 |

### 2.2 Per-Tab State

Each tab maintains:
- Selected files (independent of other tabs)
- Custom instructions text
- Selected role
- Live preview content
- Sanitization mappings (if copied)
- Scroll position in preview

### 2.3 Shared State (Workspace Level)

All tabs share:
- File tree (source of truth)
- Available roles
- Available models
- Workspace settings

---

## 3. Domain Model Specification

### 3.1 PromptSession Entity (NEW)

**File:** `src/ShieldPrompt.Domain/Entities/PromptSession.cs`

```csharp
namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents a single prompt session (tab) within a workspace.
/// </summary>
public record PromptSession
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string Name { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastModified { get; init; } = DateTime.UtcNow;
    
    // Per-session state
    public IReadOnlyList<string> SelectedFilePaths { get; init; } = Array.Empty<string>();
    public string CustomInstructions { get; init; } = string.Empty;
    public string? SelectedRoleId { get; init; }
    public string? SelectedModelId { get; init; }
    
    // UI state
    public double PreviewScrollPosition { get; init; }
    public bool IsPinned { get; init; }
    
    /// <summary>
    /// Creates a new session with default values.
    /// </summary>
    public static PromptSession CreateNew(string workspaceId, string name)
    {
        return new PromptSession
        {
            Id = Guid.NewGuid().ToString(),
            WorkspaceId = workspaceId,
            Name = name
        };
    }
}
```

### 3.2 SessionState Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/SessionState.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Runtime state of a prompt session (not persisted).
/// </summary>
public record SessionState(
    string SessionId,
    string GeneratedPrompt,
    int TokenCount,
    bool HasUnsavedChanges,
    IReadOnlyList<SanitizationMatch>? SanitizationMatches = null);
```

---

## 4. Interface Specifications (ISP-Compliant)

### 4.1 ISessionRepository (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/ISessionRepository.cs`

**ISP Compliance:** 5 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for prompt session persistence.
/// Follows ISP - session CRUD only.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Gets all sessions for a workspace.
    /// </summary>
    Task<IReadOnlyList<PromptSession>> GetByWorkspaceAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    Task<PromptSession?> GetByIdAsync(string sessionId, CancellationToken ct = default);
    
    /// <summary>
    /// Saves or updates a session.
    /// </summary>
    Task SaveAsync(PromptSession session, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a session.
    /// </summary>
    Task DeleteAsync(string sessionId, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes all sessions for a workspace.
    /// </summary>
    Task DeleteByWorkspaceAsync(string workspaceId, CancellationToken ct = default);
}
```

### 4.2 ISessionManager (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/ISessionManager.cs`

**ISP Compliance:** 5 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing active sessions in memory.
/// Follows ISP - session lifecycle only.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Gets the currently active session ID.
    /// </summary>
    string? ActiveSessionId { get; }
    
    /// <summary>
    /// Gets all active session IDs.
    /// </summary>
    IReadOnlyList<string> ActiveSessionIds { get; }
    
    /// <summary>
    /// Creates a new session and sets it as active.
    /// </summary>
    PromptSession CreateSession(string workspaceId, string name);
    
    /// <summary>
    /// Sets the active session.
    /// </summary>
    void SetActiveSession(string sessionId);
    
    /// <summary>
    /// Closes a session.
    /// </summary>
    void CloseSession(string sessionId);
    
    /// <summary>
    /// Event fired when active session changes.
    /// </summary>
    event EventHandler<SessionChangedEventArgs>? ActiveSessionChanged;
}

public record SessionChangedEventArgs(string? OldSessionId, string? NewSessionId);
```

### 4.3 ISessionStateService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/ISessionStateService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing session runtime state.
/// Follows ISP - state management only.
/// </summary>
public interface ISessionStateService
{
    /// <summary>
    /// Gets the current state for a session.
    /// </summary>
    SessionState? GetState(string sessionId);
    
    /// <summary>
    /// Updates the state for a session.
    /// </summary>
    void UpdateState(string sessionId, SessionState state);
    
    /// <summary>
    /// Marks a session as having unsaved changes.
    /// </summary>
    void MarkDirty(string sessionId);
    
    /// <summary>
    /// Clears the dirty flag for a session.
    /// </summary>
    void MarkClean(string sessionId);
}
```

---

## 5. Service Implementations

### 5.1 SessionManager

**File:** `src/ShieldPrompt.Application/Services/SessionManager.cs`

```csharp
namespace ShieldPrompt.Application.Services;

public class SessionManager : ISessionManager
{
    private readonly Dictionary<string, PromptSession> _activeSessions = new();
    private string? _activeSessionId;

    public string? ActiveSessionId => _activeSessionId;
    
    public IReadOnlyList<string> ActiveSessionIds => _activeSessions.Keys.ToList();

    public event EventHandler<SessionChangedEventArgs>? ActiveSessionChanged;

    public PromptSession CreateSession(string workspaceId, string name)
    {
        var session = PromptSession.CreateNew(workspaceId, name);
        _activeSessions[session.Id] = session;
        SetActiveSession(session.Id);
        return session;
    }

    public void SetActiveSession(string sessionId)
    {
        if (!_activeSessions.ContainsKey(sessionId))
            throw new InvalidOperationException($"Session {sessionId} not found");

        var oldId = _activeSessionId;
        _activeSessionId = sessionId;
        ActiveSessionChanged?.Invoke(this, new SessionChangedEventArgs(oldId, sessionId));
    }

    public void CloseSession(string sessionId)
    {
        if (!_activeSessions.Remove(sessionId))
            return;

        if (_activeSessionId == sessionId)
        {
            // Switch to another session or null
            _activeSessionId = _activeSessions.Keys.FirstOrDefault();
            ActiveSessionChanged?.Invoke(this, new SessionChangedEventArgs(sessionId, _activeSessionId));
        }
    }
}
```

---

## 6. ViewModel Specification

### 6.1 TabBarViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/TabBarViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for the tab bar control.
/// 
/// MVVM Compliance:
/// - NO business logic
/// - Orchestrates ISessionManager
/// - Exposes properties for binding
/// </summary>
public partial class TabBarViewModel : ObservableObject
{
    private readonly ISessionManager _sessionManager;
    private readonly ISessionRepository _sessionRepository;
    
    [ObservableProperty]
    private ObservableCollection<PromptSessionTabViewModel> _tabs = new();
    
    [ObservableProperty]
    private PromptSessionTabViewModel? _activeTab;
    
    [ObservableProperty]
    private string _workspaceId = string.Empty;

    public TabBarViewModel(ISessionManager sessionManager, ISessionRepository sessionRepository)
    {
        _sessionManager = sessionManager;
        _sessionRepository = sessionRepository;
        
        _sessionManager.ActiveSessionChanged += OnActiveSessionChanged;
    }

    [RelayCommand]
    private async Task CreateTabAsync()
    {
        var tabNumber = Tabs.Count + 1;
        var session = _sessionManager.CreateSession(WorkspaceId, $"Prompt {tabNumber}");
        await _sessionRepository.SaveAsync(session);
        
        var tabVm = new PromptSessionTabViewModel(session);
        Tabs.Add(tabVm);
        ActiveTab = tabVm;
    }

    [RelayCommand]
    private async Task CloseTabAsync(PromptSessionTabViewModel tab)
    {
        if (tab.HasUnsavedChanges)
        {
            // Notify UI to show confirmation dialog
            var confirmed = await ConfirmCloseAsync(tab);
            if (!confirmed) return;
        }
        
        _sessionManager.CloseSession(tab.SessionId);
        await _sessionRepository.DeleteAsync(tab.SessionId);
        Tabs.Remove(tab);
        
        // If no tabs left, create a default one
        if (Tabs.Count == 0)
        {
            await CreateTabAsync();
        }
    }

    [RelayCommand]
    private void SelectTab(PromptSessionTabViewModel tab)
    {
        _sessionManager.SetActiveSession(tab.SessionId);
    }

    [RelayCommand]
    private void DuplicateTab(PromptSessionTabViewModel sourceTab)
    {
        var session = PromptSession.CreateNew(WorkspaceId, $"{sourceTab.Name} (Copy)") with
        {
            SelectedFilePaths = sourceTab.Session.SelectedFilePaths,
            CustomInstructions = sourceTab.Session.CustomInstructions,
            SelectedRoleId = sourceTab.Session.SelectedRoleId
        };
        
        var tabVm = new PromptSessionTabViewModel(session);
        var index = Tabs.IndexOf(sourceTab);
        Tabs.Insert(index + 1, tabVm);
    }

    public async Task LoadSessionsAsync()
    {
        var sessions = await _sessionRepository.GetByWorkspaceAsync(WorkspaceId);
        
        foreach (var session in sessions)
        {
            Tabs.Add(new PromptSessionTabViewModel(session));
        }
        
        if (Tabs.Count == 0)
        {
            await CreateTabAsync();
        }
        else
        {
            ActiveTab = Tabs[0];
            _sessionManager.SetActiveSession(ActiveTab.SessionId);
        }
    }

    private void OnActiveSessionChanged(object? sender, SessionChangedEventArgs e)
    {
        ActiveTab = Tabs.FirstOrDefault(t => t.SessionId == e.NewSessionId);
    }

    private Task<bool> ConfirmCloseAsync(PromptSessionTabViewModel tab)
    {
        // Use messenger to request confirmation from view
        return Task.FromResult(true); // Simplified for spec
    }
}
```

### 6.2 PromptSessionTabViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/PromptSessionTabViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for a single tab representing a prompt session.
/// </summary>
public partial class PromptSessionTabViewModel : ObservableObject
{
    private readonly PromptSession _session;
    
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private bool _hasUnsavedChanges;
    
    [ObservableProperty]
    private bool _isPinned;
    
    [ObservableProperty]
    private int _tokenCount;
    
    [ObservableProperty]
    private int _selectedFileCount;

    public PromptSessionTabViewModel(PromptSession session)
    {
        _session = session;
        _name = session.Name;
        _isPinned = session.IsPinned;
        _selectedFileCount = session.SelectedFilePaths.Count;
    }

    public string SessionId => _session.Id;
    
    public PromptSession Session => _session;

    public string DisplayName => HasUnsavedChanges ? $"● {Name}" : Name;
    
    public string TooltipText => $"{Name}\n{SelectedFileCount} files, {TokenCount:N0} tokens";

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnHasUnsavedChangesChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}
```

---

## 7. View Specification

### 7.1 TabBar.axaml (NEW)

**File:** `src/ShieldPrompt.App/Views/V2/Controls/TabBar.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:ShieldPrompt.App.ViewModels.V2"
             x:DataType="vm:TabBarViewModel"
             x:Class="ShieldPrompt.App.Views.V2.Controls.TabBar">
    
    <UserControl.Styles>
        <Style Selector="ListBoxItem">
            <Setter Property="Padding" Value="12,8"/>
            <Setter Property="MinWidth" Value="100"/>
            <Setter Property="MaxWidth" Value="200"/>
        </Style>
        <Style Selector="ListBoxItem:selected">
            <Setter Property="Background" Value="#313244"/>
        </Style>
    </UserControl.Styles>
    
    <Grid ColumnDefinitions="*,Auto">
        
        <!-- Tab List -->
        <ListBox Grid.Column="0"
                 ItemsSource="{Binding Tabs}"
                 SelectedItem="{Binding ActiveTab}"
                 SelectionMode="Single"
                 Background="Transparent">
            <ListBox.ItemsPanel>
                <ItemsPanelTemplate>
                    <StackPanel Orientation="Horizontal"/>
                </ItemsPanelTemplate>
            </ListBox.ItemsPanel>
            <ListBox.ItemTemplate>
                <DataTemplate DataType="vm:PromptSessionTabViewModel">
                    <Grid ColumnDefinitions="Auto,*,Auto">
                        <!-- Pin indicator -->
                        <TextBlock Grid.Column="0"
                                   Text="📌"
                                   IsVisible="{Binding IsPinned}"
                                   Margin="0,0,6,0"/>
                        
                        <!-- Tab name -->
                        <TextBlock Grid.Column="1"
                                   Text="{Binding DisplayName}"
                                   ToolTip.Tip="{Binding TooltipText}"
                                   TextTrimming="CharacterEllipsis"/>
                        
                        <!-- Close button -->
                        <Button Grid.Column="2"
                                Content="✕"
                                Command="{Binding $parent[ListBox].DataContext.CloseTabCommand}"
                                CommandParameter="{Binding}"
                                Background="Transparent"
                                Padding="4"
                                Margin="8,0,0,0"
                                IsVisible="{Binding !IsPinned}"/>
                    </Grid>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        
        <!-- New Tab Button -->
        <Button Grid.Column="1"
                Content="+"
                Command="{Binding CreateTabCommand}"
                ToolTip.Tip="New Tab (Ctrl+T)"
                Padding="12,8"
                Margin="8,0,0,0"/>
    </Grid>
</UserControl>
```

---

## 8. Test Specifications (TDD)

### 8.1 Domain Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Domain/Entities/PromptSessionTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class PromptSessionTests
{
    [Fact]
    public void CreateNew_GeneratesUniqueId()
    {
        var session1 = PromptSession.CreateNew("ws-1", "Test");
        var session2 = PromptSession.CreateNew("ws-1", "Test");

        session1.Id.Should().NotBe(session2.Id);
    }

    [Fact]
    public void CreateNew_SetsRequiredProperties()
    {
        var session = PromptSession.CreateNew("ws-123", "My Prompt");

        session.WorkspaceId.Should().Be("ws-123");
        session.Name.Should().Be("My Prompt");
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Session_WithSelectedFiles_PersistsCorrectly()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            SelectedFilePaths = new[] { "/file1.cs", "/file2.cs" }
        };

        session.SelectedFilePaths.Should().HaveCount(2);
        session.SelectedFilePaths.Should().Contain("/file1.cs");
    }

    [Fact]
    public void Session_Record_SupportsImmutableUpdate()
    {
        var original = PromptSession.CreateNew("ws-1", "Original");
        var updated = original with { Name = "Updated" };

        original.Name.Should().Be("Original");
        updated.Name.Should().Be("Updated");
        updated.Id.Should().Be(original.Id);
    }
}
```

### 8.2 Service Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Application/Services/SessionManagerTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Application.Services;

public class SessionManagerTests
{
    private readonly SessionManager _sut;

    public SessionManagerTests()
    {
        _sut = new SessionManager();
    }

    [Fact]
    public void CreateSession_AddsToActiveSessions()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.ActiveSessionIds.Should().Contain(session.Id);
    }

    [Fact]
    public void CreateSession_SetsAsActiveSession()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.ActiveSessionId.Should().Be(session.Id);
    }

    [Fact]
    public void SetActiveSession_ChangesActiveSession()
    {
        var session1 = _sut.CreateSession("ws-1", "First");
        var session2 = _sut.CreateSession("ws-1", "Second");

        _sut.SetActiveSession(session1.Id);

        _sut.ActiveSessionId.Should().Be(session1.Id);
    }

    [Fact]
    public void SetActiveSession_WithInvalidId_ThrowsException()
    {
        var act = () => _sut.SetActiveSession("invalid-id");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CloseSession_RemovesFromActiveSessions()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.CloseSession(session.Id);

        _sut.ActiveSessionIds.Should().NotContain(session.Id);
    }

    [Fact]
    public void CloseSession_WhenActive_SwitchesToAnother()
    {
        var session1 = _sut.CreateSession("ws-1", "First");
        var session2 = _sut.CreateSession("ws-1", "Second");
        _sut.SetActiveSession(session2.Id);

        _sut.CloseSession(session2.Id);

        _sut.ActiveSessionId.Should().Be(session1.Id);
    }

    [Fact]
    public void CloseSession_WhenLastSession_ActiveBecomesNull()
    {
        var session = _sut.CreateSession("ws-1", "Test");

        _sut.CloseSession(session.Id);

        _sut.ActiveSessionId.Should().BeNull();
    }

    [Fact]
    public void ActiveSessionChanged_FiresWhenSessionChanges()
    {
        var eventFired = false;
        _sut.ActiveSessionChanged += (s, e) => eventFired = true;

        _sut.CreateSession("ws-1", "Test");

        eventFired.Should().BeTrue();
    }
}
```

### 8.3 ViewModel Tests

**File:** `tests/ShieldPrompt.Tests.Unit/ViewModels/V2/TabBarViewModelTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

public class TabBarViewModelTests
{
    private readonly ISessionManager _sessionManager;
    private readonly ISessionRepository _sessionRepository;
    private readonly TabBarViewModel _sut;

    public TabBarViewModelTests()
    {
        _sessionManager = new SessionManager(); // Use real implementation
        _sessionRepository = Substitute.For<ISessionRepository>();
        _sut = new TabBarViewModel(_sessionManager, _sessionRepository)
        {
            WorkspaceId = "ws-test"
        };
    }

    [Fact]
    public async Task CreateTabAsync_AddsNewTab()
    {
        await _sut.CreateTabCommand.ExecuteAsync(null);

        _sut.Tabs.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateTabAsync_SetsActiveTab()
    {
        await _sut.CreateTabCommand.ExecuteAsync(null);

        _sut.ActiveTab.Should().NotBeNull();
        _sut.ActiveTab.Should().Be(_sut.Tabs[0]);
    }

    [Fact]
    public async Task CreateTabAsync_SavesSession()
    {
        await _sut.CreateTabCommand.ExecuteAsync(null);

        await _sessionRepository.Received(1).SaveAsync(
            Arg.Any<PromptSession>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CloseTabAsync_RemovesTab()
    {
        await _sut.CreateTabCommand.ExecuteAsync(null);
        var tab = _sut.Tabs[0];

        await _sut.CloseTabCommand.ExecuteAsync(tab);

        _sut.Tabs.Should().NotContain(tab);
    }

    [Fact]
    public async Task CloseTabAsync_WhenLastTab_CreatesNewDefault()
    {
        await _sut.CreateTabCommand.ExecuteAsync(null);
        var tab = _sut.Tabs[0];

        await _sut.CloseTabCommand.ExecuteAsync(tab);

        _sut.Tabs.Should().HaveCount(1);
        _sut.Tabs[0].Should().NotBe(tab);
    }

    [Fact]
    public async Task LoadSessionsAsync_RestoresExistingSessions()
    {
        var sessions = new[]
        {
            PromptSession.CreateNew("ws-test", "Session 1"),
            PromptSession.CreateNew("ws-test", "Session 2")
        };
        _sessionRepository.GetByWorkspaceAsync("ws-test", Arg.Any<CancellationToken>())
            .Returns(sessions);

        await _sut.LoadSessionsAsync();

        _sut.Tabs.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadSessionsAsync_WithNoSessions_CreatesDefault()
    {
        _sessionRepository.GetByWorkspaceAsync("ws-test", Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PromptSession>());

        await _sut.LoadSessionsAsync();

        _sut.Tabs.Should().HaveCount(1);
    }

    [Fact]
    public void SelectTab_SetsActiveTab()
    {
        var session1 = _sessionManager.CreateSession("ws-test", "Tab 1");
        var session2 = _sessionManager.CreateSession("ws-test", "Tab 2");
        var tab1 = new PromptSessionTabViewModel(session1);
        var tab2 = new PromptSessionTabViewModel(session2);
        _sut.Tabs.Add(tab1);
        _sut.Tabs.Add(tab2);

        _sut.SelectTabCommand.Execute(tab1);

        _sut.ActiveTab.Should().Be(tab1);
    }
}
```

---

## 9. Implementation Checklist

### 9.1 Domain Layer

- [ ] Create `PromptSession` record
- [ ] Create `SessionState` record
- [ ] Write unit tests for domain models
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~PromptSessionTests"`

### 9.2 Application Layer

- [ ] Create `ISessionRepository` interface
- [ ] Create `ISessionManager` interface
- [ ] Create `ISessionStateService` interface
- [ ] Implement `SessionManager`
- [ ] Implement `SessionStateService`
- [ ] Write unit tests (TDD)
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~SessionManager"`

### 9.3 Infrastructure Layer

- [ ] Implement `JsonSessionRepository`
- [ ] Write integration tests

### 9.4 Presentation Layer

- [ ] Create `TabBarViewModel`
- [ ] Create `PromptSessionTabViewModel`
- [ ] Create `TabBar.axaml`
- [ ] Integrate into `MainWindowV2`
- [ ] Write ViewModel tests (TDD)
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~TabBarViewModel"`

### 9.5 Integration

- [ ] Register services in DI
- [ ] Wire tab bar to main window
- [ ] Implement keyboard shortcuts (Ctrl+T, Ctrl+W)
- [ ] End-to-end testing

---

## 10. Acceptance Criteria

| Criterion | Verification |
|-----------|--------------|
| Can create new tabs | Manual test |
| Can switch between tabs | Manual test |
| Tab state persists on switch | Manual test |
| Tabs restore on app restart | Integration test |
| Close tab shows warning if unsaved | Manual test |
| Ctrl+T creates new tab | Manual test |
| All unit tests pass | `dotnet test` |

---

## 11. UI/UX Requirements

### 11.1 Tab Appearance

- Tab shows name (truncated if long)
- Dirty indicator (●) for unsaved changes
- Pin indicator (📌) for pinned tabs
- Close button (✕) except for pinned tabs
- Hover shows tooltip with details

### 11.2 Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+T | New tab |
| Ctrl+W | Close current tab |
| Ctrl+Tab | Next tab |
| Ctrl+Shift+Tab | Previous tab |
| Ctrl+1-9 | Switch to tab N |

---

## 12. Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| Memory usage with many tabs | Virtualize inactive tab content |
| Session file corruption | Validate JSON, auto-recover |
| Lost work on crash | Auto-save sessions periodically |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial specification |

