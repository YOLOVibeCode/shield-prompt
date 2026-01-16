# Phase 5: Stored Prompts/Presets Implementation Specification

**Phase ID:** PHASE-5  
**Priority:** P1 (High Value Feature)  
**Estimated Effort:** 2-3 days  
**Prerequisites:** Phase 1, Phase 2  
**Status:** PENDING

---

## 1. Executive Summary

Stored Prompts/Presets allow users to save and reuse prompt configurations including file selections, roles, custom instructions, and model preferences. This improves workflow efficiency for repetitive analysis tasks.

---

## 2. Feature Requirements

### 2.1 Core Features

| Feature | Description | Priority |
|---------|-------------|----------|
| Save Preset | Save current prompt config as preset | P0 |
| Load Preset | Apply a saved preset | P0 |
| Preset Library | Browse and manage presets | P0 |
| Quick Access | Pin frequently used presets | P1 |
| Share Presets | Export/import presets | P2 |
| Preset Templates | Built-in starter presets | P1 |

### 2.2 Preset Contents

A preset captures:
- Selected files (by path pattern or explicit list)
- Custom instructions text
- Selected role ID
- Selected model ID
- Output format preferences
- Name and description

---

## 3. Domain Model Specification

### 3.1 PromptPreset Entity (NEW)

**File:** `src/ShieldPrompt.Domain/Entities/PromptPreset.cs`

```csharp
namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// A saved prompt configuration that can be reused.
/// </summary>
public record PromptPreset
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = "📋";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastUsed { get; init; } = DateTime.UtcNow;
    public int UsageCount { get; init; }
    
    // Scope
    public PresetScope Scope { get; init; } = PresetScope.Workspace;
    public string? WorkspaceId { get; init; }
    
    // Captured settings
    public IReadOnlyList<string> FilePatterns { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ExplicitFilePaths { get; init; } = Array.Empty<string>();
    public string CustomInstructions { get; init; } = string.Empty;
    public string? RoleId { get; init; }
    public string? ModelId { get; init; }
    public bool IncludeLineNumbers { get; init; }
    
    // UI state
    public bool IsPinned { get; init; }
    public bool IsBuiltIn { get; init; }
    
    /// <summary>
    /// Creates a new preset from current state.
    /// </summary>
    public static PromptPreset Create(string name, string? workspaceId = null)
    {
        return new PromptPreset
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            WorkspaceId = workspaceId,
            Scope = workspaceId is null ? PresetScope.Global : PresetScope.Workspace
        };
    }
}

public enum PresetScope
{
    Global,     // Available in all workspaces
    Workspace   // Specific to one workspace
}
```

### 3.2 PresetApplication Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/PresetApplication.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Result of applying a preset to current state.
/// </summary>
public record PresetApplication(
    int FilesSelected,
    int FilesNotFound,
    IReadOnlyList<string> Warnings);
```

---

## 4. Interface Specifications (ISP-Compliant)

### 4.1 IPresetRepository (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IPresetRepository.cs`

**ISP Compliance:** 5 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for preset persistence.
/// Follows ISP - preset CRUD only.
/// </summary>
public interface IPresetRepository
{
    /// <summary>
    /// Gets all global presets.
    /// </summary>
    Task<IReadOnlyList<PromptPreset>> GetGlobalPresetsAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets presets for a workspace.
    /// </summary>
    Task<IReadOnlyList<PromptPreset>> GetWorkspacePresetsAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a preset by ID.
    /// </summary>
    Task<PromptPreset?> GetByIdAsync(string presetId, CancellationToken ct = default);
    
