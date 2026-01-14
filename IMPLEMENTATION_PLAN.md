# ShieldPrompt Implementation Plan

**Approach:** TDD + Interface Segregation  
**Principle:** KISS - Build the simplest thing that works, iterate

---

## Phase 1: Core MVP (Weeks 1-3)

### 1.1 Domain Layer Setup
**Goal:** Define the core entities and enums

| Task | Interface/Type | Test First | Acceptance |
|------|---------------|------------|------------|
| Create `PatternCategory` enum | `PatternCategory.cs` | N/A (enum) | Compiles |
| Create `PolicyMode` enum | `PolicyMode.cs` | N/A (enum) | Compiles |
| Create `FileNode` entity | `FileNode.cs` | `FileNodeTests.cs` | Can represent file tree |
| Create `TokenCount` value object | `TokenCount.cs` | `TokenCountTests.cs` | Immutable, comparable |

**Files to create:**
```
src/ShieldPrompt.Domain/
├── Enums/
│   ├── PatternCategory.cs
│   └── PolicyMode.cs
├── Entities/
│   └── FileNode.cs
└── ValueObjects/
    └── TokenCount.cs
```

---

### 1.2 File Aggregation Service
**Goal:** Load directory, build file tree, aggregate contents

| Task | Interface | Implementation | Tests |
|------|-----------|----------------|-------|
| Define interface | `IFileAggregationService` | - | - |
| Load directory | - | `FileAggregationService` | `LoadDirectoryAsync_WithValidPath_ReturnsTree` |
| Apply exclusions | - | - | `LoadDirectoryAsync_WithNodeModules_ExcludesFolder` |
| Binary detection | - | - | `IsBinaryFile_WithPngExtension_ReturnsTrue` |
| Aggregate contents | - | - | `AggregateContentsAsync_WithThreeFiles_ConcatenatesAll` |

**Interface Definition:**
```csharp
public interface IFileAggregationService
{
    Task<FileNode> LoadDirectoryAsync(string path, CancellationToken ct = default);
    Task<string> AggregateContentsAsync(IEnumerable<FileNode> files, CancellationToken ct = default);
    bool IsBinaryFile(string path);
    bool IsExcluded(string path);
}
```

---

### 1.3 Token Counting Service
**Goal:** Count tokens using tiktoken encoding

| Task | Interface | Implementation | Tests |
|------|-----------|----------------|-------|
| Define interface | `ITokenCountingService` | - | - |
| Count tokens | - | `TokenCountingService` | `CountTokens_WithHelloWorld_ReturnsCorrectCount` |
| Count file tokens | - | - | `CountFileTokens_WithCSharpFile_ReturnsTokenCount` |
| Check limits | - | - | `ExceedsLimit_AtHalfCapacity_ReturnsFalse` |

**Interface Definition:**
```csharp
public interface ITokenCountingService
{
    int CountTokens(string content, string encoding = "cl100k_base");
    TokenCount CountFileTokens(FileNode file);
    bool ExceedsLimit(int tokens, ModelProfile profile);
}
```

---

### 1.4 Model Profiles
**Goal:** Define AI model context limits

| Task | Type | Tests |
|------|------|-------|
| Create `ModelProfile` record | `ModelProfile.cs` | `ModelProfile_GPT4o_HasCorrectLimit` |
| Add built-in profiles | `ModelProfiles.cs` | `ModelProfiles_AllProfiles_HaveValidEncodings` |

---

### 1.5 Basic UI Shell
**Goal:** Avalonia app with file tree and copy button

| Task | Component | Acceptance |
|------|-----------|------------|
| App entry point | `Program.cs`, `App.axaml` | App launches |
| Main window | `MainWindow.axaml` | Shows title bar |
| File tree view | `FileTreeView.axaml` | Displays folder structure |
| Copy button | `MainViewModel.cs` | Copies selected files to clipboard |

**Phase 1 Exit Criteria:**
- [ ] Can open a folder via dialog
- [ ] File tree displays with checkboxes
- [ ] Token count shows per file and total
- [ ] Copy button puts aggregated text in clipboard
- [ ] All unit tests pass

---

## Phase 2: Sanitization Engine (Weeks 4-6)

### 2.1 Pattern Registry
**Goal:** Store and retrieve detection patterns

| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `IPatternRegistry` | - |
| Add pattern | `PatternRegistry` | `AddPattern_WithValidPattern_Succeeds` |
| Get patterns by category | - | `GetPatterns_ByInfrastructure_ReturnsInfraOnly` |

**Interface Definition:**
```csharp
public interface IPatternRegistry
{
    void AddPattern(Pattern pattern);
    IEnumerable<Pattern> GetPatterns(PatternCategory? category = null);
    Pattern? GetPattern(string name);
}
```

---

### 2.2 Built-in Patterns
**Goal:** Implement 14 detection patterns

