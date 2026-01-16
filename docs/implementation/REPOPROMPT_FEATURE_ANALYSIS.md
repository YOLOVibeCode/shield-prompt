# RepoPrompt Complete Feature Analysis & Gap Report

**Version:** 1.0.0  
**Last Updated:** January 15, 2026  
**Purpose:** Comprehensive comparison of RepoPrompt features vs ShieldPrompt specifications  
**Status:** ANALYSIS & RECOMMENDATION ONLY

---

## Executive Summary

This document provides a **complete analysis** of RepoPrompt's feature set (including Pro features) compared to ShieldPrompt's current specifications. It identifies:
- Features we already have specified ✅
- Features that are partially specified ⚠️
- Features that are missing from our specs ❌
- Recommendations for specification additions

**Key Finding:** ShieldPrompt already specifies many core features but is **missing 15+ Pro-level features** that would make it competitive with RepoPrompt Pro.

---

## 1. RepoPrompt Complete Feature Inventory

### 1.1 Core/Free Features

| Feature | Description | ShieldPrompt Status |
|---------|-------------|---------------------|
| **File Tree Navigation** | Visual file/folder tree with expandable hierarchy | ✅ Specified |
| **File Preview** | Syntax-highlighted code preview | ✅ Specified |
| **Multi-file Selection** | Checkbox selection, folder selection | ✅ Specified |
| **Token Counting** | Per-file and total token display | ✅ Specified |
| **Model Context Limits** | Support for GPT-4, Claude, Gemini limits | ✅ Specified |
| **Smart Search** | Filter files by name/content | ✅ Specified |
| **Smart Exclusions** | Ignore node_modules, .git, binaries | ✅ Specified |
| **Gitignore Respect** | Honor .gitignore patterns | ✅ Specified |
| **Copy to Clipboard** | Copy formatted prompt | ✅ Specified |
| **Multiple Output Formats** | XML, Markdown, Plain Text | ✅ Specified |
| **Stored Prompts/Templates** | Pre-built prompt templates | ✅ Specified |
| **Custom Instructions** | User-provided task context | ✅ Specified |
| **Universal Clipboard** | Clipboard-based workflow | ✅ Specified |

### 1.2 Pro/Paid Features

| Feature | Description | ShieldPrompt Status |
|---------|-------------|---------------------|
| **Code Maps** | AST-based structural summaries for token compression | ❌ **MISSING** |
| **Context Builder** | AI-suggested file relevance | ⚠️ Partial (v2.1 planned) |
| **Apply Mode** | Preview diffs & apply AI changes to files | ⚠️ Partial (basic) |
| **MCP Server Integration** | Model Context Protocol for external tools | ❌ **MISSING** |
| **Multi-Model Delegation** | Use cheaper models for simple tasks | ❌ **MISSING** |
| **Custom Providers** | OpenAI-compatible API support | ❌ **MISSING** |
| **Unlimited Tokens** | No token limits (free tier has limits) | ⚠️ N/A (no tiers) |
| **Multiple Workspaces** | Open multiple repos simultaneously | ⚠️ Partial (v2 planned) |
| **Advanced Search** | Regex search, git status filters | ⚠️ Partial |
| **Multi-Machine License** | Use on 3+ machines | ❌ N/A (open source) |
| **Priority Support** | Faster support response | ❌ N/A (open source) |
| **Version History** | Prompt versioning & rollback | ❌ **MISSING** |
| **Collaboration** | Multi-user shared workspaces | ❌ **MISSING** |
| **Analytics Dashboard** | Usage metrics, token costs | ❌ **MISSING** |
| **Import/Export** | Bulk prompt import/export | ⚠️ Partial |

### 1.3 Apply Mode Details (Critical Feature)

RepoPrompt's Apply Mode includes:

| Sub-feature | Description | ShieldPrompt Status |
|-------------|-------------|---------------------|
| **Diff Preview** | Visual diff before applying | ⚠️ Basic |
| **File Operations** | Create/Update/Delete files | ✅ Specified |
| **Confirmation Flow** | Review & confirm each change | ⚠️ Basic |
| **Rollback/Undo** | Undo applied changes | ✅ Specified |
| **Partial Updates** | Update specific lines only | ⚠️ Partial |
| **Conflict Resolution** | Handle modified files | ⚠️ Basic |
| **Backup System** | Automatic backups | ✅ Specified |

---

## 2. Feature Gap Analysis

### 2.1 Critical Missing Features (MUST ADD)

