# ✅ Implementation Checklist: Role System + Format Selector + Role Editor

**Project:** ShieldPrompt v1.3.0  
**Status:** PLANNING PHASE  
**Approach:** TDD (Test-Driven Development) + ISP (Interface Segregation Principle)  
**Date:** January 15, 2026

---

## 📋 Overview: Three Major Features

1. **Role System** - Dropdown to select AI roles (Engineer, Architect, etc.)
2. **Format Pros/Cons** - Display advantages/disadvantages of each format
3. **Role Editor** - UI to customize role prompts

---

## 🎯 PHASE 1: Role System with Dropdown

### ✅ 1.1: Domain Models (ISP - Clean Separation)

**Files to create:**
- [ ] `src/ShieldPrompt.Domain/Records/Role.cs`

**Content:**
```csharp
public record Role(
    string Id,
    string Name,
    string Icon,
    string Description,
    string SystemPrompt,
    string Tone,
    string Style,
    IReadOnlyList<string> Priorities,
    IReadOnlyList<string> Expertise);
```

**Acceptance Criteria:**
- [ ] Record is immutable
- [ ] All properties are non-nullable (except where null is valid)
- [ ] Properties follow domain language
- [ ] No business logic in record

---

### ✅ 1.2: Repository Interface (ISP - Single Responsibility)

**Files to create:**
- [ ] `src/ShieldPrompt.Application/Interfaces/IRoleRepository.cs`

**Content:**
```csharp
public interface IRoleRepository
{
    IReadOnlyList<Role> GetAllRoles();
    Role? GetById(string roleId);
    Role GetDefault();
}
```

**Acceptance Criteria:**
- [ ] Interface has ≤ 5 methods (ISP compliant)
- [ ] Single responsibility: role data retrieval only
- [ ] No dependencies on infrastructure
- [ ] Returns domain types only

---

### ✅ 1.3: Role Repository Tests (TDD - RED Phase)

**Files to create:**
- [ ] `tests/ShieldPrompt.Tests.Unit/Application/Roles/RoleRepositoryTests.cs`

**Tests to write (BEFORE implementation):**
- [ ] `GetAllRoles_ReturnsAllDefinedRoles`
- [ ] `GetAllRoles_ReturnsAtLeast10Roles`
- [ ] `GetById_WithValidId_ReturnsRole`
- [ ] `GetById_WithInvalidId_ReturnsNull`
- [ ] `GetDefault_ReturnsEngineerRole`
- [ ] `AllRoles_HaveRequiredProperties_NotNullOrEmpty`
- [ ] `AllRoles_HaveUniqueIds`
- [ ] `AllRoles_HaveValidIcons_NotEmpty`
- [ ] `AllRoles_HaveSystemPrompts_AtLeast50Characters`
- [ ] `AllRoles_HavePriorities_AtLeastOne`

**Acceptance Criteria:**
- [ ] All tests written BEFORE implementation
- [ ] Tests use real implementation (not mocks)
- [ ] Tests use FluentAssertions
- [ ] Tests follow AAA pattern (Arrange, Act, Assert)
- [ ] All tests FAIL initially (RED phase)

---

### ✅ 1.4: Role Repository Implementation (TDD - GREEN Phase)

**Files to create:**
- [ ] `src/ShieldPrompt.Infrastructure/Services/YamlRoleRepository.cs`
- [ ] `config/roles.yaml`

**Implementation steps:**
- [ ] Create YAML file with 10+ roles
- [ ] Implement `YamlRoleRepository`
- [ ] Use `YamlDotNet` for parsing
- [ ] Handle file not found gracefully (fallback to embedded)
- [ ] Cache loaded roles (singleton pattern)

