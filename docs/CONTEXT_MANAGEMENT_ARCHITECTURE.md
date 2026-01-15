# Context Management Architecture
## Multi-Turn Conversation Support for AI Workflows

**Purpose:** Enable efficient context chaining across multiple AI interactions  
**Goal:** Minimize token waste while maintaining conversation coherence  
**Design Philosophy:** Smart context management, not naive concatenation

---

## 🎯 The Problem

### **Current State (ShieldPrompt v1.0):**
```
User Workflow:
1. Copy files → ChatGPT → Get response
2. Copy MORE files → ChatGPT → Get response (NO CONTEXT from #1)
3. Copy SAME files → ChatGPT → Get response (DUPLICATE tokens)

Issues:
❌ Each interaction is isolated
❌ AI doesn't remember previous responses
❌ User must manually copy AI responses back
❌ No conversation history
❌ Token waste on duplicate context
```

### **Desired State (Context-Aware):**
```
Conversation Flow:
1. Copy files → "Refactor this to async" → AI response
2. Follow-up: "Add error handling" → AI has context from #1
3. Follow-up: "Now add logging" → AI has context from #1 + #2
4. Review conversation history → See entire thread
5. Export conversation → Save for later

Benefits:
✅ AI maintains context across turns
✅ Follow-up questions work naturally
✅ Token efficiency (don't resend same code)
✅ Conversation history saved
✅ Can resume later
```

---

## 🏗️ Architectural Options

### **Option 1: Naive Concatenation (❌ NOT RECOMMENDED)**

**How it works:**
```
Turn 1: [Files] + "Refactor to async"
Turn 2: [Files] + Q1 + R1 + "Add error handling"
Turn 3: [Files] + Q1 + R1 + Q2 + R2 + "Add logging"
```

**Problems:**
- ❌ Exponential token growth
- ❌ Quickly exceeds context limits
- ❌ Wastes tokens on repeated file content
- ❌ No intelligent pruning

**Token Math:**
```
Turn 1: 5,000 tokens (files)
Turn 2: 5,000 + 1,000 (R1) = 6,000 tokens
Turn 3: 5,000 + 1,000 + 500 (R2) = 6,500 tokens
Turn 4: 5,000 + 1,000 + 500 + 800 (R3) = 7,300 tokens
Turn 10: Exceeds limit!
```

---

### **Option 2: Sliding Window (✅ GOOD)**

**How it works:**
```
Turn 1: [Files] + Q1 → R1
Turn 2: [Summary of Files] + Q1 + R1 + Q2 → R2
Turn 3: [Summary] + Q2 + R2 + Q3 → R3  (Q1/R1 dropped)
```

**Strategy:**
- First turn: Send full file contents
- Subsequent turns: Send file summary + last N turns
- Keep only recent conversation history (e.g., last 3 turns)
- Summarize old context

**Token Math:**
```
Turn 1: 5,000 tokens (files)
Turn 2: 500 (summary) + 1,000 (R1) = 1,500 tokens
Turn 3: 500 + 500 (R2) = 1,000 tokens
Turn 4: 500 + 800 (R3) = 1,300 tokens
Turn 10: Still ~1,500 tokens average
```

**Pros:**
✅ Bounded token usage
✅ Recent context preserved
✅ Works indefinitely
✅ Simple to implement

**Cons:**
⚠️ Loses early conversation context
⚠️ Summary quality matters

---

### **Option 3: Hierarchical Context (✅ BEST)**

**How it works:**
```
Context Layers:
├── Layer 1: File Metadata (always included)
│   └── File paths, structure, sizes
│
├── Layer 2: Code Summaries (always included)
│   └── Class/function signatures, no implementation
│
├── Layer 3: Active Files (current focus)
│   └── Full content of files being modified
│
└── Layer 4: Conversation History (sliding window)
    └── Last 3-5 turns of Q&A
```