#### ~~❌ Code Maps (AST Summaries)~~ → **OUT OF SCOPE**
*Decision: Not needed for ShieldPrompt's core use case. Users can select specific files.*

#### ~~❌ MCP Server Integration~~ → **OUT OF SCOPE**
*Decision: ShieldPrompt's clipboard-based workflow is sufficient. No need for protocol server.*

#### ❌ Multi-Model Delegation
**What it does:** Automatically routes simpler tasks to cheaper/faster models, complex tasks to powerful models.

**Why it matters:**
- Cost optimization (use GPT-4o-mini for simple tasks)
- Speed optimization (fast model for quick responses)
- User convenience (automatic selection)

**Specification needed:**
```
Model Delegation:
- Task classification: simple, medium, complex
- Model routing rules (configurable)
- Fallback chains
- Cost tracking per model
- UI: Auto/Manual toggle, cost display
```

#### ❌ Custom API Providers
**What it does:** Support any OpenAI-compatible API endpoint (local LLMs, alternative providers).

**Why it matters:**
- Enterprise users with private deployments
- Users with specific model preferences
- Support for Ollama, LM Studio, etc.

**Specification needed:**
```
Custom Providers:
- Add custom endpoint URL
- API key management (encrypted)
- Test connection button
- Model discovery from endpoint
- Timeout & retry configuration
```

### 2.2 Important Missing Features (SHOULD ADD)

#### ❌ Version History for Presets
**What it does:** Track changes to saved prompts/presets with ability to diff and rollback.

**Specification needed:**
```
Version History:
- Auto-save on every change
- Version list with timestamps
- Diff view between versions
- Rollback to any version
- Storage: JSON in .shieldprompt/history/
```

#### ❌ Analytics Dashboard
**What it does:** Track usage statistics, token costs, popular templates.

**Specification needed:**
```
Analytics:
- Metrics: token usage per day, model usage, template popularity
- Cost estimation (based on model pricing)
- Session history
- Export reports
- UI: Dashboard panel with charts
```

#### ❌ Collaboration Features
**What it does:** Share workspaces, prompts with team members.

**Specification needed:**
```
Collaboration:
- Share workspace link
- Role-based access (owner, editor, viewer)
- Real-time presence (who's viewing)
- Comment threads on files
- Sync via cloud or local network
```

### 2.3 Partially Specified Features (NEED ENHANCEMENT)

#### ⚠️ Apply Mode Enhancement
**Current state:** Basic file writing with backup
**Needed:**
- Rich diff preview UI (side-by-side)
- Syntax-highlighted changes
- Line-by-line accept/reject
- Conflict detection with merge options
- Batch operations with summary

#### ⚠️ Context Builder
**Current state:** Mentioned in v2.1 scope
**Needed:**
- AI-powered file suggestion based on task
- Relevance scoring
- Manual override
- Learning from user selections

#### ⚠️ Advanced Search
**Current state:** Basic file name search
**Needed:**
- Regex pattern search
- Content search (search within files)
- Git status filters (modified, staged, untracked)
- File type filters
- Saved search filters

#### ⚠️ Multi-Workspace
**Current state:** Single workspace at a time
**Needed:**
- Multiple workspace tabs
- Independent state per workspace
- Quick switch dropdown
- Recent workspaces list
- Workspace settings persistence

---

## 3. Feature Comparison Matrix (Updated)

| Feature | RepoPrompt Free | RepoPrompt Pro | ShieldPrompt Current | ShieldPrompt Plan |
|---------|-----------------|----------------|---------------------|-------------------|
| File Tree | ✅ | ✅ | ✅ | - |
| Token Counting | ✅ | ✅ | ✅ | - |
| Multiple Formats | ✅ | ✅ | ✅ | - |
| Smart Exclusions | ✅ | ✅ | ✅ | - |
| Copy to Clipboard | ✅ | ✅ | ✅ | - |
| **🔐 Sanitization** | ❌ | ❌ | ✅ | **ADVANTAGE!** |
| **🔓 Desanitization** | ❌ | ❌ | ✅ | **ADVANTAGE!** |
| Code Maps | ❌ | ✅ | ❌ | ~~OUT OF SCOPE~~ |
| Context Builder | ❌ | ✅ | ⚠️ | Enhance (v2.1) |
| Apply Mode | ✅ | ✅ | ⚠️ | **Enhance (P0)** |
| MCP Server | ❌ | ✅ | ❌ | ~~OUT OF SCOPE~~ |
| Multi-Model Delegation | ❌ | ✅ | ❌ | Consider (P2) |
| Custom Providers | ❌ | ✅ | ❌ | **ADD (P1)** |
| Version History | ❌ | ✅ | ❌ | ADD (P1) |
| Analytics | ❌ | ✅ | ❌ | ADD (P2) |
| Collaboration | ❌ | ✅ | ❌ | Consider (v3?) |
| Multi-Workspace | ✅ | ✅ | ⚠️ | **Enhance (P0)** |
| Advanced Search | ✅ | ✅ | ⚠️ | **Enhance (P0)** |

