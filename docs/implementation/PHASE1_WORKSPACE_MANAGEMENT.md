# Phase 1: Workspace Management Implementation Specification

**Phase ID:** PHASE-1  
**Priority:** P0 (Foundation)  
**Estimated Effort:** 3-4 days  
**Prerequisites:** None  
**Status:** PENDING

---

## 1. Executive Summary

Workspace Management provides the foundation for multi-workspace support, recent workspace tracking, and workspace-specific settings. This phase establishes the data structures, repositories, and UI components needed for all subsequent phases.

---

## 2. Architectural Principles

### 2.1 TDD Requirements

```
RED → GREEN → REFACTOR

1. Write failing test FIRST
2. Implement minimum code to pass
3. Refactor while keeping tests green
4. Coverage target: 90%+
```

### 2.2 ISP Requirements

```
Maximum 5 methods per interface
Each interface has single responsibility
Clients depend only on methods they use
```

### 2.3 MVVM Separation

```
┌─────────────────────────────────────────────────────────────┐
│                        VIEW (XAML)                          │
│  - ONLY data binding, no logic                              │
│  - ONLY converters for display transformation               │
│  - ONLY event routing to commands                           │
└──────────────────────────┬──────────────────────────────────┘
                           │ Binds to
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                     VIEWMODEL                               │
│  - Exposes ObservableProperties                             │
│  - Exposes Commands (RelayCommand)                          │
│  - Orchestrates services                                    │
│  - NO business logic                                        │
└──────────────────────────┬──────────────────────────────────┘
                           │ Calls
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              APPLICATION LAYER (Services)                   │
│  - ALL business logic lives here                            │
│  - Interfaces define contracts                              │
│  - Implementations are injected                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Domain Model Specification

### 3.1 Workspace Entity (EXISTS - ENHANCE)

**File:** `src/ShieldPrompt.Domain/Entities/Workspace.cs`

```csharp
namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents a workspace (project folder) with associated settings and state.
/// </summary>
public record Workspace
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public DateTime LastOpened { get; init; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    // Per-workspace settings
    public string? PreferredRole { get; init; }
    public string? PreferredModel { get; init; }
    public IReadOnlyList<string> RecentFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> CustomSettings { get; init; } 
        = new Dictionary<string, string>();
}
```

### 3.2 WorkspaceSettings Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/WorkspaceSettings.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Workspace-specific settings that can be persisted.
/// </summary>
public record WorkspaceSettings(
    string WorkspaceId,
    string? PreferredRole,
    string? PreferredModel,
    bool SanitizationEnabled = true,
    bool IncludeLineNumbers = false,
    IReadOnlyList<string>? IgnoredPaths = null);
```

---

## 4. Interface Specifications (ISP-Compliant)

### 4.1 IWorkspaceRepository (EXISTS - VERIFY)

**File:** `src/ShieldPrompt.Application/Interfaces/IWorkspaceRepository.cs`

**ISP Compliance:** 5 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for workspace persistence. Follows ISP - workspace CRUD only.
/// </summary>
public interface IWorkspaceRepository
{
    /// <summary>
    /// Gets all workspaces ordered by last opened.
    /// </summary>
    Task<IReadOnlyList<Workspace>> GetAllAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets a workspace by ID.
    /// </summary>
    Task<Workspace?> GetByIdAsync(string id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a workspace by root path.
    /// </summary>
    Task<Workspace?> GetByPathAsync(string rootPath, CancellationToken ct = default);
    
    /// <summary>
    /// Saves or updates a workspace.
    /// </summary>
    Task SaveAsync(Workspace workspace, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a workspace by ID.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken ct = default);
}
```

### 4.2 IRecentWorkspaceService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IRecentWorkspaceService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing recent workspace history. Follows ISP - history only.
/// </summary>
public interface IRecentWorkspaceService
{
    /// <summary>
    /// Gets the N most recently opened workspaces.
    /// </summary>
    Task<IReadOnlyList<Workspace>> GetRecentAsync(int count = 10, CancellationToken ct = default);
    
    /// <summary>
    /// Records a workspace as recently opened (updates timestamp).
    /// </summary>
    Task RecordOpenedAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Clears recent workspace history.
    /// </summary>
    Task ClearHistoryAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Removes a workspace from recent history.
    /// </summary>
    Task RemoveFromHistoryAsync(string workspaceId, CancellationToken ct = default);
}
```

### 4.3 IWorkspaceSettingsService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IWorkspaceSettingsService.cs`

**ISP Compliance:** 3 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for workspace-specific settings. Follows ISP - settings only.
/// </summary>
public interface IWorkspaceSettingsService
{
    /// <summary>
    /// Gets settings for a workspace.
    /// </summary>
    Task<WorkspaceSettings> GetSettingsAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Saves settings for a workspace.
    /// </summary>
    Task SaveSettingsAsync(WorkspaceSettings settings, CancellationToken ct = default);
    