**Token Allocation Strategy:**
```
Model Context: 128,000 tokens (GPT-4o example)
Reserve for Response: 32,000 tokens (25%)
Available for Input: 96,000 tokens

Allocation:
├── File Metadata: 500 tokens (always)
├── Code Map/Summaries: 2,000 tokens (always)
├── Active Files: 40,000 tokens (70-80% of budget)
└── Conversation History: 10,000 tokens (last 5 turns)

Total: ~52,500 tokens (54% of available)
Remaining: 43,500 tokens for flexibility
```

**Intelligent Prioritization:**
```
If approaching limit:
1. Reduce conversation history (keep last 3 → 2 → 1)
2. Summarize older file content
3. Keep only changed files full
4. Warn user when approaching limit
```

---

## 📋 Recommended Architecture

### **Domain Model:**

```csharp
// Conversation session
public record ConversationSession(
    string SessionId,
    DateTime StartedAt,
    List<ConversationTurn> Turns,
    HashSet<string> ActiveFilePaths);

// Single turn in conversation
public record ConversationTurn(
    int TurnNumber,
    DateTime Timestamp,
    ConversationRole Role,
    string Content,
    int TokenCount,
    TurnMetadata Metadata);

public enum ConversationRole
{
    User,       // Developer's question
    Assistant,  // AI's response
    System      // Context/instructions
}

// Metadata for intelligent management
public record TurnMetadata(
    List<string> FilesModified,      // Which files AI changed
    List<string> NewFilesCreated,    // New files AI suggested
    List<string> FilesReferenced,    // Files mentioned
    bool ContainsCode,               // Response has code blocks
    string? Summary);                // Optional summary for old turns

// Context manager configuration
public record ContextOptions(
    int MaxConversationTurns = 5,
    int MaxTokensForHistory = 10_000,
    int MaxTokensForFiles = 40_000,
    bool AutoSummarizeOldTurns = true,
    bool IncludeCodeMap = true);
```

---

## 🔄 Conversation Flow Design

### **Turn 1: Initial Request**
```csharp
Prompt Structure:
┌─────────────────────────────────────────┐
│ SYSTEM CONTEXT                          │
│ - Code map (class/function signatures)  │
│ - File structure                        │
│ - Total: ~2,000 tokens                  │
├─────────────────────────────────────────┤
│ FILE CONTENTS (Selected)                │
│ - Full code for selected files          │
│ - Sanitized automatically               │
│ - Total: ~40,000 tokens                 │
├─────────────────────────────────────────┤
│ USER REQUEST                            │
│ - "Refactor this to use async/await"    │
│ - Total: ~50 tokens                     │
└─────────────────────────────────────────┘

Total: ~42,050 tokens
```

### **Turn 2: Follow-up Question**
```csharp
Prompt Structure:
┌─────────────────────────────────────────┐
│ SYSTEM CONTEXT (Compact)                │
│ - Code map (signatures only)            │
│ - Total: ~500 tokens                    │
├─────────────────────────────────────────┤
│ ACTIVE FILES (Only changed/relevant)    │
│ - Files modified in Turn 1              │
│ - Total: ~5,000 tokens                  │
├─────────────────────────────────────────┤
│ CONVERSATION HISTORY                    │
│ - Turn 1 Question: "Refactor to async"  │
│ - Turn 1 Response: [Summary or full]    │
│ - Total: ~3,000 tokens                  │
├─────────────────────────────────────────┤
│ NEW REQUEST                             │
│ - "Add error handling to those methods" │
│ - Total: ~20 tokens                     │
└─────────────────────────────────────────┘

Total: ~8,520 tokens (80% reduction!)
```

### **Turn 5+: Long Conversation**
```csharp
Prompt Structure:
┌─────────────────────────────────────────┐
│ SYSTEM CONTEXT                          │
│ - Code map                              │
│ - Total: ~500 tokens                    │
├─────────────────────────────────────────┤
│ ACTIVE FILES (Delta only)               │
│ - Only files being discussed NOW        │
│ - Total: ~3,000 tokens                  │
├─────────────────────────────────────────┤
│ RECENT CONVERSATION (Last 3 turns)     │
│ - Turn 3 Summary: "Added error handling"│
│ - Turn 4 Q+R: Full                      │
│ - Turn 5 Q: Current                     │
│ - Total: ~5,000 tokens                  │
├─────────────────────────────────────────┤
│ CONVERSATION SUMMARY                    │
│ - "Earlier: Refactored to async..."     │
│ - Total: ~200 tokens                    │
└─────────────────────────────────────────┘

Total: ~8,700 tokens (sustainable!)
```

