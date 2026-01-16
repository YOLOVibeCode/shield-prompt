# ShieldPrompt v2.0 Implementation Master Plan

**Version:** 2.0.0  
**Last Updated:** January 15, 2026  
**Architecture:** Backend-First, Test-First, UI-Last  
**Status:** AUTHORITATIVE IMPLEMENTATION GUIDE

---

## Core Architecture Principle

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     BACKEND-FIRST, TEST-FIRST, UI-LAST                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   1. DOMAIN        → Define entities, records, enums                        │
│   2. INTERFACE     → Define small, focused interfaces (≤5 methods)          │
│   3. TESTS         → Write failing tests (TDD RED)                          │
│   4. SERVICE       → Implement to pass tests (TDD GREEN)                    │
│   5. REFACTOR      → Clean up, tests still green                            │
│   6. UI            → Wire up LAST (ViewModel → View)                        │
│                                                                              │
│   ⚠️ NEVER TOUCH UI UNTIL STEPS 1-5 ARE COMPLETE AND TESTED ⚠️              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Phase Overview

| Phase | Name | Backend Components | UI Components | Status |
|-------|------|-------------------|---------------|--------|
| 0 | Foundation | Domain, Core Interfaces | None | ✅ Done |
| 1 | Workspace Management | 3 interfaces, 3 services | 2 views | Pending |
| 2 | Multi-Tab Sessions | 2 interfaces, 2 services | 1 view | Pending |
| 3 | Enhanced File Tree | 1 interface, 1 service | Bindings only | ✅ Done |
| 4 | Git Integration | 2 interfaces, 2 services | Status display | Pending |
| 5 | Stored Prompts | 2 interfaces, 2 services | 1 dialog | Pending |
| 6 | Apply Mode | 4 interfaces, 4 services | 1 panel | Pending |
| 7 | Polish | 1 interface, 1 service | Status bar | Pending |

---

## Phase 1: Workspace Management

### Step 1: Domain (No Tests)

**Files to Create:**
```
src/ShieldPrompt.Domain/Entities/Workspace.cs      (exists)
src/ShieldPrompt.Domain/Records/WorkspaceSettings.cs
```

**Workspace Entity:**
```csharp
public record Workspace(
    string Id,
    string Name,
    string RootPath,
    DateTime LastOpened,
    DateTime CreatedAt,
    WorkspaceSettings Settings);

public record WorkspaceSettings(
    string? PreferredRoleId,
    string? PreferredModelId,
    IReadOnlyList<string> IgnorePatterns,
    IReadOnlyDictionary<string, string> CustomSettings);
```

### Step 2: Interfaces (No Tests)

**Files to Create:**
```
src/ShieldPrompt.Application/Interfaces/IWorkspaceRepository.cs    (exists)
src/ShieldPrompt.Application/Interfaces/IWorkspaceManager.cs
src/ShieldPrompt.Application/Interfaces/IRecentWorkspacesService.cs
```

**IWorkspaceManager (≤5 methods):**
```csharp
public interface IWorkspaceManager
{
    Task<Workspace> OpenAsync(string path, CancellationToken ct = default);
    Task<Workspace?> GetCurrentAsync();
    Task SaveCurrentAsync(CancellationToken ct = default);
    Task CloseCurrentAsync(CancellationToken ct = default);
}
```

**IRecentWorkspacesService (≤5 methods):**
```csharp
public interface IRecentWorkspacesService
{
    Task<IReadOnlyList<Workspace>> GetRecentAsync(int maxCount = 10);
    Task AddRecentAsync(Workspace workspace);
    Task ClearRecentAsync();
}
```

### Step 3: Tests (TDD RED)

**Files to Create:**
```
tests/ShieldPrompt.Tests.Unit/Application/WorkspaceManagerTests.cs
tests/ShieldPrompt.Tests.Unit/Application/RecentWorkspacesServiceTests.cs
tests/ShieldPrompt.Tests.Unit/Infrastructure/JsonWorkspaceRepositoryTests.cs
```

**Test Coverage Required:**
```csharp
public class WorkspaceManagerTests
{
    [Fact] public async Task OpenAsync_WithValidPath_ReturnsWorkspace() { }
    [Fact] public async Task OpenAsync_WithInvalidPath_ThrowsException() { }
    [Fact] public async Task OpenAsync_SetsCurrentWorkspace() { }
    [Fact] public async Task GetCurrentAsync_WhenNoWorkspace_ReturnsNull() { }
    [Fact] public async Task SaveCurrentAsync_PersistsChanges() { }
    [Fact] public async Task CloseCurrentAsync_ClearsCurrentWorkspace() { }
}

public class RecentWorkspacesServiceTests
{
    [Fact] public async Task GetRecentAsync_ReturnsOrderedByLastOpened() { }
    [Fact] public async Task AddRecentAsync_MovesToTop() { }
    [Fact] public async Task AddRecentAsync_LimitsToMaxCount() { }
    [Fact] public async Task ClearRecentAsync_RemovesAll() { }
}
```

