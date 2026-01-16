# 🔬 Deep Research: Agentic Code Editor Prompting Techniques

**Status:** ANALYSIS - DO NOT IMPLEMENT YET  
**Date:** January 15, 2026  
**Purpose:** Research prompt techniques used by Cursor, Aider, Cline, and similar tools

---

## 📚 How Agentic Code Editors Structure Prompts

### 1. Cursor AI

**Prompt Structure:**
```
SYSTEM:
- Role definition (senior developer)
- Project context (.cursorrules)
- Tool capabilities (file read/write/search)
- Output format requirements

USER:
- Task description
- Selected files (with content)
- Conversation history

ASSISTANT:
- Reasoning (thinking blocks)
- Tool calls (structured)
- Code changes (edit format)
```

**Key Techniques:**
1. **`.cursorrules` file** - Project-specific instructions loaded into system prompt
2. **Tool use** - Structured function calls for file operations
3. **Thinking blocks** - Visible reasoning before actions
4. **Context window management** - Intelligent file selection

**Code Edit Format:**
```
<search_replace>
<file_path>src/app.cs</file_path>
<old_string>public void OldMethod()</old_string>
<new_string>public void NewMethod()</new_string>
</search_replace>
```

---

### 2. Aider

**Prompt Structure:**
```
SYSTEM:
You are an expert software developer.
Always use best practices.
When editing files, use SEARCH/REPLACE blocks.

USER:
Here are the files:
{file_listing}

Task: {user_request}

ASSISTANT:
I'll make the following changes:

{path/to/file.py}
<<<<<<< SEARCH
def old_function():
    pass
=======
def new_function():
    return True
>>>>>>> REPLACE
```

**Key Techniques:**
1. **SEARCH/REPLACE blocks** - Git conflict-style markers for edits
2. **Whole file vs diff** - Can output entire file or just changes
3. **Repository map** - File tree with summaries
4. **Commit messages** - Auto-generates git commits

**Aider's SEARCH/REPLACE Format:**
```
{filename}
<<<<<<< SEARCH
exact content to find
=======
replacement content
>>>>>>> REPLACE
```

---

### 3. Cline (Claude Dev)

**Prompt Structure:**
```xml
<system>
You are Cline, an AI assistant for VS Code.
You have access to tools for file operations.
</system>

<tools>
<tool name="read_file">...</tool>
<tool name="write_to_file">...</tool>
<tool name="search_files">...</tool>
</tools>

<task>
{user_request}
</task>

<files>
<file path="src/app.ts">
{content}
</file>
</files>
```

**Key Techniques:**
1. **XML structure** - Claude prefers XML for structured data
2. **Tool definitions** - Function schemas in prompt
3. **Step-by-step execution** - Shows each action
4. **Approval workflow** - User confirms before writes

---

### 4. GitHub Copilot Chat

**Prompt Structure:**
```
<system>
You are GitHub Copilot, an AI programming assistant.
</system>

<context>
Active file: {filename}
Selection: {selected_code}
Language: {language}
</context>

<command>
/{command_type}
</command>

<user>
{user_message}
</user>
```

**Key Techniques:**
1. **Slash commands** - `/explain`, `/fix`, `/tests`
2. **Context injection** - Active file, selection, cursor position
3. **Workspace awareness** - Project structure
4. **Inline suggestions** - Streaming completions

---

## 🎯 Common Patterns Across All Tools

### Pattern 1: Role Definition
```
You are an expert {role} specializing in {domain}.
You follow {principles} and use {practices}.
```

### Pattern 2: Code Edit Format
| Tool | Format | Example |
|------|--------|---------|
| Cursor | `<search_replace>` XML | Structured, parseable |
| Aider | `<<<<<<< SEARCH` markers | Git-style, familiar |
| Cline | Tool calls | Function-based |
| Copilot | Inline/diff | Natural language |

### Pattern 3: Context Hierarchy
```
1. System instructions (highest priority)
2. Project rules (.cursorrules, .aiderrules)
3. File context (selected files)
4. Conversation history
5. User request (current task)
```