**Roles to define (minimum 10):**
1. [ ] 🔧 Software Engineer (default)
2. [ ] 🏗️ Software Architect
3. [ ] 🔐 Security Expert
4. [ ] 🧪 QA Engineer
5. [ ] 🚀 DevOps Engineer
6. [ ] 📝 Technical Writer
7. [ ] 🐛 Debugger
8. [ ] 👀 Code Reviewer
9. [ ] ⚡ Performance Engineer
10. [ ] 🎨 UX Engineer
11. [ ] ♻️ Refactoring Expert
12. [ ] 📊 Data Engineer

**Acceptance Criteria:**
- [ ] All 10 tests from 1.3 now PASS (GREEN phase)
- [ ] No test code changes (only production code)
- [ ] YAML file is well-formatted
- [ ] Fallback mechanism works if YAML missing

---

### ✅ 1.5: ViewModel Integration Tests (TDD - RED Phase)

**Files to create:**
- [ ] `tests/ShieldPrompt.Tests.Unit/ViewModels/MainWindowViewModelRoleTests.cs`

**Tests to write (BEFORE ViewModel changes):**
- [ ] `AvailableRoles_OnInitialization_LoadsAllRoles`
- [ ] `SelectedRole_OnStartup_IsDefaultEngineer`
- [ ] `SelectedRole_WhenChanged_UpdatesLivePreview`
- [ ] `SelectedRole_WhenChanged_UpdatesSystemPrompt`
- [ ] `SelectedRole_WithNullTemplate_ShowsRolePromptOnly`
- [ ] `SelectedRole_WithTemplate_MergesRoleAndTemplate`
- [ ] `GeneratePrompt_IncludesSelectedRolePrompt`

**Acceptance Criteria:**
- [ ] Tests use mocked `IRoleRepository`
- [ ] Tests verify reactive updates
- [ ] All tests FAIL initially (RED phase)

---

### ✅ 1.6: ViewModel Implementation (TDD - GREEN Phase)

**Files to modify:**
- [ ] `src/ShieldPrompt.App/ViewModels/MainWindowViewModel.cs`

**Changes to make:**
- [ ] Add `AvailableRoles` ObservableCollection
- [ ] Add `SelectedRole` ObservableProperty
- [ ] Add `RoleDescription` computed property
- [ ] Update constructor to accept `IRoleRepository`
- [ ] Load roles in `InitializeAsync()`
- [ ] Subscribe to `SelectedRole` changes
- [ ] Update `UpdateLivePreview()` to include role
- [ ] Update `CopyToClipboardAsync()` to include role

**Acceptance Criteria:**
- [ ] All 7 tests from 1.5 now PASS (GREEN phase)
- [ ] No new functionality beyond tests
- [ ] Constructor parameter count ≤ 13 (add 1)
- [ ] Follows existing patterns in ViewModel

---

### ✅ 1.7: UI Implementation (XAML)

**Files to modify:**
- [ ] `src/ShieldPrompt.App/Views/MainWindow.axaml`

**Changes to make:**
- [ ] Add Role dropdown to toolbar (after Format dropdown)
- [ ] Show icon + name in dropdown items
- [ ] Add role description panel below toolbar
- [ ] Bind `AvailableRoles` to dropdown
- [ ] Bind `SelectedRole` to selection
- [ ] Show/hide description panel based on selection

**UI Layout:**
```xml
<!-- In Toolbar -->
<TextBlock Text="Role:" VerticalAlignment="Center" Margin="10,0,5,0"/>
<ComboBox ItemsSource="{Binding AvailableRoles}"
          SelectedItem="{Binding SelectedRole}"
          Width="220">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="5">
                <TextBlock Text="{Binding Icon}"/>
                <TextBlock Text="{Binding Name}"/>
            </StackPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>

<!-- Role Description Panel (collapsible) -->
<Border IsVisible="{Binding !!SelectedRole}">
    <StackPanel>
        <TextBlock Text="{Binding SelectedRole.Description}"/>
        <TextBlock Text="{Binding SelectedRole.Tone, StringFormat='Tone: {0}'}"/>
    </StackPanel>
</Border>
```

