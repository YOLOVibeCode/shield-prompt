# File Manipulation Workflow - Analysis & Design

**Date:** January 14, 2026  
**Status:** ANALYSIS ONLY (Not Yet Implemented)

---

## 🔍 CURRENT STATE ASSESSMENT

### **What EXISTS:**
- ✅ Paste AI response into dialog
- ✅ Desanitize (restore aliases → original values)
- ✅ Preview restored content
- ✅ Copy restored content to clipboard

### **What's MISSING:**
- ❌ **No "Apply to Files" functionality**
- ❌ No file writer service
- ❌ No AI response parser (extract file paths + code)
- ❌ No file diff/merge logic
- ❌ No directory creation/deletion
- ❌ No backup mechanism

**Current workflow is manual:**
1. User copies restored text to clipboard
2. User manually pastes into their IDE
3. IDE handles file updates

---

## 🎯 DESIRED WORKFLOW

### **Ideal User Experience:**

```
1. User copies code from ChatGPT
2. User pastes in ShieldPrompt (Ctrl+V)
3. ShieldPrompt shows:
   - Which files will be modified
   - Which files will be created
   - Which files will be deleted
   - Preview of all changes
4. User clicks "Apply to Files"
5. ShieldPrompt:
   - Creates backups
   - Updates/creates/deletes files
   - Shows success confirmation
6. User's files are updated automatically!
```

---

## 🏗️ REQUIRED ARCHITECTURE

### **1. AI Response Parser** (NEW)

**Interface:** `IAIResponseParser`

```csharp
public interface IAIResponseParser
{
    /// <summary>
    /// Parses AI response to extract file operations.
    /// </summary>
    Task<ParsedResponse> ParseAsync(string aiResponse);
}

public record ParsedResponse(
    IReadOnlyList<FileOperation> Operations,
    string OriginalContent);

public record FileOperation(
    FileOperationType Type,
    string FilePath,
    string? Content,
    string? Reason);

public enum FileOperationType
{
    Create,
    Update,
    Delete,
    CreateDirectory,
    DeleteDirectory
}
```

**Challenges:**
- AI responses vary (ChatGPT vs Claude formatting)
- Need to detect file boundaries
- Handle markdown code blocks
- Extract file paths from context
- Distinguish create vs update

---

### **2. File Manipulation Service** (NEW)

**Interface:** `IFileManipulationService`

```csharp
public interface IFileManipulationService
{
    /// <summary>
    /// Applies file operations with safety checks.
    /// </summary>
    Task<FileManipulationResult> ApplyOperationsAsync(
        IEnumerable<FileOperation> operations,
        string baseDirectory,
        FileManipulationOptions options);
    
    /// <summary>
    /// Creates backup before applying changes.
    /// </summary>
    Task<string> CreateBackupAsync(IEnumerable<string> filePaths);
    
    /// <summary>
    /// Restores from backup.
    /// </summary>
    Task RestoreBackupAsync(string backupId);
}

public record FileManipulationResult(
    int FilesCreated,
    int FilesUpdated,
    int FilesDeleted,
    int DirectoriesCreated,
    int DirectoriesDeleted,
    string? BackupId,
    IReadOnlyList<string> Errors);

public class FileManipulationOptions
{
    public bool CreateBackup { get; init; } = true;
    public bool AllowDelete { get; init; } = false;
    public bool AllowCreateDirectories { get; init; } = true;
    public bool DryRun { get; init; } = false;
}
```

---

### **3. Enhanced PasteRestoreViewModel** (UPDATE)

**New Commands:**
```csharp
[RelayCommand]
private async Task ApplyToFilesAsync()
{
    // 1. Parse AI response to detect file operations
    var parsed = await _aiResponseParser.ParseAsync(RestoredContent);
    
    // 2. Show preview of changes
    ShowFileChangePreview(parsed.Operations);
    
    // 3. Confirm with user
    if (!await ConfirmChangesAsync(parsed.Operations))
        return;
    
    // 4. Create backup
    var backupId = await _fileManipulation.CreateBackupAsync(...);
    
    // 5. Apply changes
    var result = await _fileManipulation.ApplyOperationsAsync(
        parsed.Operations,
        _baseDirectory,
        new FileManipulationOptions { CreateBackup = true });
    
    // 6. Show results
    StatusMessage = $"✅ Applied: {result.FilesUpdated} updated, {result.FilesCreated} created";
}
```

