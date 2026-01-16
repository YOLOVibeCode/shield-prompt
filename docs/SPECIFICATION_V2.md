# ShieldPrompt v2.0 Specification Addendum
## Completely Redesigned UI with Business Logic Reuse

**Version:** 2.0.0  
**Last Updated:** January 15, 2026  
**Status:** ACTIVE - IN DEVELOPMENT

---

## Executive Summary

ShieldPrompt v2.0 introduces a **completely new MainWindow (MainWindowV2)** while **maximizing reuse of existing business logic**. This is not a refactor of v1.x - it's a ground-up UI rebuild that leverages the battle-tested Application, Domain, Infrastructure, and Sanitization layers.

---

## 1. Architectural Principle: UI Layer Separation

### 1.1 What We're Replacing

| Component | v1.x | v2.0 |
|-----------|------|------|
| Main Window | `MainWindow.axaml` | **NEW** `MainWindowV2.axaml` |
| Main ViewModel | `MainWindowViewModel.cs` | **NEW** `MainWindowV2ViewModel.cs` |
| File Tree | Embedded in MainWindow | **NEW** Dedicated `FileTreePanel.axaml` |
| Preview Pane | Embedded in MainWindow | **NEW** Dedicated `PreviewPanel.axaml` |
| Prompt Builder | Tab-based | **NEW** Wizard-driven panels |
| LLM Response | Tab-based | **NEW** Integrated dashboard |

### 1.2 What We're Reusing (100% Unchanged)

| Layer | Components | Status |
|-------|------------|--------|
| **Domain** | `FileNode`, `Role`, `Workspace`, all enums, all records | ✅ Reuse as-is |
| **Application** | `ISanitizationEngine`, `ITokenCountingService`, `IPromptComposer`, `IFileWriterService`, `IStructuredResponseParser` | ✅ Reuse as-is |
| **Infrastructure** | `IFileSystemService`, `IClipboardService`, `YamlRoleRepository`, `JsonWorkspaceRepository` | ✅ Reuse as-is |
| **Sanitization** | `SanitizationEngine`, `DesanitizationEngine`, `MappingSession`, `AliasGenerator` | ✅ Reuse as-is |

### 1.3 Dependency Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ShieldPrompt v2.0                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    NEW PRESENTATION LAYER (v2)                        │  │
│  ├──────────────────────────────────────────────────────────────────────┤  │
│  │  MainWindowV2.axaml          MainWindowV2ViewModel.cs                │  │
│  │  FileTreePanel.axaml         FileTreePanelViewModel.cs               │  │
│  │  PreviewPanel.axaml          PreviewPanelViewModel.cs                │  │
│  │  PromptWizard.axaml          PromptWizardViewModel.cs                │  │
│  │  ResponseDashboard.axaml     ResponseDashboardViewModel.cs           │  │
│  │  WorkspaceSelector.axaml     WorkspaceSelectorViewModel.cs           │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                              │                                               │
│                              │ Injects                                       │
│                              ▼                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                  EXISTING LAYERS (Unchanged)                          │  │
│  ├──────────────────────────────────────────────────────────────────────┤  │
│  │  Application:  ISanitizationEngine, IPromptComposer, ITokenCounter   │  │
│  │  Domain:       FileNode, Role, Workspace, Pattern, SanitizationMatch │  │
│  │  Infrastructure: FileSystemService, ClipboardService, Repositories   │  │
│  │  Sanitization: SanitizationEngine, DesanitizationEngine, Mapping     │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. MainWindowV2 Design

### 2.1 Layout Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ShieldPrompt v2.0                                        [_][□][X]         │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌──────┐ ┌──────────────────────────────────────────────┐ ┌─────────────┐ │
│  │ 📁   │ │ MyProject ▾  │  [Role: 🔧 Engineer ▾]       │ │ ⚙️ Settings │ │
│  │ Open │ └──────────────────────────────────────────────┘ └─────────────┘ │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────┐ ┌─────────────────────────────────────────────────┐  │
│  │                  │ │  📄 PROMPT PREVIEW                              │  │
│  │  📂 File Tree    │ │  ────────────────────────────────────────────── │  │
│  │  ───────────────│ │                                                  │  │
│  │  ☑ src/         │ │  # ShieldPrompt Analysis Request                │  │
│  │   ├ ☑ App.cs    │ │                                                  │  │
│  │   ├ ☑ User.cs   │ │  **Role:** 🔧 Software Engineer                 │  │
│  │   └ ☐ Tests/    │ │  **Files:** 3 selected (2,847 tokens)           │  │
│  │                  │ │                                                  │  │
│  │  ───────────────│ │  ## 📁 Files Included                           │  │
│  │  3 files        │ │  ### `src/App.cs`                               │  │
│  │  2,847 tokens   │ │  ```csharp                                      │  │
│  │                  │ │  // File content here...                        │  │
│  │                  │ │  ```                                            │  │
│  │                  │ │                                                  │  │
│  │                  │ ├─────────────────────────────────────────────────┤  │
│  │                  │ │  📝 Custom Instructions (optional)             │  │
│  │                  │ │  ┌─────────────────────────────────────────────┐│  │
│  │                  │ │  │ Refactor to use async/await...             ││  │
│  │                  │ │  └─────────────────────────────────────────────┘│  │
│  └──────────────────┘ └─────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│  [📋 Copy to Clipboard]  [📥 Paste Response]      🔐 12 values sanitized  │
├─────────────────────────────────────────────────────────────────────────────┤
│  ✅ Ready │ GPT-4o │ 2,847 / 128,000 tokens (2.2%) │ Session: 4h 23m      │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Key UI Improvements Over v1.x

