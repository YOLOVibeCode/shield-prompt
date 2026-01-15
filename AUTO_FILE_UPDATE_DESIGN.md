# Auto File Update Feature - Design Document

**Date:** January 14, 2026  
**Feature:** Automatically update source files from AI response  
**Status:** Design Phase

---

## 🎯 USER'S VISION (The Complete Loop)

### **Current Workflow (Manual):**
```
1. User selects files in ShieldPrompt
2. Clicks Copy → sanitizes → clipboard has safe content
3. Pastes in ChatGPT: "Refactor this code"
4. ChatGPT returns improved code (with aliases: DATABASE_0, IP_ADDRESS_0)
5. User copies ChatGPT response
6. Pastes in ShieldPrompt → desanitizes → sees restored preview
7. Clicks "Copy Restored"
8. ❌ MANUALLY pastes in IDE/editor
9. ❌ MANUALLY saves files
```

### **Desired Workflow (Automated):**
```
1. User selects files in ShieldPrompt
2. Clicks Copy → sanitizes → pastes in ChatGPT
3. ChatGPT returns improved code
4. User pastes ChatGPT response in ShieldPrompt
5. ShieldPrompt shows:
   📄 Program.cs will be updated (15 lines changed)
   📄 Database.cs will be updated (8 lines changed)
   📄 New: Config.cs will be created
6. User clicks "✅ Apply to Files"
7. ✨ ShieldPrompt AUTOMATICALLY:
   - Creates backup of existing files
   - Writes Program.cs with restored values
   - Writes Database.cs with restored values
   - Creates Config.cs (new file)
   - Shows: "✅ Updated 2 files, created 1 file"
8. ✨ DONE! Files updated automatically!
```

**This IS auto file manipulation - and it's EXACTLY what you want!**

---

## 🏗️ REQUIRED ARCHITECTURE

### **Component 1: AI Response Parser** (NEW)

**Purpose:** Extract file operations from ChatGPT/Claude response

```csharp
public interface IAIResponseParser
{
    /// <summary>
    /// Parses AI response to identify file operations.
    /// Handles ChatGPT, Claude, and other AI formats.
    /// </summary>
    ParsedAIResponse Parse(string aiResponse, IEnumerable<FileNode> originalFiles);
}

public record ParsedAIResponse(
    IReadOnlyList<FileUpdate> Updates,
    IReadOnlyList<string> Warnings);

public record FileUpdate(
    string FilePath,          // e.g., "src/Program.cs"
    string Content,           // Updated file content
    FileUpdateType Type,      // Create, Update, Delete
    int LinesChanged);        // For preview

public enum FileUpdateType
{
    Create,                   // New file
    Update,                   // Modify existing
    Delete                    // Remove file
}
```

**Parsing Strategies:**

**Strategy 1: Markdown Code Blocks (ChatGPT/Claude format)**
```
AI Response:
---
Here's the updated code:

**Program.cs**
```csharp
public class Program { ... }
```

**Database.cs**
```csharp
public class Database { ... }
```
---

Parser extracts:
- File 1: Program.cs → code block content
- File 2: Database.cs → code block content
```

**Strategy 2: Inline Comments**
```
AI Response:
---
```csharp
// File: src/Program.cs
public class Program { ... }

// File: src/Database.cs
public class Database { ... }
```
---

Parser extracts file paths from comments
```

**Strategy 3: Match Original Files**
```
User sent: Program.cs, Database.cs
AI returns: 2 code blocks
Parser assumption: Map in order (block 1 → Program.cs, block 2 → Database.cs)
```

---

### **Component 2: File Writer Service** (NEW)

**Purpose:** Safely write/create/delete files with backups