**Legend:**
- ✅ Full support
- ⚠️ Partial/Basic support
- ❌ Not supported
- ~~OUT OF SCOPE~~ = Decided not to implement

---

## 4. ShieldPrompt Unique Advantages

Features ShieldPrompt has that RepoPrompt DOES NOT:

| Feature | Description | Value |
|---------|-------------|-------|
| **🔐 Sanitization Engine** | Replace sensitive data with aliases | Enterprise security |
| **🔓 Desanitization** | Restore original values from AI response | Complete round-trip |
| **14 Pattern Categories** | Database, IP, API keys, PII, etc. | Comprehensive coverage |
| **Custom Patterns** | User-defined regex patterns | Flexibility |
| **Audit Trail** | Log of sanitized values | Compliance |
| **Session Management** | In-memory secure mappings | Zero-knowledge |
| **Role-Based Prompting** | AI persona customization | Better responses |
| **Template System** | Structured prompt templates | Consistency |

**This is our competitive moat** - RepoPrompt has NO sanitization capabilities.

---

## 5. Specification Additions Required

### 5.1 New Specification Documents Needed

| Document | Priority | Estimated Pages |
|----------|----------|-----------------|
| ~~`CODE_MAPS_SPECIFICATION.md`~~ | ~~P0~~ | **OUT OF SCOPE** |
| ~~`MCP_SERVER_SPECIFICATION.md`~~ | ~~P1~~ | **OUT OF SCOPE** |
| `MODEL_DELEGATION_SPECIFICATION.md` | P2 | 8-10 |
| `CUSTOM_PROVIDERS_SPECIFICATION.md` | P2 | 8-10 |
| `VERSION_HISTORY_SPECIFICATION.md` | P2 | 6-8 |
| `ANALYTICS_SPECIFICATION.md` | P3 | 8-10 |
| `COLLABORATION_SPECIFICATION.md` | P3 | 15-20 |

### 5.2 Existing Documents to Update

| Document | Updates Needed |
|----------|----------------|
| `PHASE6_APPLY_MODE_ENHANCEMENT.md` | Add rich diff UI, line-by-line accept |
| `UI_COMPLETE_SPECIFICATION.md` | Add menus/dialogs for new features |
| `COMPLETE_FUNCTION_INVENTORY.md` | Add functions for new features |

---

## 6. Detailed Specifications for Additional Features

### ~~6.1 Code Maps Specification~~ → OUT OF SCOPE
*Removed - Not needed for ShieldPrompt's core use case*

### ~~6.2 MCP Server Specification~~ → OUT OF SCOPE
*Removed - Clipboard-based workflow is sufficient*

---

### 6.1 Model Delegation Specification

#### 6.3.1 Overview
Automatically route prompts to appropriate models based on complexity.

#### 6.3.2 Complexity Classification

| Level | Criteria | Default Model |
|-------|----------|---------------|
| **Simple** | Token count < 1000, single file, simple task | GPT-4o-mini |
| **Medium** | Token count 1000-10000, multiple files | GPT-4o |
| **Complex** | Token count > 10000, architecture queries | Claude 3.5 Sonnet |

#### 6.3.3 Configuration

```yaml
# model-delegation.yaml
delegation:
  enabled: true
  rules:
    - name: "Simple queries"
      condition:
        max_tokens: 1000
        max_files: 1
      model: "gpt-4o-mini"
      
    - name: "Medium complexity"
      condition:
        max_tokens: 10000
        max_files: 10
      model: "gpt-4o"
      
    - name: "Complex analysis"
      condition:
        any: true
      model: "claude-3.5-sonnet"
      
  fallback_chain:
    - "gpt-4o"
    - "claude-3.5-sonnet"
    - "gpt-4o-mini"
```

#### 6.3.4 Interface