| Pattern | Category | Test Case |
|---------|----------|-----------|
| Server/DB names | Infrastructure | `"ProductionDB"` → detected |
| Private IPs | Infrastructure | `"192.168.1.50"` → detected |
| Connection strings | Infrastructure | `"Server=prod;..."` → detected |
| Windows paths | Infrastructure | `"C:\Users\..."` → detected |
| Internal hostnames | Infrastructure | `"db.internal.corp"` → detected |
| SSN | PII | `"123-45-6789"` → detected |
| Credit cards | PII | `"4111111111111111"` → detected (Luhn valid) |
| AWS keys | PII | `"AKIAIOSFODNN7EXAMPLE"` → detected |
| GitHub tokens | PII | `"ghp_xxxxxxxxxxxx"` → detected |
| OpenAI keys | PII | `"sk-xxxxxxxx"` → detected |
| Private keys | PII | `"-----BEGIN RSA PRIVATE KEY-----"` → detected |
| Passwords in code | PII | `"password = 'secret123'"` → detected |
| JWT tokens | PII | `"eyJhbGciOiJI..."` → detected |
| URLs with creds | Extended | `"https://user:pass@host"` → detected |

---

### 2.3 Mapping Session
**Goal:** Store original↔alias mappings in memory

| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `IMappingSession` | - |
| Add mapping | `MappingSession` | `AddMapping_WithNewValue_StoresMapping` |
| Get original | - | `GetOriginal_WithValidAlias_ReturnsOriginal` |
| Consistent aliases | - | `AddMapping_SameValueTwice_ReturnsSameAlias` |

**Interface Definition:**
```csharp
public interface IMappingSession
{
    string SessionId { get; }
    void AddMapping(string original, string alias, PatternCategory category);
    string? GetOriginal(string alias);
    string? GetAlias(string original);
    IReadOnlyDictionary<string, string> GetAllMappings();
    void Clear();
}
```

---

### 2.4 Alias Generator
**Goal:** Generate consistent aliases like `DATABASE_0`

| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `IAliasGenerator` | - |
| Generate by category | `AliasGenerator` | `Generate_ForDatabase_ReturnsDATABASE_0` |
| Increment counter | - | `Generate_CalledTwice_ReturnsDATABASE_0_And_1` |

---

### 2.5 Sanitization Engine
**Goal:** Scan content and replace sensitive values

| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `ISanitizationEngine` | - |
| Sanitize content | `SanitizationEngine` | `Sanitize_WithDBName_ReplacesWithAlias` |
| Multiple matches | - | `Sanitize_WithThreeIPs_ReplacesAll` |
| Same value twice | - | `Sanitize_SameValueTwice_UsesSameAlias` |
| Return match list | - | `Sanitize_WithMatches_ReturnsMatchDetails` |

**Interface Definition:**
```csharp
public interface ISanitizationEngine
{
    SanitizationResult Sanitize(string content, SanitizationOptions options);
}
```

---

### 2.6 Desanitization Engine
**Goal:** Restore original values from aliases

| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `IDesanitizationEngine` | - |
| Restore aliases | `DesanitizationEngine` | `Desanitize_WithDATABASE_0_RestoresOriginal` |
| Round-trip test | - | `Sanitize_ThenDesanitize_ReturnsOriginal` |

**Interface Definition:**
```csharp
public interface IDesanitizationEngine
{
    DesanitizationResult Desanitize(string content, IMappingSession session);
}
```

---

### 2.7 UI Integration
**Goal:** Wire sanitization into copy/paste

| Task | Component | Acceptance |
|------|-----------|------------|
| Sanitize on copy | `MainViewModel` | Copy button sanitizes before clipboard |
| Show match count | Status bar | "🔐 12 values masked" |
| Paste dialog | `PasteRestoreDialog.axaml` | Dialog opens on Ctrl+V |
| Restore and copy | `PasteRestoreViewModel` | Restores aliases and copies |

**Phase 2 Exit Criteria:**
- [ ] All 14 patterns detect correctly (unit tests)
- [ ] Sanitize replaces sensitive values with aliases
- [ ] Desanitize restores originals from aliases
- [ ] Round-trip test: original → sanitize → AI response → desanitize → original
- [ ] UI shows sanitization preview before copy

---

## Phase 3: Enhanced UX (Weeks 7-8)

### 3.1 Syntax Highlighting
| Task | Component | Acceptance |
|------|-----------|------------|
| Add AvaloniaEdit | `PreviewPane.axaml` | Code has colors |
| Language detection | `LanguageDetector.cs` | Detects by extension |

### 3.2 Multiple Output Formats
| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `IPromptFormatter` | - |
| XML format | `XmlPromptFormatter` | `Format_WithFiles_ReturnsValidXml` |
| Markdown format | `MarkdownPromptFormatter` | `Format_WithFiles_ReturnsMarkdown` |
| Plain text | `PlainTextPromptFormatter` | `Format_WithFiles_ReturnsPlainText` |