**Acceptance Criteria:**
- [ ] Dropdown appears in toolbar
- [ ] Icons display correctly
- [ ] Description panel shows/hides
- [ ] Responsive layout maintained
- [ ] Tooltip shows full description

---

### ✅ 1.8: DI Registration

**Files to modify:**
- [ ] `src/ShieldPrompt.App/App.axaml.cs`

**Changes to make:**
- [ ] Register `IRoleRepository` → `YamlRoleRepository` (singleton)
- [ ] Update `MainWindowViewModel` constructor call

**Acceptance Criteria:**
- [ ] Service registered before ViewModel
- [ ] Follows existing DI pattern
- [ ] No circular dependencies

---

### ✅ 1.9: Integration Testing

**Manual testing checklist:**
- [ ] App starts without errors
- [ ] Role dropdown appears in toolbar
- [ ] Default role is "Software Engineer"
- [ ] All 12 roles appear in dropdown
- [ ] Selecting role shows description
- [ ] Live preview updates with role prompt
- [ ] Generate button includes role in prompt
- [ ] Role persists when changing templates
- [ ] Role works with focus areas

**Automated testing:**
- [ ] Run all tests: `dotnet test`
- [ ] All existing tests still pass
- [ ] 17 new tests pass (10 repo + 7 viewmodel)
- [ ] No flaky tests introduced

---

### ✅ 1.10: Documentation

**Files to update:**
- [ ] `docs/CHANGELOG.md`
- [ ] `README.md` (add role system section)

**Acceptance Criteria:**
- [ ] Changelog entry for v1.3.0
- [ ] README explains how to use roles
- [ ] README explains how to customize roles.yaml

---

## 🎯 PHASE 2: Format Pros/Cons Display

### ✅ 2.1: Domain Models (ISP)

**Files to create:**
- [ ] `src/ShieldPrompt.Domain/Records/PromptFormat.cs`

**Content:**
```csharp
public record PromptFormat(
    string Id,
    string Name,
    string Icon,
    string Description,
    IReadOnlyList<string> Pros,
    IReadOnlyList<string> Cons,
    string BestFor,
    TokenCostLevel TokenCost);

public enum TokenCostLevel
{
    Low,
    Medium,
    High
}
```

**Acceptance Criteria:**
- [ ] Record is immutable
- [ ] Enum for token cost
- [ ] Clear property names

---

### ✅ 2.2: Format Repository Interface (ISP)

**Files to create:**
- [ ] `src/ShieldPrompt.Application/Interfaces/IPromptFormatRepository.cs`

**Content:**
```csharp
public interface IPromptFormatRepository
{
    IReadOnlyList<PromptFormat> GetAllFormats();
    PromptFormat? GetById(string formatId);
    PromptFormat GetDefault();
}
```

**Acceptance Criteria:**
- [ ] ≤ 5 methods (ISP)
- [ ] Single responsibility
- [ ] Domain types only

---

### ✅ 2.3: Format Repository Tests (TDD - RED)

**Files to create:**
- [ ] `tests/ShieldPrompt.Tests.Unit/Application/Formats/PromptFormatRepositoryTests.cs`

**Tests to write:**
- [ ] `GetAllFormats_ReturnsAllDefinedFormats`
- [ ] `GetAllFormats_ReturnsAtLeast5Formats`
- [ ] `GetById_WithValidId_ReturnsFormat`
- [ ] `GetById_WithInvalidId_ReturnsNull`
- [ ] `GetDefault_ReturnsMarkdownFormat`
- [ ] `AllFormats_HaveProsCons_AtLeastOne`
- [ ] `AllFormats_HaveBestFor_NotEmpty`
- [ ] `AllFormats_HaveValidTokenCost`

**Acceptance Criteria:**
- [ ] All tests FAIL initially
- [ ] Tests written before implementation