---

## 🚨 CRITICAL ISSUES TO SOLVE

### **Issue #1: Detecting File Boundaries in AI Response**

**Challenge:** AI responses vary wildly:

```
ChatGPT format:
---
Here's the updated code:

```csharp
// Program.cs
public class Program { ... }
```

```csharp
// Database.cs  
public class Database { ... }
```
---

Claude format:
---
I'll update these files:

**Program.cs**
```csharp
public class Program { ... }
```

**Database.cs**
```csharp
public class Database { ... }
```
---
```

**Solution Needed:**
- Heuristic parsing (detect patterns)
- File path extraction from comments/headers
- Code block detection (```)
- Fallback to manual file selection

---

### **Issue #2: Knowing Which Files to Update**

**Challenge:**
- AI might return full file content
- Or just changed sections
- Or multiple files
- User didn't tell AI which files to modify

**Solution Options:**

**A) Match Original Files (Recommended)**
- Remember which files were sent to AI
- Only update those files
- Safer, more predictable

**B) Parse File Paths from Response**
- Look for `// Filename:` comments
- Extract from markdown headers
- Detect file extensions in code blocks

**C) User Selects Manually**
- Show detected code blocks
- User maps to files
- Most control, most manual

---

### **Issue #3: Safety & Backups**

**Must Have:**
- ✅ Create backup before ANY file modification
- ✅ Undo capability (restore from backup)
- ✅ Dry-run mode (preview without applying)
- ✅ Confirmation dialog (show what will change)
- ✅ Fail-safe (on error, restore backup)

**Backup Strategy:**
```
~/.shieldprompt/backups/
└── {timestamp}-{guid}/
    ├── manifest.json          (which files, why, when)
    ├── src/
    │   ├── Program.cs.bak
    │   └── Database.cs.bak
    └── config/
        └── appsettings.json.bak
```

---

## 📋 RECOMMENDED APPROACH (Phased)

### **Phase 1: Simple File Writer (MVP)**

**Scope:**
- User manually selects which file to update
- No automatic parsing
- Single file at a time
- With backup

**Implementation:**
```csharp
[RelayCommand]
private async Task ApplyToSingleFileAsync(string filePath)
{
    // 1. Create backup
    await _fileManipulation.BackupFileAsync(filePath);
    
    // 2. Write restored content
    await File.WriteAllTextAsync(filePath, RestoredContent);
    
    // 3. Success
    StatusMessage = $"✅ Updated {Path.GetFileName(filePath)}";
}
```

**Pros:**
- ✅ Simple, safe, works immediately
- ✅ User controls which file
- ✅ No complex parsing

**Cons:**
- Manual file selection per update
- Not fully automated

---

### **Phase 2: Multi-File Parser (Advanced)**

**Scope:**
- Parse AI response for multiple files
- Detect file paths from comments/headers
- Apply to multiple files at once
- With preview and confirmation

**Complexity:** HIGH
- AI response format detection
- File path extraction
- Code block parsing
- Multi-file diff preview

---

### **Phase 3: Full Automation (Future)**

**Scope:**
- Remember which files were sent
- Automatically map AI response to original files
- Smart merge (only changed sections)
- Full undo/redo integration

---

## 🧪 REQUIRED TESTS

### **FileManipulationService Tests:**