    /// <summary>
    /// Resets settings for a workspace to defaults.
    /// </summary>
    Task ResetSettingsAsync(string workspaceId, CancellationToken ct = default);
}
```

---

## 5. Service Implementations

### 5.1 RecentWorkspaceService

**File:** `src/ShieldPrompt.Application/Services/RecentWorkspaceService.cs`

```csharp
namespace ShieldPrompt.Application.Services;

public class RecentWorkspaceService(IWorkspaceRepository repository) : IRecentWorkspaceService
{
    public async Task<IReadOnlyList<Workspace>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        var all = await repository.GetAllAsync(ct);
        return all.Take(count).ToList();
    }

    public async Task RecordOpenedAsync(string workspaceId, CancellationToken ct = default)
    {
        var workspace = await repository.GetByIdAsync(workspaceId, ct);
        if (workspace is null) return;
        
        await repository.SaveAsync(workspace with { LastOpened = DateTime.UtcNow }, ct);
    }

    public async Task ClearHistoryAsync(CancellationToken ct = default)
    {
        var all = await repository.GetAllAsync(ct);
        foreach (var ws in all)
        {
            await repository.DeleteAsync(ws.Id, ct);
        }
    }

    public async Task RemoveFromHistoryAsync(string workspaceId, CancellationToken ct = default)
    {
        await repository.DeleteAsync(workspaceId, ct);
    }
}
```

---

## 6. ViewModel Specification

### 6.1 WorkspaceSelectorViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/WorkspaceSelectorViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for workspace selector dropdown.
/// 
/// MVVM Compliance:
/// - NO business logic (delegated to services)
/// - ONLY property exposure and command orchestration
/// </summary>
public partial class WorkspaceSelectorViewModel : ObservableObject
{
    private readonly IRecentWorkspaceService _recentService;
    private readonly IWorkspaceRepository _repository;
    
    [ObservableProperty]
    private ObservableCollection<Workspace> _recentWorkspaces = new();
    
    [ObservableProperty]
    private Workspace? _selectedWorkspace;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;
    
    public WorkspaceSelectorViewModel(
        IRecentWorkspaceService recentService,
        IWorkspaceRepository repository)
    {
        _recentService = recentService;
        _repository = repository;
    }
    
    [RelayCommand]
    private async Task LoadRecentAsync()
    {
        IsLoading = true;
        try
        {
            var recent = await _recentService.GetRecentAsync(10);
            RecentWorkspaces = new ObservableCollection<Workspace>(recent);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task SelectWorkspaceAsync(Workspace workspace)
    {
        SelectedWorkspace = workspace;
        await _recentService.RecordOpenedAsync(workspace.Id);
        
        // Notify parent ViewModel via messenger or event
        WeakReferenceMessenger.Default.Send(new WorkspaceSelectedMessage(workspace));
    }
    
    [RelayCommand]
    private async Task RemoveFromRecentAsync(Workspace workspace)
    {
        await _recentService.RemoveFromHistoryAsync(workspace.Id);
        RecentWorkspaces.Remove(workspace);
    }
}

// Message for cross-ViewModel communication
public record WorkspaceSelectedMessage(Workspace Workspace);
```

---

## 7. View Specification

### 7.1 WorkspaceSelector.axaml (NEW)