---

### ✅ 2.4: Format Repository Implementation (TDD - GREEN)

**Files to create:**
- [ ] `src/ShieldPrompt.Infrastructure/Services/YamlPromptFormatRepository.cs`
- [ ] `config/prompt-formats.yaml`

**Formats to define (minimum 5):**
1. [ ] 📝 Markdown (default)
2. [ ] 🏷️ XML (Claude-optimized)
3. [ ] 📦 JSON (API/automation)
4. [ ] 🔄 Aider-style (Git workflows)
5. [ ] 📄 Plain Text (minimal)

**Acceptance Criteria:**
- [ ] All 8 tests from 2.3 now PASS
- [ ] YAML file complete
- [ ] Fallback mechanism works

---

### ✅ 2.5: ViewModel Integration Tests (TDD - RED)

**Files to create:**
- [ ] `tests/ShieldPrompt.Tests.Unit/ViewModels/MainWindowViewModelFormatTests.cs`

**Tests to write:**
- [ ] `AvailableFormats_OnInitialization_LoadsAllFormats`
- [ ] `SelectedFormat_OnStartup_IsMarkdown`
- [ ] `SelectedFormat_WhenChanged_ShowsProsAndCons`
- [ ] `SelectedFormat_WhenChanged_UpdatesPreview`
- [ ] `GeneratePrompt_UsesSelectedFormat`

**Acceptance Criteria:**
- [ ] All tests FAIL initially

---

### ✅ 2.6: ViewModel Implementation (TDD - GREEN)

**Files to modify:**
- [ ] `src/ShieldPrompt.App/ViewModels/MainWindowViewModel.cs`

**Changes to make:**
- [ ] Add `AvailableFormats` ObservableCollection
- [ ] Add `SelectedFormat` ObservableProperty
- [ ] Add `ShowFormatDetails` bool property
- [ ] Update constructor to accept `IPromptFormatRepository`
- [ ] Load formats in `InitializeAsync()`
- [ ] Subscribe to `SelectedFormat` changes
- [ ] Update prompt generation to use format

**Acceptance Criteria:**
- [ ] All 5 tests from 2.5 now PASS
- [ ] Constructor parameter count ≤ 14 (add 1)

---

### ✅ 2.7: UI Implementation (XAML)

**Files to modify:**
- [ ] `src/ShieldPrompt.App/Views/MainWindow.axaml`

**Changes to make:**
- [ ] Add Format dropdown to toolbar
- [ ] Add pros/cons panel that shows on selection
- [ ] Style pros/cons with icons (✅/⚠️)
- [ ] Show "Best for" section
- [ ] Show token cost indicator

**UI Layout:**
```xml
<!-- Format Dropdown -->
<ComboBox ItemsSource="{Binding AvailableFormats}"
          SelectedItem="{Binding SelectedFormat}"
          Width="150"/>

<!-- Pros/Cons Panel (popup or sidebar) -->
<Border IsVisible="{Binding ShowFormatDetails}">
    <StackPanel>
        <TextBlock Text="✅ Pros:" FontWeight="Bold"/>
        <ItemsControl ItemsSource="{Binding SelectedFormat.Pros}"/>
        
        <TextBlock Text="⚠️ Cons:" FontWeight="Bold"/>
        <ItemsControl ItemsSource="{Binding SelectedFormat.Cons}"/>
        
        <TextBlock Text="{Binding SelectedFormat.BestFor, StringFormat='💡 Best for: {0}'}"/>
    </StackPanel>
</Border>
```

**Acceptance Criteria:**
- [ ] Dropdown functional
- [ ] Pros/cons display correctly
- [ ] Toggle visibility works
- [ ] Icons render properly

---

### ✅ 2.8: DI Registration

**Files to modify:**
- [ ] `src/ShieldPrompt.App/App.axaml.cs`