```csharp
public interface IFileWriterService
{
    /// <summary>
    /// Applies file updates with safety checks and backups.
    /// </summary>
    Task<FileWriteResult> ApplyUpdatesAsync(
        IEnumerable<FileUpdate> updates,
        string baseDirectory,
        FileWriteOptions options);
    
    /// <summary>
    /// Creates backup before writing.
    /// </summary>
    Task<string> CreateBackupAsync(IEnumerable<string> filePaths);
    
    /// <summary>
    /// Restores from backup (undo).
    /// </summary>
    Task RestoreBackupAsync(string backupId);
}

public record FileWriteResult(
    int FilesCreated,
    int FilesUpdated,
    int FilesDeleted,
    string BackupId,
    IReadOnlyList<string> Errors);

public class FileWriteOptions
{
    public bool CreateBackup { get; init; } = true;
    public bool AllowCreateDirectories { get; init; } = true;
    public bool AllowDelete { get; init; } = false;    // Default: NO deletes
    public bool DryRun { get; init; } = false;          // Preview only
}
```

**Safety Features:**
- ✅ **Automatic backup** before ANY write
- ✅ **Atomic operations** (all succeed or all roll back)
- ✅ **Validate paths** (prevent directory traversal attacks)
- ✅ **Confirm deletions** (require explicit user approval)
- ✅ **Undo support** (restore from backup)

---

### **Component 3: File Diff Viewer** (NEW)

**Purpose:** Show user what will change before applying

```csharp
public interface IFileDiffGenerator
{
    /// <summary>
    /// Generates diff preview for file changes.
    /// </summary>
    FileDiff GenerateDiff(string originalContent, string newContent);
}

public record FileDiff(
    string FilePath,
    int LinesAdded,
    int LinesRemoved,
    int LinesChanged,
    IReadOnlyList<DiffHunk> Hunks);

public record DiffHunk(
    int StartLine,
    IReadOnlyList<DiffLine> Lines);

public record DiffLine(
    DiffLineType Type,
    int LineNumber,
    string Content);

public enum DiffLineType
{
    Unchanged,
    Added,
    Removed,
    Modified
}
```

**UI Preview:**
```
📄 Program.cs (15 lines changed)
  ─────────────────────────────────────
  Line 10: - var db = "DATABASE_0";
  Line 10: + var db = Configuration["Database"] ?? "ProductionDB";
  
  Line 15: + // New validation method
  Line 16: + public void Validate() { ... }
  
  Summary: +7 added, -2 removed, 6 modified

📄 Config.cs (NEW FILE)
  ─────────────────────────────────────
  Will create: 25 lines

📄 OldHelper.cs (WILL BE DELETED)
  ─────────────────────────────────────
  ⚠️ This file will be removed!
  [User must explicitly confirm deletions]
```

---

### **Component 4: Enhanced PasteRestoreViewModel** (UPDATE)

**New Properties:**
```csharp
public ObservableCollection<FileUpdatePreview> FileUpdates { get; } = new();

[ObservableProperty]
private bool _canApplyToFiles;

[ObservableProperty]
private string _applyButtonText = "Apply to Files";

public record FileUpdatePreview(
    string FilePath,
    string FileName,
    FileUpdateType Type,
    int LinesChanged,
    string Summary);
```

**New Commands:**
```csharp
[RelayCommand]
private async Task ParseAIResponseAsync()
{
    // 1. Parse AI response to detect file operations
    var parsed = await _aiResponseParser.ParseAsync(
        RestoredContent,  // Already desanitized!
        _originalFiles);   // Files user originally sent
    
    // 2. Generate previews
    FileUpdates.Clear();
    foreach (var update in parsed.Updates)
    {
        var preview = await GeneratePreviewAsync(update);
        FileUpdates.Add(preview);
    }
    
    // 3. Enable apply button
    CanApplyToFiles = FileUpdates.Count > 0;
    ApplyButtonText = $"Apply to {FileUpdates.Count} File(s)";
}

[RelayCommand(CanExecute = nameof(CanApplyToFiles))]
private async Task ApplyToFilesAsync()
{
    try
    {
        StatusMessage = "Creating backup...";
        
        // 1. Parse response
        var updates = await _aiResponseParser.ParseAsync(RestoredContent, _originalFiles);
        
        // 2. Create backup
        var backupId = await _fileWriter.CreateBackupAsync(
            updates.Updates.Select(u => u.FilePath));
        
        StatusMessage = $"Applying changes to {updates.Updates.Count} files...";
        
        // 3. Apply updates
        var result = await _fileWriter.ApplyUpdatesAsync(
            updates.Updates,
            _baseDirectory,
            new FileWriteOptions 
            { 
                CreateBackup = true,
                AllowDelete = false  // Require explicit confirmation
            });
        
        // 4. Update undo stack
        await _undoRedoManager.ExecuteAsync(new FileUpdateAction(
            backupId,
            result,
            _fileWriter));
        
        // 5. Success!
        StatusMessage = $"✅ Updated {result.FilesUpdated} files, created {result.FilesCreated} files";
        
        // Close dialog
        CloseDialog();
    }
    catch (Exception ex)
    {
        StatusMessage = $"❌ Error: {ex.Message}";
        // Backup still exists - user can restore manually
    }
}
```

