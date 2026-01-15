# ShieldPrompt - Product Specification Document
## Secure Prompt Generation for Enterprise AI Workflows

**Version:** 1.0.0  
**Last Updated:** January 14, 2026  
**Target Platform:** .NET 8+ Desktop (Windows, macOS, Linux via Avalonia UI)

---

## Executive Summary

ShieldPrompt is a .NET desktop application that combines the best features of RepoPrompt and PasteMax for code aggregation and prompt generation, with enterprise-grade data sanitization from OpenCode Enterprise Shield. It enables developers to safely use AI coding assistants like ChatGPT/Claude by automatically masking sensitive data before copy and restoring it after paste.

### Core Value Proposition

```
┌─────────────────────────────────────────────────────────────────────────┐
│  SELECT FILES  →  AGGREGATE  →  🔐 SANITIZE  →  COPY TO CLIPBOARD       │
│                                                                         │
│  PASTE FROM AI  →  🔓 DESANITIZE  →  APPLY TO CODEBASE                 │
└─────────────────────────────────────────────────────────────────────────┘
```

**Without ShieldPrompt:**
- Production database names exposed: `ProductionDB`
- Internal hostnames leaked: `db-prod-01.internal.company.com`
- API keys in code sent to external AI
- Compliance violations (HIPAA, GDPR, SOC 2)

**With ShieldPrompt:**
- AI sees: `DATABASE_0`, `HOSTNAME_0`, `API_KEY_0`
- Developer gets back fully functional code with real values restored
- Complete audit trail of what was sanitized
- Zero-knowledge architecture

---

## Table of Contents