**File:** `src/ShieldPrompt.App/Views/V2/Controls/WorkspaceSelector.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:ShieldPrompt.App.ViewModels.V2"
             x:DataType="vm:WorkspaceSelectorViewModel"
             x:Class="ShieldPrompt.App.Views.V2.Controls.WorkspaceSelector">
    
    <!-- 
    VIEW RESPONSIBILITIES:
    - Data binding ONLY
    - No code-behind logic
    - Commands routed to ViewModel
    -->
    
    <Grid ColumnDefinitions="*,Auto">
        <ComboBox ItemsSource="{Binding RecentWorkspaces}"
                  SelectedItem="{Binding SelectedWorkspace}"
                  MinWidth="200"
                  PlaceholderText="Select workspace...">
            <ComboBox.ItemTemplate>
                <DataTemplate DataType="entities:Workspace">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="📂"/>
                        <TextBlock Text="{Binding Name}"/>
                        <TextBlock Text="{Binding RootPath}" 
                                   Foreground="#6c7086"
                                   FontSize="11"/>
                    </StackPanel>
                </DataTemplate>
            </ComboBox.ItemTemplate>
        </ComboBox>
        
        <Button Grid.Column="1"
                Command="{Binding RemoveFromRecentCommand}"
                CommandParameter="{Binding SelectedWorkspace}"
                ToolTip.Tip="Remove from recent"
                IsVisible="{Binding SelectedWorkspace, Converter={x:Static ObjectConverters.IsNotNull}}">
            <TextBlock Text="✕"/>
        </Button>
    </Grid>
</UserControl>
```

---

## 8. Test Specifications (TDD)

### 8.1 Domain Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Domain/Entities/WorkspaceTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Domain.Entities;