### Step 4: Services (TDD GREEN)

**Files to Create:**
```
src/ShieldPrompt.Application/Services/WorkspaceManager.cs
src/ShieldPrompt.Application/Services/RecentWorkspacesService.cs
```

### Step 5: Infrastructure

**Files to Create:**
```
src/ShieldPrompt.Infrastructure/Repositories/JsonWorkspaceRepository.cs  (exists)
```

### Step 6: ViewModel (UI Wiring)

**Files to Modify:**
```
src/ShieldPrompt.App/ViewModels/MainWindowViewModel.cs
  - Add: IWorkspaceManager injection
  - Add: OpenWorkspaceCommand
  - Add: RecentWorkspaces property
```

### Step 7: View (UI Binding)

**Files to Modify:**
```
src/ShieldPrompt.App/Views/MainWindow.axaml
  - Add: Workspace dropdown binding
  - Add: Recent workspaces menu
```

---

## Phase 2: Multi-Tab Sessions

### Step 1: Domain

**Files to Create:**
```
src/ShieldPrompt.Domain/Entities/PromptSession.cs
src/ShieldPrompt.Domain/Records/SessionState.cs
```

```csharp
public record PromptSession(
    string Id,
    string Name,
    DateTime CreatedAt,
    bool IsDirty,
    bool IsPinned,
    SessionState State);

public record SessionState(
    IReadOnlyList<string> SelectedFiles,
    string? RoleId,
    string? CustomInstructions,
    IReadOnlyList<string> FocusAreas);
```

### Step 2: Interfaces

```csharp
public interface ISessionManager
{
    IReadOnlyList<PromptSession> Sessions { get; }
    PromptSession? ActiveSession { get; }
    Task<PromptSession> CreateSessionAsync(string? name = null);
    Task SwitchToSessionAsync(string sessionId);
    Task CloseSessionAsync(string sessionId);
}

public interface ISessionStateSerializer
{
    Task<SessionState> LoadAsync(string sessionId);
    Task SaveAsync(string sessionId, SessionState state);
}
```

### Step 3: Tests

```csharp
public class SessionManagerTests
{
    [Fact] public async Task CreateSessionAsync_AddsToSessions() { }
    [Fact] public async Task CreateSessionAsync_SetsAsActive() { }
    [Fact] public async Task SwitchToSessionAsync_ChangesActiveSession() { }
    [Fact] public async Task CloseSessionAsync_RemovesFromSessions() { }
    [Fact] public async Task CloseSessionAsync_WhenDirty_RequiresConfirmation() { }
}
```

### Steps 4-7: Implementation → UI

(Same pattern as Phase 1)

---

## Phase 4: Git Integration

### Step 1: Domain

```csharp
public enum GitStatus { Untracked, Modified, Staged, Committed, Ignored }

public record GitFileInfo(
    string Path,
    GitStatus Status,
    int? AddedLines,
    int? RemovedLines);
```

### Step 2: Interfaces

```csharp
public interface IGitStatusProvider
{
    Task<bool> IsGitRepositoryAsync(string path);
    Task<string?> GetCurrentBranchAsync(string repoPath);
    Task<IReadOnlyList<GitFileInfo>> GetFileStatusesAsync(string repoPath);
}

public interface IGitFileFilter
{
    Task<IEnumerable<FileNode>> FilterByStatusAsync(
        IEnumerable<FileNode> files, 
        GitStatus status);
}
```

### Step 3: Tests

```csharp
public class GitStatusProviderTests
{
    [Fact] public async Task IsGitRepositoryAsync_WithGitFolder_ReturnsTrue() { }
    [Fact] public async Task IsGitRepositoryAsync_WithoutGitFolder_ReturnsFalse() { }
    [Fact] public async Task GetCurrentBranchAsync_ReturnsCurrentBranch() { }
    [Fact] public async Task GetFileStatusesAsync_ReturnsModifiedFiles() { }
}
```

---

## Phase 5: Stored Prompts (Presets)

### Step 1: Domain