---

## 🎯 Interface Design (ISP-Compliant)

### **Core Interfaces:**

```csharp
// Context management (6 methods)
public interface IConversationManager
{
    Task<ConversationSession> CreateSessionAsync(IEnumerable<FileNode> files);
    Task AddTurnAsync(ConversationSession session, string question, string? response = null);
    Task<string> BuildPromptAsync(ConversationSession session, string newQuestion);
    Task<ConversationSession> LoadSessionAsync(string sessionId);
    Task SaveSessionAsync(ConversationSession session);
}

// Context optimization (4 methods)
public interface IContextOptimizer
{
    string SummarizeTurn(ConversationTurn turn);
    string CreateCodeMap(IEnumerable<FileNode> files);
    IEnumerable<ConversationTurn> PruneHistory(
        IEnumerable<ConversationTurn> turns, 
        int maxTokens);
    int EstimateTokens(ConversationSession session);
}

// Conversation persistence (3 methods)
public interface IConversationRepository
{
    Task SaveAsync(ConversationSession session);
    Task<ConversationSession?> LoadAsync(string sessionId);
    Task<IEnumerable<ConversationSession>> GetRecentAsync(int count = 10);
}
```

**All ISP-compliant: <10 methods each ✅**

---

## 💡 Smart Features to Implement

### **1. Automatic Context Pruning**

**Strategy:** Keep conversation coherent but token-efficient

```csharp
public class SmartContextPruner
{
    public ConversationContext Prune(ConversationSession session, int maxTokens)
    {
        var budget = new TokenBudget(maxTokens);
        
        // Always include (mandatory)
        budget.Allocate("Code Map", 2_000);
        budget.Allocate("Current Question", 100);
        
        // Prioritize active files
        var activeFiles = GetActiveFiles(session);
        var fileTokens = Math.Min(activeFiles.TokenCount, budget.Remaining * 0.7);
        budget.Allocate("Active Files", fileTokens);
        
        // Fill remaining with conversation history
        var historyTokens = budget.Remaining;
        var recentTurns = GetRecentTurns(session, historyTokens);
        budget.Allocate("History", recentTurns.TokenCount);
        
        return new ConversationContext(codeMap, activeFiles, recentTurns);
    }
}
```

### **2. File Change Tracking**

**Track which files AI modified:**

```csharp
public record FileModification(
    string FilePath,
    int TurnNumber,
    string OriginalContent,
    string ModifiedContent,
    ChangeType Type);

public enum ChangeType
{
    Modified,
    Created,
    Deleted,
    Renamed
}

// When AI responds, parse code blocks and track changes
public class ChangeDetector
{
    public IEnumerable<FileModification> DetectChanges(
        ConversationTurn aiResponse,
        IEnumerable<FileNode> currentFiles)
    {
        // Parse ```filename.cs blocks
        // Compare with current file content
        // Return what changed
    }
}
```

### **3. Intelligent Summarization**

**Summarize old turns to save tokens:**

```csharp
public interface ITurnSummarizer
{
    string Summarize(ConversationTurn turn);
}

public class SmartSummarizer : ITurnSummarizer
{
    public string Summarize(ConversationTurn turn)
    {
        // If turn was a question
        if (turn.Role == ConversationRole.User)
        {
            return $"Q: {ExtractIntent(turn.Content)}";
        }
        
        // If turn was AI response
        if (turn.Metadata.FilesModified.Any())
        {
            return $"R: Modified {string.Join(", ", turn.Metadata.FilesModified)}";
        }
        
        return $"R: {ExtractKeyPoints(turn.Content)}";
    }
    