public class WorkspaceTests
{
    [Fact]
    public void Workspace_WithRequiredProperties_CreatesSuccessfully()
    {
        // Arrange & Act
        var workspace = new Workspace
        {
            Id = "ws-123",
            Name = "MyProject",
            RootPath = "/path/to/project"
        };

        // Assert
        workspace.Id.Should().Be("ws-123");
        workspace.Name.Should().Be("MyProject");
        workspace.RootPath.Should().Be("/path/to/project");
        workspace.LastOpened.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Workspace_WithPreferredRole_PersistsCorrectly()
    {
        var workspace = new Workspace
        {
            Id = "ws-123",
            Name = "Test",
            RootPath = "/test",
            PreferredRole = "security_expert"
        };

        workspace.PreferredRole.Should().Be("security_expert");
    }

    [Fact]
    public void Workspace_WithRecord_SupportsImmutableUpdate()
    {
        var original = new Workspace
        {
            Id = "ws-123",
            Name = "Original",
            RootPath = "/test"
        };

        var updated = original with { Name = "Updated" };

        original.Name.Should().Be("Original");
        updated.Name.Should().Be("Updated");
        updated.Id.Should().Be(original.Id);
    }
}
```

### 8.2 Service Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Application/Services/RecentWorkspaceServiceTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Application.Services;

public class RecentWorkspaceServiceTests
{
    private readonly IWorkspaceRepository _repository;
    private readonly RecentWorkspaceService _sut;

    public RecentWorkspaceServiceTests()
    {
        _repository = Substitute.For<IWorkspaceRepository>();
        _sut = new RecentWorkspaceService(_repository);
    }

    [Fact]
    public async Task GetRecentAsync_WithWorkspaces_ReturnsOrderedByLastOpened()
    {
        // Arrange
        var workspaces = new[]
        {
            new Workspace { Id = "1", Name = "Old", RootPath = "/old", LastOpened = DateTime.UtcNow.AddDays(-2) },
            new Workspace { Id = "2", Name = "New", RootPath = "/new", LastOpened = DateTime.UtcNow }
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(workspaces.OrderByDescending(w => w.LastOpened).ToList());

        // Act
        var result = await _sut.GetRecentAsync(10);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("New"); // Most recent first
    }

    [Fact]
    public async Task GetRecentAsync_WithCountLimit_ReturnsLimitedResults()
    {
        // Arrange
        var workspaces = Enumerable.Range(1, 20)
            .Select(i => new Workspace { Id = i.ToString(), Name = $"WS{i}", RootPath = $"/ws{i}" })
            .ToList();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(workspaces);

        // Act
        var result = await _sut.GetRecentAsync(5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task RecordOpenedAsync_UpdatesLastOpenedTimestamp()
    {
        // Arrange
        var workspace = new Workspace { Id = "123", Name = "Test", RootPath = "/test", LastOpened = DateTime.UtcNow.AddDays(-1) };
        _repository.GetByIdAsync("123", Arg.Any<CancellationToken>()).Returns(workspace);

        // Act
        await _sut.RecordOpenedAsync("123");

        // Assert
        await _repository.Received(1).SaveAsync(
            Arg.Is<Workspace>(w => w.Id == "123" && w.LastOpened > workspace.LastOpened),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFromHistoryAsync_DeletesWorkspace()
    {
        // Act
        await _sut.RemoveFromHistoryAsync("123");

        // Assert
        await _repository.Received(1).DeleteAsync("123", Arg.Any<CancellationToken>());
    }
}
```

### 8.3 ViewModel Tests

**File:** `tests/ShieldPrompt.Tests.Unit/ViewModels/V2/WorkspaceSelectorViewModelTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

public class WorkspaceSelectorViewModelTests
{
    private readonly IRecentWorkspaceService _recentService;
    private readonly IWorkspaceRepository _repository;
    private readonly WorkspaceSelectorViewModel _sut;

    public WorkspaceSelectorViewModelTests()
    {
        _recentService = Substitute.For<IRecentWorkspaceService>();
        _repository = Substitute.For<IWorkspaceRepository>();
        _sut = new WorkspaceSelectorViewModel(_recentService, _repository);
    }

    [Fact]
    public async Task LoadRecentAsync_PopulatesRecentWorkspaces()
    {
        // Arrange
        var workspaces = new[]
        {
            new Workspace { Id = "1", Name = "Project1", RootPath = "/p1" },
            new Workspace { Id = "2", Name = "Project2", RootPath = "/p2" }
        };
        _recentService.GetRecentAsync(10, Arg.Any<CancellationToken>()).Returns(workspaces);

        // Act
        await _sut.LoadRecentCommand.ExecuteAsync(null);

        // Assert
        _sut.RecentWorkspaces.Should().HaveCount(2);
        _sut.RecentWorkspaces[0].Name.Should().Be("Project1");
    }

    [Fact]
    public async Task LoadRecentAsync_SetsIsLoadingDuringOperation()
    {
        // Arrange
        var loadingStates = new List<bool>();
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_sut.IsLoading))
                loadingStates.Add(_sut.IsLoading);
        };
        _recentService.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Workspace>());

        // Act
        await _sut.LoadRecentCommand.ExecuteAsync(null);

        // Assert
        loadingStates.Should().Contain(true); // Was true at some point
        _sut.IsLoading.Should().BeFalse(); // Ends as false
    }

    [Fact]
    public async Task SelectWorkspaceAsync_RecordsInRecentHistory()
    {
        // Arrange
        var workspace = new Workspace { Id = "123", Name = "Test", RootPath = "/test" };

        // Act
        await _sut.SelectWorkspaceCommand.ExecuteAsync(workspace);

        // Assert
        _sut.SelectedWorkspace.Should().Be(workspace);
        await _recentService.Received(1).RecordOpenedAsync("123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFromRecentAsync_RemovesFromListAndService()
    {
        // Arrange
        var workspace = new Workspace { Id = "123", Name = "Test", RootPath = "/test" };
        _sut.RecentWorkspaces.Add(workspace);

        // Act
        await _sut.RemoveFromRecentCommand.ExecuteAsync(workspace);

        // Assert
        _sut.RecentWorkspaces.Should().NotContain(workspace);
        await _recentService.Received(1).RemoveFromHistoryAsync("123", Arg.Any<CancellationToken>());
    }
}
```

---

## 9. Implementation Checklist

### 9.1 Domain Layer

- [ ] Verify `Workspace` record has all required properties
- [ ] Add `WorkspaceSettings` record if needed
- [ ] Write unit tests for domain models
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~WorkspaceTests"`

### 9.2 Application Layer

- [ ] Create `IRecentWorkspaceService` interface
- [ ] Create `IWorkspaceSettingsService` interface
- [ ] Implement `RecentWorkspaceService`
- [ ] Implement `WorkspaceSettingsService`
- [ ] Write unit tests for services (TDD)
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~RecentWorkspaceService"`

### 9.3 Infrastructure Layer

- [ ] Verify `JsonWorkspaceRepository` implementation
- [ ] Add workspace settings file handling if needed
- [ ] Write integration tests

### 9.4 Presentation Layer

- [ ] Create `WorkspaceSelectorViewModel`
- [ ] Create `WorkspaceSelector.axaml`
- [ ] Write ViewModel unit tests (TDD)
- [ ] Integrate into `MainWindowV2`
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~WorkspaceSelectorViewModel"`

### 9.5 DI Registration

- [ ] Register new services in `App.axaml.cs`
- [ ] Verify injection works correctly

---

## 10. Acceptance Criteria

| Criterion | Verification |
|-----------|--------------|
| Recent workspaces display in dropdown | Manual test |
| Selecting workspace updates UI | Manual test |
| Workspace removed from recent when deleted | Unit test |
| Settings persist across sessions | Integration test |
| All unit tests pass | `dotnet test` |
| Code coverage ≥ 90% | Coverage report |

---

## 11. Dependencies

| Dependency | Status |
|------------|--------|
| `Workspace` domain entity | ✅ Exists |
| `IWorkspaceRepository` | ✅ Exists |
| `JsonWorkspaceRepository` | ✅ Exists |
| `MainWindowV2ViewModel` | ✅ Exists |

---

## 12. Advanced Search Feature (Added)

### 12.1 Overview

Advanced Search provides powerful file filtering beyond simple name matching, including regex patterns, content search, and git status filters.

### 12.2 Interface: IFileSearchService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IFileSearchService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for advanced file searching.
/// Follows ISP - search operations only.
/// </summary>
public interface IFileSearchService
{
    /// <summary>
    /// Searches files by name pattern (glob or regex).
    /// </summary>
    IEnumerable<FileNode> SearchByPattern(FileNode root, string pattern, bool isRegex = false);
    