    /// <summary>
    /// Saves or updates a preset.
    /// </summary>
    Task SaveAsync(PromptPreset preset, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a preset.
    /// </summary>
    Task DeleteAsync(string presetId, CancellationToken ct = default);
}
```

### 4.2 IPresetService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IPresetService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for applying and managing presets.
/// Follows ISP - preset operations only.
/// </summary>
public interface IPresetService
{
    /// <summary>
    /// Creates a preset from current session state.
    /// </summary>
    PromptPreset CreateFromSession(PromptSession session, string name);
    
    /// <summary>
    /// Applies a preset to a session.
    /// </summary>
    Task<PresetApplication> ApplyToSessionAsync(
        PromptPreset preset, 
        PromptSession session, 
        FileNode rootNode,
        CancellationToken ct = default);
    
    /// <summary>
    /// Records that a preset was used.
    /// </summary>
    Task RecordUsageAsync(string presetId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets pinned presets for quick access.
    /// </summary>
    Task<IReadOnlyList<PromptPreset>> GetPinnedPresetsAsync(string? workspaceId, CancellationToken ct = default);
}
```

### 4.3 IPresetExportService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IPresetExportService.cs`

**ISP Compliance:** 2 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for exporting and importing presets.
/// Follows ISP - export/import only.
/// </summary>
public interface IPresetExportService
{
    /// <summary>
    /// Exports a preset to JSON.
    /// </summary>
    string ExportToJson(PromptPreset preset);
    
    /// <summary>
    /// Imports a preset from JSON.
    /// </summary>
    PromptPreset? ImportFromJson(string json);
}
```

---

## 5. Service Implementations

### 5.1 PresetService

**File:** `src/ShieldPrompt.Application/Services/PresetService.cs`

```csharp
namespace ShieldPrompt.Application.Services;

public class PresetService(IPresetRepository repository) : IPresetService
{
    public PromptPreset CreateFromSession(PromptSession session, string name)
    {
        return PromptPreset.Create(name, session.WorkspaceId) with
        {
            ExplicitFilePaths = session.SelectedFilePaths.ToList(),
            CustomInstructions = session.CustomInstructions,
            RoleId = session.SelectedRoleId,
            ModelId = session.SelectedModelId
        };
    }

    public async Task<PresetApplication> ApplyToSessionAsync(
        PromptPreset preset,
        PromptSession session,
        FileNode rootNode,
        CancellationToken ct = default)
    {
        var warnings = new List<string>();
        var selectedPaths = new List<string>();
        var notFound = 0;

        // Apply explicit file paths
        foreach (var path in preset.ExplicitFilePaths)
        {
            if (FindFileNode(rootNode, path) is { } node)
            {
                node.IsSelected = true;
                selectedPaths.Add(path);
            }
            else
            {
                notFound++;
                warnings.Add($"File not found: {path}");
            }
        }

        // Apply file patterns
        foreach (var pattern in preset.FilePatterns)
        {
            var matches = FindMatchingFiles(rootNode, pattern);
            foreach (var match in matches)
            {
                match.IsSelected = true;
                selectedPaths.Add(match.Path);
            }
        }

        return new PresetApplication(
            FilesSelected: selectedPaths.Distinct().Count(),
            FilesNotFound: notFound,
            Warnings: warnings);
    }

    public async Task RecordUsageAsync(string presetId, CancellationToken ct = default)
    {
        var preset = await repository.GetByIdAsync(presetId, ct);
        if (preset is null) return;

        await repository.SaveAsync(preset with
        {
            LastUsed = DateTime.UtcNow,
            UsageCount = preset.UsageCount + 1
        }, ct);
    }

    public async Task<IReadOnlyList<PromptPreset>> GetPinnedPresetsAsync(
        string? workspaceId, 
        CancellationToken ct = default)
    {
        var global = await repository.GetGlobalPresetsAsync(ct);
        var pinned = global.Where(p => p.IsPinned).ToList();

        if (workspaceId is not null)
        {
            var workspace = await repository.GetWorkspacePresetsAsync(workspaceId, ct);
            pinned.AddRange(workspace.Where(p => p.IsPinned));
        }

        return pinned.OrderByDescending(p => p.UsageCount).ToList();
    }

    private static FileNode? FindFileNode(FileNode root, string path)
    {
        if (root.Path == path) return root;
        foreach (var child in root.Children)
        {
            if (FindFileNode(child, path) is { } found)
                return found;
        }
        return null;
    }

    private static IEnumerable<FileNode> FindMatchingFiles(FileNode root, string pattern)
    {
        // Simple glob pattern matching
        var regex = GlobToRegex(pattern);
        return GetAllFiles(root).Where(f => regex.IsMatch(f.Path));
    }

    private static IEnumerable<FileNode> GetAllFiles(FileNode node)
    {
        if (!node.IsDirectory) yield return node;
        foreach (var child in node.Children)
            foreach (var file in GetAllFiles(child))
                yield return file;
    }