---

## 🔍 **PARSING STRATEGIES**

### **Strategy 1: Smart Detection (Recommended)**

**Heuristics:**
1. Look for file paths in markdown headers: `**Program.cs**`
2. Look for file comments: `// File: src/Program.cs`
3. Look for code blocks with language hints: ` ```csharp `
4. Match number of code blocks to number of original files
5. If ambiguous, ask user to confirm mapping

**Example:**
```
AI Response:
---
I'll update your code:

**Program.cs**
```csharp
public class Program
{
    private readonly string _db = Configuration["Database"];
}
```

**Database.cs**
```csharp
public class Database
{
    // Improved implementation
}
```
---

Parser Output:
- File 1: "Program.cs" → [code block 1]
- File 2: "Database.cs" → [code block 2]
```

---

### **Strategy 2: Remember Original Files (Fallback)**

**Approach:**
- When user clicks Copy, remember which files were selected
- When AI response has N code blocks, map to original N files
- Assumption: AI returns files in same order

**Pros:**
- ✅ Works even if AI doesn't include file names
- ✅ Deterministic

**Cons:**
- ⚠️ Assumes order matches
- ⚠️ Breaks if AI returns fewer/more files

---

### **Strategy 3: User Confirmation (Safest)**

**Approach:**
- Parse AI response to extract code blocks
- Show dialog: "Which file should this code update?"
- User selects from dropdown (list of original files)
- User confirms mapping before apply

**Pros:**
- ✅ User controls everything
- ✅ No ambiguity
- ✅ Safest

**Cons:**
- More clicks (but still faster than manual paste)

---

## 📋 IMPLEMENTATION PLAN (TDD)

### **Phase 1: AI Response Parser (2 hours)**

**Tests:**
```csharp
[Fact]
public void Parse_ChatGPTWithFileHeaders_ShouldExtractFiles()
{
    var response = @"
**Program.cs**
```csharp
public class Program { }
```
";
    var parsed = _parser.Parse(response, originalFiles);
    
    parsed.Updates.Should().HaveCount(1);
    parsed.Updates[0].FilePath.Should().Be("Program.cs");
    parsed.Updates[0].Type.Should().Be(FileUpdateType.Update);
}

[Fact]
public void Parse_MultipleFiles_ShouldExtractAll()

[Fact]
public void Parse_WithFileComments_ShouldExtractPaths()

[Fact]
public void Parse_NoFileMarkers_ShouldMapToOriginalFiles()
```

---

### **Phase 2: File Writer Service (2 hours)**

**Tests:**
```csharp
[Fact]
public async Task ApplyUpdates_ShouldCreateBackup_BeforeWriting()

[Fact]
public async Task ApplyUpdates_WhenDirectoryMissing_ShouldCreate()

[Fact]
public async Task ApplyUpdates_OnError_ShouldRollback()

[Fact]
public async Task ApplyUpdates_ShouldUpdateMultipleFiles_Atomically()