**Changes to make:**
- [ ] Register `IPromptFormatRepository` → `YamlPromptFormatRepository`

**Acceptance Criteria:**
- [ ] Registration before ViewModel
- [ ] No circular dependencies

---

### ✅ 2.9: Integration Testing

**Manual testing checklist:**
- [ ] Format dropdown appears
- [ ] All 5 formats listed
- [ ] Default is Markdown
- [ ] Pros/cons show on selection
- [ ] Format affects output (eventually)
- [ ] Works with role selection

**Automated testing:**
- [ ] 13 new tests pass (8 repo + 5 viewmodel)
- [ ] All previous tests still pass

---

## 🎯 PHASE 3: Role Editor Tab

### ✅ 3.1: Extended Interface (ISP)

**Files to create:**
- [ ] `src/ShieldPrompt.Application/Interfaces/IRoleEditor.cs`

**Content:**
```csharp
public interface IRoleEditor
{
    Task<Result<Role>> CreateRoleAsync(Role role, CancellationToken cancellationToken = default);
    Task<Result<Role>> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetCustomRolesAsync(CancellationToken cancellationToken = default);
}
```

**Acceptance Criteria:**
- [ ] Separate interface for write operations (ISP!)
- [ ] Uses Result<T> pattern (no exceptions for flow)
- [ ] Async operations
- [ ] ≤ 5 methods

---

### ✅ 3.2: Role Editor Tests (TDD - RED)

**Files to create:**
- [ ] `tests/ShieldPrompt.Tests.Unit/Application/Roles/RoleEditorTests.cs`

**Tests to write:**
- [ ] `CreateRoleAsync_WithValidRole_SavesSuccessfully`
- [ ] `CreateRoleAsync_WithDuplicateId_ReturnsError`
- [ ] `CreateRoleAsync_WithInvalidData_ReturnsError`
- [ ] `UpdateRoleAsync_WithExistingRole_UpdatesSuccessfully`
- [ ] `UpdateRoleAsync_WithNonExistentRole_ReturnsError`
- [ ] `DeleteRoleAsync_WithCustomRole_DeletesSuccessfully`
- [ ] `DeleteRoleAsync_WithBuiltInRole_ReturnsError`
- [ ] `GetCustomRolesAsync_ReturnsOnlyUserCreated`
- [ ] `CreateRoleAsync_PersistsToFile`
- [ ] `UpdateRoleAsync_UpdatesFileCorrectly`

**Acceptance Criteria:**
- [ ] All tests FAIL initially
- [ ] Tests cover success and error paths
- [ ] Tests verify persistence

---

### ✅ 3.3: Role Editor Implementation (TDD - GREEN)

**Files to create:**
- [ ] `src/ShieldPrompt.Infrastructure/Services/YamlRoleEditor.cs`

**Implementation details:**
- [ ] Save custom roles to `~/.config/ShieldPrompt/custom-roles.yaml`
- [ ] Separate from built-in roles
- [ ] Validate role data before saving
- [ ] Prevent editing/deleting built-in roles
- [ ] Thread-safe file operations

**Acceptance Criteria:**
- [ ] All 10 tests from 3.2 now PASS
- [ ] File I/O is atomic (no corruption)
- [ ] Validation comprehensive

---

### ✅ 3.4: ViewModel for Role Editor (TDD - RED)

**Files to create:**
- [ ] `src/ShieldPrompt.App/ViewModels/RoleEditorViewModel.cs`
- [ ] `tests/ShieldPrompt.Tests.Unit/ViewModels/RoleEditorViewModelTests.cs`

**Tests to write:**
- [ ] `LoadRoles_OnInitialize_LoadsAllRoles`
- [ ] `NewRoleCommand_CreatesNewRole`
- [ ] `SaveRoleCommand_WithValidData_Saves`
- [ ] `SaveRoleCommand_WithInvalidData_ShowsError`
- [ ] `DeleteRoleCommand_ConfirmsBeforeDelete`
- [ ] `DeleteRoleCommand_WithBuiltIn_ShowsError`
- [ ] `SelectedRole_WhenChanged_LoadsDetails`
- [ ] `IsDirty_TracksChanges`