### Pattern 4: Output Structure
```
1. Reasoning/Planning (optional)
2. Actions/Changes (required)
3. Explanation (optional)
4. Follow-up suggestions (optional)
```

---

## 🎭 Role System Analysis

### What Roles Are Used

| Role | Purpose | Prompt Snippet |
|------|---------|----------------|
| **Software Engineer** | General coding | "You are an expert software developer..." |
| **Architect** | System design | "You are a software architect analyzing..." |
| **Security Expert** | Vulnerability review | "You are a security auditor checking..." |
| **QA Engineer** | Test writing | "You are a QA engineer creating tests..." |
| **DevOps** | Infrastructure | "You are a DevOps engineer setting up..." |
| **Code Reviewer** | PR review | "You are a senior engineer reviewing..." |
| **Documentation Writer** | Docs | "You are a technical writer creating..." |
| **Debugger** | Bug fixing | "You are debugging this code..." |

### Role Prompt Structure

```
You are a {ROLE_NAME}.

EXPERTISE:
{list of skills and knowledge areas}

PRIORITIES:
{what this role cares about most}

CONSTRAINTS:
{limitations and rules to follow}

OUTPUT STYLE:
{how to format responses}

WHEN UNSURE:
{fallback behavior}
```

---

## 📋 Recommended Feature Set for ShieldPrompt

Based on research, here's what you should add:

### Feature 1: Format Selector with Pros/Cons

**UI Element:** Dropdown or radio buttons

```
┌─────────────────────────────────────────┐
│ Output Format: [Markdown ▼]             │
├─────────────────────────────────────────┤
│ ✅ Pros:                                │
│ • Universal LLM support                 │
│ • Human-readable                        │
│ • Syntax highlighting                   │
│ • Token efficient                       │
│                                         │
│ ⚠️ Cons:                                │
│ • No strict schema                      │
│ • May need manual parsing               │
│                                         │
│ 💡 Best for: General use, most LLMs     │
└─────────────────────────────────────────┘
```

**Available Formats:**
| Format | Best For | Token Cost |
|--------|----------|------------|
| Markdown | General, GPT, Gemini | Low |
| XML | Claude, structured data | Medium |
| JSON | APIs, automation | Medium |
| Aider-style | Git workflows | Low |
| Plain Text | Simple tasks | Lowest |

---

### Feature 2: Role Selector Dropdown

**UI Element:** Dropdown in toolbar

```
┌─────────────────────────────────────────┐
│ Role: [🔧 Software Engineer ▼]          │
├─────────────────────────────────────────┤
│ 📋 This role will:                      │
│ • Focus on code quality and tests       │
│ • Use Clean Architecture patterns       │
│ • Apply TDD principles                  │
│ • Consider performance implications     │
│                                         │
│ 🎨 Tone: Technical, precise, practical  │
│ 📝 Style: Code-first with explanations  │
└─────────────────────────────────────────┘
```

**Built-in Roles:**
1. 🔧 Software Engineer
2. 🏗️ Software Architect
3. 🔐 Security Expert
4. 🧪 QA Engineer
5. 🚀 DevOps Engineer
6. 📝 Technical Writer
7. 🐛 Debugger
8. 👀 Code Reviewer
9. ⚡ Performance Engineer
10. ✨ Custom (user-defined)

---

### Feature 3: Role Customization Tab

**UI Element:** Separate tab/panel

```
┌─────────────────────────────────────────────────────────┐
│ 📋 Role Editor                                     [×]  │
├─────────────────────────────────────────────────────────┤
│ Role: [🔧 Software Engineer ▼]    [New] [Delete] [Save] │
├─────────────────────────────────────────────────────────┤
│ Name: [Software Engineer          ]                     │
│ Icon: [🔧 ▼]                                           │
├─────────────────────────────────────────────────────────┤
│ System Prompt:                                          │
│ ┌─────────────────────────────────────────────────────┐│
│ │You are an expert software engineer.                 ││
│ │                                                     ││
│ │EXPERTISE:                                           ││
│ │- Clean Architecture                                 ││
│ │- Test-Driven Development                            ││
│ │- SOLID principles                                   ││
│ │                                                     ││
│ │PRIORITIES:                                          ││
│ │1. Code correctness                                  ││
│ │2. Maintainability                                   ││
│ │3. Performance                                       ││
│ └─────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────┤
│ Tone: [Technical ▼]  Length: [Detailed ▼]              │
├─────────────────────────────────────────────────────────┤
│ Preview:                                                │
│ ┌─────────────────────────────────────────────────────┐│
│ │You are an expert software engineer.                 ││
│ │                                                     ││
│ │EXPERTISE:                                           ││
│ │- Clean Architecture                                 ││
│ │...                                                  ││
│ └─────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────┘
```