```csharp
[Fact]
public async Task WriteFile_ShouldCreateBackup_BeforeWriting()

[Fact]
public async Task WriteFile_WhenDirectoryMissing_ShouldCreateIt()

[Fact]
public async Task WriteFile_OnError_ShouldRestoreBackup()

[Fact]
public async Task DeleteFile_ShouldMoveToBackup_NotPermanentDelete()

[Fact]
public async Task ApplyOperations_WithMultipleFiles_ShouldApplyAllOrNone()

[Fact]
public async Task RestoreBackup_ShouldRestoreAllFiles_ToOriginalState()

[Fact]
public async Task CreateDirectory_WhenParentMissing_ShouldCreateParents()

[Fact]
public async Task DeleteDirectory_WhenNotEmpty_ShouldFail_UnlessForced()
```

### **AIResponseParser Tests:**

```csharp
[Fact]
public async Task Parse_ChatGPTFormat_ShouldExtractFileAndContent()

[Fact]
public async Task Parse_ClaudeFormat_ShouldExtractFileAndContent()

[Fact]
public async Task Parse_MultipleFiles_ShouldReturnAllOperations()

[Fact]
public async Task Parse_NoFileMarkers_ShouldReturnSingleOperation()
```

---

## ⚠️ CRITICAL CONSIDERATIONS

### **Security:**
- ✅ **Never delete without backup**
- ✅ **Validate file paths** (prevent directory traversal)
- ✅ **Confirm destructive operations**
- ✅ **Atomic operations** (all or none)

### **UX:**
- ✅ **Clear preview** of all changes
- ✅ **One-click undo**
- ✅ **Progress indicator** for large operations
- ✅ **Error recovery** with helpful messages

### **Complexity:**
- ⚠️ **Don't over-engineer** for v1.x
- ⚠️ **Start simple** (manual file selection)
- ⚠️ **Iterate based on user feedback**

---

## 🎯 IMMEDIATE RECOMMENDATION

**For v1.x (Keep It Simple):**

**Current workflow is SAFE and WORKS:**
1. User copies restored content to clipboard
2. User pastes in their IDE
3. IDE handles file updates (with its own undo)

**Benefits:**
- ✅ Users stay in control
- ✅ Use familiar IDE tools
- ✅ IDE undo/redo works
- ✅ No risk of ShieldPrompt corrupting files

**Enhancements (Low Risk):**
- Add "Open in Editor" button (launches default editor)
- Add "Save As..." to save restored content to specific file
- Keep full automation for v2.0

---

## 📝 PROPOSED: Simple "Save As" Feature (30 minutes)

**Add to PasteRestoreDialog:**

```csharp
[RelayCommand]
private async Task SaveRestoredContentAsync()
{
    var dialog = new SaveFileDialog
    {
        Title = "Save Restored Content",
        DefaultExtension = ".cs"
    };
    
    var filePath = await dialog.ShowAsync();
    if (string.IsNullOrEmpty(filePath))
        return;
    
    // Create backup if file exists
    if (File.Exists(filePath))
    {
        File.Copy(filePath, filePath + ".bak", overwrite: true);
    }
    
    // Write restored content
    await File.WriteAllTextAsync(filePath, RestoredContent);
    
    StatusMessage = $"✅ Saved to {Path.GetFileName(filePath)}";
}
```

**Button in UI:**
```xml
<Button Content="💾 Save As..." Command="{Binding SaveRestoredContentCommand}"/>
```

**Benefits:**
- ✅ Simple, safe, quick to implement
- ✅ User chooses file location
- ✅ Creates backup automatically
- ✅ Works with single-file AI responses

---

## 🎯 RECOMMENDATIONS

### **For Immediate Testing (v1.x):**
1. ✅ **Current workflow works** - Test manually:
   - Copy tutorial files
   - Paste in ChatGPT
   - Get AI response
   - Paste in ShieldPrompt
   - Copy restored
   - Paste in IDE
   - ✅ Verify desanitization works

2. ✅ **Add "Save As" feature** - Quick win
   - Allows saving to specific file
   - With backup
   - Low risk

### **For Future (v2.0):**
3. ⏭️ **Full automation** - Design carefully:
   - AI response parser
   - Multi-file diff viewer
   - Atomic file operations
   - Full undo integration

---

**NEXT: Let's test the CURRENT desanitization workflow thoroughly!**