**ViewModel properties:**
- [ ] `AvailableRoles` ObservableCollection
- [ ] `SelectedRole` Role?
- [ ] `EditingRole` Role (working copy)
- [ ] `IsNew` bool
- [ ] `IsDirty` bool
- [ ] `StatusMessage` string
- [ ] Commands: New, Save, Delete, Cancel

**Acceptance Criteria:**
- [ ] All tests FAIL initially
- [ ] ViewModel follows MVVM pattern
- [ ] ISP compliant (uses IRoleEditor)

---

### ✅ 3.5: ViewModel Implementation (TDD - GREEN)

**Files to implement:**
- [ ] Complete `RoleEditorViewModel.cs`

**Acceptance Criteria:**
- [ ] All 8 tests from 3.4 now PASS
- [ ] Proper error handling
- [ ] Validation logic

---

### ✅ 3.6: UI Implementation (XAML)

**Files to create:**
- [ ] `src/ShieldPrompt.App/Views/RoleEditorWindow.axaml`
- [ ] `src/ShieldPrompt.App/Views/RoleEditorWindow.axaml.cs`

**UI Layout:**
```
┌─────────────────────────────────────────────────────┐
│ Role Editor                                    [×]  │
├─────────────────────────────────────────────────────┤
│ ┌─────────────┬─────────────────────────────────┐  │
│ │ Roles       │ Edit Role                       │  │
│ ├─────────────┤                                 │  │
│ │ 🔧 Engineer │ Name: [               ]         │  │
│ │ 🏗️ Architect│ Icon: [🔧 ▼]                   │  │
│ │ 🔐 Security │                                 │  │
│ │ ...         │ System Prompt:                  │  │
│ │             │ ┌─────────────────────────────┐ │  │
│ │ [New]       │ │                             │ │  │
│ │             │ │                             │ │  │
│ │             │ └─────────────────────────────┘ │  │
│ │             │                                 │  │
│ │             │ Priorities:                     │  │
│ │             │ [Add Priority]                  │  │
│ │             │                                 │  │
│ │             │ [Save] [Delete] [Cancel]        │  │
│ └─────────────┴─────────────────────────────────┘  │
├─────────────────────────────────────────────────────┤
│ Status: Ready                                       │
└─────────────────────────────────────────────────────┘
```

**Components:**
- [ ] Role list (left panel)
- [ ] Editor panel (right side)
- [ ] Name, icon, description inputs
- [ ] System prompt TextBox (multiline)
- [ ] Priorities list (add/remove)
- [ ] Expertise list (add/remove)
- [ ] Save, Delete, Cancel buttons
- [ ] Status bar

**Acceptance Criteria:**
- [ ] Responsive layout
- [ ] Data binding works
- [ ] Validation feedback visible
- [ ] Confirmation dialogs for destructive actions

---

### ✅ 3.7: Menu Integration

**Files to modify:**
- [ ] `src/ShieldPrompt.App/Views/MainWindow.axaml`

**Changes to make:**
- [ ] Add "Role Editor..." to Tools menu
- [ ] Bind to `OpenRoleEditorCommand` in MainWindowViewModel
- [ ] Keyboard shortcut: `Ctrl+Shift+R`

**Acceptance Criteria:**
- [ ] Menu item appears
- [ ] Command opens editor window
- [ ] Keyboard shortcut works

---

### ✅ 3.8: DI Registration

**Files to modify:**
- [ ] `src/ShieldPrompt.App/App.axaml.cs`

**Changes to make:**
- [ ] Register `IRoleEditor` → `YamlRoleEditor`
- [ ] Register `RoleEditorViewModel` (scoped or transient)

