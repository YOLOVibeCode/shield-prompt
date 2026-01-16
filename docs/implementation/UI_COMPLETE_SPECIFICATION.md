# ShieldPrompt v2.0 Complete UI Specification

**Version:** 2.0.0  
**Last Updated:** January 15, 2026  
**Status:** AUTHORITATIVE REFERENCE

---

## Table of Contents

1. [Windows & Forms](#1-windows--forms)
2. [Menu Bar](#2-menu-bar)
3. [Toolbar](#3-toolbar)
4. [File Tree Panel](#4-file-tree-panel)
5. [Preview Panel](#5-preview-panel)
6. [Instructions Panel](#6-instructions-panel)
7. [Action Bar](#7-action-bar)
8. [Status Bar](#8-status-bar)
9. [Tab Bar](#9-tab-bar)
10. [Dialogs](#10-dialogs)
11. [Context Menus](#11-context-menus)
12. [Keyboard Shortcuts](#12-keyboard-shortcuts)
13. [Mouse Operations](#13-mouse-operations)
14. [Drag & Drop](#14-drag--drop)

---

## 1. Windows & Forms

### 1.1 MainWindowV2 (Primary Window)

| Property | Value |
|----------|-------|
| **Title** | ShieldPrompt v2.0 |
| **Default Size** | 1200 × 800 |
| **Minimum Size** | 800 × 600 |
| **Startup Position** | Center Screen |
| **Resizable** | Yes |
| **Maximizable** | Yes |
| **Minimizable** | Yes |

**Layout Structure:**
```
┌─────────────────────────────────────────────────────────────────────────┐
│ [Menu Bar]                                                               │
├─────────────────────────────────────────────────────────────────────────┤
│ [Toolbar]                                                                │
├─────────────────────────────────────────────────────────────────────────┤
│ [Tab Bar]                                                                │
├────────────────┬────────────────────────────────────────────────────────┤
│                │                                                        │
│  [File Tree]   │  [Preview Panel]                                       │
│                │                                                        │
│   280px        │  ────────────────────────────────────────────────────  │
│   (resizable)  │  [Instructions Panel]                                  │
│                │                                                        │
├────────────────┴────────────────────────────────────────────────────────┤
│ [Action Bar]                                                             │
├─────────────────────────────────────────────────────────────────────────┤
│ [Status Bar]                                                             │
└─────────────────────────────────────────────────────────────────────────┘
```

**ViewModel:** `MainWindowV2ViewModel`

**TDD Tests Required:**
- [ ] `MainWindowV2_OnStartup_ShowsCorrectTitle`
- [ ] `MainWindowV2_OnResize_MaintainsMinimumSize`
- [ ] `MainWindowV2_OnClose_SavesLayoutState`
- [ ] `MainWindowV2_OnStartup_RestoresLayoutState`

---

### 1.2 Settings Window

| Property | Value |
|----------|-------|
| **Title** | Settings |
| **Size** | 600 × 500 |
| **Modal** | Yes |
| **Resizable** | No |

**Tabs:**
1. General
2. Roles
3. Output Format
4. Sanitization
5. Appearance
6. Keyboard Shortcuts
7. About

**ViewModel:** `SettingsViewModel`

**Operations per Tab:**

#### General Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| Set Default Model | Choose default AI model | `SetDefaultModel(string modelId)` |
| Set Default Role | Choose default role | `SetDefaultRole(string roleId)` |
| Toggle Auto-Backup | Enable/disable backups | `SetAutoBackup(bool enabled)` |
| Set Backup Location | Choose backup folder | `SetBackupLocation(string path)` |
| Clear Recent | Clear recent workspaces | `ClearRecentWorkspaces()` |

#### Roles Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| View Built-in Roles | Browse built-in roles | `LoadBuiltInRoles()` |
| Create Custom Role | Add new custom role | `CreateCustomRole()` |
| Edit Custom Role | Modify custom role | `EditCustomRole(string roleId)` |
| Delete Custom Role | Remove custom role | `DeleteCustomRole(string roleId)` |
| Import Role | Import from YAML | `ImportRole(string filePath)` |
| Export Role | Export to YAML | `ExportRole(string roleId, string path)` |

#### Output Format Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| Set Format Mode | action-only, with-summary, full | `SetFormatMode(FormatMode mode)` |
| Toggle Partial Updates | Enable/disable partial updates | `SetPartialUpdates(bool enabled)` |
| Set Conflict Resolution | Choose conflict strategy | `SetConflictResolution(ConflictStrategy strategy)` |

#### Sanitization Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| View Patterns | List sanitization patterns | `LoadPatterns()` |
| Add Pattern | Create new pattern | `AddPattern()` |
| Edit Pattern | Modify pattern | `EditPattern(string patternId)` |
| Delete Pattern | Remove pattern | `DeletePattern(string patternId)` |
| Toggle Category | Enable/disable category | `ToggleCategory(PatternCategory category)` |

#### Appearance Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| Set Theme | Light/Dark/System | `SetTheme(Theme theme)` |
| Set Font Size | Preview font size | `SetFontSize(int size)` |
| Set Font Family | Preview font family | `SetFontFamily(string family)` |
| Toggle Line Numbers | Show/hide line numbers | `SetLineNumbers(bool show)` |

#### Keyboard Shortcuts Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| View Shortcuts | List all shortcuts | `LoadShortcuts()` |
| Edit Shortcut | Modify shortcut | `EditShortcut(string actionId)` |
| Reset Shortcuts | Restore defaults | `ResetShortcuts()` |

#### About Tab
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| View Version | Show version info | (display only) |
| View License | Show license | (display only) |
| Check Updates | Check for updates | `CheckForUpdates()` |
| View GitHub | Open GitHub page | `OpenGitHub()` |

---

### 1.3 Apply Dashboard (Overlay/Panel)

| Property | Value |
|----------|-------|
| **Type** | Modal Overlay |
| **Size** | 80% of window |
| **Dismissible** | Yes (Escape key) |

**ViewModel:** `ApplyDashboardViewModel`

**Operations:**
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| Parse Response | Parse LLM response | `ParseResponseAsync()` |
| Preview Changes | Generate diff preview | `GeneratePreviewAsync()` |
| Select Operation | Toggle operation selection | `ToggleSelection(FileOperation op)` |
| Select All | Select all operations | `SelectAll()` |
| Deselect All | Deselect all operations | `DeselectAll()` |
| Apply All | Apply all selected | `ApplyAllAsync()` |
| Apply Selected | Apply only selected | `ApplySelectedAsync()` |
| View Diff | Show detailed diff | `ViewDiff(FileOperation op)` |
| Resolve Conflict | Handle conflict | `ResolveConflict(ConflictInfo conflict)` |
| Dismiss | Close dashboard | `Dismiss()` |

---

### 1.4 Preset Library (Panel/Flyout)

| Property | Value |
|----------|-------|
| **Type** | Side Panel or Flyout |
| **Width** | 320px |
| **Position** | Right side |

**ViewModel:** `PresetLibraryViewModel`

**Operations:**
| Operation | Description | ViewModel Method |
|-----------|-------------|------------------|
| Load Presets | Fetch all presets | `LoadPresetsAsync()` |
| Search Presets | Filter by query | `SearchPresets(string query)` |
| Apply Preset | Apply to current session | `ApplyPresetAsync(PromptPreset preset)` |
| Pin Preset | Pin for quick access | `TogglePinAsync(PromptPreset preset)` |
| Duplicate Preset | Clone preset | `DuplicatePresetAsync(PromptPreset preset)` |
| Delete Preset | Remove preset | `DeletePresetAsync(PromptPreset preset)` |
| Export Preset | Export to JSON | `ExportPreset(PromptPreset preset)` |
| Import Preset | Import from JSON | `ImportPreset()` |

---

## 2. Menu Bar

### 2.1 File Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **Open Folder...** | Ctrl+O | Open folder dialog | `OpenFolderCommand` |
| **Open Recent** | → | Submenu of recent folders | - |
| ├─ _Recent Item 1_ | - | Open recent workspace | `OpenRecentCommand(path)` |
| ├─ _Recent Item 2_ | - | Open recent workspace | `OpenRecentCommand(path)` |
| └─ Clear Recent | - | Clear recent list | `ClearRecentCommand` |
| **Refresh** | Ctrl+R | Reload current folder | `RefreshCommand` |
| **Close Folder** | Ctrl+W | Close current workspace | `CloseFolderCommand` |
| --- | - | Separator | - |
| **Save Preset...** | Ctrl+Shift+S | Save as preset | `SavePresetCommand` |
| **Load Preset** | → | Submenu of presets | - |
| --- | - | Separator | - |
| **Settings...** | Ctrl+, | Open settings | `OpenSettingsCommand` |
| --- | - | Separator | - |
| **Exit** | Alt+F4 | Close application | `ExitCommand` |

**TDD Tests:**
- [ ] `FileMenu_OpenFolder_OpensDialog`
- [ ] `FileMenu_OpenRecent_LoadsWorkspace`
- [ ] `FileMenu_Refresh_ReloadsFileTree`
- [ ] `FileMenu_SavePreset_OpensDialog`

---

### 2.2 Edit Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **Undo** | Ctrl+Z | Undo last action | `UndoCommand` |
| **Redo** | Ctrl+Y | Redo last undone | `RedoCommand` |
| --- | - | Separator | - |
| **Copy Prompt** | Ctrl+C | Copy to clipboard | `CopyToClipboardCommand` |
| **Paste Response** | Ctrl+V | Paste LLM response | `PasteResponseCommand` |
| --- | - | Separator | - |
| **Select All Files** | Ctrl+A | Select all in tree | `SelectAllFilesCommand` |
| **Deselect All Files** | Ctrl+Shift+A | Deselect all | `DeselectAllFilesCommand` |
| **Invert Selection** | Ctrl+I | Toggle all selections | `InvertSelectionCommand` |
| --- | - | Separator | - |
| **Find File...** | Ctrl+F | Focus file search | `FocusSearchCommand` |

**TDD Tests:**
- [ ] `EditMenu_Undo_RevertsLastAction`
- [ ] `EditMenu_SelectAll_SelectsAllFiles`
- [ ] `EditMenu_CopyPrompt_CopiesToClipboard`

---

### 2.3 View Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **File Tree** | Ctrl+1 | Show/focus file tree | `FocusFileTreeCommand` |
| **Preview** | Ctrl+2 | Show/focus preview | `FocusPreviewCommand` |
| **Instructions** | Ctrl+3 | Show/focus instructions | `FocusInstructionsCommand` |
| --- | - | Separator | - |
| **Collapse File Tree** | - | Collapse left panel | `ToggleFileTreeCommand` |
| **Collapse Instructions** | - | Collapse bottom panel | `ToggleInstructionsCommand` |
| --- | - | Separator | - |
| **Zoom In** | Ctrl+= | Increase font size | `ZoomInCommand` |
| **Zoom Out** | Ctrl+- | Decrease font size | `ZoomOutCommand` |
| **Reset Zoom** | Ctrl+0 | Reset to default | `ResetZoomCommand` |
| --- | - | Separator | - |
| **Toggle Theme** | Ctrl+T | Switch light/dark | `ToggleThemeCommand` |
| **Full Screen** | F11 | Toggle full screen | `ToggleFullScreenCommand` |

**TDD Tests:**
- [ ] `ViewMenu_CollapseFileTree_HidesPanel`
- [ ] `ViewMenu_ZoomIn_IncreasesFontSize`
- [ ] `ViewMenu_ToggleTheme_SwitchesTheme`

---

### 2.4 Selection Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **Select Modified Files** | Ctrl+M | Select git modified | `SelectModifiedCommand` |
| **Select Staged Files** | Ctrl+Shift+M | Select git staged | `SelectStagedCommand` |
| **Select by Pattern...** | Ctrl+G | Glob pattern dialog | `SelectByPatternCommand` |
| --- | - | Separator | - |
| **Expand All** | Ctrl+Shift+E | Expand all folders | `ExpandAllCommand` |
| **Collapse All** | Ctrl+Shift+C | Collapse all folders | `CollapseAllCommand` |
| --- | - | Separator | - |
| **Ignore Pattern...** | - | Add to ignore list | `AddIgnorePatternCommand` |
| **Clear Ignored** | - | Clear custom ignores | `ClearIgnoredCommand` |

**TDD Tests:**
- [ ] `SelectionMenu_SelectModified_SelectsGitModifiedFiles`
- [ ] `SelectionMenu_SelectByPattern_OpensDialog`
- [ ] `SelectionMenu_ExpandAll_ExpandsAllNodes`

---

### 2.5 Tabs Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **New Tab** | Ctrl+T | Create new session | `NewTabCommand` |
| **Close Tab** | Ctrl+W | Close current tab | `CloseTabCommand` |
| **Close Other Tabs** | - | Close all except current | `CloseOtherTabsCommand` |
| **Close All Tabs** | - | Close all tabs | `CloseAllTabsCommand` |
| --- | - | Separator | - |
| **Duplicate Tab** | Ctrl+Shift+D | Clone current tab | `DuplicateTabCommand` |
| **Pin Tab** | - | Pin/unpin current | `PinTabCommand` |
| --- | - | Separator | - |
| **Next Tab** | Ctrl+Tab | Switch to next | `NextTabCommand` |
| **Previous Tab** | Ctrl+Shift+Tab | Switch to previous | `PreviousTabCommand` |
| **Go to Tab 1-9** | Ctrl+1-9 | Jump to tab number | `GoToTabCommand(int index)` |

**TDD Tests:**
- [ ] `TabsMenu_NewTab_CreatesNewSession`
- [ ] `TabsMenu_CloseTab_ClosesCurrentSession`
- [ ] `TabsMenu_DuplicateTab_ClonesState`

---

### 2.6 Tools Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **Clear Sanitization Session** | - | Reset mappings | `ClearSanitizationCommand` |
| **Export Mappings...** | - | Export sanitization map | `ExportMappingsCommand` |
| --- | - | Separator | - |
| **Validate Response Format** | - | Check LLM response | `ValidateResponseCommand` |
| --- | - | Separator | - |
| **Show Backup History** | - | View backups | `ShowBackupHistoryCommand` |
| **Restore from Backup...** | - | Restore files | `RestoreBackupCommand` |
| --- | - | Separator | - |
| **Open in Terminal** | - | Open workspace in terminal | `OpenTerminalCommand` |
| **Open in File Explorer** | - | Open in OS file manager | `OpenExplorerCommand` |

**TDD Tests:**
- [ ] `ToolsMenu_ClearSanitization_ResetsMappings`
- [ ] `ToolsMenu_RestoreBackup_RestoresFiles`

---

### 2.7 Help Menu

| Menu Item | Shortcut | Description | Command |
|-----------|----------|-------------|---------|
| **Documentation** | F1 | Open online docs | `OpenDocsCommand` |
| **Keyboard Shortcuts** | Ctrl+/ | Show shortcuts | `ShowShortcutsCommand` |
| --- | - | Separator | - |
| **Report Issue** | - | Open GitHub issues | `ReportIssueCommand` |
| **Request Feature** | - | Open feature request | `RequestFeatureCommand` |
| --- | - | Separator | - |
| **Check for Updates** | - | Check for new version | `CheckUpdatesCommand` |
| **About ShieldPrompt** | - | Show about dialog | `ShowAboutCommand` |

---

## 3. Toolbar

### 3.1 Primary Toolbar

```
[📁 Open] [🔄 Refresh] | [Workspace ▾] [Role ▾] [Model ▾] | [⚙️ Settings]
```

| Button | Icon | Tooltip | Command | Enabled Condition |
|--------|------|---------|---------|-------------------|
| Open | 📁 | Open Folder (Ctrl+O) | `OpenFolderCommand` | Always |
| Refresh | 🔄 | Refresh (Ctrl+R) | `RefreshCommand` | Workspace loaded |
| Workspace | 📂 | Select Workspace | Dropdown | Always |
| Role | 🎭 | Select Role | Dropdown | Always |
| Model | 🤖 | Select AI Model | Dropdown | Always |
| Settings | ⚙️ | Settings (Ctrl+,) | `OpenSettingsCommand` | Always |

### 3.2 Workspace Dropdown

| Item | Description |
|------|-------------|
| Current workspace name | Displays current selection |
| --- | Separator |
| Recent workspace 1 | `SelectWorkspaceCommand(id)` |
| Recent workspace 2 | `SelectWorkspaceCommand(id)` |
| ... | |
| --- | Separator |
| Open New... | `OpenFolderCommand` |
| Clear Recent | `ClearRecentCommand` |

### 3.3 Role Dropdown

| Item | Description |
|------|-------------|
| 🔧 General Review | Default role |
| 🔒 Security Expert | Security-focused |
| 🐛 Debug Assistant | Debugging focus |
| 📐 Architecture Reviewer | Architecture focus |
| ⚡ Performance Analyst | Performance focus |
| 📝 Documentation Writer | Documentation focus |
| --- | Separator |
| Custom roles... | User-defined roles |
| --- | Separator |
| Manage Roles... | Opens settings |

**Hover behavior:** Shows role description tooltip

### 3.4 Model Dropdown

| Item | Context Limit |
|------|---------------|
| GPT-4o | 128K |
| GPT-4o Mini | 128K |
| Claude 3.5 Sonnet | 200K |
| Claude 3 Opus | 200K |
| Gemini 2.5 Pro | 1M |

---

## 4. File Tree Panel

### 4.1 Panel Structure

```
┌────────────────────────────────────────┐
│ 🔍 [Search files...                  ] │
├────────────────────────────────────────┤
│ ☑ 📂 src/                         +120 │
│   ├ ☑ 📁 components/               +45 │
│   │   ├ ☑ 🔷 App.cs                 23 │
│   │   └ ☑ 🔷 User.cs                18 │
│   └ ☐ 📁 tests/                        │
│       └ ☐ 🔷 AppTests.cs            15 │
│ ☑ 📄 README.md                       8 │
├────────────────────────────────────────┤
│ 3 files selected │ 2,847 tokens        │
└────────────────────────────────────────┘
```

### 4.2 File Tree Operations

| Operation | Trigger | Command |
|-----------|---------|---------|
| Select/Deselect File | Click checkbox | `ToggleFileSelection(FileNode)` |
| Select Folder | Click folder checkbox | `ToggleFolderSelection(FileNode)` |
| Expand Folder | Click folder | `ToggleFolderExpansion(FileNode)` |
| View File Content | Double-click file | `ViewFileContent(FileNode)` |
| Search Files | Type in search box | `SearchFiles(string query)` |
| Clear Search | Click X or Escape | `ClearSearch()` |

### 4.3 Cascading Selection Logic

| Action | Result |
|--------|--------|
| Check folder | All children become checked |
| Uncheck folder | All children become unchecked |
| Check all children | Parent becomes checked |
| Check some children | Parent becomes indeterminate (◐) |
| Uncheck all children | Parent becomes unchecked |

### 4.4 File Tree Node Display

| Element | Description |
|---------|-------------|
| Checkbox | Three-state: ☑ checked, ☐ unchecked, ◐ indeterminate |
| Icon | Folder (📁/📂) or file type icon |
| Name | File/folder name |
| Extension | File extension (dimmed) |
| Token Count | Per-file token estimate |
| Git Status | M (modified), A (added), ? (untracked) |

### 4.5 Filter Options

| Filter | Description | Command |
|--------|-------------|---------|
| Hide ignored | Hide .gitignore files | `ToggleIgnoredFilter()` |
| Hide binary | Hide binary files | `ToggleBinaryFilter()` |
| Show modified only | Only git modified | `ShowModifiedOnly()` |

**TDD Tests:**
- [ ] `FileTree_SelectFolder_SelectsAllChildren`
- [ ] `FileTree_DeselectChild_MakesParentIndeterminate`
- [ ] `FileTree_Search_FiltersMatchingFiles`
- [ ] `FileTree_ToggleExpand_ExpandsOrCollapsesFolder`

---

## 5. Preview Panel

### 5.1 Panel Structure

```
┌────────────────────────────────────────────────────────────┐
│ 📄 LIVE PREVIEW                    (2,847 tokens) [📋 Copy]│
├────────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────────────────┐ │
│ │ # Code Review Request                                  │ │
│ │                                                        │ │
│ │ **Role:** 🔧 Software Engineer                         │ │
│ │ **Task:** General code review                          │ │
│ │ **Files:** 3 selected (2,847 tokens)                   │ │
│ │                                                        │ │
│ │ ## Files Included                                      │ │
│ │                                                        │ │
│ │ ### `src/App.cs`                                       │ │
│ │ ```csharp                                              │ │
│ │ public class App { ... }                               │ │
│ │ ```                                                    │ │
│ │                                                        │ │
│ │ ## Instructions                                        │ │
│ │ Please review the code for...                          │ │
│ └────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
```

### 5.2 Preview Operations

| Operation | Trigger | Command |
|-----------|---------|---------|
| Copy Preview | Click Copy button | `CopyPreviewCommand` |
| Click to Copy | Click anywhere in preview | `CopyPreviewCommand` |
| Scroll | Mouse wheel | Native scroll |
| Select Text | Drag to select | Native selection |
| Copy Selection | Ctrl+C on selection | Native copy |

### 5.3 Real-time Update Triggers

Preview updates automatically when:
- Files selected/deselected
- Role changed
- Model changed
- Custom instructions changed
- Template changed

**Debounce:** 300ms after last change

**TDD Tests:**
- [ ] `Preview_OnFileSelection_UpdatesContent`
- [ ] `Preview_OnRoleChange_UpdatesPrompt`
- [ ] `Preview_ClickToCopy_CopiesToClipboard`
- [ ] `Preview_UpdatesWithDebounce_PreventsExcessiveUpdates`

---

## 6. Instructions Panel

### 6.1 Panel Structure

```
┌────────────────────────────────────────────────────────────┐
│ 📝 Custom Instructions (optional)                          │
├────────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────────────────┐ │
│ │ Focus on error handling and edge cases.                │ │
│ │ Suggest improvements for performance.                  │ │
│ │ |                                                      │ │
│ └────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
```

### 6.2 Instructions Operations

| Operation | Trigger | Command |
|-----------|---------|---------|
| Type Instructions | Keyboard input | Updates `CustomInstructions` property |
| Clear Instructions | Click X or select all + delete | `ClearInstructionsCommand` |
| Insert Template | Ctrl+Space | Shows instruction snippets |
| Undo/Redo | Ctrl+Z / Ctrl+Y | Native text undo |

### 6.3 Instruction Snippets (Ctrl+Space)

| Snippet | Inserted Text |
|---------|---------------|
| Security Focus | "Focus on security vulnerabilities and best practices." |
| Performance Focus | "Analyze for performance optimizations." |
| Error Handling | "Review error handling and edge cases." |
| Documentation | "Suggest documentation improvements." |
| Refactoring | "Suggest refactoring opportunities." |
| Testing | "Suggest additional test cases." |

**TDD Tests:**
- [ ] `Instructions_OnType_UpdatesPreview`
- [ ] `Instructions_InsertSnippet_InsertsAtCursor`

---

## 7. Action Bar

### 7.1 Bar Structure

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ☑ 🔐 Sanitize │ 12 values sanitized │ [📋 Copy to Clipboard] [📥 Paste] │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Action Bar Elements

| Element | Type | Description | Command |
|---------|------|-------------|---------|
| Sanitize Checkbox | CheckBox | Enable/disable sanitization | `ToggleSanitization()` |
| Sanitization Count | Text | "N values sanitized" | (display only) |
| Copy to Clipboard | Button (Primary) | Copy sanitized prompt | `CopyToClipboardCommand` |
| Paste Response | Button | Paste and parse LLM response | `PasteResponseCommand` |

### 7.3 Sanitization Indicator

| State | Display |
|-------|---------|
| Enabled, 0 values | "🔐 Sanitization enabled" |
| Enabled, N values | "🔐 N values sanitized" |
| Disabled | "⚠️ Sanitization disabled" |

**TDD Tests:**
- [ ] `ActionBar_ToggleSanitization_UpdatesState`
- [ ] `ActionBar_CopyToClipboard_SanitizesAndCopies`
- [ ] `ActionBar_PasteResponse_OpensDashboard`

---

## 8. Status Bar

### 8.1 Bar Structure

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ✅ Ready │ 📁 3/15 files │ ⎇ main │ 2,847 / 128,000 ████░░░ │ Session: 4h│
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Status Bar Elements

| Element | Description | Update Trigger |
|---------|-------------|----------------|
| Status Message | Current operation status | On operation start/complete |
| File Count | "N/M files" selected/total | On selection change |
| Git Branch | "⎇ branchname" | On workspace load |
| Token Usage | "N / limit" with progress bar | On selection change |
| Model Name | Current model name | On model change |
| Session Timer | "Session: Xh Ym" | Every minute |

### 8.3 Status Message States

| Status | Color | Example |
|--------|-------|---------|
| Info | White | "Ready" |
| Success | Green | "✅ Copied to clipboard!" |
| Warning | Yellow | "⚠️ Large file included" |
| Error | Red | "❌ Error: File not found" |
| Progress | Blue | "Loading..." with spinner |

### 8.4 Token Usage Indicator

| Usage | Color | Warning |
|-------|-------|---------|
| 0-50% | Green | None |
| 50-80% | Yellow | None |
| 80-95% | Orange | "Approaching limit" |
| 95-100% | Red | "Near/at limit" |

**TDD Tests:**
- [ ] `StatusBar_OnFileSelection_UpdatesFileCount`
- [ ] `StatusBar_OnTokenChange_UpdatesProgress`
- [ ] `StatusBar_OnError_ShowsErrorMessage`

---

## 9. Tab Bar

### 9.1 Bar Structure

```
┌─────────────────────────────────────────────────────────────────────────┐
│ [📌 Prompt 1] [● Prompt 2 ✕] [Prompt 3 ✕]                          [+] │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Tab Elements

| Element | Description |
|---------|-------------|
| Pin Icon | 📌 shown if pinned |
| Dirty Indicator | ● shown if unsaved changes |
| Tab Name | Session name |
| Close Button | ✕ to close (hidden if pinned) |
| New Tab Button | + to create new tab |

### 9.3 Tab Operations

| Operation | Trigger | Command |
|-----------|---------|---------|
| Switch Tab | Click tab | `SelectTabCommand(tab)` |
| Close Tab | Click ✕ or Ctrl+W | `CloseTabCommand(tab)` |
| New Tab | Click + or Ctrl+T | `NewTabCommand` |
| Rename Tab | Double-click name | `RenameTabCommand(tab)` |
| Pin/Unpin Tab | Right-click → Pin | `TogglePinCommand(tab)` |
| Reorder Tab | Drag and drop | `ReorderTabCommand(from, to)` |

### 9.4 Tab Context Menu

| Item | Description | Command |
|------|-------------|---------|
| Close | Close this tab | `CloseTabCommand` |
| Close Others | Close all except this | `CloseOtherTabsCommand` |
| Close to Right | Close tabs to the right | `CloseToRightCommand` |
| --- | Separator | - |
| Pin/Unpin | Toggle pinned state | `TogglePinCommand` |
| Duplicate | Clone this tab | `DuplicateTabCommand` |
| --- | Separator | - |
| Rename | Rename this tab | `RenameTabCommand` |

**TDD Tests:**
- [ ] `TabBar_NewTab_CreatesSession`
- [ ] `TabBar_CloseTab_RemovesSession`
- [ ] `TabBar_SwitchTab_ChangesActiveSession`
- [ ] `TabBar_DirtyIndicator_ShowsWhenUnsaved`

---

## 10. Dialogs

### 10.1 Open Folder Dialog

| Property | Value |
|----------|-------|
| Type | OS native folder picker |
| Title | "Select folder to analyze" |

### 10.2 Save Preset Dialog

| Field | Type | Required |
|-------|------|----------|
| Preset Name | TextBox | Yes |
| Description | TextBox | No |
| Icon | Emoji picker | No (default 📋) |
| Scope | Radio: Global / Workspace | Yes |
| Include Files | Checkbox | Yes (default true) |
| Include Instructions | Checkbox | Yes (default true) |
| Include Role | Checkbox | Yes (default true) |

**Buttons:** Save, Cancel

### 10.3 Select by Pattern Dialog

| Field | Type | Description |
|-------|------|-------------|
| Pattern | TextBox | Glob pattern (e.g., `**/*.cs`) |
| Preview | ListBox | Files matching pattern |

**Buttons:** Select, Cancel

### 10.4 Confirm Close Unsaved Dialog

| Element | Value |
|---------|-------|
| Title | "Unsaved Changes" |
| Message | "Tab 'X' has unsaved changes. Close anyway?" |
| Buttons | Don't Save, Cancel, Save |

### 10.5 Confirm Undo AI Changes Dialog

| Element | Value |
|---------|-------|
| Title | "Undo AI Changes?" |
| Message | "This will restore N files to their state before the AI changes were applied." |
| Details | List of affected files |
| Buttons | Cancel, Undo Changes |

### 10.6 Resolve Conflict Dialog

| Element | Description |
|---------|-------------|
| File Path | Path of conflicting file |
| Conflict Type | "File was modified" / "File exists" / etc. |
| Current Content | Current file content (if exists) |
| Proposed Content | AI-proposed content |
| Options | Keep Current / Use Proposed / Merge (future) |

**Buttons:** Skip, Apply Selected Option

### 10.7 Role Editor Dialog

| Field | Type | Description |
|-------|------|-------------|
| Name | TextBox | Role display name |
| Icon | Emoji picker | Role icon |
| Description | TextBox | Short description |
| System Prompt | TextArea | Full system prompt |
| Tone | TextBox | e.g., "professional", "casual" |
| Priorities | TagInput | Priority areas |
| Expertise | TagInput | Expertise areas |

**Buttons:** Save, Cancel, Delete (if editing)

### 10.8 Pattern Editor Dialog

| Field | Type | Description |
|-------|------|-------------|
| Name | TextBox | Pattern name |
| Regex Pattern | TextBox | Regular expression |
| Category | Dropdown | Database, IP, PII, etc. |
| Enabled | Checkbox | Active/inactive |
| Test Input | TextArea | Test the pattern |
| Test Output | TextArea | Match results |

**Buttons:** Save, Cancel, Delete (if editing)

### 10.9 About Dialog

| Element | Value |
|---------|-------|
| Logo | ShieldPrompt logo |
| Name | ShieldPrompt v2.0.0 |
| Description | "Enterprise-Grade Secure AI Prompt Generation" |
| Copyright | © 2026 ShieldPrompt Team |
| Links | GitHub, Documentation, License |

**Buttons:** OK

---

## 11. Context Menus

### 11.1 File Tree Context Menu

| Item | Condition | Command |
|------|-----------|---------|
| Select | File/folder | `ToggleSelection()` |
| Select All in Folder | Folder | `SelectAllInFolder()` |
| Deselect All in Folder | Folder | `DeselectAllInFolder()` |
| --- | - | - |
| Expand | Folder, collapsed | `ExpandNode()` |
| Collapse | Folder, expanded | `CollapseNode()` |
| Expand All | Folder | `ExpandAll()` |
| Collapse All | Folder | `CollapseAll()` |
| --- | - | - |
| Copy Path | Any | `CopyPath()` |
| Copy Relative Path | Any | `CopyRelativePath()` |
| --- | - | - |
| Open in Explorer | Any | `OpenInExplorer()` |
| Open in Editor | File | `OpenInEditor()` |
| --- | - | - |
| Add to Ignore | Any | `AddToIgnore()` |

### 11.2 Preview Context Menu

| Item | Command |
|------|---------|
| Copy All | `CopyPreviewCommand` |
| Copy Selection | `CopySelectionCommand` |
| --- | - |
| Select All | `SelectAllCommand` |

### 11.3 Tab Context Menu

(See Section 9.4)

### 11.4 Preset Context Menu

| Item | Command |
|------|---------|
| Apply | `ApplyPresetCommand` |
| Duplicate | `DuplicatePresetCommand` |
| --- | - |
| Pin/Unpin | `TogglePinCommand` |
| --- | - |
| Export | `ExportPresetCommand` |
| Delete | `DeletePresetCommand` |

---

## 12. Keyboard Shortcuts

### 12.1 Global Shortcuts

| Shortcut | Action | Command |
|----------|--------|---------|
| Ctrl+O | Open folder | `OpenFolderCommand` |
| Ctrl+R | Refresh | `RefreshCommand` |
| Ctrl+W | Close tab | `CloseTabCommand` |
| Ctrl+, | Settings | `OpenSettingsCommand` |
| Ctrl+Z | Undo | `UndoCommand` |
| Ctrl+Y | Redo | `RedoCommand` |
| Ctrl+C | Copy prompt | `CopyToClipboardCommand` |
| Ctrl+V | Paste response | `PasteResponseCommand` |
| F1 | Help | `OpenDocsCommand` |
| F11 | Full screen | `ToggleFullScreenCommand` |
| Escape | Close dialog/cancel | `CancelCommand` |

### 12.2 File Tree Shortcuts

| Shortcut | Action | Command |
|----------|--------|---------|
| Space | Toggle selection | `ToggleSelection()` |
| Enter | Expand/collapse | `ToggleExpansion()` |
| Ctrl+A | Select all | `SelectAllCommand` |
| Ctrl+Shift+A | Deselect all | `DeselectAllCommand` |
| / | Focus search | `FocusSearchCommand` |
| Escape | Clear search | `ClearSearchCommand` |
| Up/Down | Navigate | Native |
| Left | Collapse | `CollapseNode()` |
| Right | Expand | `ExpandNode()` |

### 12.3 Tab Shortcuts

| Shortcut | Action | Command |
|----------|--------|---------|
| Ctrl+T | New tab | `NewTabCommand` |
| Ctrl+W | Close tab | `CloseTabCommand` |
| Ctrl+Tab | Next tab | `NextTabCommand` |
| Ctrl+Shift+Tab | Previous tab | `PreviousTabCommand` |
| Ctrl+1-9 | Go to tab N | `GoToTabCommand(N)` |
| Ctrl+Shift+D | Duplicate tab | `DuplicateTabCommand` |

### 12.4 Preview Shortcuts

| Shortcut | Action | Command |
|----------|--------|---------|
| Ctrl+Shift+C | Copy preview | `CopyPreviewCommand` |
| Ctrl+= | Zoom in | `ZoomInCommand` |
| Ctrl+- | Zoom out | `ZoomOutCommand` |
| Ctrl+0 | Reset zoom | `ResetZoomCommand` |

### 12.5 Instructions Shortcuts

| Shortcut | Action | Command |
|----------|--------|---------|
| Ctrl+Space | Insert snippet | `InsertSnippetCommand` |
| Ctrl+Enter | Focus preview | `FocusPreviewCommand` |

---

## 13. Mouse Operations

### 13.1 Click Operations

| Target | Click | Double-Click |
|--------|-------|--------------|
| File checkbox | Toggle selection | - |
| Folder checkbox | Toggle cascade selection | - |
| Folder name | Expand/collapse | - |
| File name | Select node | View file content |
| Tab | Switch to tab | Rename tab |
| Preview panel | Copy to clipboard | - |
| Status bar item | - | Show details |

### 13.2 Right-Click Operations

| Target | Result |
|--------|--------|
| File/folder | File tree context menu |
| Tab | Tab context menu |
| Preview | Preview context menu |
| Preset | Preset context menu |

### 13.3 Middle-Click Operations

| Target | Result |
|--------|--------|
| Tab | Close tab |

---

## 14. Drag & Drop

### 14.1 Supported Operations

| Source | Target | Result |
|--------|--------|--------|
| OS folder | Main window | Open folder as workspace |
| OS file | File tree | Add file to selection |
| Tab | Tab bar | Reorder tabs |

### 14.2 Visual Feedback

| State | Visual |
|-------|--------|
| Drag start | Opacity reduced, cursor change |
| Valid drop | Highlight drop target |
| Invalid drop | "Not allowed" cursor |
| Drop complete | Flash confirmation |

**TDD Tests:**
- [ ] `DragDrop_FolderToWindow_OpensWorkspace`
- [ ] `DragDrop_TabReorder_ChangeTabOrder`

---

## Implementation Checklist Summary

### Forms & Windows
- [ ] MainWindowV2
- [ ] Settings Window (7 tabs)
- [ ] Apply Dashboard
- [ ] Preset Library Panel

### Menu Items (Total: ~60)
- [ ] File Menu (10 items)
- [ ] Edit Menu (10 items)
- [ ] View Menu (12 items)
- [ ] Selection Menu (8 items)
- [ ] Tabs Menu (10 items)
- [ ] Tools Menu (8 items)
- [ ] Help Menu (6 items)

### Toolbar Elements
- [ ] Primary toolbar (6 elements)
- [ ] Workspace dropdown
- [ ] Role dropdown
- [ ] Model dropdown

### Panels
- [ ] File Tree Panel (with cascading selection)
- [ ] Preview Panel (with click-to-copy)
- [ ] Instructions Panel
- [ ] Action Bar
- [ ] Status Bar
- [ ] Tab Bar

### Dialogs (9 total)
- [ ] Open Folder
- [ ] Save Preset
- [ ] Select by Pattern
- [ ] Confirm Close Unsaved
- [ ] Confirm Undo AI Changes
- [ ] Resolve Conflict
- [ ] Role Editor
- [ ] Pattern Editor
- [ ] About

### Context Menus (4 total)
- [ ] File Tree context menu
- [ ] Preview context menu
- [ ] Tab context menu
- [ ] Preset context menu

### Keyboard Shortcuts
- [ ] Global shortcuts (~12)
- [ ] File tree shortcuts (~10)
- [ ] Tab shortcuts (~7)
- [ ] Preview shortcuts (~4)
- [ ] Instructions shortcuts (~2)

### Mouse & Drag Operations
- [ ] Click operations
- [ ] Right-click context menus
- [ ] Middle-click operations
- [ ] Drag & drop operations

---

**Total Operations Documented:** ~200+

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial complete UI specification |

