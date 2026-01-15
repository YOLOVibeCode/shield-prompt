# 🧙 ShieldPrompt Wizard UI Requirements v2.0

**Status:** APPROVED FOR IMPLEMENTATION  
**Version:** 2.0  
**Last Updated:** January 15, 2026  
**Priority:** HIGH - Next Major Feature

---

## 📋 Executive Summary

Transform ShieldPrompt from a simple file-to-clipboard tool into a **wizard-driven prompt generation system** with:
- Step-by-step guided workflow
- Prompt templates (Code Review, Debug, Analyze, etc.)
- Custom instructions support
- **Fully responsive layout**
- **Draggable/resizable panels**
- Real-time live preview

---

## 🎯 Core Requirements

### R1: Wizard-Driven Workflow
| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| R1.1 | Display step indicator showing current workflow position | Medium | ⬜ |
| R1.2 | Guide users through: Select Files → Choose Task → Customize → Copy | High | ⬜ |
| R1.3 | Allow skipping steps for power users | Medium | ⬜ |
| R1.4 | Persist wizard state between sessions | Low | ⬜ |

### R2: Prompt Template System
| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| R2.1 | Dropdown selector for prompt templates in toolbar | High | ⬜ |
| R2.2 | Built-in templates: Code Review, Debug, Explain, Refactor, Security, Test Gen, Document | High | ⬜ |
| R2.3 | Templates stored in YAML for user customization | Medium | ⬜ |
| R2.4 | User-defined custom templates (import/export) | Low | ⬜ |
| R2.5 | Focus area multi-select (Security, Performance, etc.) | Medium | ⬜ |

### R3: Custom Instructions
| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| R3.1 | Free-form text area for custom instructions | High | ⬜ |
| R3.2 | Placeholder text guides user on what to enter | Medium | ⬜ |
| R3.3 | Character/token counter for custom instructions | Medium | ⬜ |
| R3.4 | Save frequently used instructions as snippets | Low | ⬜ |

### R4: Live Preview
| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| R4.1 | Real-time preview of generated prompt | High | ⬜ |
| R4.2 | Preview updates on any change (debounced 300ms) | High | ⬜ |
| R4.3 | Syntax highlighting in preview (Markdown) | Medium | ⬜ |
| R4.4 | Token count with model-specific limit warnings | High | ⬜ |
| R4.5 | Collapsible/expandable file sections in preview | Medium | ⬜ |

### R5: Responsive Layout ⭐ NEW
| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| R5.1 | All panels must be fully responsive | High | ⬜ |
| R5.2 | Minimum window size: 800x600 | High | ⬜ |
| R5.3 | Panels stack vertically on narrow windows (<1000px) | High | ⬜ |
| R5.4 | Font sizes scale proportionally | Medium | ⬜ |
| R5.5 | Touch-friendly targets (min 44x44px) | Medium | ⬜ |
| R5.6 | Scrollable overflow for all content areas | High | ⬜ |

### R6: Draggable/Resizable Panels ⭐ NEW
| ID | Requirement | Priority | Status |
|----|-------------|----------|--------|
| R6.1 | All major panels can be resized via drag handles | High | ⬜ |
| R6.2 | Horizontal splitter between file tree and right panel | High | ⬜ |
| R6.3 | Vertical splitter between prompt builder and preview | High | ⬜ |
| R6.4 | Double-click splitter to reset to default size | Medium | ⬜ |
| R6.5 | Minimum panel sizes enforced (150px) | High | ⬜ |
| R6.6 | Panel sizes persist between sessions | Medium | ⬜ |
| R6.7 | Collapse/expand panels with button or double-click | Medium | ⬜ |
| R6.8 | Visual drag handle indicator (grip dots) | Medium | ⬜ |

---

## 🖼️ UI Layout Specification