```csharp
public record PromptPreset(
    string Id,
    string Name,
    string? Description,
    string Icon,
    DateTime CreatedAt,
    DateTime ModifiedAt,
    bool IsPinned,
    PresetScope Scope,
    PresetContents Contents);

public record PresetContents(
    string? RoleId,
    IReadOnlyList<string>? SelectedFilePatterns,
    string? CustomInstructions,
    IReadOnlyList<string>? FocusAreas);

public enum PresetScope { Global, Workspace }
```

### Step 2: Interfaces

```csharp
public interface IPresetRepository
{
    Task<IReadOnlyList<PromptPreset>> GetAllAsync(PresetScope? scope = null);
    Task<PromptPreset?> GetByIdAsync(string id);
    Task SaveAsync(PromptPreset preset);
    Task DeleteAsync(string id);
}

public interface IPresetApplier
{
    Task ApplyAsync(PromptPreset preset, ISessionManager sessionManager);
    Task<PromptPreset> CreateFromCurrentAsync(string name, ISessionManager sessionManager);
}
```

### Step 3: Tests

```csharp
public class PresetRepositoryTests
{
    [Fact] public async Task GetAllAsync_ReturnsAllPresets() { }
    [Fact] public async Task GetAllAsync_WithScope_FiltersCorrectly() { }
    [Fact] public async Task SaveAsync_CreatesNewPreset() { }
    [Fact] public async Task SaveAsync_UpdatesExistingPreset() { }
    [Fact] public async Task DeleteAsync_RemovesPreset() { }
}

public class PresetApplierTests
{
    [Fact] public async Task ApplyAsync_SetsRole() { }
    [Fact] public async Task ApplyAsync_SelectsMatchingFiles() { }
    [Fact] public async Task ApplyAsync_SetsCustomInstructions() { }
    [Fact] public async Task CreateFromCurrentAsync_CapturesState() { }
}
```

---

## Phase 6: Apply Mode Enhancement (CRITICAL)

### Step 1: Domain

```csharp
public record DiffPreview(
    string FilePath,
    FileOperationType Operation,
    IReadOnlyList<DiffLine> Lines,
    int AddedCount,
    int RemovedCount);

public record DiffLine(
    int? OldLineNumber,
    int? NewLineNumber,
    DiffLineType Type,
    string Content);

public enum DiffLineType { Context, Added, Removed }

public record ApplyResult(
    bool Success,
    int FilesCreated,
    int FilesUpdated,
    int FilesDeleted,
    string? BackupId,
    IReadOnlyList<ApplyError> Errors);

public record ApplyError(string FilePath, string Message);
```

### Step 2: Interfaces (4 small interfaces - ISP)

```csharp
// Interface 1: Diff generation
public interface IDiffGenerator
{
    Task<DiffPreview> GenerateDiffAsync(string originalContent, string newContent, string filePath);
    Task<IReadOnlyList<DiffPreview>> GenerateDiffsAsync(IEnumerable<FileOperation> operations, string baseDirectory);
}

// Interface 2: Diff rendering
public interface IDiffRenderer
{
    string RenderSideBySide(DiffPreview diff);
    string RenderUnified(DiffPreview diff);
}

// Interface 3: Conflict detection
public interface IConflictDetector
{
    Task<bool> HasConflictAsync(FileOperation operation, string baseDirectory);
    Task<ConflictInfo?> GetConflictInfoAsync(FileOperation operation, string baseDirectory);
}

// Interface 4: Selective application
public interface ISelectiveApplier
{
    Task<ApplyResult> ApplySelectedAsync(
        IEnumerable<FileOperation> operations,
        IEnumerable<string> selectedPaths,
        string baseDirectory,
        ApplyOptions options,
        CancellationToken ct = default);
}
```

### Step 3: Tests (Comprehensive)

```csharp
public class DiffGeneratorTests
{
    [Fact] public async Task GenerateDiffAsync_WithAddedLines_ShowsAdditions() { }
    [Fact] public async Task GenerateDiffAsync_WithRemovedLines_ShowsRemovals() { }
    [Fact] public async Task GenerateDiffAsync_WithChangedLines_ShowsBothTypes() { }
    [Fact] public async Task GenerateDiffAsync_PreservesContext() { }
}

public class ConflictDetectorTests
{
    [Fact] public async Task HasConflictAsync_WhenFileUnmodified_ReturnsFalse() { }
    [Fact] public async Task HasConflictAsync_WhenFileModified_ReturnsTrue() { }
    [Fact] public async Task HasConflictAsync_WhenFileDeleted_ReturnsTrue() { }
}

public class SelectiveApplierTests
{
    [Fact] public async Task ApplySelectedAsync_AppliesOnlySelected() { }
    [Fact] public async Task ApplySelectedAsync_SkipsUnselected() { }
    [Fact] public async Task ApplySelectedAsync_CreatesBackup() { }
    [Fact] public async Task ApplySelectedAsync_ReturnsCorrectCounts() { }
}
```

