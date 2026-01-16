# ShieldPrompt Complete Function Inventory & Rebuild Plan

**Version:** 2.0.0  
**Last Updated:** January 15, 2026  
**Purpose:** Complete analysis of every function for ground-up rebuild  
**Status:** AUTHORITATIVE REFERENCE

---

## Executive Summary

This document provides a **complete inventory of every function** in ShieldPrompt for a ground-up rebuild. It catalogs:
- Every service method
- Every ViewModel command
- Every domain model
- Every interface contract
- Every business rule

**Source Analysis:**
- Old Source: `/Users/admin/Dev/YOLOProjects/shield-prompt/src_old` 
- New Source: `/Users/admin/Dev/YOLOProjects/shield-prompt/src`

---

## Table of Contents

1. [Domain Layer Functions](#1-domain-layer)
2. [Application Layer Functions](#2-application-layer)
3. [Infrastructure Layer Functions](#3-infrastructure-layer)
4. [Sanitization Layer Functions](#4-sanitization-layer)
5. [Presentation Layer Functions](#5-presentation-layer)
6. [Cross-Cutting Concerns](#6-cross-cutting-concerns)
7. [Rebuild Priority Matrix](#7-rebuild-priority-matrix)

---

## 1. Domain Layer

### 1.1 Entities

#### FileNode
**File:** `ShieldPrompt.Domain/Entities/FileNode.cs`

| Property/Method | Type | Description | Priority |
|-----------------|------|-------------|----------|
| `Path` | `string` | Full path to file/directory | P0 |
| `Name` | `string` | File/directory name | P0 |
| `IsDirectory` | `bool` | Whether node is directory | P0 |
| `IsSelected` | `bool` | Selection state for prompt | P0 |
| `Content` | `string` | File text content | P0 |
| `Children` | `IReadOnlyList<FileNode>` | Child nodes | P0 |
| `Extension` | `string` | File extension | P0 |
| `AddChild(FileNode)` | `void` | Add child node | P0 |

#### Workspace
**File:** `ShieldPrompt.Domain/Entities/Workspace.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `Id` | `string` | Unique identifier | P0 |
| `Name` | `string` | Display name | P0 |
| `RootPath` | `string` | Workspace root path | P0 |
| `LastOpened` | `DateTime` | Last access time | P1 |
| `CreatedAt` | `DateTime` | Creation time | P1 |
| `PreferredRole` | `string?` | Preferred role ID | P1 |
| `PreferredModel` | `string?` | Preferred model ID | P1 |
| `RecentFiles` | `IReadOnlyList<string>` | Recent files | P2 |
| `CustomSettings` | `IReadOnlyDictionary` | Per-workspace settings | P2 |

#### Pattern
**File:** `ShieldPrompt.Domain/Entities/Pattern.cs`

| Property/Method | Type | Description | Priority |
|-----------------|------|-------------|----------|
| `Name` | `string` | Pattern name | P0 |
| `RegexPattern` | `string` | Regex string | P0 |
| `Category` | `PatternCategory` | Pattern category | P0 |
| `Priority` | `int` | Processing priority | P0 |
| `Enabled` | `bool` | Is pattern active | P0 |
| `CreateRegex()` | `Regex` | Compile regex | P0 |

#### ModelProfile
**File:** `ShieldPrompt.Domain/Entities/ModelProfile.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `Name` | `string` | Model identifier | P0 |
| `DisplayName` | `string` | UI display name | P0 |
| `ContextLimit` | `int` | Max tokens | P0 |
| `TokenizerEncoding` | `string` | Tiktoken encoding | P0 |

#### AppSettings
**File:** `ShieldPrompt.Domain/Entities/AppSettings.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `LastFolderPath` | `string?` | Last opened folder | P1 |
| `LastFormatName` | `string?` | Last format | P1 |
| `LastModelName` | `string?` | Last model | P1 |
| `LastSelectedFiles` | `List<string>?` | Last selection | P1 |

---

### 1.2 Records

#### Role
**File:** `ShieldPrompt.Domain/Records/Role.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `Id` | `string` | Unique identifier | P0 |
| `Name` | `string` | Display name | P0 |
| `Icon` | `string` | Emoji icon | P0 |
| `Description` | `string` | Short description | P0 |
| `SystemPrompt` | `string` | Full system prompt | P0 |
| `Tone` | `string?` | Tone guidance | P1 |
| `Style` | `string?` | Style guidance | P1 |
| `Priorities` | `IReadOnlyList<string>` | Priority areas | P1 |
| `Expertise` | `IReadOnlyList<string>` | Expertise areas | P1 |

#### PromptTemplate
**File:** `ShieldPrompt.Domain/Records/PromptTemplate.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `Id` | `string` | Unique identifier | P0 |
| `Name` | `string` | Display name | P0 |
| `Icon` | `string` | Emoji icon | P0 |
| `Description` | `string` | Short description | P0 |
| `SystemPrompt` | `string` | Template content | P0 |
| `FocusOptions` | `IReadOnlyList<string>` | Focus area options | P1 |
| `RequiresCustomInput` | `bool` | Requires instructions | P1 |

#### SanitizationResult
**File:** `ShieldPrompt.Domain/Records/SanitizationResult.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `SanitizedContent` | `string` | Result content | P0 |
| `WasSanitized` | `bool` | Had matches | P0 |
| `Matches` | `IReadOnlyList<SanitizationMatch>` | All matches | P0 |
| `TotalMatches` | `int` | Match count | P0 |

#### SanitizationMatch
**File:** `ShieldPrompt.Domain/Records/SanitizationMatch.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `Original` | `string` | Original value | P0 |
| `Alias` | `string` | Generated alias | P0 |
| `Category` | `PatternCategory` | Pattern category | P0 |
| `PatternName` | `string` | Matching pattern | P0 |
| `StartIndex` | `int` | Position in text | P0 |
| `Length` | `int` | Match length | P0 |

#### FileOperation
**File:** `ShieldPrompt.Domain/Records/FileOperation.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `Type` | `FileOperationType` | CREATE/UPDATE/DELETE | P0 |
| `Path` | `string` | Target file path | P0 |
| `Content` | `string?` | New content | P0 |
| `Reason` | `string?` | Change reason | P1 |
| `StartLine` | `int?` | For partial update | P1 |
| `EndLine` | `int?` | For partial update | P1 |

#### OutputFormatSettings
**File:** `ShieldPrompt.Domain/Records/OutputFormatSettings.cs`

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `ResponseFormat` | `ResponseFormat` | Format mode | P0 |
| `EnablePartialUpdates` | `bool` | Allow line edits | P1 |
| `AutoApply` | `bool` | Auto-apply changes | P1 |
| `ConflictResolution` | `ConflictStrategy` | How to resolve | P1 |

---

### 1.3 Enums

| Enum | Values | Priority |
|------|--------|----------|
| `PatternCategory` | Database, Server, IPAddress, Hostname, ConnectionString, FilePath, SSN, CreditCard, APIKey, AWSKey, GitHubToken, OpenAIKey, AnthropicKey, SlackToken, AzureKey, PrivateKey, Password, BearerToken, Custom | P0 |
| `PolicyMode` | Unrestricted, SanitizedOnly, Blocked | P1 |
| `FileOperationType` | Create, Update, PartialUpdate, Delete | P0 |
| `ResponseFormat` | Auto, HybridXmlMarkdown, PureXml, StructuredMarkdown, Json, PlainText | P0 |

---

## 2. Application Layer

### 2.1 Interfaces & Methods

#### IFileAggregationService
**File:** `ShieldPrompt.Application/Interfaces/IFileAggregationService.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `LoadDirectoryAsync` | `Task<FileNode>(string path, CancellationToken)` | Load directory tree | P0 |

#### ITokenCountingService
**File:** `ShieldPrompt.Application/Interfaces/ITokenCountingService.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `CountTokens` | `int(string content)` | Count tokens in text | P0 |
| `CountFileTokensAsync` | `Task<TokenCount>(FileNode file)` | Count file tokens | P0 |

#### IPromptComposer (NEW NAME for IPromptBuilderService)
**File:** `ShieldPrompt.Application/Interfaces/IPromptComposer.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `Compose` | `ComposedPrompt(PromptTemplate, IEnumerable<FileNode>, PromptOptions)` | Build prompt | P0 |

#### IPromptFormatter
**File:** `ShieldPrompt.Application/Interfaces/IPromptFormatter.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `Format` | `string(IEnumerable<FileNode>)` | Format files | P0 |
| `FormatName` | `string { get; }` | Format identifier | P0 |

#### IPromptTemplateRepository
**File:** `ShieldPrompt.Application/Interfaces/IPromptTemplateRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `GetAllTemplates` | `IEnumerable<PromptTemplate>()` | Get all templates | P0 |
| `GetById` | `PromptTemplate?(string id)` | Get by ID | P0 |
| `GetDefault` | `PromptTemplate()` | Get default template | P0 |

#### IRoleRepository
**File:** `ShieldPrompt.Application/Interfaces/IRoleRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `GetAllRoles` | `IEnumerable<Role>()` | Get all roles | P0 |
| `GetById` | `Role?(string id)` | Get by ID | P0 |
| `GetDefault` | `Role()` | Get default role | P0 |

#### ICustomRoleRepository
**File:** `ShieldPrompt.Application/Interfaces/ICustomRoleRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `GetCustomRoles` | `IEnumerable<Role>()` | Get custom roles | P1 |
| `SaveRole` | `Task(Role)` | Save custom role | P1 |
| `DeleteRole` | `Task(string id)` | Delete custom role | P1 |

#### IStructuredResponseParser
**File:** `ShieldPrompt.Application/Interfaces/IStructuredResponseParser.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `ParseAsync` | `Task<ParseResult>(string, ResponseFormat, CancellationToken)` | Parse LLM response | P0 |
| `CanParse` | `bool(string, ResponseFormat)` | Check if parseable | P0 |
| `DetectFormats` | `IReadOnlyList<ResponseFormat>(string)` | Detect format | P0 |

#### IAIResponseParser (LEGACY - merge with IStructuredResponseParser)
**File:** `ShieldPrompt.Application/Interfaces/IAIResponseParser.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `ParseFileChanges` | `IEnumerable<FileChange>(string)` | Extract file changes | P0 |

#### IFileWriterService
**File:** `ShieldPrompt.Application/Interfaces/IFileWriterService.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `ApplyUpdatesAsync` | `Task<FileWriteResult>(IEnumerable<FileUpdate>, string, FileWriteOptions, CancellationToken)` | Apply file updates | P0 |
| `CreateBackupAsync` | `Task<string>(IEnumerable<string>, CancellationToken)` | Create backup | P0 |
| `RestoreBackupAsync` | `Task(string, CancellationToken)` | Restore from backup | P0 |

#### IUndoRedoManager
**File:** `ShieldPrompt.Application/Interfaces/IUndoRedoManager.cs`

| Property/Method | Signature | Description | Priority |
|-----------------|-----------|-------------|----------|
| `CanUndo` | `bool { get; }` | Can undo | P0 |
| `CanRedo` | `bool { get; }` | Can redo | P0 |
| `UndoDescription` | `string? { get; }` | Undo action name | P0 |
| `RedoDescription` | `string? { get; }` | Redo action name | P0 |
| `UndoAsync` | `Task()` | Execute undo | P0 |
| `RedoAsync` | `Task()` | Execute redo | P0 |
| `RegisterAction` | `void(IUndoableAction)` | Add action to stack | P0 |
| `PeekUndo` | `IUndoableAction?()` | Get next undo | P0 |
| `Clear` | `void()` | Clear stack | P0 |
| `StateChanged` | `event EventHandler?` | State change event | P0 |

#### IUndoableAction
**File:** `ShieldPrompt.Application/Interfaces/IUndoableAction.cs`

| Property/Method | Signature | Description | Priority |
|-----------------|-----------|-------------|----------|
| `Description` | `string { get; }` | Action description | P0 |
| `RequiresConfirmation` | `bool { get; }` | Need confirmation | P0 |
| `ConfirmationMessage` | `string? { get; }` | Confirmation text | P0 |
| `ExecuteAsync` | `Task()` | Do action | P0 |
| `UndoAsync` | `Task()` | Undo action | P0 |

#### IWorkspaceRepository
**File:** `ShieldPrompt.Application/Interfaces/IWorkspaceRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `GetAllAsync` | `Task<IReadOnlyList<Workspace>>()` | Get all workspaces | P1 |
| `GetByIdAsync` | `Task<Workspace?>(string)` | Get by ID | P1 |
| `GetByPathAsync` | `Task<Workspace?>(string)` | Get by path | P1 |
| `SaveAsync` | `Task(Workspace)` | Save workspace | P1 |
| `DeleteAsync` | `Task(string)` | Delete workspace | P1 |

#### IOutputFormatSettingsRepository
**File:** `ShieldPrompt.Application/Interfaces/IOutputFormatSettingsRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `LoadAsync` | `Task<OutputFormatSettings>()` | Load settings | P1 |
| `SaveAsync` | `Task(OutputFormatSettings)` | Save settings | P1 |

#### IFormatMetadataRepository
**File:** `ShieldPrompt.Application/Interfaces/IFormatMetadataRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `GetAll` | `IEnumerable<FormatMetadata>()` | Get all formats | P1 |
| `GetById` | `FormatMetadata?(string)` | Get by ID | P1 |

---

### 2.2 Service Implementations

#### FileAggregationService
**Functions:**
- `LoadDirectoryAsync()` - Recursively load directory tree
- Smart exclusions for binary, node_modules, .git
- Parallel file loading

#### TokenCountingService  
**Functions:**
- `CountTokens()` - Use TiktokenSharp for counting
- `CountFileTokensAsync()` - Async file token counting
- Caching for performance

#### PromptComposer
**Functions:**
- `Compose()` - Build complete prompt from template + files + options
- `SubstituteVariables()` - Replace template variables
- `BuildUserContent()` - Format file listings
- `BuildFullPrompt()` - Combine all sections
- `DetectPrimaryLanguage()` - Detect code language
- `MapExtensionToLanguage()` - Extension to language mapping

#### UndoRedoManager
**Functions:**
- Stack-based undo/redo
- Action batching
- Confirmation support for dangerous actions
- State change notifications

#### FileWriterService
**Functions:**
- `ApplyUpdatesAsync()` - Apply file operations
- `CreateBackupAsync()` - Create backup before changes
- `RestoreBackupAsync()` - Restore from backup
- Path validation (security)
- Directory creation

#### StructuredResponseParser
**Functions:**
- `ParseAsync()` - Parse LLM response
- `ParseHybridXmlMarkdownAsync()` - Parse XML-in-Markdown
- `ParsePureXmlAsync()` - Parse pure XML
- `ParseStructuredMarkdownAsync()` - Parse structured MD
- `ParseJsonAsync()` - Parse JSON format
- `ParsePlainTextAsync()` - Plain text fallback
- `DetectFormats()` - Auto-detect format
- `ExtractXmlFromMarkdown()` - Extract XML from fences
- `ParseShieldPromptXml()` - Parse <shieldprompt> XML
- `ParseFileElement()` - Parse <file> element
- `ValidateFilePath()` - Security validation

---

### 2.3 Formatters

#### PlainTextFormatter
**Functions:**
- `Format()` - Plain text file format
- `FormatName` = "Plain Text"

#### MarkdownFormatter  
**Functions:**
- `Format()` - Markdown with code fences
- `FormatName` = "Markdown"

#### XmlFormatter
**Functions:**
- `Format()` - XML structure format
- `FormatName` = "XML"

#### HybridXmlMarkdownFormatter
**Functions:**
- `Format()` - Response format instructions
- Action-only mode support

---

## 3. Infrastructure Layer

### 3.1 Interfaces

#### ISettingsRepository
**File:** `ShieldPrompt.Infrastructure/Interfaces/ISettingsRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `LoadAsync` | `Task<AppSettings>()` | Load settings | P0 |
| `SaveAsync` | `Task(AppSettings)` | Save settings | P0 |

#### ILayoutStateRepository
**File:** `ShieldPrompt.Infrastructure/Interfaces/ILayoutStateRepository.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `LoadLayoutStateAsync` | `Task<LayoutState?>()` | Load layout | P1 |
| `SaveLayoutStateAsync` | `Task(LayoutState)` | Save layout | P1 |
| `ResetToDefaultAsync` | `Task()` | Reset layout | P1 |

### 3.2 Repository Implementations

#### JsonSettingsRepository
**Functions:**
- `LoadAsync()` - Load JSON settings file
- `SaveAsync()` - Save JSON settings file
- Path resolution per platform

#### YamlPromptTemplateRepository
**Functions:**
- `GetAllTemplates()` - Load from YAML
- `GetById()` - Find by ID
- `GetDefault()` - Get default template

#### YamlRoleRepository
**Functions:**
- `GetAllRoles()` - Load from YAML
- `GetById()` - Find by ID
- `GetDefault()` - Get software_engineer role

#### JsonCustomRoleRepository
**Functions:**
- `GetCustomRoles()` - Load user roles
- `SaveRole()` - Save custom role
- `DeleteRole()` - Delete custom role

#### JsonLayoutStateRepository
**Functions:**
- `LoadLayoutStateAsync()` - Load panel state
- `SaveLayoutStateAsync()` - Save panel state
- `ResetToDefaultAsync()` - Reset to defaults

#### JsonWorkspaceRepository
**Functions:**
- `GetAllAsync()` - List workspaces
- `GetByIdAsync()` - Get by ID
- `GetByPathAsync()` - Get by path
- `SaveAsync()` - Save workspace
- `DeleteAsync()` - Delete workspace

---

## 4. Sanitization Layer

### 4.1 Interfaces

#### ISanitizationEngine
**File:** `ShieldPrompt.Sanitization/Interfaces/ISanitizationEngine.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `Sanitize` | `SanitizationResult(string, SanitizationOptions)` | Sanitize content | P0 |

#### IDesanitizationEngine
**File:** `ShieldPrompt.Sanitization/Interfaces/IDesanitizationEngine.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `Desanitize` | `DesanitizationResult(string, IMappingSession)` | Restore content | P0 |

#### IMappingSession
**File:** `ShieldPrompt.Sanitization/Interfaces/IMappingSession.cs`

| Property/Method | Signature | Description | Priority |
|-----------------|-----------|-------------|----------|
| `SessionId` | `string { get; }` | Session identifier | P0 |
| `CreatedAt` | `DateTime { get; }` | Creation time | P0 |
| `ExpiresAt` | `DateTime { get; }` | Expiry time | P0 |
| `AddMapping` | `void(string, string, PatternCategory)` | Add mapping | P0 |
| `GetOriginal` | `string?(string)` | Alias to original | P0 |
| `GetAlias` | `string?(string)` | Original to alias | P0 |
| `GetAllMappings` | `IEnumerable<KeyValuePair<string,string>>()` | Get all | P0 |
| `Clear` | `void()` | Clear mappings | P0 |
| `Dispose` | `void()` | Secure dispose | P0 |

#### IPatternRegistry
**File:** `ShieldPrompt.Sanitization/Interfaces/IPatternRegistry.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `GetPatterns` | `IEnumerable<Pattern>()` | Get all patterns | P0 |
| `AddPattern` | `void(Pattern)` | Add pattern | P1 |
| `RemovePattern` | `void(string)` | Remove pattern | P1 |

#### IAliasGenerator
**File:** `ShieldPrompt.Sanitization/Interfaces/IAliasGenerator.cs`

| Method | Signature | Description | Priority |
|--------|-----------|-------------|----------|
| `Generate` | `string(PatternCategory)` | Generate alias | P0 |

### 4.2 Implementations

#### SanitizationEngine
**Functions:**
- `Sanitize()` - Main sanitization entry
- `ShouldProcessPattern()` - Check if pattern enabled
- Pattern matching with compiled regex
- Reverse-order replacement (preserve indices)
- Session mapping storage

#### DesanitizationEngine
**Functions:**
- `Desanitize()` - Restore original values
- Alias detection in text
- Session lookup

#### MappingSession
**Functions:**
- In-memory mapping store
- Thread-safe operations
- Secure disposal (overwrite before clear)
- Session timeout handling

#### AliasGenerator
**Functions:**
- `Generate()` - Create category-specific alias
- Counter per category
- Consistent naming (DATABASE_0, IP_ADDRESS_0, etc.)

#### PatternRegistry
**Functions:**
- Built-in pattern loading
- Custom pattern loading from YAML
- Pattern validation

### 4.3 Built-in Patterns (14 Total)

| Pattern | Category | Regex | Priority |
|---------|----------|-------|----------|
| Database Names | Database | `(?i)(prod\|production\|...)` | P0 |
| Private IPs | IPAddress | `\b(10\.\d{1,3}...)` | P0 |
| Connection Strings | ConnectionString | `(?i)(server\|host...)` | P0 |
| Windows Paths | FilePath | `(?i)([a-z]:\\...)` | P0 |
| Internal Hostnames | Hostname | `\b[a-z0-9]...internal...` | P0 |
| SSN | SSN | `\b\d{3}[-\s]?\d{2}...` | P0 |
| Credit Cards | CreditCard | `\b(?:4\d{3}\|5...)` | P0 |
| AWS Keys | AWSKey | `AKIA[0-9A-Z]{16}` | P0 |
| GitHub Tokens | GitHubToken | `gh[pousr]_...` | P0 |
| OpenAI Keys | OpenAIKey | `sk-[A-Za-z0-9]{48}` | P0 |
| Anthropic Keys | AnthropicKey | `sk-ant-...` | P0 |
| Slack Tokens | SlackToken | `xox[baprs]-...` | P0 |
| Private Keys | PrivateKey | `-----BEGIN...PRIVATE KEY-----` | P0 |
| Passwords | Password | `(?i)(password\|pwd...)` | P0 |

---

## 5. Presentation Layer

### 5.1 ViewModels

#### MainWindowViewModel
**Total Commands: 35+**

| Command | Method | Description | Priority |
|---------|--------|-------------|----------|
| `OpenFolderCommand` | `OpenFolderAsync()` | Open folder picker | P0 |
| `CopyToClipboardCommand` | `CopyToClipboardAsync()` | Copy sanitized prompt | P0 |
| `OpenPasteRestoreDialogCommand` | `OpenPasteRestoreDialogAsync()` | Paste dialog | P0 |
| `UndoCommand` | `UndoAsync()` | Undo action | P0 |
| `RedoCommand` | `RedoAsync()` | Redo action | P0 |
| `SelectAllCommand` | `SelectAll()` | Select all files | P0 |
| `DeselectAllCommand` | `DeselectAll()` | Deselect all | P0 |
| `ToggleSanitizationPreviewCommand` | `ToggleSanitizationPreview()` | Toggle preview | P1 |
| `CopyLivePreviewCommand` | `CopyLivePreview()` | Copy preview | P0 |
| `ExitCommand` | `Exit()` | Close app | P0 |
| `LoadTutorialProjectCommand` | `LoadTutorialProjectAsync()` | Load tutorial | P2 |
| `ClearSessionCommand` | `ClearSession()` | Clear sanitization | P1 |
| `ShowDiagnosticsCommand` | `ShowDiagnosticsAsync()` | Show diagnostics | P2 |
| `ViewLogsCommand` | `ViewLogsAsync()` | Open logs folder | P2 |
| `OpenUserGuideCommand` | `OpenUserGuide()` | Open docs | P2 |
| `OpenTutorialCommand` | `OpenTutorial()` | Open tutorial | P2 |
| `ShowKeyboardShortcutsCommand` | `ShowKeyboardShortcuts()` | Show shortcuts | P2 |
| `OpenGitHubCommand` | `OpenGitHub()` | Open GitHub | P2 |
| `CheckForUpdatesCommand` | `CheckForUpdatesAsync()` | Check updates | P2 |
| `ShowAboutDialogCommand` | `ShowAboutDialog()` | Show about | P2 |
| `IncreaseFontSizeCommand` | `IncreaseFontSize()` | Increase font | P2 |
| `DecreaseFontSizeCommand` | `DecreaseFontSize()` | Decrease font | P2 |
| `ResetFontSizeCommand` | `ResetFontSize()` | Reset font | P2 |
| `ToggleToolbarCommand` | `ToggleToolbar()` | Toggle toolbar | P2 |
| `ToggleStatusBarCommand` | `ToggleStatusBar()` | Toggle status | P2 |
| `ResetLayoutCommand` | `ResetLayout()` | Reset layout | P1 |

**Observable Properties: 30+**

| Property | Type | Description | Priority |
|----------|------|-------------|----------|
| `RootNode` | `FileNode?` | File tree root | P0 |
| `RootNodeViewModel` | `FileNodeViewModel?` | Tree VM root | P0 |
| `StatusText` | `string` | Status message | P0 |
| `IsLoading` | `bool` | Loading state | P0 |
| `TotalTokens` | `int` | Token count | P0 |
| `SelectedFileCount` | `int` | Selected files | P0 |
| `SanitizedValueCount` | `int` | Sanitized count | P0 |
| `SelectedModel` | `ModelProfile` | Current model | P0 |
| `SelectedFormatter` | `IPromptFormatter` | Current format | P0 |
| `SelectedTemplate` | `PromptTemplate?` | Current template | P0 |
| `SelectedRole` | `Role?` | Current role | P0 |
| `CustomInstructions` | `string` | User instructions | P0 |
| `LivePreview` | `string` | Preview content | P0 |
| `PreviewTokenCount` | `int` | Preview tokens | P0 |
| `ShowTokenWarning` | `bool` | Token warning | P1 |
| `ShowSanitizationPreview` | `bool` | Show preview | P1 |
| `SanitizationPreview` | `string` | Preview text | P1 |
| `CanUndo` | `bool` | Can undo | P0 |
| `CanRedo` | `bool` | Can redo | P0 |
| `UndoDescription` | `string?` | Undo name | P0 |
| `RedoDescription` | `string?` | Redo name | P0 |
| `ShowToolbar` | `bool` | Toolbar visible | P1 |
| `ShowStatusBar` | `bool` | Status visible | P1 |
| `FileTreeWidth` | `double` | Panel width | P1 |
| `PromptBuilderHeightRatio` | `double` | Panel height | P1 |
| `IsFileTreeCollapsed` | `bool` | Collapsed state | P1 |
| `IsPromptBuilderCollapsed` | `bool` | Collapsed state | P1 |
| `IsPreviewCollapsed` | `bool` | Collapsed state | P1 |
| `IsCopyFlashActive` | `bool` | Flash animation | P1 |
| `HasFocusAreas` | `bool` | Focus areas available | P1 |

**Private Methods:**

| Method | Description | Priority |
|--------|-------------|----------|
| `InitializeAsync()` | Load settings, restore state | P0 |
| `SaveSettingsAsync()` | Persist settings | P0 |
| `LoadFolderAsync()` | Load directory | P0 |
| `UpdateTokenCountsAsync()` | Calculate tokens | P0 |
| `UpdateSelectedFileCount()` | Update counter | P0 |
| `UpdateEstimatedTokenCount()` | Update counter | P0 |
| `UpdateLivePreview()` | Regenerate preview | P0 |
| `UpdateSanitizationPreview()` | Update preview | P0 |
| `SubscribeToFileSelectionChanges()` | Event subscription | P0 |
| `OnUndoRedoStateChanged()` | Undo state handler | P0 |
| `OnViewModelPropertyChanged()` | Property handler | P0 |
| `OnLayoutPropertyChanged()` | Layout handler | P1 |
| `SaveLayoutStateAsync()` | Save layout | P1 |
| `GetSelectedFiles()` | Get selected nodes | P0 |
| `GetSelectedFilesFromViewModel()` | Get from VM | P0 |
| `GetSelectedFilePaths()` | Get paths | P0 |
| `RestoreFileSelection()` | Restore selection | P1 |
| `SelectAllNodes()` | Select all helper | P0 |
| `CountFiles()` | Count files | P0 |
| `SumSelectedTokens()` | Sum tokens | P0 |
| `CountSelectedFiles()` | Count selected | P0 |
| `GetCategoryIcon()` | Get emoji icon | P1 |
| `OpenUrl()` | Open URL helper | P2 |

#### FileNodeViewModel (v1)
**Properties:**
- `Path`, `Name`, `IsDirectory`, `Extension`
- `IsSelected`, `IsExpanded`, `TokenCount`
- `Children` (recursive)
- `Icon` (computed)
- `TokenCountDisplay` (computed)

#### FileNodeViewModel (v2 - Enhanced)
**Properties:**
- All v1 properties
- `IsChecked` (three-state: true/false/null)
- `Parent` (for upward propagation)
- `GitStatus` (for v2.0)

**Methods:**
- `OnIsCheckedChanged()` - Cascading selection
- `SetChildrenChecked()` - Cascade down
- `UpdateStateFromChildren()` - Cascade up
- `NotifyParentOfChange()` - Notify parent
- `SelectAll()` - Select all descendants
- `DeselectAll()` - Deselect all descendants
- `GetSelectedFiles()` - Get selected files
- `GetAllFiles()` - Get all files

#### RoleEditorViewModel
**Commands & Properties:**
- `SaveRoleCommand`
- `CancelCommand`
- `DeleteRoleCommand`
- `RoleName`, `Description`, `Icon`, `SystemPrompt`
- `Tone`, `Priorities`, `Expertise`

#### OutputFormatSettingsViewModel
**Commands & Properties:**
- `SaveSettingsCommand`
- `ResponseFormat`, `EnablePartialUpdates`
- `AutoApply`, `ConflictResolution`

#### LlmResponseViewModel
**Commands & Properties:**
- `ParseResponseCommand`
- `ApplyChangesCommand`
- `ResponseText`, `ParsedOperations`
- `Statistics`, `HasErrors`

#### PasteRestoreViewModel
**Commands & Properties:**
- `RestoreCommand`
- `CopyRestoredCommand`
- `ApplyToFilesCommand`
- `RawResponse`, `RestoredContent`
- `DetectedAliases`, `FileOperations`

---

### 5.2 Views (XAML)

#### MainWindow.axaml
**Sections:**
- Menu bar (File, Edit, View, Tools, Help)
- Toolbar (Open, Refresh, Model dropdown, Role dropdown, Template dropdown)
- File tree panel (left)
- Preview panel (center)
- Instructions panel (bottom)
- Action bar (Copy, Paste, Sanitization count)
- Status bar (Ready, file count, tokens)

#### PasteRestoreDialog.axaml
**Sections:**
- Response input area
- Alias detection list
- Restored preview
- Action buttons

---

## 6. Cross-Cutting Concerns

### 6.1 Configuration Files

| File | Purpose | Format | Priority |
|------|---------|--------|----------|
| `config/roles.yaml` | Role definitions | YAML | P0 |
| `config/prompt-templates.yaml` | Template definitions | YAML | P0 |
| `settings.json` | App settings | JSON | P0 |
| `layout.json` | UI layout state | JSON | P1 |
| `custom-roles.json` | User-defined roles | JSON | P1 |
| `output-format-settings.json` | Format settings | JSON | P1 |

### 6.2 External Dependencies

| Package | Purpose | Version |
|---------|---------|---------|
| `Avalonia` | UI Framework | 11.2.* |
| `CommunityToolkit.Mvvm` | MVVM | 8.3.* |
| `TiktokenSharp` | Token counting | 1.2.* |
| `TextCopy` | Clipboard | 6.2.* |
| `YamlDotNet` | YAML parsing | 16.* |

---

## 7. Rebuild Priority Matrix

### Phase 1: Core MVP (P0 - Week 1-2)

| Component | Functions | Tests |
|-----------|-----------|-------|
| FileNode entity | All properties | ✅ |
| FileAggregationService | LoadDirectoryAsync | ✅ |
| TokenCountingService | CountTokens, CountFileTokens | ✅ |
| FileNodeViewModel (basic) | Selection, expansion | ✅ |
| MainWindowViewModel (basic) | OpenFolder, CopyToClipboard | ✅ |
| MainWindow.axaml (basic) | File tree, preview | - |

### Phase 2: Sanitization (P0 - Week 2-3)

| Component | Functions | Tests |
|-----------|-----------|-------|
| Pattern entity | All properties, CreateRegex | ✅ |
| PatternRegistry | GetPatterns | ✅ |
| AliasGenerator | Generate | ✅ |
| MappingSession | All methods | ✅ |
| SanitizationEngine | Sanitize | ✅ |
| DesanitizationEngine | Desanitize | ✅ |

### Phase 3: Prompt Building (P0 - Week 3-4)

| Component | Functions | Tests |
|-----------|-----------|-------|
| PromptTemplate record | All properties | ✅ |
| Role record | All properties | ✅ |
| PromptComposer | Compose | ✅ |
| Formatters | Format | ✅ |
| TemplateRepository | GetAllTemplates | ✅ |
| RoleRepository | GetAllRoles | ✅ |

### Phase 4: Response Parsing (P0 - Week 4-5)

| Component | Functions | Tests |
|-----------|-----------|-------|
| FileOperation record | All properties | ✅ |
| StructuredResponseParser | ParseAsync, all formats | ✅ |
| FileWriterService | ApplyUpdates, Backup | ✅ |
| UndoRedoManager | All methods | ✅ |

### Phase 5: Enhanced Features (P1 - Week 5-6)

| Component | Functions | Tests |
|-----------|-----------|-------|
| Workspace entity | All properties | ✅ |
| WorkspaceRepository | CRUD | ✅ |
| CustomRoleRepository | CRUD | ✅ |
| LayoutStateRepository | Save/Load | ✅ |
| OutputFormatSettings | All properties | ✅ |

### Phase 6: V2 UI (P1 - Week 6-8)

| Component | Functions | Tests |
|-----------|-----------|-------|
| MainWindowV2ViewModel | All commands | ✅ |
| FileNodeViewModel (v2) | Cascading selection | ✅ |
| Multi-tab sessions | All | ✅ |
| Git integration | Status display | ✅ |
| Presets | Save/Load | ✅ |

---

## Summary Statistics

| Category | Count |
|----------|-------|
| **Domain Entities** | 5 |
| **Domain Records** | 8 |
| **Domain Enums** | 4 |
| **Application Interfaces** | 15 |
| **Application Services** | 8 |
| **Infrastructure Repositories** | 7 |
| **Sanitization Components** | 5 |
| **ViewModels** | 7 |
| **Total Functions** | ~200+ |
| **Total Tests Required** | ~300+ |

---

## Recommendation

### Ground-Up Rebuild Strategy

1. **Start Fresh** in `/Users/admin/Dev/YOLOProjects/shield-prompt/src`
2. **Follow TDD** - Write tests first for every function
3. **ISP Compliance** - Every interface ≤5 methods
4. **MVVM Strict** - Zero business logic in ViewModels
5. **Reuse Domain** - Domain models are clean, reuse as-is
6. **Clean Interfaces** - Define all interfaces before implementation
7. **Incremental** - Build layer by layer (Domain → Application → Infrastructure → Presentation)

### Estimated Timeline

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Phase 1 | 2 weeks | Core file loading, display |
| Phase 2 | 1 week | Full sanitization |
| Phase 3 | 1 week | Prompt generation |
| Phase 4 | 1 week | Response parsing |
| Phase 5 | 1 week | Enhanced features |
| Phase 6 | 2 weeks | V2 UI |
| **Total** | **8 weeks** | **Complete rebuild** |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Complete function inventory |