### Desktop Layout (≥1200px)

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│ Menu Bar                                                           [_] [□] [X] │
├─────────────────────────────────────────────────────────────────────────────────┤
│ ① Select Files  ──────►  ② Choose Task  ──────►  ③ Customize  ──────►  ④ Copy  │
├─────────────────────────────────────────────────────────────────────────────────┤
│ Toolbar: [📁 Open] [🔄 Refresh] | Task: [Code Review ▼] | Model: [GPT-4o ▼]    │
├────────────────────┬──┬─────────────────────────────────────────────────────────┤
│                    │▐▐│                                                          │
│   FILE TREE        │▐▐│   PROMPT BUILDER                                         │
│                    │▐▐│   ┌────────────────────────────────────────────────────┐ │
│   📁 src/          │▐▐│   │ Focus: [☑ Security] [☑ Performance] [☐ Style]     │ │
│     ☑ App.cs       │▐▐│   └────────────────────────────────────────────────────┘ │
│     ☑ User.cs      │▐▐│                                                          │
│     ☐ Config.json  │▐▐│   Custom Instructions:                                   │
│   📁 tests/        │▐▐│   ┌────────────────────────────────────────────────────┐ │
│     ☐ Tests.cs     │▐▐│   │ Focus on error handling and null checks.           │ │
│                    │▐▐│   │ The app crashes when user input is empty.          │ │
│                    │▐▐│   └────────────────────────────────────────────────────┘ │
│  ─────────────     │▐▐│   (245 chars | ~50 tokens)                               │
│  3 files           │▐▐├──────────────────────────────────────────────────────────┤
│  2,450 tokens      │▐▐│ ═══════════════════════════════════════════════════════  │
│                    │▐▐├──────────────────────────────────────────────────────────┤
│  [Select All]      │▐▐│   LIVE PREVIEW                              [Expand ↗]  │
│  [Deselect All]    │▐▐│   ┌────────────────────────────────────────────────────┐ │
│                    │▐▐│   │ You are an expert code reviewer...                 │ │
│                    │▐▐│   │                                                    │ │
│                    │▐▐│   │ ## Files to Review                                 │ │
│                    │▐▐│   │ ### `src/App.cs`                                   │ │
│                    │▐▐│   │ ```csharp                                          │ │
│                    │▐▐│   │ public class App { ... }                           │ │
│                    │▐▐│   │ ```                                                │ │
│                    │▐▐│   └────────────────────────────────────────────────────┘ │
│                    │▐▐│                                                          │
│                    │▐▐│   📊 3,200 tokens | ⚠️ 80% of GPT-4o limit              │
├────────────────────┴──┴──────────────────────────────────────────────────────────┤
│ ✅ Ready | 3 files selected | 🔐 12 values sanitized | [📋 Copy to Clipboard]   │
└──────────────────────────────────────────────────────────────────────────────────┘