### 3.3 File Watching
| Task | Component | Acceptance |
|------|-----------|------------|
| Watch for changes | `FileWatcherService` | Tree updates on file add/delete |
| Debounce updates | - | Multiple rapid changes = single update |

### 3.4 Search & Filter
| Task | Component | Acceptance |
|------|-----------|------------|
| Search box | `FileTreeView.axaml` | Filters tree as you type |
| Fuzzy matching | `SearchService` | "usrsrv" matches "UserService" |

### 3.5 Model Selection
| Task | Component | Acceptance |
|------|-----------|------------|
| Model dropdown | `MainWindow.axaml` | Can select GPT-4o, Claude, etc. |
| Limit warning | Status bar | Shows warning when exceeding |

**Phase 3 Exit Criteria:**
- [ ] Preview pane shows syntax highlighting
- [ ] Can switch output format (XML/Markdown/Plain)
- [ ] File tree auto-updates on file changes
- [ ] Search filters file tree
- [ ] Model dropdown affects token limit warning

---

## Phase 4: Enterprise Features (Weeks 9-10)

### 4.1 Custom Patterns
| Task | Component | Tests |
|------|-----------|-------|
| YAML parser | `PatternLoader` | `Load_WithValidYaml_ReturnsPatterns` |
| Runtime reload | `IPatternRegistry` | Patterns update without restart |

### 4.2 Audit Logging
| Task | Interface | Tests |
|------|-----------|-------|
| Define interface | `IAuditLogger` | - |
| Log sanitization | `AuditLogger` | `Log_Sanitization_CreatesEntry` |
| SQLite storage | `AuditRepository` | Entries persist across restart |

### 4.3 Settings Persistence
| Task | Component | Tests |
|------|-----------|-------|
| Save settings | `ISettingsRepository` | `Save_Settings_PersistsToFile` |
| Load on startup | - | App remembers last folder |

**Phase 4 Exit Criteria:**
- [ ] Custom patterns work from YAML file
- [ ] Audit log records all sanitizations
- [ ] Settings persist between sessions

---

## Phase 5: Polish & Release (Weeks 11-12)

### 5.1 Installers
| Task | Platform | Artifact |
|------|----------|----------|
| Windows | MSI/MSIX | `ShieldPrompt-1.0.0-win-x64.msi` |
| macOS | DMG | `ShieldPrompt-1.0.0-osx-arm64.dmg` |
| Linux | AppImage | `ShieldPrompt-1.0.0-linux-x64.AppImage` |

### 5.2 Performance
| Target | Metric | Test |
|--------|--------|------|
| Directory load | <2s for 10k files | Benchmark test |
| Sanitization | <50ms per 100KB | Benchmark test |

### 5.3 Documentation
| Document | Location |
|----------|----------|
| User guide | `docs/user-guide.md` |
| Pattern reference | `docs/patterns.md` |

**Phase 5 Exit Criteria:**
- [ ] Installers work on all platforms
- [ ] Performance targets met
- [ ] Documentation complete

---

## Quick Reference: Interface Summary

```csharp
// Domain
public interface IFileAggregationService { ... }  // 4 methods
public interface ITokenCountingService { ... }    // 3 methods
public interface IPatternRegistry { ... }         // 3 methods
public interface IMappingSession { ... }          // 6 methods
public interface IAliasGenerator { ... }          // 1 method
public interface ISanitizationEngine { ... }      // 1 method
public interface IDesanitizationEngine { ... }    // 1 method
public interface IPromptFormatter { ... }         // 1 method
public interface IAuditLogger { ... }             // 2 methods
public interface ISettingsRepository { ... }      // 2 methods
```

**Total: 10 interfaces, ~24 methods** - All ISP-compliant (<10 methods each)

---

## TDD Workflow Reminder

For each task:
```
1. Write failing test
2. Write minimum code to pass
3. Refactor if needed
4. Commit
```

**Run tests continuously:**
```bash
dotnet watch test --project tests/ShieldPrompt.Tests.Unit
```

---

## Getting Started

```bash
# 1. Create solution structure
dotnet new sln -n ShieldPrompt
dotnet new classlib -n ShieldPrompt.Domain -o src/ShieldPrompt.Domain
dotnet new classlib -n ShieldPrompt.Application -o src/ShieldPrompt.Application
# ... (see SPECIFICATION.md Section 3.3 for full structure)

# 2. Start with Domain (no dependencies)
cd src/ShieldPrompt.Domain

# 3. Write first test
# tests/ShieldPrompt.Tests.Unit/Domain/FileNodeTests.cs

# 4. Make it pass
# src/ShieldPrompt.Domain/Entities/FileNode.cs
```

---

*Implementation plan follows SPECIFICATION.md directly. No additional architecture required.*