| Feature | v1.x | v2.0 |
|---------|------|------|
| **Layout** | Tab-based | Single-view panels |
| **File Selection** | Tree embedded in tab | Dedicated collapsible panel |
| **Preview** | Separate tab | Real-time live preview |
| **Role Selection** | Dropdown in toolbar | Prominent workspace header |
| **Instructions** | Separate input area | Inline with preview |
| **Response Handling** | Separate tab | Modal dashboard overlay |
| **Panel Sizing** | Fixed | Draggable splitters |
| **Responsive** | Desktop only | Desktop/Tablet/Mobile breakpoints |

### 2.3 Panel System

```csharp
/// <summary>
/// Defines a resizable, collapsible panel in the v2 UI.
/// </summary>
public interface IPanel
{
    string PanelId { get; }
    string Title { get; }
    bool IsCollapsed { get; set; }
    double Width { get; set; }
    double MinWidth { get; }
    double MaxWidth { get; }
}

/// <summary>
/// Manages panel layout state persistence.
/// REUSES: Existing Infrastructure.Persistence patterns
/// </summary>
public interface IPanelLayoutRepository
{
    Task<PanelLayout> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(PanelLayout layout, CancellationToken ct = default);
}
```

---

## 3. Business Logic Reuse Strategy

### 3.1 Direct Injection (No Wrapper)

These services are injected directly into v2 ViewModels:

```csharp
public class MainWindowV2ViewModel : ObservableObject
{
    // REUSED: Existing application layer services
    private readonly ISanitizationEngine _sanitization;
    private readonly IDesanitizationEngine _desanitization;
    private readonly ITokenCountingService _tokenService;
    private readonly IPromptComposer _promptComposer;
    private readonly IFileSystemService _fileSystem;
    private readonly IClipboardService _clipboard;
    private readonly IRoleRepository _roleRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IStructuredResponseParser _responseParser;
    private readonly IFileWriterService _fileWriter;
    private readonly IUndoRedoManager _undoManager;
    
    // NEW: v2-specific services
    private readonly IPanelLayoutRepository _layoutRepository;
    
    public MainWindowV2ViewModel(
        // All services injected via DI - no changes to Application layer
        ISanitizationEngine sanitization,
        IDesanitizationEngine desanitization,
        ITokenCountingService tokenService,
        IPromptComposer promptComposer,
        IFileSystemService fileSystem,
        IClipboardService clipboard,
        IRoleRepository roleRepository,
        IWorkspaceRepository workspaceRepository,
        IStructuredResponseParser responseParser,
        IFileWriterService fileWriter,
        IUndoRedoManager undoManager,
        IPanelLayoutRepository layoutRepository)
    {
        _sanitization = sanitization;
        _desanitization = desanitization;
        // ... etc.
    }
}
```

### 3.2 Domain Model Reuse

All existing domain models are used without modification:

```csharp
// REUSED AS-IS from ShieldPrompt.Domain
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

public partial class FileTreePanelViewModel : ObservableObject
{
    // Uses existing FileNode exactly as-is
    [ObservableProperty]
    private ObservableCollection<FileNode> _rootNodes = new();
    
    // Uses existing Role exactly as-is
    [ObservableProperty]
    private Role _selectedRole;
    
    // Uses existing Workspace exactly as-is
    [ObservableProperty]
    private Workspace? _currentWorkspace;
}
```

### 3.3 Service Interface Compatibility Matrix