```csharp
public interface IModelDelegator
{
    ModelProfile SelectModel(PromptContext context);
    Task<DelegationResult> ExecuteWithDelegation(string prompt, CancellationToken ct);
}

public record DelegationResult(
    ModelProfile UsedModel,
    string Response,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCost,
    TimeSpan Latency);
```

---

### 6.2 Custom Providers Specification

#### 6.4.1 Overview
Support any OpenAI-compatible API for local or alternative LLMs.

#### 6.4.2 Provider Configuration

```csharp
public record CustomProvider(
    string Id,
    string Name,
    string BaseUrl,
    string? ApiKey,  // Encrypted storage
    IReadOnlyList<CustomModel> Models,
    int TimeoutSeconds,
    int MaxRetries);

public record CustomModel(
    string Id,
    string DisplayName,
    int ContextLimit,
    decimal InputPricePerMToken,
    decimal OutputPricePerMToken);
```

#### 6.4.3 UI Requirements

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ⚙️ Custom Provider Settings                                     [Save] │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ Provider Name: [Ollama Local                               ]           │
│                                                                         │
│ Base URL:      [http://localhost:11434/v1                  ]           │
│                                                                         │
│ API Key:       [••••••••••••••••••] (optional)          [Show]         │
│                                                                         │
│ [Test Connection]  ✅ Connected - 3 models available                   │
│                                                                         │
│ Available Models:                                                       │
│ ┌─────────────────────────────────────────────────────────────────────┐│
│ │ ☑ llama3.1:70b         Context: 128K   Cost: Free                  ││
│ │ ☑ codellama:34b        Context: 16K    Cost: Free                  ││
│ │ ☐ mistral:7b           Context: 32K    Cost: Free                  ││
│ └─────────────────────────────────────────────────────────────────────┘│
│                                                                         │
│ Timeout: [30] seconds    Retries: [3]                                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### 6.4.4 Known Compatible Providers

| Provider | URL Pattern | Notes |
|----------|-------------|-------|
| Ollama | `http://localhost:11434/v1` | Local LLMs |
| LM Studio | `http://localhost:1234/v1` | Local LLMs |
| Together AI | `https://api.together.xyz/v1` | Cloud |
| Anyscale | `https://api.endpoints.anyscale.com/v1` | Cloud |
| Groq | `https://api.groq.com/openai/v1` | Fast inference |
| Azure OpenAI | `https://{resource}.openai.azure.com` | Enterprise |

---

## 7. Recommendations Summary (Updated)

### 7.1 Priority 0 (Must Have for v2.0)

| Feature | Effort | Impact |
|---------|--------|--------|
| **Enhanced Apply Mode** | Medium | Core differentiator |
| **Advanced Search** | Low | User expectation |
| **Multi-Workspace** | Medium | Productivity |

### 7.2 Priority 1 (Should Have for v2.1)

| Feature | Effort | Impact |
|---------|--------|--------|
| **Custom Providers** | Medium | Enterprise users, local LLMs |
| **Context Builder** | Medium | AI-powered file suggestion |
| **Version History** | Medium | Preset management |

### 7.3 Priority 2 (Nice to Have for v2.2+)

| Feature | Effort | Impact |
|---------|--------|--------|
| **Model Delegation** | Medium | Cost optimization |
| **Analytics Dashboard** | Medium | Usage insights |
| **Collaboration** | Very High | Team features |

### 7.4 OUT OF SCOPE (Decided)

| Feature | Reason |
|---------|--------|
| ~~Code Maps~~ | Not needed - users can select specific files |
| ~~MCP Server~~ | Not needed - clipboard workflow is sufficient |

### 7.5 Competitive Position

```
                    RepoPrompt Pro ($99/year)
                              │
                    ┌─────────┴─────────┐
                    │                   │
              File Selection      Apply Mode
                    │                   │
                    └─────────┬─────────┘
                              │
                     ShieldPrompt v2.0 (FREE)
                              │
                    ┌─────────┴─────────┐
                    │                   │
            🔐 Sanitization      🔓 Desanitization
               (UNIQUE!)            (UNIQUE!)
                              │
                      BETTER VALUE
                    (Free + Security)
```

---

## 8. Next Steps

1. **Create specification documents** for missing features
2. **Update UI specification** with new dialogs/menus
3. **Prioritize implementation order** based on impact
4. **Estimate development effort** per feature
5. **Define MVP** for each feature (what's essential vs nice-to-have)

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Complete RepoPrompt feature analysis |