    // Example:
    // Original (1,500 tokens):
    //   "Here's the refactored code with async/await... [code blocks]"
    // 
    // Summary (50 tokens):
    //   "R: Refactored UserService.cs, OrderService.cs to async/await"
}
```

### **4. Context Budget Management**

**Visual indicator for users:**

```csharp
public record ContextBudget(
    int TotalAvailable,
    int UsedByFiles,
    int UsedByHistory,
    int UsedByCodeMap,
    int Remaining)
{
    public double PercentUsed => (double)(TotalAvailable - Remaining) / TotalAvailable;
    public bool IsNearLimit => PercentUsed > 0.8;
    public string Warning => PercentUsed switch
    {
        > 0.9 => "⚠️ Context almost full - consider starting new conversation",
        > 0.8 => "⚡ Context 80% full - will auto-prune old turns",
        > 0.6 => "📊 Context 60% full",
        _ => "✅ Context available"
    };
}
```

---

## 🎨 UI/UX Design

### **Conversation Panel (New Component)**

```
┌─────────────────────────────────────────────────────────────────┐
│  ShieldPrompt - Conversation Mode                               │
├─────────────────────────────────────────────────────────────────┤
│  📁 Files (3)  |  💬 Conversation (5 turns)  |  🛡️ Protection   │
├────────────────┬────────────────────────────────────────────────┤
│                │  Conversation: "UserService Refactor"          │
│  [File Tree]   │  ─────────────────────────────────────────────│
│                │                                                 │
│  ☑ User.cs     │  Turn 1 (You):                                 │
│  ☑ UserSvc.cs  │  "Refactor to async/await"                     │
│  ☐ Order.cs    │                                                 │
│                │  Turn 1 (AI):                                   │
│  Context: 45%  │  "Here's the async version... [Modified 2     │
│  📊────────     │   files]"                                      │
│                │                                                 │
│                │  Turn 2 (You):                                 │
│                │  "Add error handling"                          │
│                │                                                 │
│                │  Turn 2 (AI):                                   │
│                │  "Added try-catch blocks... [Modified 2 files]"│
│                │                                                 │
│                │  Turn 3 (You):                                 │
│                │  ▌ Type next question...                       │
│                │                                                 │
├────────────────┴────────────────────────────────────────────────┤
│  [Copy Full Context] [Copy Last Turn] [New Conversation]       │
│  📊 5 turns | 12,450 tokens | 🔐 8 values masked               │
└─────────────────────────────────────────────────────────────────┘
```

**Features:**
- See entire conversation history
- Add follow-up questions inline
- Visual token budget gauge
- Export/import conversations
- Multiple conversation tabs

---

## 📊 Efficient Prompt Formats

### **Format A: Full Context (Turn 1)**

```markdown
# Project Context

## Code Structure
[Code map with signatures - 2,000 tokens]

## Active Files
[Full file contents - 40,000 tokens]

## Task
Refactor UserService to use async/await patterns.

## Constraints
- Maintain backward compatibility
- Add cancellation token support
- Use ConfigureAwait(false) in library code
```

### **Format B: Follow-up (Turn 2+)**

```markdown
# Conversation Context

## Previous Discussion
Turn 1:
- Q: Refactor to async/await
- R: Refactored UserService.cs, OrderService.cs [Summary]
  Modified files: UserService.cs, OrderService.cs

## Current Code State
### UserService.cs (last modified: Turn 1)
[Only show this file if relevant to new question]

## New Request
Add comprehensive error handling to the async methods.

## Conversation History
[Last 3-5 turns summarized]
```

### **Format C: Code Map Only (Quick Questions)**

```markdown
# Quick Context

## Code Map
- UserService.cs
  - GetUserAsync(int id)
  - CreateUserAsync(User user)
- OrderService.cs
  - ProcessOrderAsync(Order order)

## Question
What's the best pattern for handling database timeouts in async methods?