### Steps 4-7: Implementation → UI

```
Service Implementation:
├── DiffGenerator.cs          → Uses DiffPlex library
├── DiffRenderer.cs           → Text formatting
├── ConflictDetector.cs       → File system checks
└── SelectiveApplier.cs       → Orchestrates apply

ViewModel (LAST):
├── ApplyDashboardViewModel.cs
│   ├── DiffPreviews property (bound to service)
│   ├── ApplySelectedCommand (delegates to ISelectiveApplier)
│   └── NO diff logic - all delegated

View (VERY LAST):
├── ApplyDashboard.axaml
│   ├── ListBox bound to DiffPreviews
│   ├── CheckBoxes for selection
│   └── Pure XAML bindings only
```

---

## Phase 7: Status Bar & Polish

### Step 1: Domain

```csharp
public record StatusInfo(
    string Message,
    StatusSeverity Severity,
    bool IsLoading,
    ProgressInfo? Progress);

public record ProgressInfo(int Current, int Total, string? Description);

public enum StatusSeverity { Info, Success, Warning, Error }
```

### Step 2: Interface

```csharp
public interface IStatusReporter
{
    StatusInfo CurrentStatus { get; }
    event EventHandler<StatusInfo>? StatusChanged;
    
    void ReportInfo(string message);
    void ReportSuccess(string message);
    void ReportWarning(string message);
    void ReportError(string message);
    void ReportProgress(int current, int total, string? description = null);
    void ClearProgress();
}
```

### Step 3: Tests

```csharp
public class StatusReporterTests
{
    [Fact] public void ReportInfo_SetsCorrectSeverity() { }
    [Fact] public void ReportProgress_UpdatesProgressInfo() { }
    [Fact] public void ReportSuccess_RaisesStatusChangedEvent() { }
    [Fact] public void ClearProgress_SetsProgressToNull() { }
}
```

---

## Test Organization

```
tests/ShieldPrompt.Tests.Unit/
├── Domain/
│   ├── Entities/
│   │   └── WorkspaceTests.cs
│   └── Records/
│       └── FileOperationTests.cs
├── Application/
│   ├── Services/
│   │   ├── WorkspaceManagerTests.cs
│   │   ├── SessionManagerTests.cs
│   │   ├── DiffGeneratorTests.cs
│   │   ├── ConflictDetectorTests.cs
│   │   └── SelectiveApplierTests.cs
│   └── Parsers/
│       └── StructuredResponseParserTests.cs
├── Infrastructure/
│   ├── Repositories/
│   │   ├── JsonWorkspaceRepositoryTests.cs
│   │   └── JsonPresetRepositoryTests.cs
│   └── Services/
│       └── GitStatusProviderTests.cs
└── ViewModels/
    └── (Minimal - VMs have no logic to test)
```

---

## Implementation Checklist Per Feature

Use this checklist for EVERY feature:

```markdown
## Feature: [Name]

### Backend (COMPLETE BEFORE UI)
- [ ] Domain entity/record created
- [ ] Interface defined (≤5 methods)
- [ ] Test file created
- [ ] All interface methods have tests
- [ ] Tests fail (TDD RED confirmed)
- [ ] Service implemented
- [ ] All tests pass (TDD GREEN confirmed)
- [ ] Code refactored (tests still green)

### UI (ONLY AFTER BACKEND COMPLETE)
- [ ] ViewModel command added
- [ ] ViewModel delegates to service (no logic)
- [ ] XAML binding added
- [ ] Manual test: feature works
```

---

## Summary

**Key Principles Enforced:**

1. ✅ **Backend-First**: All logic in services
2. ✅ **Test-First**: TDD RED → GREEN → REFACTOR
3. ✅ **UI-Last**: Only wiring, no logic
4. ✅ **ISP Compliant**: Interfaces ≤5 methods
5. ✅ **MVVM Strict**: ViewModels delegate only

**Total Backend Components:**
- 15+ Interfaces
- 15+ Services
- 100+ Tests

**Total UI Components:**
- ~10 ViewModels (thin, delegation only)
- ~10 Views (binding only)

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 2.0.0 | 2026-01-15 | Architect | Complete implementation plan with Backend-First architecture |