[Fact]
public async Task RestoreBackup_ShouldRestoreAllFiles()
```

---

### **Phase 3: UI Integration (1 hour)**

**Add to PasteRestoreDialog:**
- File update preview list
- "Apply to Files" button
- Progress indicator
- Success/error messages

---

## 🎯 REALISTIC TIMELINE

**Full Implementation:**
- AI Response Parser: 2 hours (with tests)
- File Writer Service: 2 hours (with tests)
- Diff Generator: 1 hour (optional, for preview)
- UI Integration: 1 hour
- Integration Testing: 1 hour
- **Total: ~7 hours** for complete feature

---

## ✅ WHAT YOU'LL GET

### **After Implementation:**

**User Experience:**
1. ✅ Copy files from ShieldPrompt
2. ✅ Paste in ChatGPT, get improvements
3. ✅ Paste ChatGPT response back
4. ✅ See preview: "Will update Program.cs (15 lines)"
5. ✅ Click "Apply to Files"
6. ✅ **FILES AUTOMATICALLY UPDATED!** ✨
7. ✅ Undo anytime (Ctrl+Z restores from backup)

**Safety:**
- ✅ Automatic backups before ANY change
- ✅ Preview all changes before applying
- ✅ Atomic operations (all or none)
- ✅ Full undo support
- ✅ No file corruption risk

---

## 🚨 CRITICAL DECISIONS

### **Decision 1: How to Map AI Response to Files?**

**Recommended:** Hybrid approach
1. Try to parse file names from headers (`**Program.cs**`)
2. Try to parse from comments (`// File: Program.cs`)
3. Fallback: Map to original files in order
4. If ambiguous: Ask user to confirm

### **Decision 2: What if AI Returns Different Number of Files?**

**Scenarios:**
- AI combines 3 files into 1 → **Ask user which file to update**
- AI splits 1 file into 3 → **Ask user where to save new files**
- AI suggests deleting file → **Require explicit confirmation**

### **Decision 3: How to Handle Deletions?**

**Recommended:** Explicit confirmation
- Never auto-delete
- Show warning dialog
- Require user to check "I understand this will delete X files"
- Move to backup instead of permanent delete

---

## 🧪 TEST COVERAGE REQUIREMENTS

### **AI Response Parser Tests: ~15 tests**
- ChatGPT format (with **headers**)
- Claude format (with **bold**)
- Comment format (`// File:`)
- Multiple files
- Single file
- No file markers
- Mixed formats
- New file detection
- Delete suggestion detection

### **File Writer Tests: ~12 tests**
- Create backup before write
- Update single file
- Update multiple files
- Create new file
- Create directory if missing
- Rollback on error
- Restore from backup
- Atomic operations
- Path validation (security)

### **Integration Tests: ~5 tests**
- Complete workflow (copy → ChatGPT → paste → apply)
- Multi-file update
- New file creation
- Undo file updates
- Error recovery

**Total: ~32 new tests**

---

## 💡 RECOMMENDED APPROACH

### **Start Simple, Iterate:**

**v1.1 - Basic File Update (Quick Win - 4 hours):**
- Parse single file responses
- Update that one file
- With backup
- Simple preview

**v1.2 - Multi-File Support (Medium - 3 hours):**
- Parse multiple files
- Smart file name detection
- Update all at once

**v2.0 - Full Automation (Future - 5 hours):**
- Create/delete files
- Directory manipulation
- Advanced diff viewer
- Conflict resolution

---

## 🎯 IMMEDIATE NEXT STEPS (If You Approve)

1. **Create IAIResponseParser interface + tests** (TDD RED)
2. **Implement simple parser** (ChatGPT markdown format)
3. **Create IFileWriterService interface + tests** (TDD RED)
4. **Implement file writer with backups**
5. **Add "Apply to Files" button to dialog**
6. **Wire up the command**
7. **Test end-to-end with tutorial project**

**Estimated time:** 4 hours for MVP (single file support)

---

## 🎁 VALUE PROPOSITION

**This completes the loop!** ShieldPrompt becomes:

**Before:** Copy/paste helper with sanitization  
**After:** **Complete AI-assisted development workflow!**

**User never leaves ShieldPrompt:**
1. Select files
2. Copy → ChatGPT
3. Paste back
4. **Files automatically updated!** ✨

**This is a KILLER FEATURE!** 🚀

---

**Ready to implement?** Say the word and I'll start with TDD!