| Interface | Defined In | Reused By v2 | Notes |
|-----------|------------|--------------|-------|
| `ISanitizationEngine` | Application | ✅ | Core sanitization logic |
| `IDesanitizationEngine` | Application | ✅ | Core desanitization logic |
| `ITokenCountingService` | Application | ✅ | Token counting for any text |
| `IPromptComposer` | Application | ✅ | Prompt generation |
| `IFileWriterService` | Application | ✅ | File system operations |
| `IStructuredResponseParser` | Application | ✅ | LLM response parsing |
| `IUndoRedoManager` | Application | ✅ | Undo/Redo operations |
| `IResponseFormatStrategy` | Application | ✅ | Format strategies |
| `IRoleRepository` | Application | ✅ | Role management |
| `IWorkspaceRepository` | Application | ✅ | Workspace management |
| `IFileSystemService` | Infrastructure | ✅ | File I/O operations |
| `IClipboardService` | Infrastructure | ✅ | Clipboard access |
| `IOutputFormatSettingsRepository` | Application | ✅ | Settings persistence |

---

## 4. New v2 Components

### 4.1 New ViewModels (Presentation Layer Only)

| ViewModel | Purpose | Reuses |
|-----------|---------|--------|
| `MainWindowV2ViewModel` | Main window orchestration | All application services |
| `FileTreePanelViewModel` | File selection panel | `IFileSystemService`, `FileNode` |
| `PreviewPanelViewModel` | Live prompt preview | `IPromptComposer`, `ITokenCountingService` |
| `WorkspaceSelectorViewModel` | Workspace dropdown | `IWorkspaceRepository`, `Workspace` |
| `RoleSelectorViewModel` | Role dropdown with descriptions | `IRoleRepository`, `Role` |
| `ResponseDashboardViewModel` | LLM response handling | `IStructuredResponseParser`, `IFileWriterService` |
| `StatusBarViewModel` | Status information | `ITokenCountingService`, `IMappingSession` |

### 4.2 New Views (Presentation Layer Only)

| View | Description |
|------|-------------|
| `MainWindowV2.axaml` | Main window with 3-panel layout |
| `FileTreePanel.axaml` | Left panel - file selection |
| `PreviewPanel.axaml` | Center panel - live preview |
| `InstructionsPanel.axaml` | Bottom of center - custom instructions |
| `WorkspaceHeader.axaml` | Top header - workspace/role selection |
| `ActionBar.axaml` | Bottom action buttons |
| `StatusBar.axaml` | Bottom status information |
| `ResponseDashboardOverlay.axaml` | Modal overlay for LLM response |

### 4.3 New Infrastructure (Minimal)

| Component | Purpose |
|-----------|---------|
| `IPanelLayoutRepository` | Persist panel sizes/collapse states |
| `JsonPanelLayoutRepository` | JSON implementation |

---

## 5. Migration Strategy

### 5.1 Parallel Development

Both MainWindow (v1) and MainWindowV2 will exist temporarily:

```csharp
// App.axaml.cs
private static void ConfigureServices(IServiceCollection services)
{
    // REUSED: All existing services (unchanged)
    services.AddSingleton<ISanitizationEngine, SanitizationEngine>();
    services.AddSingleton<IDesanitizationEngine, DesanitizationEngine>();
    services.AddSingleton<ITokenCountingService, TokenCountingService>();
    services.AddSingleton<IPromptComposer, PromptComposer>();
    // ... all existing registrations ...
    
    // NEW: v2-specific services
    services.AddSingleton<IPanelLayoutRepository, JsonPanelLayoutRepository>();
    
    // NEW: v2 ViewModels
    services.AddTransient<MainWindowV2ViewModel>();
    services.AddTransient<FileTreePanelViewModel>();
    services.AddTransient<PreviewPanelViewModel>();
    // ... etc.
    
    // PRESERVED: v1 ViewModel (for comparison during development)
    services.AddSingleton<MainWindowViewModel>();
}
```

### 5.2 Feature Flag for Testing

```csharp
// App.axaml.cs
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        // Feature flag: USE_V2_UI
        var useV2 = Environment.GetEnvironmentVariable("SHIELDPROMPT_V2") == "1";
        
        if (useV2)
        {
            desktop.MainWindow = new MainWindowV2
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowV2ViewModel>()
            };
        }
        else
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }
    }
}
```

### 5.3 Test Reuse

Existing unit tests for business logic remain valid:

```
tests/
├── ShieldPrompt.Tests.Unit/
│   ├── Application/           # UNCHANGED - All tests still pass
│   │   ├── Sanitization/      # ✅ SanitizationEngineTests
│   │   ├── Parsers/           # ✅ StructuredResponseParserTests
│   │   ├── Formatters/        # ✅ HybridXmlMarkdownFormatterTests
│   │   └── Roles/             # ✅ RoleRepositoryTests
│   ├── Domain/                # UNCHANGED
│   │   └── Entities/          # ✅ FileNodeTests, WorkspaceTests
│   ├── ViewModels/            # MIXED
│   │   ├── MainWindowViewModel*.cs  # Legacy tests (keep for v1)
│   │   └── V2/                       # NEW: v2 ViewModel tests
│   │       ├── MainWindowV2ViewModelTests.cs
│   │       ├── FileTreePanelViewModelTests.cs
│   │       └── PreviewPanelViewModelTests.cs
```