Legend:
▐▐ = Draggable splitter (horizontal)
══ = Draggable splitter (vertical)
```

### Tablet Layout (800px - 1199px)

```
┌─────────────────────────────────────────────────────────────┐
│ Menu | [📁] [🔄] | Task: [Code Review ▼]        [_] [□] [X] │
├─────────────────────────────────────────────────────────────┤
│ ① Files  ② Task  ③ Custom  ④ Copy                          │
├──────────────────────┬──┬───────────────────────────────────┤
│  FILE TREE           │▐▐│  PROMPT BUILDER                   │
│  (Collapsible)       │▐▐│                                   │
│  📁 src/             │▐▐│  Focus: [☑Sec] [☑Perf]           │
│    ☑ App.cs          │▐▐│                                   │
│    ☑ User.cs         │▐▐│  Instructions:                    │
│                      │▐▐│  ┌─────────────────────────────┐  │
│  3 files | 2,450 tok │▐▐│  │ Focus on error handling... │  │
│                      │▐▐│  └─────────────────────────────┘  │
├──────────────────────┴──┴───────────────────────────────────┤
│  LIVE PREVIEW                                    [Expand ↗] │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ You are an expert code reviewer...                     │ │
│  │ ## Files to Review                                     │ │
│  └────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│ 3 files | 🔐 12 masked | 3,200 tok | [📋 Copy]              │
└─────────────────────────────────────────────────────────────┘
```

### Mobile/Narrow Layout (<800px)

```
┌───────────────────────────────────┐
│ ShieldPrompt        [≡] [_][□][X] │
├───────────────────────────────────┤
│ ① ② ③ ④                          │
├───────────────────────────────────┤
│ [Files ▼] [Code Review ▼] [GPT▼] │
├───────────────────────────────────┤
│ ┌───────────────────────────────┐ │
│ │ 📁 FILE TREE (Collapsed)      │ │
│ │ 3 files selected | 2,450 tok  │ │
│ │ [Expand ▼]                    │ │
│ └───────────────────────────────┘ │
├───────────────────────────────────┤
│ ┌───────────────────────────────┐ │
│ │ PROMPT BUILDER                │ │
│ │ Focus: [☑Sec] [☑Perf]        │ │
│ │ Instructions:                 │ │
│ │ ┌───────────────────────────┐ │ │
│ │ │ Focus on error handling   │ │ │
│ │ └───────────────────────────┘ │ │
│ └───────────────────────────────┘ │
├───────────────────────────────────┤
│ ┌───────────────────────────────┐ │
│ │ PREVIEW (Collapsed)           │ │
│ │ 3,200 tokens | [Expand ▼]     │ │
│ └───────────────────────────────┘ │
├───────────────────────────────────┤
│ [     📋 Copy to Clipboard     ] │
├───────────────────────────────────┤
│ ✅ Ready | 🔐 12 masked          │
└───────────────────────────────────┘
```

---

## ✅ Implementation Checklist

### Phase 0: Preparation (Day 0)
- [ ] **P0.1** Read and understand current `MainWindow.axaml` layout
- [ ] **P0.2** Read and understand `MainWindowViewModel.cs` bindings
- [ ] **P0.3** Create feature branch: `feature/wizard-ui-v2`
- [ ] **P0.4** Update `.cursorrules` if needed

---

### Phase 1: Responsive Grid Foundation (Days 1-2)

#### 1.1 Grid Restructuring
- [ ] **1.1.1** Replace fixed `ColumnDefinitions="300,*"` with responsive definitions
- [ ] **1.1.2** Add `GridSplitter` between file tree and right panel
- [ ] **1.1.3** Set `MinWidth="200"` on file tree panel
- [ ] **1.1.4** Set `MinWidth="400"` on right panel

#### 1.2 Avalonia GridSplitter Implementation
- [ ] **1.2.1** Add `GridSplitter` control with `Width="6"` and `Background="Transparent"`
- [ ] **1.2.2** Style splitter with hover effect (grip dots appear)
- [ ] **1.2.3** Add cursor change on hover (`Cursor="SizeWE"`)
- [ ] **1.2.4** Test splitter drag behavior

#### 1.3 Responsive Triggers
- [ ] **1.3.1** Create `AdaptiveTrigger` for width breakpoints
- [ ] **1.3.2** Define breakpoints: `<800`, `800-1199`, `≥1200`
- [ ] **1.3.3** Switch to stacked layout at `<1000px`
- [ ] **1.3.4** Test window resize behavior

#### 1.4 Tests for Phase 1
- [ ] **1.4.1** Write test: `GridSplitter_DragLeft_DecreasesFileTreeWidth`
- [ ] **1.4.2** Write test: `GridSplitter_DragRight_IncreasesFileTreeWidth`
- [ ] **1.4.3** Write test: `MinWidth_Enforced_WhenDraggingPastLimit`
- [ ] **1.4.4** Write test: `LayoutSwitches_WhenWindowNarrowerThan1000px`

---

### Phase 2: Prompt Template System (Days 3-4)

#### 2.1 Domain Models
- [ ] **2.1.1** Create `PromptTemplate` record in Domain layer
- [ ] **2.1.2** Create `PromptOptions` record with focus areas
- [ ] **2.1.3** Create `ComposedPrompt` record for output
- [ ] **2.1.4** Define `PromptTemplateCategory` enum

#### 2.2 Interfaces (ISP-Compliant)
- [ ] **2.2.1** Create `IPromptTemplateRepository` interface (≤5 methods)
- [ ] **2.2.2** Create `IPromptComposer` interface (≤3 methods)
- [ ] **2.2.3** Create `IPromptPreviewGenerator` interface (≤2 methods)

#### 2.3 Infrastructure Implementation
- [ ] **2.3.1** Create `YamlPromptTemplateRepository` implementation
- [ ] **2.3.2** Create `config/prompt-templates.yaml` with 8 default templates
- [ ] **2.3.3** Implement YAML parsing with YamlDotNet
- [ ] **2.3.4** Add fallback embedded templates if YAML missing

#### 2.4 Application Services
- [ ] **2.4.1** Implement `PromptComposer` service
- [ ] **2.4.2** Compose: System Prompt + Custom Instructions + Files
- [ ] **2.4.3** Add template variable substitution (`{language}`, `{file_count}`)
- [ ] **2.4.4** Implement focus area injection into prompt

#### 2.5 Tests for Phase 2
- [ ] **2.5.1** Write test: `Repository_LoadsAllTemplates_FromYaml`
- [ ] **2.5.2** Write test: `Repository_ReturnsFallback_WhenYamlMissing`
- [ ] **2.5.3** Write test: `Composer_IncludesSystemPrompt_FromTemplate`
- [ ] **2.5.4** Write test: `Composer_InjectsCustomInstructions_InCorrectPosition`
- [ ] **2.5.5** Write test: `Composer_SubstitutesVariables_Correctly`
- [ ] **2.5.6** Write test: `Composer_IncludesFocusAreas_WhenSelected`

---

### Phase 3: Prompt Builder Panel UI (Days 5-6)

#### 3.1 Template Selector
- [ ] **3.1.1** Add `ComboBox` for template selection in toolbar
- [ ] **3.1.2** Display template icon + name in dropdown
- [ ] **3.1.3** Show template description as tooltip
- [ ] **3.1.4** Bind `SelectedTemplate` to ViewModel

#### 3.2 Focus Area Picker
- [ ] **3.2.1** Create `ItemsControl` with `CheckBox` items for focus areas
- [ ] **3.2.2** Horizontal wrap layout for focus chips
- [ ] **3.2.3** Bind selected focus areas to ViewModel
- [ ] **3.2.4** Update focus options when template changes

#### 3.3 Custom Instructions Editor
- [ ] **3.3.1** Add `TextBox` with `AcceptsReturn="True"` and `TextWrapping="Wrap"`
- [ ] **3.3.2** Add placeholder text (watermark) when empty
- [ ] **3.3.3** Add character counter below text box
- [ ] **3.3.4** Add token estimate for custom instructions
- [ ] **3.3.5** Bind to `CustomInstructions` property

#### 3.4 ViewModel Updates
- [ ] **3.4.1** Add `SelectedTemplate` observable property
- [ ] **3.4.2** Add `CustomInstructions` observable property
- [ ] **3.4.3** Add `SelectedFocusAreas` observable collection
- [ ] **3.4.4** Add `AvailableFocusAreas` computed from template
- [ ] **3.4.5** Wire up reactive updates to preview

#### 3.5 Tests for Phase 3
- [ ] **3.5.1** Write test: `TemplateChange_UpdatesFocusOptions`
- [ ] **3.5.2** Write test: `CustomInstructions_UpdatesTokenCount`
- [ ] **3.5.3** Write test: `FocusAreaToggle_UpdatesPreview`

---

### Phase 4: Live Preview Panel (Days 7-8)

#### 4.1 Preview Panel UI
- [ ] **4.1.1** Create scrollable `TextBox` or `TextBlock` for preview
- [ ] **4.1.2** Apply monospace font for code sections
- [ ] **4.1.3** Add vertical `GridSplitter` between builder and preview
- [ ] **4.1.4** Add expand/collapse button for preview panel

#### 4.2 Reactive Preview Updates
- [ ] **4.2.1** Subscribe to all input changes (files, template, custom, focus)
- [ ] **4.2.2** Implement debounce (300ms) to prevent excessive updates
- [ ] **4.2.3** Show "Updating..." indicator during composition
- [ ] **4.2.4** Bind `LivePreview` string to preview panel

#### 4.3 Token Limit Warnings
- [ ] **4.3.1** Calculate tokens using existing `ITokenCountingService`
- [ ] **4.3.2** Show warning when >80% of model limit
- [ ] **4.3.3** Show error when >100% of model limit
- [ ] **4.3.4** Display as colored badge below preview

#### 4.4 Markdown Rendering (Optional Enhancement)
- [ ] **4.4.1** Add Markdown.Avalonia or similar package
- [ ] **4.4.2** Render preview as formatted Markdown
- [ ] **4.4.3** Toggle between raw/rendered view

#### 4.5 Tests for Phase 4
- [ ] **4.5.1** Write test: `Preview_UpdatesWithin500ms_AfterChange`
- [ ] **4.5.2** Write test: `Preview_IncludesAllSelectedFiles`
- [ ] **4.5.3** Write test: `TokenWarning_ShowsAt80Percent`
- [ ] **4.5.4** Write test: `TokenError_ShowsAt100Percent`

---

### Phase 5: Draggable Panel System (Days 9-10)

#### 5.1 Horizontal Splitter (File Tree | Right Panel)
- [ ] **5.1.1** Implement `GridSplitter` between columns 0 and 1
- [ ] **5.1.2** Add grip handle visual (6 dots)
- [ ] **5.1.3** Set `MinWidth` constraints on both sides
- [ ] **5.1.4** Double-click to reset to default (300px)

#### 5.2 Vertical Splitter (Prompt Builder | Preview)
- [ ] **5.2.1** Split right panel into two rows
- [ ] **5.2.2** Implement vertical `GridSplitter`
- [ ] **5.2.3** Set `MinHeight` constraints (150px each)
- [ ] **5.2.4** Double-click to reset to 50/50 split

#### 5.3 Panel State Persistence
- [ ] **5.3.1** Add `FileTreeWidth` to `AppSettings`
- [ ] **5.3.2** Add `PreviewPanelHeight` to `AppSettings`
- [ ] **5.3.3** Save panel sizes on change (debounced)
- [ ] **5.3.4** Restore panel sizes on app start

#### 5.4 Collapse/Expand Behavior
- [ ] **5.4.1** Add collapse button to file tree header
- [ ] **5.4.2** Add collapse button to preview panel header
- [ ] **5.4.3** Animate collapse/expand transition
- [ ] **5.4.4** Remember collapsed state in settings

#### 5.5 Splitter Styling
- [ ] **5.5.1** Create `GridSplitterStyle` in App.axaml
- [ ] **5.5.2** Default: subtle line (1px)
- [ ] **5.5.3** Hover: highlight + grip dots visible
- [ ] **5.5.4** Dragging: accent color highlight

#### 5.6 Tests for Phase 5
- [ ] **5.6.1** Write test: `HorizontalSplitter_PersistsWidth_OnRestart`
- [ ] **5.6.2** Write test: `VerticalSplitter_PersistsHeight_OnRestart`
- [ ] **5.6.3** Write test: `DoubleClick_ResetsToDefault`
- [ ] **5.6.4** Write test: `CollapseButton_HidesPanel`
- [ ] **5.6.5** Write test: `MinWidth_PreventsTooSmall`

---

### Phase 6: Wizard Step Indicator (Days 11-12)

#### 6.1 Step Indicator Component
- [ ] **6.1.1** Create `WizardStepIndicator` UserControl
- [ ] **6.1.2** Display 4 steps: Files → Task → Custom → Copy
- [ ] **6.1.3** Highlight current/completed steps
- [ ] **6.1.4** Make steps clickable to jump

#### 6.2 Step State Management
- [ ] **6.2.1** Add `CurrentStep` property to ViewModel
- [ ] **6.2.2** Auto-advance step when condition met
- [ ] **6.2.3** Show completion checkmarks on finished steps

#### 6.3 Step Validation
- [ ] **6.3.1** Step 1 complete when ≥1 file selected
- [ ] **6.3.2** Step 2 complete when template selected
- [ ] **6.3.3** Step 3 always optional (can skip)
- [ ] **6.3.4** Step 4 enabled when steps 1-2 complete

#### 6.4 Tests for Phase 6
- [ ] **6.4.1** Write test: `Step1Complete_WhenFilesSelected`
- [ ] **6.4.2** Write test: `Step2Complete_WhenTemplateSelected`
- [ ] **6.4.3** Write test: `CopyEnabled_OnlyWhenSteps12Complete`

---

### Phase 7: Default Templates (Day 13)

#### 7.1 Create Template YAML File
- [ ] **7.1.1** Create `config/prompt-templates.yaml`

#### 7.2 Template: Code Review
```yaml
- id: code_review
  name: "Code Review"
  icon: "🔍"
  system_prompt: |
    You are an expert code reviewer. Review the following code for:
    - Bugs and logic errors
    - Security vulnerabilities  
    - Performance issues
    - Code style violations
    - Potential null reference exceptions
    
    Provide specific, actionable feedback with line references.
  focus_options: [Security, Performance, Readability, Testing, Error Handling]