[No full file contents - pure architectural question]
```

---

## 🎯 Smart Context Selection

### **Algorithm: What to Include**

```csharp
public class ContextSelector
{
    public ConversationContext SelectContext(
        ConversationSession session,
        string newQuestion,
        ContextOptions options)
    {
        var budget = new TokenBudget(options.MaxTokensForFiles);
        
        // Step 1: Detect intent
        var intent = AnalyzeIntent(newQuestion);
        
        // Step 2: Select files based on intent
        var selectedFiles = intent.Type switch
        {
            IntentType.FollowUp => GetFilesFromLastTurn(session),
            IntentType.NewTopic => GetAllActiveFiles(session),
            IntentType.Architectural => new List<FileNode>(), // Code map only
            IntentType.Debugging => GetFilesWithErrors(session),
            _ => GetAllActiveFiles(session)
        };
        
        // Step 3: Determine detail level
        var detailLevel = CalculateDetailLevel(selectedFiles, budget);
        
        // Step 4: Select conversation history
        var historyBudget = options.MaxTokensForHistory;
        var history = SelectRelevantHistory(session, newQuestion, historyBudget);
        
        return new ConversationContext(
            CodeMap: GenerateCodeMap(session.ActiveFiles),
            Files: selectedFiles,
            DetailLevel: detailLevel,
            History: history
        );
    }
}

public enum IntentType
{
    FollowUp,        // "Add error handling" (continue previous)
    NewTopic,        // "Now let's work on OrderService" (switch focus)
    Architectural,   // "What's the best pattern for X?" (no code needed)
    Debugging,       // "Why is this failing?" (need full context)
    Clarification    // "What did you mean by X?" (minimal context)
}
```

---

## 💾 Persistence Strategy

### **Option A: JSON Files (Recommended for MVP)**

```
~/.config/ShieldPrompt/conversations/
├── session-2026-01-14-143022.json
├── session-2026-01-14-151533.json
└── active-session.json  (current)

Structure:
{
  "sessionId": "2026-01-14-143022",
  "startedAt": "2026-01-14T14:30:22Z",
  "title": "UserService Refactor",
  "activFiles": [...],
  "turns": [
    {
      "number": 1,
      "role": "user",
      "content": "Refactor to async",
      "timestamp": "...",
      "tokenCount": 42050,
      "metadata": {
        "filesModified": [],
        "summary": null
      }
    },
    {
      "number": 2,
      "role": "assistant",
      "content": "Here's the async version...",
      "timestamp": "...",
      "tokenCount": 3200,
      "metadata": {
        "filesModified": ["UserService.cs", "OrderService.cs"],
        "summary": "Refactored 2 files to async/await"
      }
    }
  ]
}
```

### **Option B: SQLite (For Advanced Features)**

**Tables:**
```sql
CREATE TABLE conversations (
    session_id TEXT PRIMARY KEY,
    title TEXT,
    started_at TIMESTAMP,
    last_updated TIMESTAMP,
    total_turns INTEGER,
    total_tokens INTEGER
);

CREATE TABLE conversation_turns (
    turn_id TEXT PRIMARY KEY,
    session_id TEXT,
    turn_number INTEGER,
    role TEXT,
    content TEXT,
    token_count INTEGER,
    timestamp TIMESTAMP,
    summary TEXT,
    FOREIGN KEY (session_id) REFERENCES conversations(session_id)
);

CREATE TABLE turn_file_references (
    turn_id TEXT,
    file_path TEXT,
    was_modified BOOLEAN,
    FOREIGN KEY (turn_id) REFERENCES conversation_turns(turn_id)
);
```

**Benefits:**
- Query by date, file, topic
- Analyze conversation patterns
- Token usage analytics
- Search across conversations

---

## 🔍 Example Scenarios

### **Scenario 1: Iterative Refactoring**

```
Files: UserService.cs (200 lines), OrderService.cs (300 lines)

Turn 1:
  User: "Refactor to async/await"
  Context: [Both files full] = 5,000 tokens
  AI: Refactors both files
  
Turn 2:
  User: "Add error handling"
  Context: [Code map] + [Both files full] + [Turn 1 summary] = 6,000 tokens
  AI: Adds try-catch blocks
  
Turn 3:
  User: "Add logging"
  Context: [Code map] + [Both files full] + [Turn 2-3 summary] = 6,500 tokens
  AI: Adds logging
  
Turn 4:
  User: "Optimize the query in UserService"
  Context: [Code map] + [UserService ONLY] + [Turn 3-4 summary] = 4,000 tokens
  AI: Optimizes just UserService
