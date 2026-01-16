# ShieldPrompt v2.0 Implementation Guide

**Last Updated:** January 15, 2026  
**Status:** ACTIVE DEVELOPMENT

---

## Overview

This directory contains detailed implementation specifications for ShieldPrompt v2.0. Each phase document follows strict TDD, ISP, and MVVM principles.

---

## Implementation Principles

### TDD (Test-Driven Development)

```
RED → GREEN → REFACTOR

1. Write failing test FIRST
2. Implement minimum code to pass
3. Refactor while keeping tests green
4. Target: 90%+ code coverage
```

### ISP (Interface Segregation Principle)

```
• Maximum 5 methods per interface
• Each interface has single responsibility
• Clients depend only on methods they use
• Split large interfaces into focused ones
```

### MVVM Separation

```
VIEW (XAML)
├── ONLY data binding
├── ONLY converters for display
└── ONLY event routing to commands

VIEWMODEL
├── Exposes ObservableProperties
├── Exposes Commands (RelayCommand)
├── Orchestrates services
└── NO business logic

APPLICATION LAYER
├── ALL business logic lives here
├── Interfaces define contracts
└── Implementations are injected
```

---

## Phase Summary

| Phase | Name | Priority | Effort | Status |
|-------|------|----------|--------|--------|
| 0 | Definition & Planning | P0 | ✅ | COMPLETED |
| 1 | [Workspace Management](PHASE1_WORKSPACE_MANAGEMENT.md) | P0 | 3-4 days | PENDING |
| 2 | [Multi-Tab Sessions](PHASE2_MULTI_TAB_SESSIONS.md) | P1 | 4-5 days | PENDING |
| 3 | Enhanced File Tree | P0 | 2-3 days | ✅ COMPLETED |
| 4 | [Git Integration](PHASE4_GIT_INTEGRATION.md) | P1 | 3-4 days | PENDING |
| 5 | [Stored Prompts](PHASE5_STORED_PROMPTS.md) | P1 | 2-3 days | PENDING |
| 6 | [Apply Mode Enhancement](PHASE6_APPLY_MODE_ENHANCEMENT.md) | P0 | 5-6 days | PENDING |
| 7 | [Status Bar & Polish](PHASE7_STATUS_BAR_POLISH.md) | P2 | 2-3 days | PENDING |

**Total Estimated Effort:** 22-28 days

---

## Implementation Order

### Recommended Sequence

```
Phase 1 (Workspace) ─────────────────────────────────────┐
                                                          │
Phase 3 (File Tree) ─────► Phase 4 (Git) ────────────────┼─► Phase 7 (Polish)
        ✅ COMPLETED                                       │
                                                          │
Phase 2 (Tabs) ──────────► Phase 5 (Presets) ────────────┤
                                                          │
Phase 6 (Apply Mode) ─────────────────────────────────────┘
```

### Critical Path (P0)

1. **Phase 1: Workspace Management** - Foundation for all features
2. **Phase 6: Apply Mode Enhancement** - Core value proposition
3. **Phase 7: Status Bar & Polish** - Production readiness

---

## Quick Start for Implementers

### Before Starting Any Phase

```bash
# 1. Read the phase specification completely
# 2. Review existing related code
# 3. Write failing tests FIRST
# 4. Run existing tests to ensure baseline
dotnet test
```

### Implementation Workflow