```
- [ ] **7.2.1** Implement Code Review template

#### 7.3 Template: Debug
```yaml
- id: debug
  name: "Debug Issue"
  icon: "🐛"
  system_prompt: |
    You are a debugging expert. The user is experiencing an issue.
    
    Analyze the code to:
    1. Identify the root cause
    2. Explain why it fails
    3. Provide a fix with code changes
    
    User's issue description:
    {custom_instructions}
  requires_custom: true
  placeholder: "Describe the error or unexpected behavior..."
```
- [ ] **7.3.1** Implement Debug template

#### 7.4 Template: Explain
```yaml
- id: explain
  name: "Explain Code"
  icon: "📖"
  system_prompt: |
    You are a code explainer. Explain this code clearly for someone 
    who is learning. Include:
    - What the code does (high level)
    - How it works (step by step)
    - Key concepts used
    - Potential gotchas
```
- [ ] **7.4.1** Implement Explain template

#### 7.5 Template: Refactor
```yaml
- id: refactor
  name: "Refactor"
  icon: "♻️"
  system_prompt: |
    You are a refactoring expert. Suggest improvements while 
    maintaining the same behavior. Focus on:
    - SOLID principles
    - DRY (Don't Repeat Yourself)
    - Clean code practices
    - Testability improvements
    
    Show before/after code snippets.
  focus_options: [SOLID, DRY, Testability, Readability, Performance]
```
- [ ] **7.5.1** Implement Refactor template

#### 7.6 Template: Security Audit
```yaml
- id: security
  name: "Security Audit"
  icon: "🔐"
  system_prompt: |
    You are a security auditor. Analyze this code for vulnerabilities:
    - OWASP Top 10
    - Injection attacks (SQL, XSS, etc.)
    - Authentication/authorization flaws
    - Data exposure risks
    - Cryptographic weaknesses
    
    Rate severity (Critical/High/Medium/Low) for each finding.
```
- [ ] **7.6.1** Implement Security Audit template

#### 7.7 Template: Generate Tests
```yaml
- id: test_gen
  name: "Generate Tests"
  icon: "🧪"
  system_prompt: |
    You are a testing expert. Generate comprehensive tests for this code:
    - Unit tests with high coverage
    - Edge cases and boundary conditions
    - Error handling scenarios
    - Use {test_framework} framework
    
    Follow AAA pattern (Arrange, Act, Assert).
  focus_options: [Unit Tests, Integration Tests, Edge Cases, Mocking]
```
- [ ] **7.7.1** Implement Generate Tests template

#### 7.8 Template: Documentation
```yaml
- id: documentation
  name: "Generate Docs"
  icon: "📝"
  system_prompt: |
    You are a technical writer. Generate documentation for this code:
    - XML documentation comments
    - README sections
    - API documentation
    - Usage examples
```
- [ ] **7.8.1** Implement Documentation template

#### 7.9 Template: Generic (Fallback)
```yaml
- id: generic
  name: "Custom Prompt"
  icon: "💬"
  system_prompt: ""
  requires_custom: true
  placeholder: "Enter your instructions for the AI..."
```
- [ ] **7.9.1** Implement Generic template

---

### Phase 8: Integration & Polish (Days 14-15)

#### 8.1 DI Registration
- [ ] **8.1.1** Register `IPromptTemplateRepository` in `App.axaml.cs`
- [ ] **8.1.2** Register `IPromptComposer` in `App.axaml.cs`
- [ ] **8.1.3** Update `MainWindowViewModel` constructor

#### 8.2 Copy Flow Integration
- [ ] **8.2.1** Update `CopyToClipboardCommand` to use composed prompt
- [ ] **8.2.2** Include system prompt + user content in clipboard
- [ ] **8.2.3** Apply sanitization to entire composed prompt
- [ ] **8.2.4** Update status bar with new metrics

#### 8.3 Settings Persistence
- [ ] **8.3.1** Add `LastTemplateId` to `AppSettings`
- [ ] **8.3.2** Add `LastFocusAreas` to `AppSettings`
- [ ] **8.3.3** Add `PanelWidths` to `AppSettings`
- [ ] **8.3.4** Restore all settings on startup

#### 8.4 Keyboard Shortcuts
- [ ] **8.4.1** Add `Ctrl+1` through `Ctrl+8` for template quick-select
- [ ] **8.4.2** Add `Ctrl+Enter` to copy immediately
- [ ] **8.4.3** Add `F2` to focus custom instructions

#### 8.5 Polish & UX
- [ ] **8.5.1** Add loading indicators where needed
- [ ] **8.5.2** Add tooltips to all controls
- [ ] **8.5.3** Ensure tab order is logical
- [ ] **8.5.4** Test with keyboard-only navigation
- [ ] **8.5.5** Test with screen reader (accessibility)

---

### Phase 9: Testing & Documentation (Days 16-17)

#### 9.1 Integration Tests
- [ ] **9.1.1** Write test: `FullWorkflow_SelectFiles_ChooseTemplate_Copy`
- [ ] **9.1.2** Write test: `TemplateChange_UpdatesPreview_WithinDebounce`
- [ ] **9.1.3** Write test: `ResponsiveLayout_SwitchesAtBreakpoint`
- [ ] **9.1.4** Write test: `PanelSizes_PersistAcrossRestart`

#### 9.2 Manual QA Checklist
- [ ] **9.2.1** Test all templates copy correctly
- [ ] **9.2.2** Test responsive behavior at all breakpoints
- [ ] **9.2.3** Test draggable splitters on all platforms
- [ ] **9.2.4** Test keyboard navigation
- [ ] **9.2.5** Test with 0 files, 1 file, 100 files
- [ ] **9.2.6** Test with very long custom instructions

#### 9.3 Documentation
- [ ] **9.3.1** Update `README.md` with new features
- [ ] **9.3.2** Update `docs/USER_GUIDE.md`
- [ ] **9.3.3** Add template customization guide
- [ ] **9.3.4** Update `CHANGELOG.md`

---

### Phase 10: Release (Day 18)

#### 10.1 Version Bump
- [ ] **10.1.1** Update `VERSION` to `1.2.0`
- [ ] **10.1.2** Update `ShieldPrompt.App.csproj` version
- [ ] **10.1.3** Update About dialog version

#### 10.2 Release Process
- [ ] **10.2.1** Run all tests locally
- [ ] **10.2.2** Create PR and merge to main
- [ ] **10.2.3** Create and push git tag `v1.2.0`
- [ ] **10.2.4** Verify GitHub Actions build succeeds
- [ ] **10.2.5** Verify all platform artifacts generated
- [ ] **10.2.6** Test downloaded artifacts on each platform

---

## 📊 Effort Estimation

| Phase | Description | Days | Priority |
|-------|-------------|------|----------|
| 0 | Preparation | 0.5 | - |
| 1 | Responsive Grid | 2 | 🔴 High |
| 2 | Template System | 2 | 🔴 High |
| 3 | Prompt Builder UI | 2 | 🔴 High |
| 4 | Live Preview | 2 | 🔴 High |
| 5 | Draggable Panels | 2 | 🔴 High |
| 6 | Wizard Steps | 2 | 🟡 Medium |
| 7 | Default Templates | 1 | 🔴 High |
| 8 | Integration | 2 | 🔴 High |
| 9 | Testing & Docs | 2 | 🔴 High |
| 10 | Release | 0.5 | 🔴 High |
| **TOTAL** | | **18 days** | |

---

## 🛠️ Technical Dependencies

### New NuGet Packages Needed
| Package | Purpose | Required |
|---------|---------|----------|
| `YamlDotNet` | Parse template YAML files | Yes |
| `Markdown.Avalonia` | Render Markdown preview (optional) | No |
| `Avalonia.Xaml.Behaviors` | Animation support | No |

### Existing Packages (No Changes)
- `CommunityToolkit.Mvvm` - Already used
- `TiktokenSharp` - Already used
- `TextCopy` - Already used

---

## ✅ Acceptance Criteria

### Must Have (MVP)
- [ ] User can select a prompt template from dropdown
- [ ] User can enter custom instructions
- [ ] Live preview updates in real-time
- [ ] Draggable horizontal splitter works
- [ ] Responsive layout stacks on narrow windows
- [ ] All panel sizes persist between sessions

### Should Have
- [ ] Draggable vertical splitter works
- [ ] Wizard step indicator shows progress
- [ ] Focus area multi-select works
- [ ] 8 default templates available

### Nice to Have
- [ ] Markdown rendering in preview
- [ ] User-defined custom templates
- [ ] Template import/export

---

## 🔒 Security Considerations

1. **Template Injection**: Templates must not execute user code
2. **YAML Parsing**: Use safe YAML parser (no arbitrary object instantiation)
3. **Sanitization**: All content (including templates) must go through sanitization
4. **File Paths**: Templates cannot read arbitrary files

---

**Document Version:** 2.0  
**Author:** Architect  
**Approved By:** [Pending User Approval]

---

*Ready to implement? Say "Implement Phase 1" to begin!*