```

**Token Efficiency:** Each turn stays under 10K tokens instead of growing exponentially!

### **Scenario 2: Cross-File Changes**

```
Files: 5 service files, 10 models

Turn 1:
  User: "Add authentication to all services"
  Context: [All 15 files] = 30,000 tokens
  AI: Suggests auth pattern
  
Turn 2:
  User: "Apply that to UserService first"
  Context: [Code map of all] + [UserService full] + [Turn 1 summary] = 8,000 tokens
  AI: Implements for UserService
  
Turn 3:
  User: "Now OrderService"
  Context: [Code map] + [OrderService full] + [Turn 2 result as example] = 9,000 tokens
  AI: Implements for OrderService (using Turn 2 as template)
```

**Benefit:** Don't resend all 15 files every time!

---

## 🎯 UI Workflow

### **New Conversation:**
```
1. User selects files
2. Clicks "Start Conversation" (not just "Copy")
3. Enters first question in dialog
4. ShieldPrompt:
   - Builds context
   - Sanitizes
   - Copies to clipboard
5. User pastes in ChatGPT
6. ChatGPT responds
7. User copies response
8. Clicks "Add to Conversation"
9. ShieldPrompt:
   - Stores response
   - Detects file changes
   - Ready for next turn
```

### **Continue Conversation:**
```
1. Type follow-up question in ShieldPrompt
2. Click "Copy Follow-up" (not full context)
3. Paste in ChatGPT (AI still has previous context)
4. Get response
5. Add to conversation
6. Repeat!
```

### **Review History:**
```
1. Click "Conversation History" tab
2. See all turns
3. Click any turn to see full Q&A
4. Export as markdown
5. Resume conversation anytime
```

---

## 📋 Recommended Implementation Phases

### **Phase 1: Basic Context Chaining (MVP)**
- [ ] ConversationSession entity
- [ ] Add turn to session
- [ ] Build prompt with last N turns
- [ ] Simple UI for follow-up questions

**Deliverable:** Can have multi-turn conversations with sliding window

### **Phase 2: Smart Optimization**
- [ ] File change detection
- [ ] Automatic summarization
- [ ] Active file tracking
- [ ] Context budget visualization

**Deliverable:** Efficient token usage, visual feedback

### **Phase 3: Persistence & Management**
- [ ] Save/load conversations
- [ ] List recent conversations
- [ ] Resume old conversations
- [ ] Export to markdown

**Deliverable:** Never lose conversation history

### **Phase 4: Advanced Features**
- [ ] Multiple conversation tabs
- [ ] Conversation search
- [ ] Token analytics
- [ ] Automatic code map generation

**Deliverable:** Power user features

---

## 🎯 Recommended Approach

### **START SIMPLE:**

**Phase 1 MVP - Minimal Context Chaining:**
```csharp
// Just track last 3 turns
public class SimpleConversationManager
{
    private readonly Queue<ConversationTurn> _turns = new(3);
    
    public string BuildPrompt(string newQuestion)
    {
        var sb = new StringBuilder();
        
        // Add last 3 turns
        foreach (var turn in _turns)
        {
            sb.AppendLine($"{turn.Role}: {turn.Content}");
        }
        
        // Add new question
        sb.AppendLine($"User: {newQuestion}");
        
        return sb.ToString();
    }
}
```

**Benefits:**
- ✅ Simple to implement (1-2 hours)
- ✅ Solves 80% of use cases
- ✅ No complexity
- ✅ Easy to extend later

**THEN EVOLVE:**
- Add summarization
- Add file tracking
- Add persistence
- Add analytics

---

## 🔄 Alternative: Cursor-Style Approach

### **How Cursor Does It:**

```
Cursor maintains:
1. Workspace context (file tree, recent files)
2. Current file content
3. Last N conversation turns
4. Automatic code changes

For each request:
- Includes relevant files only
- Adds recent conversation
- Applies changes automatically
- Updates conversation history
```

### **ShieldPrompt Adaptation:**

```
We already have:
✅ Workspace context (file tree)
✅ File selection
✅ Sanitization