```bash
# For each feature:

# 1. Write domain models/records
# 2. Write interface (ISP-compliant)
# 3. Write unit tests for interface (RED)
# 4. Implement service (GREEN)
# 5. Refactor while tests pass
# 6. Write ViewModel (inject services)
# 7. Write ViewModel tests
# 8. Create View (XAML binding only)
# 9. Integration test
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific phase tests
dotnet test --filter "FullyQualifiedName~Workspace"
dotnet test --filter "FullyQualifiedName~Session"
dotnet test --filter "FullyQualifiedName~Git"
dotnet test --filter "FullyQualifiedName~Preset"
dotnet test --filter "FullyQualifiedName~Apply"

# Run V2 tests only
dotnet test --filter "FullyQualifiedName~V2"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           PRESENTATION                                   │
│  ShieldPrompt.App/                                                       │
│  ├── ViewModels/V2/          (NEW v2 ViewModels)                        │
│  └── Views/V2/               (NEW v2 XAML views)                        │
├─────────────────────────────────────────────────────────────────────────┤
│                           APPLICATION                                    │
│  ShieldPrompt.Application/                                               │
│  ├── Interfaces/             (Service contracts - ISP compliant)        │
│  └── Services/               (Business logic implementations)           │
├─────────────────────────────────────────────────────────────────────────┤
│                             DOMAIN                                       │
│  ShieldPrompt.Domain/                                                    │
│  ├── Entities/               (FileNode, Workspace, PromptSession, etc)  │
│  ├── Records/                (Immutable data carriers)                  │
│  └── Enums/                  (Status types, operation types)            │
├─────────────────────────────────────────────────────────────────────────┤
│                          INFRASTRUCTURE                                  │
│  ShieldPrompt.Infrastructure/                                            │
│  ├── Services/               (File system, clipboard, git)              │
│  └── Repositories/           (JSON/YAML persistence)                    │
├─────────────────────────────────────────────────────────────────────────┤
│                           SANITIZATION                                   │
│  ShieldPrompt.Sanitization/                                              │
│  └── (Existing - unchanged)                                             │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## File Organization

```
docs/implementation/
├── README.md                              # This file (master guide)
├── IMPLEMENTATION_MASTER_PLAN.md          # ⭐⭐ MAIN: Backend-First implementation plan
├── FEATURE_AUDIT_COMPLETE.md              # ⭐ Complete feature audit & gap analysis
├── REPOPROMPT_FEATURE_ANALYSIS.md         # RepoPrompt Pro comparison & gaps
├── COMPLETE_FUNCTION_INVENTORY.md         # EVERY function for ground-up rebuild
├── UI_COMPLETE_SPECIFICATION.md           # COMPLETE UI/UX reference (~200 operations)
├── PHASE1_WORKSPACE_MANAGEMENT.md         # Workspace management + Advanced Search
├── PHASE2_MULTI_TAB_SESSIONS.md           # Multi-tab sessions spec
├── PHASE4_GIT_INTEGRATION.md              # Git integration spec
├── PHASE5_STORED_PROMPTS.md               # Stored prompts spec
├── PHASE6_APPLY_MODE_ENHANCEMENT.md       # Apply mode spec (CRITICAL)
└── PHASE7_STATUS_BAR_POLISH.md            # Status bar & polish spec
```

## ⚠️ ARCHITECTURE PRINCIPLE

```
BACKEND-FIRST, TEST-FIRST, UI-LAST

1. DOMAIN     → Define entities, records
2. INTERFACE  → Define small interfaces (≤5 methods)
3. TEST       → Write failing tests (TDD RED)
4. SERVICE    → Implement to pass tests (TDD GREEN)
5. REFACTOR   → Clean up, tests still green
6. UI         → Wire up LAST (ViewModel → View)