**Acceptance Criteria:**
- [ ] Services registered correctly
- [ ] ViewModel gets dependencies

---

### ✅ 3.9: Integration Testing

**Manual testing checklist:**
- [ ] Open role editor from menu
- [ ] List shows all roles
- [ ] Built-in roles are read-only (gray out delete)
- [ ] Create new custom role
- [ ] Edit custom role
- [ ] Delete custom role
- [ ] Custom role appears in main dropdown
- [ ] Cancel discards changes
- [ ] Validation works (empty name = error)
- [ ] File persists on disk

**Automated testing:**
- [ ] 18 new tests pass (10 editor + 8 viewmodel)
- [ ] All previous tests still pass

---

## 📊 PROGRESS TRACKING

### Phase 1: Role System
- [ ] 1.1 Domain Models
- [ ] 1.2 Repository Interface
- [ ] 1.3 Repository Tests (RED)
- [ ] 1.4 Repository Implementation (GREEN)
- [ ] 1.5 ViewModel Tests (RED)
- [ ] 1.6 ViewModel Implementation (GREEN)
- [ ] 1.7 UI Implementation
- [ ] 1.8 DI Registration
- [ ] 1.9 Integration Testing
- [ ] 1.10 Documentation

**Estimated Time:** 1-2 days  
**Tests Added:** 17

---

### Phase 2: Format Pros/Cons
- [ ] 2.1 Domain Models
- [ ] 2.2 Format Repository Interface
- [ ] 2.3 Format Repository Tests (RED)
- [ ] 2.4 Format Repository Implementation (GREEN)
- [ ] 2.5 ViewModel Tests (RED)
- [ ] 2.6 ViewModel Implementation (GREEN)
- [ ] 2.7 UI Implementation
- [ ] 2.8 DI Registration
- [ ] 2.9 Integration Testing

**Estimated Time:** 1 day  
**Tests Added:** 13

---

### Phase 3: Role Editor
- [ ] 3.1 Extended Interface
- [ ] 3.2 Role Editor Tests (RED)
- [ ] 3.3 Role Editor Implementation (GREEN)
- [ ] 3.4 ViewModel Tests (RED)
- [ ] 3.5 ViewModel Implementation (GREEN)
- [ ] 3.6 UI Implementation
- [ ] 3.7 Menu Integration
- [ ] 3.8 DI Registration
- [ ] 3.9 Integration Testing

**Estimated Time:** 2-3 days  
**Tests Added:** 18

---

## 📈 TOTALS

**Total Subtasks:** 78  
**Total Tests:** 48 new tests  
**Total Time:** 4-6 days  
**Files Created:** ~15  
**Files Modified:** ~5

---

## ✅ DEFINITION OF DONE

### For Each Phase:
- [ ] All tests written BEFORE implementation (TDD)
- [ ] All tests passing (GREEN)
- [ ] All interfaces ISP-compliant (≤ 10 methods, ideally ≤ 5)
- [ ] No code duplication (DRY)
- [ ] XML documentation on public APIs
- [ ] No linter warnings introduced
- [ ] Manual testing completed
- [ ] Integration with existing features verified
- [ ] Documentation updated

### For Complete Feature:
- [ ] All 3 phases complete
- [ ] 48 new tests passing
- [ ] 0 existing tests broken
- [ ] App runs without errors
- [ ] User can use all features
- [ ] README updated
- [ ] CHANGELOG updated
- [ ] Ready for v1.3.0 release

---

## 🚀 READY TO START?

**Current Status:** ✅ CHECKLIST COMPLETE - AWAITING GO SIGNAL

**Next Step:** Confirm which phase(s) to implement, then begin with Phase 1.1.

**Command to start:** Say "Let us continue" and I'll begin with TDD + ISP implementation!

---

**Last Updated:** January 15, 2026  
**Status:** READY FOR IMPLEMENTATION