    private static Regex GlobToRegex(string glob)
    {
        var pattern = "^" + Regex.Escape(glob)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", ".") + "$";
        return new Regex(pattern, RegexOptions.IgnoreCase);
    }
}
```

---

## 6. ViewModel Specification

### 6.1 PresetLibraryViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/PresetLibraryViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for preset library panel.
/// </summary>
public partial class PresetLibraryViewModel : ObservableObject
{
    private readonly IPresetRepository _repository;
    private readonly IPresetService _presetService;
    
    [ObservableProperty]
    private ObservableCollection<PromptPreset> _presets = new();
    
    [ObservableProperty]
    private ObservableCollection<PromptPreset> _pinnedPresets = new();
    
    [ObservableProperty]
    private PromptPreset? _selectedPreset;
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;
    
    [ObservableProperty]
    private string _workspaceId = string.Empty;
    
    [ObservableProperty]
    private bool _showGlobalOnly;

    public PresetLibraryViewModel(IPresetRepository repository, IPresetService presetService)
    {
        _repository = repository;
        _presetService = presetService;
    }

    [RelayCommand]
    private async Task LoadPresetsAsync()
    {
        var global = await _repository.GetGlobalPresetsAsync();
        var workspace = string.IsNullOrEmpty(WorkspaceId) 
            ? Array.Empty<PromptPreset>() 
            : await _repository.GetWorkspacePresetsAsync(WorkspaceId);
        
        Presets = new ObservableCollection<PromptPreset>(
            global.Concat(workspace).OrderByDescending(p => p.LastUsed));
        
        PinnedPresets = new ObservableCollection<PromptPreset>(
            Presets.Where(p => p.IsPinned));
    }

    [RelayCommand]
    private async Task ApplyPresetAsync(PromptPreset preset)
    {
        // Notify parent to apply preset
        WeakReferenceMessenger.Default.Send(new ApplyPresetMessage(preset));
        await _presetService.RecordUsageAsync(preset.Id);
    }

    [RelayCommand]
    private async Task DeletePresetAsync(PromptPreset preset)
    {
        if (preset.IsBuiltIn)
        {
            // Cannot delete built-in presets
            return;
        }
        
        await _repository.DeleteAsync(preset.Id);
        Presets.Remove(preset);
        PinnedPresets.Remove(preset);
    }

    [RelayCommand]
    private async Task TogglePinAsync(PromptPreset preset)
    {
        var updated = preset with { IsPinned = !preset.IsPinned };
        await _repository.SaveAsync(updated);
        
        var index = Presets.IndexOf(preset);
        if (index >= 0) Presets[index] = updated;
        
        if (updated.IsPinned)
            PinnedPresets.Add(updated);
        else
            PinnedPresets.Remove(preset);
    }

    [RelayCommand]
    private async Task DuplicatePresetAsync(PromptPreset preset)
    {
        var duplicate = preset with
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{preset.Name} (Copy)",
            IsBuiltIn = false,
            CreatedAt = DateTime.UtcNow,
            UsageCount = 0
        };
        
        await _repository.SaveAsync(duplicate);
        Presets.Add(duplicate);
    }

    partial void OnSearchQueryChanged(string value)
    {
        // Filter presets by search query
        // Implementation would filter the collection
    }
}

public record ApplyPresetMessage(PromptPreset Preset);
```

### 6.2 SavePresetDialogViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/SavePresetDialogViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for save preset dialog.
/// </summary>
public partial class SavePresetDialogViewModel : ObservableObject
{
    private readonly IPresetRepository _repository;
    private readonly IPresetService _presetService;
    
    [ObservableProperty]
    private string _presetName = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private string _icon = "📋";
    
    [ObservableProperty]
    private PresetScope _scope = PresetScope.Workspace;
    
    [ObservableProperty]
    private bool _includeFilePaths = true;
    
    [ObservableProperty]
    private bool _includeCustomInstructions = true;
    
    [ObservableProperty]
    private bool _includeRole = true;
    
    [ObservableProperty]
    private string? _validationError;

    public bool CanSave => !string.IsNullOrWhiteSpace(PresetName) && ValidationError is null;