⚠️ NEVER TOUCH UI UNTIL STEPS 1-5 ARE COMPLETE AND TESTED
```

### UI Specification Coverage

The `UI_COMPLETE_SPECIFICATION.md` documents:
- **4 Windows/Forms** (MainWindowV2, Settings, Apply Dashboard, Preset Library)
- **7 Menu Bars** with ~60 menu items
- **7 Panels** (File Tree, Preview, Instructions, Action Bar, Status Bar, Tab Bar, Toolbar)
- **9 Dialogs** (Open Folder, Save Preset, Confirm Close, etc.)
- **4 Context Menus** (File Tree, Preview, Tab, Preset)
- **35+ Keyboard Shortcuts**
- **Mouse & Drag/Drop operations**

---

## Interfaces Per Phase

### Phase 1: Workspace Management
- `IWorkspaceRepository` (exists)
- `IRecentWorkspaceService` (new)
- `IWorkspaceSettingsService` (new)

### Phase 2: Multi-Tab Sessions
- `ISessionRepository` (new)
- `ISessionManager` (new)
- `ISessionStateService` (new)

### Phase 4: Git Integration
- `IGitStatusProvider` (new)
- `IGitRepositoryService` (new)
- `IGitIgnoreService` (new)

### Phase 5: Stored Prompts
- `IPresetRepository` (new)
- `IPresetService` (new)
- `IPresetExportService` (new)

### Phase 6: Apply Mode Enhancement
- `IStructuredResponseParser` (exists)
- `IFileApplyService` (new)
- `IBackupService` (new)
- `IDiffService` (new)

### Phase 7: Status Bar & Polish
- `IStatusService` (new)
- `IProgressService` (new)

---

## Testing Strategy

### Unit Tests (Per Phase)

| Phase | Test File Pattern |
|-------|-------------------|
| 1 | `*WorkspaceService*Tests.cs` |
| 2 | `*SessionManager*Tests.cs`, `*TabBar*Tests.cs` |
| 4 | `*GitStatus*Tests.cs`, `*GitRepository*Tests.cs` |
| 5 | `*PresetService*Tests.cs`, `*PresetLibrary*Tests.cs` |
| 6 | `*FileApply*Tests.cs`, `*Backup*Tests.cs` |
| 7 | `*StatusBar*Tests.cs` |

### Integration Tests

```
tests/ShieldPrompt.Tests.Integration/
├── WorkspaceIntegrationTests.cs
├── SessionPersistenceTests.cs
├── GitIntegrationTests.cs
├── PresetPersistenceTests.cs
└── ApplyWorkflowTests.cs
```

---

## Definition of Done (Per Phase)

- [ ] All interfaces defined (ISP-compliant: ≤5 methods each)
- [ ] All domain models created
- [ ] Unit tests written FIRST (TDD)
- [ ] All unit tests pass
- [ ] Code coverage ≥ 90%
- [ ] ViewModels created (no business logic)
- [ ] Views created (binding only)
- [ ] Integration test written
- [ ] Manual testing completed
- [ ] Documentation updated

---

## Common Patterns

### Service Registration

```csharp
// In App.axaml.cs ConfigureServices()
services.AddSingleton<IWorkspaceRepository, JsonWorkspaceRepository>();
services.AddSingleton<IRecentWorkspaceService, RecentWorkspaceService>();
services.AddTransient<MainWindowV2ViewModel>();
```

### ViewModel Pattern

```csharp
public partial class MyViewModel : ObservableObject
{
    // Injected services (NO direct implementations)
    private readonly IMyService _service;
    
    // Observable properties
    [ObservableProperty]
    private string _myProperty;
    
    // Commands delegate to services
    [RelayCommand]
    private async Task DoSomethingAsync()
    {
        await _service.DoWorkAsync(); // Delegate to service
    }
}
```

### Test Pattern

```csharp
public class MyServiceTests
{
    private readonly IMyService _sut;
    private readonly IDependency _dependency;

    public MyServiceTests()
    {
        _dependency = Substitute.For<IDependency>();
        _sut = new MyService(_dependency);
    }

    [Fact]
    public async Task Method_Scenario_ExpectedBehavior()
    {
        // Arrange
        _dependency.SomeMethod().Returns(expectedValue);
        
        // Act
        var result = await _sut.Method();
        
        // Assert
        result.Should().Be(expected);
    }
}
```

---

## Contact

For questions about these specifications, refer to:
- `.cursorrules` for coding standards
- `docs/SPECIFICATION_V2.md` for overall v2 architecture
- Existing test files for patterns

---

**Remember: TDD is not optional. Write tests FIRST.**