Add:
1. Conversation history (last 5 turns)
2. File change tracking
3. Smart context selection
4. Persistent sessions

Don't add (stay focused):
❌ Automatic code writing (user controls)
❌ Real-time sync
❌ Chat interface (keep copy/paste)
```

---

## 📊 Token Efficiency Comparison

### **Without Context Management:**
```
Turn 1: 42,000 tokens (files)
Turn 2: 42,000 + 3,000 (R1) = 45,000 tokens
Turn 3: 42,000 + 3,000 + 2,000 (R2) = 47,000 tokens
Turn 4: 42,000 + 3,000 + 2,000 + 1,500 (R3) = 48,500 tokens
Turn 5: APPROACHING LIMIT
```

### **With Smart Context Management:**
```
Turn 1: 42,000 tokens (files full)
Turn 2: 2,000 (map) + 5,000 (changed files) + 3,000 (R1) = 10,000 tokens
Turn 3: 2,000 + 5,000 + 2,000 (R2) = 9,000 tokens
Turn 4: 2,000 + 5,000 + 1,500 (R3) = 8,500 tokens
Turn 10: Still ~9,000 tokens average
```

**Efficiency Gain:** **80% token reduction** after turn 1!

---

## 🎯 Recommendations

### **DO (High Value):**
1. ✅ **Implement sliding window** - Last 3-5 turns
2. ✅ **Track file changes** - Know what AI modified
3. ✅ **Create code maps** - Signatures instead of full content
4. ✅ **Save conversations** - JSON files in user directory
5. ✅ **Visual context budget** - Show token usage

### **DON'T (Low Value / High Complexity):**
1. ❌ **Advanced summarization AI** - Keep it simple
2. ❌ **Real-time collaboration** - Out of scope
3. ❌ **Chat interface** - Keep copy/paste workflow
4. ❌ **Automatic code application** - User maintains control
5. ❌ **Conversation branching** - Over-engineering

### **MAYBE (Future Consideration):**
1. 🤔 **Conversation search** - If users have many sessions
2. 🤔 **Token analytics** - Track usage patterns
3. 🤔 **Smart file suggestion** - "You might also need OrderService.cs"
4. 🤔 **Conversation templates** - Pre-built workflows

---

## 📐 Estimated Implementation Effort

| Phase | Features | Effort | Value |
|-------|----------|--------|-------|
| **Phase 1** | Basic conversation tracking, sliding window | 4-6 hours | HIGH |
| **Phase 2** | File change detection, code maps | 4-6 hours | MEDIUM |
| **Phase 3** | Persistence, conversation list | 3-4 hours | MEDIUM |
| **Phase 4** | Advanced UI, analytics | 6-8 hours | LOW |

**Recommendation:** Start with **Phase 1 only** (4-6 hours)
- Solves the core problem
- Easy to implement
- High user value
- Can iterate based on feedback

---

## 🎊 Summary

### **The Goal:**
Enable multi-turn conversations with AI while maintaining token efficiency and conversation coherence.

### **The Solution:**
**Hierarchical Context with Sliding Window**
- Code maps (always)
- Active files (smart selection)
- Recent history (last 3-5 turns)
- Automatic pruning

### **The Benefit:**
- 80% token reduction on follow-ups
- Indefinite conversation length
- Maintains coherence
- User stays in control

### **The Implementation:**
Start simple (sliding window), evolve based on usage.

**Estimated ROI:** Very high - users can have 10+ turn conversations instead of 2-3.

---

## 🎯 Decision Required

**Do you want me to:**

**Option A:** Implement Phase 1 now (4-6 hours)
- Basic conversation tracking
- Last 3 turns in context
- Simple UI for follow-ups

**Option B:** Create detailed spec first
- Complete interface definitions
- TDD test scenarios
- UI mockups

**Option C:** Ship v1.0 as-is, add this in v1.1
- Get user feedback first
- Validate demand
- Iterate based on real usage

**Recommendation:** **Option C** - Ship what you have (it's excellent!), add context management in v1.1 based on user demand.

---

ROLE: architect STRICT=true