1. [Feature Analysis: RepoPrompt & PasteMax](#1-feature-analysis-repoprompt--pastemax)
2. [Feature Analysis: OpenCode Enterprise Shield](#2-feature-analysis-opencode-enterprise-shield)
3. [Architecture Overview](#3-architecture-overview)
4. [Core Features](#4-core-features)
5. [User Interface Design](#5-user-interface-design)
6. [Data Flow & Processing](#6-data-flow--processing)
7. [Sanitization Engine](#7-sanitization-engine)
8. [Technical Requirements](#8-technical-requirements)
9. [Security Considerations](#9-security-considerations)
10. [Implementation Phases](#10-implementation-phases)

---

## 1. Feature Analysis: RepoPrompt & PasteMax

### 1.1 PasteMax Features (Incorporated)

| Feature | Description | Priority |
|---------|-------------|----------|
| **File Tree Navigation** | Expandable tree view with folder/file hierarchy | P0 - Critical |
| **Smart Search** | Search files by name or content with fuzzy matching | P0 - Critical |
| **Multi-Sort Options** | Sort by name, size, token count, last modified | P1 - High |
| **File Change Watcher** | Auto-update when files are added/modified/deleted | P1 - High |
| **Manual Refresh** | Full directory re-scan on demand | P2 - Medium |
| **Token Counting** | Per-file and total token count display | P0 - Critical |
| **Model Context Limits** | Presets for GPT-4o, Claude 3.5, Gemini 2.5, etc. | P0 - Critical |
| **Context Limit Warning** | Visual alert when exceeding model limits | P0 - Critical |
| **File Previewer** | Syntax-highlighted preview pane | P1 - High |
| **Multi-Select** | Select multiple files via checkboxes/shift-click | P0 - Critical |
| **Binary Detection** | Auto-exclude binary files | P0 - Critical |
| **Smart Exclusions** | Auto-exclude node_modules, .git, package-lock.json | P0 - Critical |
| **Gitignore Respect** | Honor .gitignore patterns | P1 - High |

### 1.2 RepoPrompt Features (Incorporated)

| Feature | Description | Priority |
|---------|-------------|----------|
| **Code Maps** | High-level indexed summaries of codebase structure | P1 - High |
| **XML Formatting** | Structure prompts in XML for multi-file instructions | P1 - High |
| **Manual Context Curation** | Select specific files, folders, snippets | P0 - Critical |
| **Context Size Display** | Real-time token count with model awareness | P0 - Critical |
| **Folder-Level Selection** | Select entire directories recursively | P0 - Critical |
| **Prompt Templates** | Pre-built templates for common tasks | P2 - Medium |
| **Diff Output Support** | Format for receiving multi-file diffs | P1 - High |

### 1.3 Novel Features (ShieldPrompt Originals)

| Feature | Description | Priority |
|---------|-------------|----------|
| **Sanitization Engine** | Enterprise-grade sensitive data masking | P0 - Critical |
| **Desanitization Paste** | Restore original values from AI responses | P0 - Critical |
| **Mapping Session** | In-memory alias-to-original mapping | P0 - Critical |
| **Audit Trail** | Log all sanitization events | P1 - High |
| **Custom Patterns** | User-defined regex patterns | P1 - High |
| **Policy Modes** | Unrestricted/SanitizedOnly/Blocked | P2 - Medium |

---

## 2. Feature Analysis: OpenCode Enterprise Shield

### 2.1 Detection Patterns (14 Built-in)

ShieldPrompt will implement all detection patterns from Enterprise Shield:

#### Infrastructure Patterns

```csharp
// Pattern Category: Infrastructure
public static class InfrastructurePatterns
{
    // 1. Server/Database Names
    public static readonly Regex ServerNames = new(
        @"(?i)(prod|production|staging|dev|test|qa|uat)[-_]?(db|database|server|host|sql|mysql|postgres|mongo|redis|elastic)",
        RegexOptions.Compiled);
    
    // 2. IP Addresses (RFC 1918 Private Ranges)
    public static readonly Regex PrivateIPs = new(
        @"\b(10\.\d{1,3}\.\d{1,3}\.\d{1,3}|172\.(1[6-9]|2\d|3[01])\.\d{1,3}\.\d{1,3}|192\.168\.\d{1,3}\.\d{1,3})\b",
        RegexOptions.Compiled);
    
    // 3. Connection Strings
    public static readonly Regex ConnectionStrings = new(
        @"(?i)(server|host|data source|datasource)=[^;]+;",
        RegexOptions.Compiled);
    
    // 4. File Paths (Windows/UNC)
    public static readonly Regex WindowsPaths = new(
        @"(?i)([a-z]:\\[^\s<>:""|\?\*]+|\\\\[a-z0-9_.$●-]+\\[^\s<>:""|\?\*]+)",
        RegexOptions.Compiled);
    
    // 5. Internal Hostnames
    public static readonly Regex InternalHostnames = new(
        @"\b[a-z0-9][-a-z0-9]*\.(internal|local|corp|intra|private)\.[a-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
}
```

#### Critical PII Patterns

```csharp
// Pattern Category: Critical PII
public static class PIIPatterns
{
    // 6. SSN (US Social Security Numbers)
    public static readonly Regex SSN = new(
        @"\b\d{3}[-\s]?\d{2}[-\s]?\d{4}\b",
        RegexOptions.Compiled);
    
    // 7. Credit Cards (with Luhn validation in processor)
    public static readonly Regex CreditCard = new(
        @"\b(?:4\d{3}|5[1-5]\d{2}|3[47]\d{2}|6(?:011|5\d{2}))\d{12,15}\b",
        RegexOptions.Compiled);
    
    // 8. API Keys
    public static readonly Regex AWSKey = new(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled);
    public static readonly Regex GitHubToken = new(@"gh[pousr]_[A-Za-z0-9_]{36,}", RegexOptions.Compiled);
    public static readonly Regex OpenAIKey = new(@"sk-[A-Za-z0-9]{48}", RegexOptions.Compiled);
    public static readonly Regex AnthropicKey = new(@"sk-ant-[A-Za-z0-9-]{95}", RegexOptions.Compiled);
    public static readonly Regex SlackToken = new(@"xox[baprs]-[A-Za-z0-9-]+", RegexOptions.Compiled);
    public static readonly Regex AzureKey = new(@"[A-Za-z0-9+/]{43}=", RegexOptions.Compiled);
    
    // 9. Private Keys
    public static readonly Regex PrivateKey = new(
        @"-----BEGIN (?:RSA |EC |DSA |OPENSSH )?PRIVATE KEY-----",
        RegexOptions.Compiled);
    
    // 10. Passwords in Code
    public static readonly Regex PasswordInCode = new(
        @"(?i)(password|passwd|pwd|secret|api_key|apikey|auth_token|authtoken)\s*[=:]\s*[""']?[^\s""']{8,}",
        RegexOptions.Compiled);
    
    // 11. Bearer Tokens (JWT)
    public static readonly Regex JWTToken = new(
        @"eyJ[A-Za-z0-9-_]+\.eyJ[A-Za-z0-9-_]+\.[A-Za-z0-9-_.+/=]+",
        RegexOptions.Compiled);
}
```

#### Extended Patterns (Custom)

```csharp
// Pattern Category: Extended/Custom
public static class ExtendedPatterns
{
    // 12. Email Addresses (Internal Domains)
    public static readonly Regex InternalEmail = new(
        @"\b[A-Za-z0-9._%+-]+@(company|corp|internal)\.[a-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    // 13. Phone Numbers
    public static readonly Regex PhoneNumber = new(
        @"\b(?:\+1[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b",
        RegexOptions.Compiled);
    
    // 14. URLs with Credentials
    public static readonly Regex CredentialURL = new(
        @"(?i)(https?|ftp)://[^:]+:[^@]+@[^\s]+",
        RegexOptions.Compiled);
}
```

### 2.2 Enterprise Features to Implement

| Feature | Description | Implementation |
|---------|-------------|----------------|
| **Zero-Knowledge Architecture** | Sensitive data never reaches AI | Core design principle |
| **Session Isolation** | Per-session encrypted mappings | AES-256-GCM in memory |
| **Audit Trail** | Log all sanitization events | JSON/SQLite local log |
| **Policy Modes** | Unrestricted/SanitizedOnly/Blocked | Per-pattern configuration |
| **Configurable Rules** | YAML-based pattern management | Custom patterns file |
| **Fail-Secure Design** | Block on error, never leak | Default-deny approach |

---

## 3. Architecture Overview

### 3.1 System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            ShieldPrompt Application                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐      │
│  │   Presentation   │    │    Application   │    │   Infrastructure │      │
│  │      Layer       │    │      Layer       │    │      Layer       │      │
│  ├──────────────────┤    ├──────────────────┤    ├──────────────────┤      │
│  │                  │    │                  │    │                  │      │
│  │  • MainWindow    │    │  • FileService   │    │  • FileSystem    │      │
│  │  • FileTreeView  │◄──►│  • TokenService  │◄──►│  • Clipboard     │      │
│  │  • PreviewPane   │    │  • SanitizeServ  │    │  • Settings      │      │
│  │  • StatusBar     │    │  • MapService    │    │  • AuditLog      │      │
│  │  • SettingsView  │    │  • PromptBuilder │    │  • PatternStore  │      │
│  │                  │    │                  │    │                  │      │
│  └──────────────────┘    └──────────────────┘    └──────────────────┘      │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                           Domain Layer                                │  │
│  ├──────────────────────────────────────────────────────────────────────┤  │
│  │                                                                       │  │
│  │  • FileNode          • SanitizationMapping    • AuditEntry           │  │
│  │  • TokenCount        • Pattern                • PromptConfig         │  │
│  │  • SelectionState    • PolicyMode             • ModelProfile         │  │
│  │                                                                       │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Technology Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| **UI Framework** | Avalonia UI 11+ | Cross-platform, XAML-based, mature |
| **Runtime** | .NET 8 LTS | Long-term support, performance |
| **Tokenizer** | TiktokenSharp | OpenAI's tiktoken port for .NET |
| **Clipboard** | TextCopy | Cross-platform clipboard access |
| **File Watching** | System.IO.FileSystemWatcher | Native .NET file monitoring |
| **Syntax Highlighting** | AvaloniaEdit | Code editor with highlighting |
| **Settings Storage** | JSON + Data Protection API | Encrypted local settings |
| **Audit Log** | SQLite + EF Core | Lightweight embedded database |

### 3.3 Project Structure

```
ShieldPrompt/
├── src/
│   ├── ShieldPrompt.App/                  # Main application entry
│   │   ├── App.axaml
│   │   ├── App.axaml.cs
│   │   └── Program.cs
│   │
│   ├── ShieldPrompt.Presentation/         # UI Layer
│   │   ├── Views/
│   │   │   ├── MainWindow.axaml
│   │   │   ├── FileTreeView.axaml
│   │   │   ├── PreviewPane.axaml
│   │   │   ├── PromptOutputView.axaml
│   │   │   └── SettingsView.axaml
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
│   │   │   ├── FileTreeViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   └── Converters/
│   │
│   ├── ShieldPrompt.Application/          # Application Services
│   │   ├── Services/
│   │   │   ├── IFileAggregationService.cs
│   │   │   ├── FileAggregationService.cs
│   │   │   ├── ITokenCountingService.cs
│   │   │   ├── TokenCountingService.cs
│   │   │   ├── ISanitizationService.cs
│   │   │   ├── SanitizationService.cs
│   │   │   ├── IDesanitizationService.cs
│   │   │   ├── DesanitizationService.cs
│   │   │   ├── IPromptBuilderService.cs
│   │   │   └── PromptBuilderService.cs
│   │   └── DTOs/
│   │
│   ├── ShieldPrompt.Domain/               # Domain Models
│   │   ├── Entities/
│   │   │   ├── FileNode.cs
│   │   │   ├── SanitizationMapping.cs
│   │   │   ├── Pattern.cs
│   │   │   └── AuditEntry.cs
│   │   ├── ValueObjects/
│   │   │   ├── TokenCount.cs
│   │   │   └── FilePath.cs
│   │   └── Enums/
│   │       ├── PolicyMode.cs
│   │       └── PatternCategory.cs
│   │
│   ├── ShieldPrompt.Infrastructure/       # Infrastructure
│   │   ├── FileSystem/
│   │   │   ├── FileSystemService.cs
│   │   │   └── GitignoreParser.cs
│   │   ├── Clipboard/
│   │   │   └── ClipboardService.cs
│   │   ├── Persistence/
│   │   │   ├── SettingsRepository.cs
│   │   │   ├── PatternRepository.cs
│   │   │   └── AuditRepository.cs
│   │   └── Patterns/
│   │       ├── BuiltInPatterns.cs
│   │       └── PatternLoader.cs
│   │
│   └── ShieldPrompt.Sanitization/         # Sanitization Engine
│       ├── Engine/
│       │   ├── SanitizationEngine.cs
│       │   └── DesanitizationEngine.cs
│       ├── Patterns/
│       │   ├── InfrastructurePatterns.cs
│       │   ├── PIIPatterns.cs
│       │   └── ExtendedPatterns.cs
│       ├── Validators/
│       │   └── LuhnValidator.cs
│       └── Mapping/
│           ├── MappingSession.cs
│           └── AliasGenerator.cs
│
├── tests/
│   ├── ShieldPrompt.Tests.Unit/
│   └── ShieldPrompt.Tests.Integration/
│
├── config/
│   ├── default-patterns.yaml
│   └── model-profiles.json
│
└── docs/
    └── user-guide.md
```

---

## 4. Core Features

### 4.1 File Aggregation Module

#### 4.1.1 Directory Loading

```csharp
public interface IFileAggregationService
{
    Task<FileNode> LoadDirectoryAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<FileNode>> GetSelectedFilesAsync();
    Task<string> AggregateContentsAsync(IEnumerable<FileNode> files);
    void SetExclusionPatterns(IEnumerable<string> patterns);
}
```

**Capabilities:**
- Recursive directory traversal with depth limiting
- Parallel file loading for performance
- Memory-efficient streaming for large files
- Gitignore pattern parsing and application

#### 4.1.2 Smart Exclusions

Default exclusion patterns:

```yaml
# Default Exclusions
directories:
  - node_modules
  - .git
  - .svn
  - .hg
  - __pycache__
  - .venv
  - venv
  - bin
  - obj
  - dist
  - build
  - .next
  - .nuxt

files:
  - "*.lock"
  - "package-lock.json"
  - "yarn.lock"
  - "pnpm-lock.yaml"
  - "Cargo.lock"
  - "*.min.js"
  - "*.min.css"
  - "*.map"
  - "*.dll"
  - "*.exe"
  - "*.so"
  - "*.dylib"

binary_extensions:
  - .png, .jpg, .jpeg, .gif, .bmp, .ico, .webp
  - .pdf, .doc, .docx, .xls, .xlsx
  - .zip, .tar, .gz, .rar, .7z
  - .mp3, .mp4, .avi, .mov, .wav
  - .ttf, .otf, .woff, .woff2
```

### 4.2 Token Counting Module

#### 4.2.1 Model Profiles

```csharp
public record ModelProfile(
    string Name,
    string DisplayName,
    int ContextLimit,
    string TokenizerEncoding,
    double ReservedForResponse);

public static class ModelProfiles
{
    public static readonly ModelProfile GPT4o = new(
        "gpt-4o", "GPT-4o", 128_000, "cl100k_base", 0.25);
    
    public static readonly ModelProfile GPT4oMini = new(
        "gpt-4o-mini", "GPT-4o Mini", 128_000, "cl100k_base", 0.25);
    
    public static readonly ModelProfile Claude35Sonnet = new(
        "claude-3.5-sonnet", "Claude 3.5 Sonnet", 200_000, "cl100k_base", 0.25);
    
    public static readonly ModelProfile Claude3Opus = new(
        "claude-3-opus", "Claude 3 Opus", 200_000, "cl100k_base", 0.25);
    
    public static readonly ModelProfile Gemini25Pro = new(
        "gemini-2.5-pro", "Gemini 2.5 Pro", 1_000_000, "cl100k_base", 0.25);
    
    public static readonly ModelProfile DeepSeekV3 = new(
        "deepseek-v3", "DeepSeek V3", 128_000, "cl100k_base", 0.25);
}
```

#### 4.2.2 Token Service

```csharp
public interface ITokenCountingService
{
    int CountTokens(string content, string encoding = "cl100k_base");
    TokenCount CountFileTokens(FileNode file);
    TokenCount CountTotalTokens(IEnumerable<FileNode> files);
    bool ExceedsLimit(int tokens, ModelProfile profile);
    int GetAvailableTokens(ModelProfile profile);
}
```

### 4.3 Prompt Builder Module

#### 4.3.1 Output Formats

**XML Format (RepoPrompt Style):**

```xml
<project_context>
  <description>User's task description here</description>
  <files>
    <file path="src/Services/UserService.cs">
      <content><![CDATA[
// File contents here (SANITIZED)
      ]]></content>
    </file>
    <file path="src/Models/User.cs">
      <content><![CDATA[
// File contents here (SANITIZED)
      ]]></content>
    </file>
  </files>
  <code_map>
    <!-- Optional high-level structure -->
  </code_map>
</project_context>
```

**Markdown Format:**

```markdown
# Project Context

## Task
User's task description here

## Files

### `src/Services/UserService.cs`
```csharp
// File contents here (SANITIZED)
```

### `src/Models/User.cs`
```csharp
// File contents here (SANITIZED)
```
```

**Plain Text Format:**

```
=== FILE: src/Services/UserService.cs ===
// File contents here (SANITIZED)

=== FILE: src/Models/User.cs ===
// File contents here (SANITIZED)
```

#### 4.3.2 Code Map Generation

```csharp
public interface ICodeMapService
{
    Task<CodeMap> GenerateMapAsync(IEnumerable<FileNode> files);
}

public record CodeMap(
    IReadOnlyList<ClassSummary> Classes,
    IReadOnlyList<FunctionSummary> Functions,
    IReadOnlyList<string> Imports);

public record ClassSummary(
    string Name,
    string FilePath,
    IReadOnlyList<string> Methods,
    IReadOnlyList<string> Properties);
```

---

## 5. User Interface Design

### 5.1 Main Window Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ShieldPrompt                                              [_][□][X]        │
├─────────────────────────────────────────────────────────────────────────────┤
│  📁 Open Folder   🔄 Refresh   ⚙️ Settings                 Model: [GPT-4o ▼]│
├──────────────────────┬──────────────────────────────────────────────────────┤
│                      │                                                       │
│  🔍 Search files...  │  📄 Preview: UserService.cs                          │
│  ─────────────────── │  ──────────────────────────────────────────────────  │
│                      │                                                       │
│  📂 MyProject        │  ```csharp                                           │
│  ├─ 📂 src           │  public class UserService                            │
│  │  ├─ ☑ Program.cs  │  {                                                   │
│  │  ├─ ☑ User.cs     │      private readonly IDb _db;                       │
│  │  └─ ☐ Tests/      │      // ... code preview ...                         │
│  ├─ 📄 README.md     │  }                                                   │
│  └─ 📄 .gitignore    │  ```                                                 │
│                      │                                                       │
│  ─────────────────── │  ──────────────────────────────────────────────────  │
│  Sort: [Name ▼]      │                                                       │
│                      │  🔐 Sanitization Preview:                             │
│                      │  ┌──────────────────────────────────────────────────┐│
│                      │  │ ProductionDB → DATABASE_0                        ││
│                      │  │ 192.168.1.50 → IP_ADDRESS_0                      ││
│                      │  │ sk-abc123... → OPENAI_KEY_0                      ││
│                      │  └──────────────────────────────────────────────────┘│
│                      │                                                       │
├──────────────────────┴──────────────────────────────────────────────────────┤
│                                                                              │
│  📝 Task Description (optional):                                            │
│  ┌──────────────────────────────────────────────────────────────────────────┐
│  │ Help me refactor this service to use async/await patterns                │
│  └──────────────────────────────────────────────────────────────────────────┘
│                                                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  Format: [XML ▼]  │ 📋 Copy Sanitized │ 📥 Paste & Restore │ 🔐 12 masked   │
├─────────────────────────────────────────────────────────────────────────────┤
│  ✅ Ready │ 3 files selected │ 2,847 / 128,000 tokens (2.2%)               │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Key UI Components

#### 5.2.1 File Tree Panel

- **Checkbox Selection**: Multi-select with shift/ctrl click
- **Folder Selection**: Check folder to include all children
- **Visual Indicators**:
  - 🟢 Green: Selected
  - ⚪ Gray: Excluded by rules
  - 🔴 Red: Binary file (non-selectable)
  - 🟡 Yellow: Large file warning (>1000 tokens)
- **Token Count Display**: Show tokens per file in tree
- **Search**: Real-time fuzzy search with highlighting

#### 5.2.2 Preview Pane

- **Syntax Highlighting**: Language-aware via AvaloniaEdit
- **Sanitization Preview**: Toggle to show sanitized version
- **Line Numbers**: With selection support
- **Minimap**: Optional code overview

#### 5.2.3 Status Bar

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ ✅ Ready │ 3/47 files │ 2,847 tokens │ 🔐 12 values masked │ Session: 4h23m│
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.3 Paste & Restore Dialog

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  📥 Paste AI Response                                           [X]         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Paste the AI response here:                                                │
│  ┌──────────────────────────────────────────────────────────────────────────┐
│  │ Here's the refactored code using DATABASE_0:                             │
│  │                                                                          │
│  │ ```csharp                                                                │
│  │ public async Task<User> GetUserAsync(int id)                             │
│  │ {                                                                        │
│  │     var conn = GetConnection("DATABASE_0");                              │
│  │     ...                                                                  │
│  │ }                                                                        │
│  │ ```                                                                      │
│  └──────────────────────────────────────────────────────────────────────────┘
│                                                                              │
│  🔄 Detected Aliases to Restore:                                            │
│  ┌──────────────────────────────────────────────────────────────────────────┐
│  │ ☑ DATABASE_0 → ProductionDB                                              │
│  │ ☑ IP_ADDRESS_0 → 192.168.1.50                                            │
│  └──────────────────────────────────────────────────────────────────────────┘
│                                                                              │
│  Output Preview:                                                            │
│  ┌──────────────────────────────────────────────────────────────────────────┐
│  │ Here's the refactored code using ProductionDB:                           │
│  │ ...                                                                      │
│  └──────────────────────────────────────────────────────────────────────────┘
│                                                                              │
│           [ Cancel ]  [ Copy Restored to Clipboard ]  [ Save to Files ]     │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Data Flow & Processing

### 6.1 Copy Flow (Sanitize)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         COPY FLOW (SANITIZE)                                │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Select  │───►│  Aggregate   │───►│  Sanitize    │───►│  Copy to     │
│  Files   │    │  Contents    │    │  Content     │    │  Clipboard   │
└──────────┘    └──────────────┘    └──────────────┘    └──────────────┘
                                           │
                                           ▼
                                    ┌──────────────┐
                                    │  Store       │
                                    │  Mappings    │
                                    └──────────────┘

Step 1: User selects files in tree
Step 2: Contents aggregated with format template
Step 3: Sanitization engine scans and replaces:
        - ProductionDB → DATABASE_0
        - 192.168.1.50 → IP_ADDRESS_0
        - sk-abc123... → OPENAI_KEY_0
Step 4: Mapping stored in encrypted session
Step 5: Sanitized content copied to clipboard
Step 6: Audit entry created
```

### 6.2 Paste Flow (Desanitize)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         PASTE FLOW (DESANITIZE)                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Paste   │───►│  Detect      │───►│  Restore     │───►│  Output      │
│  Response│    │  Aliases     │    │  Values      │    │  Options     │
└──────────┘    └──────────────┘    └──────────────┘    └──────────────┘
                       │                   │
                       ▼                   ▼
                ┌──────────────┐    ┌──────────────┐
                │  Lookup      │◄───│  Session     │
                │  Mappings    │    │  Store       │
                └──────────────┘    └──────────────┘

Step 1: User pastes AI response
Step 2: Engine detects alias patterns (DATABASE_0, IP_ADDRESS_0, etc.)
Step 3: Lookup original values from session mapping
Step 4: Replace aliases with originals:
        - DATABASE_0 → ProductionDB
        - IP_ADDRESS_0 → 192.168.1.50
        - OPENAI_KEY_0 → sk-abc123...
Step 5: Present output options:
        - Copy restored to clipboard
        - Save directly to files (if file markers present)
Step 6: Audit entry created
```

### 6.3 Session Lifecycle

```csharp
public interface IMappingSession
{
    string SessionId { get; }
    DateTime CreatedAt { get; }
    DateTime ExpiresAt { get; }
    
    void AddMapping(string original, string alias, PatternCategory category);
    string? GetOriginal(string alias);
    string? GetAlias(string original);
    IReadOnlyDictionary<string, string> GetAllMappings();
    
    void Clear();
    void ExtendSession(TimeSpan duration);
}

// Session stored in memory, encrypted with Data Protection API
// Default expiry: 4 hours (configurable)
// Cleared on application exit or manual clear
```

---

## 7. Sanitization Engine

### 7.1 Engine Architecture

```csharp
public interface ISanitizationEngine
{
    SanitizationResult Sanitize(string content, SanitizationOptions options);
    DesanitizationResult Desanitize(string content, IMappingSession session);
}

public record SanitizationOptions(
    bool EnableInfrastructure = true,
    bool EnablePII = true,
    bool EnableCustomPatterns = true,
    PolicyMode Mode = PolicyMode.SanitizedOnly);

public record SanitizationResult(
    string SanitizedContent,
    IReadOnlyList<SanitizationMatch> Matches,
    int TotalMatches);

public record SanitizationMatch(
    string Original,
    string Alias,
    PatternCategory Category,
    string PatternName,
    int StartIndex,
    int Length);
```

### 7.2 Pattern Processing Pipeline

```csharp
public class SanitizationEngine : ISanitizationEngine
{
    private readonly IEnumerable<IPatternProcessor> _processors;
    private readonly IAliasGenerator _aliasGenerator;
    private readonly IMappingSession _session;

    public SanitizationResult Sanitize(string content, SanitizationOptions options)
    {
        var matches = new List<SanitizationMatch>();
        var result = content;
        
        foreach (var processor in _processors.OrderByDescending(p => p.Priority))
        {
            if (!ShouldProcess(processor, options))
                continue;
                
            var processorMatches = processor.FindMatches(result);
            
            foreach (var match in processorMatches.OrderByDescending(m => m.StartIndex))
            {
                // Check if already mapped (same value appears multiple times)
                var alias = _session.GetAlias(match.Original) 
                    ?? _aliasGenerator.Generate(match.Category);
                
                _session.AddMapping(match.Original, alias, match.Category);
                
                result = result.Remove(match.StartIndex, match.Length)
                               .Insert(match.StartIndex, alias);
                
                matches.Add(match with { Alias = alias });
            }
        }
        
        return new SanitizationResult(result, matches, matches.Count);
    }
}
```

### 7.3 Alias Generation

```csharp
public interface IAliasGenerator
{
    string Generate(PatternCategory category);
}

public class AliasGenerator : IAliasGenerator
{
    private readonly Dictionary<PatternCategory, int> _counters = new();
    
    public string Generate(PatternCategory category)
    {
        var count = _counters.GetValueOrDefault(category, 0);
        _counters[category] = count + 1;
        
        return category switch
        {
            PatternCategory.Database => $"DATABASE_{count}",
            PatternCategory.Server => $"SERVER_{count}",
            PatternCategory.IPAddress => $"IP_ADDRESS_{count}",
            PatternCategory.Hostname => $"HOSTNAME_{count}",
            PatternCategory.ConnectionString => $"CONN_STRING_{count}",
            PatternCategory.FilePath => $"FILE_PATH_{count}",
            PatternCategory.SSN => $"SSN_{count}",
            PatternCategory.CreditCard => $"CREDIT_CARD_{count}",
            PatternCategory.APIKey => $"API_KEY_{count}",
            PatternCategory.AWSKey => $"AWS_KEY_{count}",
            PatternCategory.GitHubToken => $"GITHUB_TOKEN_{count}",
            PatternCategory.OpenAIKey => $"OPENAI_KEY_{count}",
            PatternCategory.PrivateKey => $"PRIVATE_KEY_{count}",
            PatternCategory.Password => $"PASSWORD_{count}",
            PatternCategory.BearerToken => $"BEARER_TOKEN_{count}",
            PatternCategory.Email => $"EMAIL_{count}",
            PatternCategory.Phone => $"PHONE_{count}",
            PatternCategory.URL => $"URL_{count}",
            PatternCategory.Custom => $"CUSTOM_{count}",
            _ => $"VALUE_{count}"
        };
    }
}
```

### 7.4 Custom Pattern Support

```yaml
# config/custom-patterns.yaml

custom_patterns:
  - name: "Company Project Codes"
    category: custom
    pattern: "PRJ-[A-Z]{3}-\\d{4}"
    alias_prefix: "PROJECT_CODE"
    enabled: true
    
  - name: "Internal Ticket IDs"
    category: custom
    pattern: "JIRA-\\d{5,}"
    alias_prefix: "TICKET"
    enabled: true
    
  - name: "Employee IDs"
    category: pii
    pattern: "EMP\\d{6}"
    alias_prefix: "EMPLOYEE_ID"
    enabled: true
    
  - name: "Custom API Endpoints"
    category: infrastructure
    pattern: "https://api\\.mycompany\\.internal/.*"
    alias_prefix: "INTERNAL_API"
    enabled: true
```

---

## 8. Technical Requirements

### 8.1 Performance Requirements

| Metric | Target | Notes |
|--------|--------|-------|
| Directory Load | <2s for 10,000 files | Async with progress |
| Token Counting | <100ms for 1MB content | Cached results |
| Sanitization | <50ms per 100KB | Compiled regex |
| Desanitization | <10ms per 100KB | Direct mapping lookup |
| UI Responsiveness | <16ms frame time | 60fps target |
| Memory Usage | <500MB for large repos | Streaming large files |

### 8.2 Compatibility Requirements

| Platform | Version | Notes |
|----------|---------|-------|
| Windows | 10/11 (x64, ARM64) | Primary target |
| macOS | 12+ (Intel, Apple Silicon) | Full support |
| Linux | Ubuntu 22.04+, Fedora 38+ | AppImage/deb/rpm |
| .NET | 8.0 LTS | Required runtime |

### 8.3 Dependencies

```xml
<!-- ShieldPrompt.csproj -->
<ItemGroup>
    <!-- UI Framework -->
    <PackageReference Include="Avalonia" Version="11.2.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.2.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.*" />
    <PackageReference Include="AvaloniaEdit" Version="11.1.*" />
    
    <!-- MVVM -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.*" />
    
    <!-- Tokenization -->
    <PackageReference Include="TiktokenSharp" Version="1.1.*" />
    
    <!-- Clipboard -->
    <PackageReference Include="TextCopy" Version="6.2.*" />
    
    <!-- Configuration -->
    <PackageReference Include="YamlDotNet" Version="16.*" />
    
    <!-- Persistence -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
    
    <!-- Security -->
    <PackageReference Include="Microsoft.AspNetCore.DataProtection" Version="8.0.*" />
</ItemGroup>
```

---

## 9. Security Considerations

### 9.1 Threat Model

| Threat | Mitigation |
|--------|------------|
| Sensitive data leaked to AI | Sanitization engine with 14+ patterns |
| Mapping file stolen | In-memory only, encrypted with DPAPI |
| Audit logs tampered | Cryptographic signatures (optional) |
| Malicious regex DoS | Timeout on pattern matching |
| Clipboard sniffing | Clear after configurable timeout |

### 9.2 Security Controls

```csharp
public class SecurityConfig
{
    // Session security
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(4);
    public bool ClearOnExit { get; set; } = true;
    public bool EncryptMappings { get; set; } = true;
    
    // Clipboard security
    public TimeSpan ClipboardClearTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool ClearClipboardOnPaste { get; set; } = true;
    
    // Audit settings
    public bool EnableAuditLog { get; set; } = true;
    public int AuditRetentionDays { get; set; } = 30;
    public bool SignAuditEntries { get; set; } = false;
    
    // Fail-secure
    public bool BlockOnPatternError { get; set; } = true;
    public bool RequireManualReview { get; set; } = false;
}
```

### 9.3 Audit Log Schema

```csharp
public record AuditEntry(
    Guid Id,
    DateTime Timestamp,
    AuditAction Action,
    string SessionId,
    int MatchCount,
    IReadOnlyList<string> Categories,
    string? FileHash,
    string? Signature);

public enum AuditAction
{
    SessionStarted,
    ContentSanitized,
    ContentDesanitized,
    SessionCleared,
    SessionExpired,
    PatternError
}
```

---

## 10. Implementation Phases

### Phase 1: Core MVP (Weeks 1-3)

**Deliverables:**
- [ ] Basic Avalonia UI with file tree
- [ ] Directory loading with exclusions
- [ ] Token counting service
- [ ] Single-model context limit
- [ ] Copy to clipboard (without sanitization)

**Exit Criteria:**
- Can load a directory and copy selected files to clipboard
- Token count displays accurately

### Phase 2: Sanitization Engine (Weeks 4-6)

**Deliverables:**
- [ ] All 14 built-in patterns implemented
- [ ] Mapping session management
- [ ] Sanitize on copy
- [ ] Desanitize on paste dialog
- [ ] Basic audit logging

**Exit Criteria:**
- All pattern tests passing
- Round-trip sanitize/desanitize works correctly

### Phase 3: Enhanced UX (Weeks 7-8)

**Deliverables:**
- [ ] Syntax-highlighted preview
- [ ] Multiple output formats (XML, Markdown, Plain)
- [ ] File change watching
- [ ] Search/filter in file tree
- [ ] Model profile selection

**Exit Criteria:**
- All PasteMax parity features working
- User testing feedback incorporated

### Phase 4: Enterprise Features (Weeks 9-10)

**Deliverables:**
- [ ] Custom pattern support (YAML)
- [ ] Policy modes (Unrestricted/SanitizedOnly/Blocked)
- [ ] Signed audit entries
- [ ] Code map generation
- [ ] Settings persistence

**Exit Criteria:**
- Custom patterns can be added via config
- Audit log integrity verified

### Phase 5: Polish & Release (Weeks 11-12)

**Deliverables:**
- [ ] Installer packages (Windows MSI, macOS DMG, Linux AppImage)
- [ ] Auto-update mechanism
- [ ] Performance optimization
- [ ] Documentation
- [ ] Release builds

**Exit Criteria:**
- Installers tested on all platforms
- Performance targets met

---

## Appendix A: Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+O` | Open folder |
| `Ctrl+R` | Refresh directory |
| `Ctrl+C` | Copy sanitized to clipboard |
| `Ctrl+V` | Open paste & restore dialog |
| `Ctrl+A` | Select all files |
| `Ctrl+D` | Deselect all files |
| `Ctrl+F` | Focus search |
| `Ctrl+,` | Open settings |
| `Escape` | Clear search / Close dialog |
| `Space` | Toggle selected file |

---

## Appendix B: Configuration File Locations

| Platform | Settings Path |
|----------|---------------|
| Windows | `%APPDATA%\ShieldPrompt\settings.json` |
| macOS | `~/Library/Application Support/ShieldPrompt/settings.json` |
| Linux | `~/.config/ShieldPrompt/settings.json` |

| Platform | Patterns Path |
|----------|---------------|
| Windows | `%APPDATA%\ShieldPrompt\custom-patterns.yaml` |
| macOS | `~/Library/Application Support/ShieldPrompt/custom-patterns.yaml` |
| Linux | `~/.config/ShieldPrompt/custom-patterns.yaml` |

| Platform | Audit Log Path |
|----------|----------------|
| Windows | `%APPDATA%\ShieldPrompt\audit.db` |
| macOS | `~/Library/Application Support/ShieldPrompt/audit.db` |
| Linux | `~/.config/ShieldPrompt/audit.db` |

---

## Appendix C: Glossary

| Term | Definition |
|------|------------|
| **Sanitization** | Process of replacing sensitive values with aliases |
| **Desanitization** | Process of restoring original values from aliases |
| **Mapping** | Association between an original value and its alias |
| **Session** | Temporary in-memory store of mappings |
| **Pattern** | Regex definition for detecting sensitive data |
| **Alias** | Generated replacement value (e.g., `DATABASE_0`) |
| **Token** | Unit of text processed by AI models |
| **Context Limit** | Maximum tokens a model can process |
| **Code Map** | High-level structural summary of codebase |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-14 | ShieldPrompt Team | Initial specification |

---

*This specification is a living document and will be updated as requirements evolve.*