    public SavePresetDialogViewModel(IPresetRepository repository, IPresetService presetService)
    {
        _repository = repository;
        _presetService = presetService;
    }

    public async Task<PromptPreset?> SaveAsync(PromptSession session)
    {
        if (!CanSave) return null;

        var preset = _presetService.CreateFromSession(session, PresetName) with
        {
            Description = Description,
            Icon = Icon,
            Scope = Scope,
            ExplicitFilePaths = IncludeFilePaths ? session.SelectedFilePaths : Array.Empty<string>(),
            CustomInstructions = IncludeCustomInstructions ? session.CustomInstructions : string.Empty,
            RoleId = IncludeRole ? session.SelectedRoleId : null
        };

        await _repository.SaveAsync(preset);
        return preset;
    }

    partial void OnPresetNameChanged(string value)
    {
        ValidationError = string.IsNullOrWhiteSpace(value) ? "Name is required" : null;
        OnPropertyChanged(nameof(CanSave));
    }
}
```

---

## 7. View Specification

### 7.1 PresetLibrary.axaml (NEW)

**File:** `src/ShieldPrompt.App/Views/V2/Controls/PresetLibrary.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:ShieldPrompt.App.ViewModels.V2"
             x:DataType="vm:PresetLibraryViewModel"
             x:Class="ShieldPrompt.App.Views.V2.Controls.PresetLibrary">
    
    <Grid RowDefinitions="Auto,Auto,*">
        
        <!-- Search -->
        <TextBox Grid.Row="0"
                 Text="{Binding SearchQuery}"
                 Watermark="🔍 Search presets..."
                 Margin="0,0,0,12"/>
        
        <!-- Pinned Presets -->
        <ItemsControl Grid.Row="1"
                      ItemsSource="{Binding PinnedPresets}"
                      IsVisible="{Binding PinnedPresets.Count}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate DataType="entities:PromptPreset">
                    <Button Command="{Binding $parent[ItemsControl].DataContext.ApplyPresetCommand}"
                            CommandParameter="{Binding}"
                            Margin="0,0,8,8"
                            Padding="8,6">
                        <StackPanel Orientation="Horizontal" Spacing="6">
                            <TextBlock Text="{Binding Icon}"/>
                            <TextBlock Text="{Binding Name}"/>
                        </StackPanel>
                    </Button>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
        
        <!-- All Presets -->
        <ListBox Grid.Row="2"
                 ItemsSource="{Binding Presets}"
                 SelectedItem="{Binding SelectedPreset}"
                 Background="Transparent">
            <ListBox.ItemTemplate>
                <DataTemplate DataType="entities:PromptPreset">
                    <Grid ColumnDefinitions="Auto,*,Auto">
                        <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8">
                            <TextBlock Text="{Binding Icon}"/>
                            <StackPanel>
                                <TextBlock Text="{Binding Name}" FontWeight="SemiBold"/>
                                <TextBlock Text="{Binding Description}" 
                                           Foreground="#6c7086"
                                           FontSize="11"/>
                            </StackPanel>
                        </StackPanel>
                        