    /// <summary>
    /// Searches files by content.
    /// </summary>
    Task<IEnumerable<FileNode>> SearchByContentAsync(FileNode root, string query, CancellationToken ct = default);
    
    /// <summary>
    /// Filters files by extension.
    /// </summary>
    IEnumerable<FileNode> FilterByExtension(FileNode root, IEnumerable<string> extensions);
    
    /// <summary>
    /// Filters files by git status.
    /// </summary>
    IEnumerable<FileNode> FilterByGitStatus(FileNode root, GitFileStatus status);
}
```

### 12.3 Search Options

| Filter Type | Description | Example |
|-------------|-------------|---------|
| Glob Pattern | Simple wildcard matching | `**/*.cs`, `src/*.ts` |
| Regex Pattern | Regular expression matching | `.*Controller\.cs$` |
| Content Search | Search within file contents | `TODO:` |
| Extension Filter | Filter by file extension | `.cs`, `.ts`, `.py` |
| Git Status Filter | Filter by git state | Modified, Staged, Untracked |

### 12.4 Test Specification

```csharp
public class FileSearchServiceTests
{
    [Fact]
    public void SearchByPattern_WithGlob_FindsMatchingFiles() { }
    
    [Fact]
    public void SearchByPattern_WithRegex_FindsMatchingFiles() { }
    
    [Fact]
    public async Task SearchByContentAsync_WithQuery_FindsFilesContainingText() { }
    
    [Fact]
    public void FilterByExtension_WithMultipleExtensions_FiltersCorrectly() { }
    
    [Fact]
    public void FilterByGitStatus_WithModified_ReturnsOnlyModifiedFiles() { }
}
```

### 12.5 Implementation Checklist

- [ ] Create `IFileSearchService` interface
- [ ] Implement `FileSearchService`
- [ ] Add search UI to file tree panel
- [ ] Add filter dropdown for extensions
- [ ] Add git status filter dropdown
- [ ] Write unit tests (TDD)

---

## 13. Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| Workspace file corruption | Validate JSON on load, graceful degradation |
| Large number of workspaces | Pagination/virtualization in UI |
| Concurrent access | File locking in repository |
| Content search performance | Limit to selected files, add timeout |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial specification |