---

### Feature 4: Format Pros/Cons Display

**Implementation:**

```csharp
public record PromptFormat(
    string Id,
    string Name,
    string Icon,
    string Description,
    IReadOnlyList<string> Pros,
    IReadOnlyList<string> Cons,
    string BestFor,
    string TokenCost);
```

**Example Data:**

```yaml
formats:
  - id: markdown
    name: Markdown
    icon: 📝
    description: Universal format with syntax highlighting
    pros:
      - Universal LLM support
      - Human-readable
      - Syntax highlighting in code blocks
      - Token efficient
    cons:
      - No strict schema validation
      - LLM may ignore structure
    best_for: General use, ChatGPT, Gemini, Claude
    token_cost: Low

  - id: xml
    name: XML
    icon: 🏷️
    description: Structured format preferred by Claude
    pros:
      - Strict structure
      - Claude optimized
      - Easy to parse programmatically
      - CDATA for code safety
    cons:
      - More verbose (higher token cost)
      - Harder to read for humans
    best_for: Claude, structured workflows
    token_cost: Medium
```

---

## 🔄 Complete UI Flow

### Step 1: Select Format
```
[Format: Markdown ▼] → Shows pros/cons panel
```

### Step 2: Select Role
```
[Role: Software Engineer ▼] → Shows role description
```

### Step 3: Select Template
```
[Task: Code Review ▼] → Shows template with focus areas
```

### Step 4: Configure
```
[Focus: ☑ Security ☑ Performance]
[Custom Instructions: ...]
```

### Step 5: Preview
```
Live preview updates with:
- Selected format structure
- Role system prompt
- Template content
- Focus areas
- Files
```

---

## 🎯 Implementation Recommendation

### Phase 1: Format Selector with Pros/Cons
1. Create `PromptFormat` record
2. Create `IPromptFormatRepository` interface
3. Add YAML config: `config/prompt-formats.yaml`
4. Add dropdown UI with pros/cons panel
5. Update preview based on format

### Phase 2: Role Selector
1. Create `Role` record
2. Create `IRoleRepository` interface
3. Add YAML config: `config/roles.yaml`
4. Add dropdown UI with role description
5. Inject role into system prompt

### Phase 3: Role Editor Tab
1. Create new `RoleEditorView.axaml`
2. Create `RoleEditorViewModel`
3. Allow CRUD operations on roles
4. Save to user's config directory
5. Sync with role dropdown

### Phase 4: Advanced Format Options
1. Add Aider-style SEARCH/REPLACE format
2. Add XML format for Claude
3. Add JSON format for automation
4. Allow format templates per role

---

## 📊 Priority Matrix

| Feature | Value | Effort | Priority |
|---------|-------|--------|----------|
| Format pros/cons display | High | Low | ⭐⭐⭐ P1 |
| Role dropdown | High | Medium | ⭐⭐⭐ P1 |
| Role customization tab | Medium | High | ⭐⭐ P2 |
| Multiple format support | Medium | High | ⭐⭐ P2 |
| Aider-style output | Low | Medium | ⭐ P3 |

---

## 🚀 Recommended Next Steps

1. **Analyze current implementation** - What exists
2. **Define domain models** - Role, Format records
3. **Design ISP-compliant interfaces** - IRoleRepository, etc.
4. **Write tests first** (TDD)
5. **Implement incrementally**

---

**This document is for analysis only. Implementation should follow after approval.**

---

**Last Updated:** January 15, 2026  
**Status:** RECOMMENDATION - AWAITING DECISION