                        <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4">
                            <Button Command="{Binding $parent[ListBox].DataContext.TogglePinCommand}"
                                    CommandParameter="{Binding}"
                                    ToolTip.Tip="Pin/Unpin">
                                <TextBlock Text="{Binding IsPinned, Converter={StaticResource PinConverter}}"/>
                            </Button>
                            <Button Command="{Binding $parent[ListBox].DataContext.DeletePresetCommand}"
                                    CommandParameter="{Binding}"
                                    ToolTip.Tip="Delete"
                                    IsVisible="{Binding !IsBuiltIn}">
                                <TextBlock Text="🗑️"/>
                            </Button>
                        </StackPanel>
                    </Grid>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </Grid>
</UserControl>
```

---

## 8. Test Specifications (TDD)

### 8.1 PresetService Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Application/Services/PresetServiceTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Application.Services;

public class PresetServiceTests
{
    private readonly IPresetRepository _repository;
    private readonly PresetService _sut;

    public PresetServiceTests()
    {
        _repository = Substitute.For<IPresetRepository>();
        _sut = new PresetService(_repository);
    }

    [Fact]
    public void CreateFromSession_CapturesSessionState()
    {
        var session = PromptSession.CreateNew("ws-1", "Test") with
        {
            SelectedFilePaths = new[] { "/file1.cs", "/file2.cs" },
            CustomInstructions = "Review for security",
            SelectedRoleId = "security_expert"
        };

        var preset = _sut.CreateFromSession(session, "Security Review");

        preset.Name.Should().Be("Security Review");
        preset.ExplicitFilePaths.Should().HaveCount(2);
        preset.CustomInstructions.Should().Be("Review for security");
        preset.RoleId.Should().Be("security_expert");
    }

    [Fact]
    public async Task ApplyToSessionAsync_SelectsExistingFiles()
    {
        var preset = PromptPreset.Create("Test") with
        {
            ExplicitFilePaths = new[] { "/root/file1.cs" }
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        result.FilesSelected.Should().Be(1);
        result.FilesNotFound.Should().Be(0);
    }

    [Fact]
    public async Task ApplyToSessionAsync_ReportsMissingFiles()
    {
        var preset = PromptPreset.Create("Test") with
        {
            ExplicitFilePaths = new[] { "/nonexistent/file.cs" }
        };
        var root = CreateTestFileTree();
        var session = PromptSession.CreateNew("ws-1", "Session");

        var result = await _sut.ApplyToSessionAsync(preset, session, root);

        result.FilesNotFound.Should().Be(1);
        result.Warnings.Should().Contain(w => w.Contains("not found"));
    }

    [Fact]
    public async Task RecordUsageAsync_IncrementsUsageCount()
    {
        var preset = PromptPreset.Create("Test") with { Id = "preset-1", UsageCount = 5 };
        _repository.GetByIdAsync("preset-1", Arg.Any<CancellationToken>()).Returns(preset);

        await _sut.RecordUsageAsync("preset-1");

        await _repository.Received(1).SaveAsync(
            Arg.Is<PromptPreset>(p => p.UsageCount == 6),
            Arg.Any<CancellationToken>());
    }

    private static FileNode CreateTestFileTree()
    {
        var root = new FileNode("/root", "root", true);
        root.AddChild(new FileNode("/root/file1.cs", "file1.cs", false));
        root.AddChild(new FileNode("/root/file2.cs", "file2.cs", false));
        return root;
    }
}
```

---

## 9. Built-In Presets

### 9.1 Default Presets

| Name | Description | Files | Role |
|------|-------------|-------|------|
| Code Review | General code review | `**/*.cs` | General Review |
| Security Audit | Security-focused review | `**/*.cs`, `**/*.config` | Security Expert |
| Bug Investigation | Debug and fix issues | (selected files) | Debug Assistant |
| Documentation | Generate/review docs | `**/README.md`, `**/*.md` | Documentation |
| Performance Review | Performance analysis | `**/*.cs` | Performance |

---

## 10. Implementation Checklist

### 10.1 Domain Layer

- [ ] Create `PromptPreset` record
- [ ] Create `PresetScope` enum
- [ ] Create `PresetApplication` record
- [ ] Write unit tests

### 10.2 Application Layer

- [ ] Create `IPresetRepository` interface
- [ ] Create `IPresetService` interface
- [ ] Create `IPresetExportService` interface
- [ ] Implement `PresetService`
- [ ] Implement `PresetExportService`
- [ ] Write unit tests (TDD)

### 10.3 Infrastructure Layer

- [ ] Implement `JsonPresetRepository`
- [ ] Add built-in presets YAML/JSON

### 10.4 Presentation Layer

- [ ] Create `PresetLibraryViewModel`
- [ ] Create `SavePresetDialogViewModel`
- [ ] Create `PresetLibrary.axaml`
- [ ] Create `SavePresetDialog.axaml`
- [ ] Write ViewModel tests (TDD)

### 10.5 Integration

- [ ] Register services in DI
- [ ] Add preset button to main window
- [ ] Add keyboard shortcut (Ctrl+Shift+S to save)
- [ ] End-to-end testing

---

## 11. Acceptance Criteria

| Criterion | Verification |
|-----------|--------------|
| Can save preset from current state | Manual test |
| Can load preset and apply | Manual test |
| Pinned presets show in quick access | Manual test |
| Built-in presets available | Manual test |
| Export/import works | Integration test |
| All unit tests pass | `dotnet test` |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial specification |