---

## 6. v2.0 Feature Comparison: ShieldPrompt vs RepoPrompt

| Feature | RepoPrompt | ShieldPrompt v1.x | ShieldPrompt v2.0 |
|---------|------------|-------------------|-------------------|
| **Drag & Drop Folder** | ✅ | ❌ | ✅ |
| **Multi-Tab Workspaces** | ✅ | ❌ | ✅ |
| **File Tree with Tokens** | ✅ | ✅ | ✅ Enhanced |
| **Live Preview** | ✅ | ✅ | ✅ Real-time |
| **Role-Based Prompting** | ❌ | ✅ | ✅ Enhanced |
| **Data Sanitization** | ❌ | ✅ | ✅ |
| **LLM Response Parsing** | ❌ | ✅ | ✅ Dashboard |
| **Auto-Apply Changes** | ❌ | ✅ | ✅ Enhanced |
| **Undo/Redo** | ❌ | ✅ | ✅ |
| **Git Integration** | ✅ | ❌ | 🚧 v2.1 |
| **Code Maps (AST)** | ✅ | ❌ | 🚧 v2.1 |
| **Stored Prompts** | ✅ | ❌ | ✅ |
| **Panel Persistence** | ✅ | ✅ | ✅ |
| **Custom Roles** | ❌ | ✅ | ✅ |
| **Responsive Layout** | ❌ | ❌ | ✅ |

---

## 7. Implementation Phases for v2.0

### Phase 1: Foundation (Week 1)
- [ ] Create `MainWindowV2.axaml` basic structure
- [ ] Create `MainWindowV2ViewModel` with service injection
- [ ] Verify all existing services inject correctly
- [ ] Feature flag to switch between v1 and v2

### Phase 2: File Tree Panel (Week 1-2)
- [ ] Create `FileTreePanel.axaml`
- [ ] Create `FileTreePanelViewModel`
- [ ] Wire to existing `IFileSystemService`
- [ ] Implement file selection checkboxes
- [ ] Display per-file token counts

### Phase 3: Preview Panel (Week 2)
- [ ] Create `PreviewPanel.axaml`
- [ ] Create `PreviewPanelViewModel`
- [ ] Wire to existing `IPromptComposer`
- [ ] Implement real-time live preview
- [ ] Add click-to-copy functionality

### Phase 4: Workspace & Role (Week 2-3)
- [ ] Create `WorkspaceHeader.axaml`
- [ ] Implement workspace dropdown
- [ ] Implement role dropdown with descriptions
- [ ] Wire to existing repositories

### Phase 5: Actions & Status (Week 3)
- [ ] Create `ActionBar.axaml`
- [ ] Create `StatusBar.axaml`
- [ ] Implement copy/paste actions
- [ ] Wire to existing sanitization/desanitization

### Phase 6: Response Dashboard (Week 3-4)
- [ ] Create `ResponseDashboardOverlay.axaml`
- [ ] Wire to existing `IStructuredResponseParser`
- [ ] Wire to existing `IFileWriterService`
- [ ] Implement preview and apply workflow

### Phase 7: Polish (Week 4)
- [ ] Panel persistence
- [ ] Responsive breakpoints
- [ ] Keyboard shortcuts
- [ ] Remove v1 UI (after validation)

---

## 8. Definition of Done

### v2.0 Launch Criteria

- [ ] All v1 features work in v2 UI
- [ ] All existing unit tests pass (no changes to Application/Domain/Infrastructure)
- [ ] New ViewModel tests written with TDD
- [ ] Panel layout persists across sessions
- [ ] Responsive layout works at desktop/tablet breakpoints
- [ ] Performance: <100ms for UI updates
- [ ] Memory: No increase over v1 baseline
- [ ] Feature flag removed, v2 is default

---

## 9. Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking existing business logic | Zero changes to Application/Domain layers |
| UI regressions | Feature flag allows A/B testing |
| Performance issues | Profile v2 against v1 baseline |
| Test coverage gaps | TDD for all new ViewModels |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 2.0.0 | 2026-01-15 | ShieldPrompt Team | Initial v2 specification |

---

*This specification supplements SPECIFICATION.md and defines the v2.0 UI rebuild strategy.*

