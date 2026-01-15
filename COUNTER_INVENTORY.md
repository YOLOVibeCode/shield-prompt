# Counter Inventory & Testing Plan

**Date:** January 14, 2026  
**Purpose:** Comprehensive audit of all counters in ShieldPrompt

---

## 📊 COUNTER INVENTORY

### **1. SelectedFileCount** (MainWindowViewModel)
**Location:** Status bar (bottom right)  
**Display:** `"{SelectedFileCount} files"`  
**Updates When:**
- User checks/unchecks files in tree
- SelectAll/DeselectAll commands
- Loading folder with previous selection
- Undo/Redo file selection actions

**Current Update Location:**
- `CopyToClipboardAsync()` - Line 421

**Issue:** ❌ Only updates on copy, not on file selection change!

---

### **2. TotalTokens** (MainWindowViewModel)
**Location:** Status bar (bottom right)  
**Display:** `"{TotalTokens:N0} tokens"`  
**Updates When:**
- After copying to clipboard
- Should update: when file selection changes (real-time)

**Current Update Location:**
- `CopyToClipboardAsync()` - Line 440 (after sanitization)

**Issue:** ❌ Only updates on copy, not live!

---

### **3. SanitizedValueCount** (MainWindowViewModel)
**Location:** Status bar (bottom right)  
**Display:** `"🔐 {SanitizedValueCount} masked"`  
**Updates When:**
- After copying (post-sanitization)
- Clear session command

**Current Update Location:**
- `CopyToClipboardAsync()` - Line 441
- `ClearSession()` - Line 580

**Status:** ✅ Correct (only relevant after copy)

---

### **4. TokenCount per File** (FileNodeViewModel)
**Location:** File tree, next to each file name  
**Display:** `"({TokenCount} tok)"`  
**Updates When:**
- Background task after loading folder
- Should update: when file content changes

**Current Update Location:**
- `UpdateTokenCountsAsync()` - Line 386 (background task)

**Status:** ⚠️ Runs once on load, doesn't update if files change

---

### **5. File Count in Status** (Computed)
**Location:** Status message after loading  
**Display:** `"Loaded {count} files from {folder}"`  
**Updates When:**
- After loading folder

**Current Update Location:**
- `LoadFolderAsync()` - Line 349

**Status:** ✅ Correct (one-time message)

---

## ❌ IDENTIFIED ISSUES

### **Issue #1: SelectedFileCount Not Live**
**Problem:**
- Counter only updates on copy
- User checks 3 files → still shows "0 files"
- Must copy to see count update

**Expected Behavior:**
- Check a file → immediately shows "1 file"
- Check another → shows "2 files"
- Uncheck → decrements

**Root Cause:**
- No PropertyChanged handler on FileNodeViewModel.IsSelected
- Counter not calculated on selection changes

---

### **Issue #2: TotalTokens Not Live**
**Problem:**
- Token count only updates after copy
- User selects files → doesn't see estimated tokens
- Can't preview if selection exceeds model limit

**Expected Behavior:**
- Select files → token count updates (aggregated)
- Deselect files → token count decrements
- Live feedback on context limits

**Root Cause:**
- Token counting only happens in CopyToClipboardAsync
- Not triggered on selection changes

---

### **Issue #3: Per-File Token Counts Static**
**Problem:**
- Counts calculated once on folder load
- If file changes on disk → count doesn't update
- No refresh mechanism

**Expected Behavior:**
- Background update of token counts
- Refresh on F5 recalculates
- File watcher updates counts (future)

**Root Cause:**
- Single background task on load
- No file watching
- No refresh invalidation

---

## ✅ RECOMMENDED FIXES

### **Fix #1: Make SelectedFileCount Live**

```csharp
// In MainWindowViewModel constructor
public MainWindowViewModel(...)
{
    // ... existing code ...
    
    // Subscribe to property changes
    this.PropertyChanged += OnPropertyChanged;
}

private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(RootNodeViewModel))
    {
        SubscribeToFileSelectionChanges();
    }
}

private void SubscribeToFileSelectionChanges()
{
    if (RootNodeViewModel == null) return;
    
    SubscribeToNode(RootNodeViewModel);
}

private void SubscribeToNode(FileNodeViewModel node)
{
    node.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(FileNodeViewModel.IsSelected))
        {
            UpdateSelectedFileCount();
        }
    };
    
    foreach (var child in node.Children)
    {
        SubscribeToNode(child);
    }
}

private void UpdateSelectedFileCount()
{
    if (RootNodeViewModel == null)
    {
        SelectedFileCount = 0;
        return;
    }
    
    SelectedFileCount = CountSelectedFiles(RootNodeViewModel);
}

private int CountSelectedFiles(FileNodeViewModel node)
{
    var count = (!node.IsDirectory && node.IsSelected) ? 1 : 0;
    foreach (var child in node.Children)
    {
        count += CountSelectedFiles(child);
    }
    return count;
}
```

---

### **Fix #2: Make TotalTokens Live (Optional)**

**Note:** This is computationally expensive (re-aggregating and counting on every selection change).

**Alternative:** Show estimated tokens based on per-file counts:

```csharp
private void UpdateEstimatedTokens()
{
    if (RootNodeViewModel == null)
    {
        TotalTokens = 0;
        return;
    }
    
    // Sum token counts of selected files
    TotalTokens = SumSelectedTokens(RootNodeViewModel);
}

private int SumSelectedTokens(FileNodeViewModel node)
{
    var sum = (!node.IsDirectory && node.IsSelected) ? node.TokenCount : 0;
    foreach (var child in node.Children)
    {
        sum += SumSelectedTokens(child);
    }
    return sum;
}
```

**Trade-off:**
- ✅ Fast (no re-aggregation)
- ⚠️ Approximate (doesn't include formatting overhead)
- ⚠️ Shows "~2,381 tokens" instead of exact

---

### **Fix #3: Refresh Token Counts**

```csharp
[RelayCommand]
private async Task RefreshFolderAsync()
{
    if (RootNode == null) return;
    
    // Re-load current folder (refreshes file list + token counts)
    await LoadFolderAsync(RootNode.Path);
}
```

Already works via F5/Ctrl+R → OpenFolderCommand!

---

## 🧪 REQUIRED TESTS

### **Test Suite: MainWindowViewModelCounterTests.cs**

```csharp
public class MainWindowViewModelCounterTests
{
    [Fact]
    public void SelectedFileCount_WhenFileSelected_ShouldUpdate()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadSampleFiles();
        
        // Act
        vm.RootNodeViewModel.Children[0].IsSelected = true;
        
        // Assert
        vm.SelectedFileCount.Should().Be(1);
    }
    
    [Fact]
    public void SelectedFileCount_WhenFileDeselected_ShouldDecrement()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadSampleFiles();
        vm.RootNodeViewModel.Children[0].IsSelected = true;
        vm.RootNodeViewModel.Children[1].IsSelected = true;
        
        // Act
        vm.RootNodeViewModel.Children[0].IsSelected = false;
        
        // Assert
        vm.SelectedFileCount.Should().Be(1);
    }
    
    [Fact]
    public void SelectedFileCount_WhenSelectAll_ShouldCountAllFiles()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadSampleFiles(); // Has 5 files
        
        // Act
        vm.SelectAllCommand.Execute(null);
        
        // Assert
        vm.SelectedFileCount.Should().Be(5);
    }
    
    [Fact]
    public void SelectedFileCount_WhenDeselectAll_ShouldBeZero()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadSampleFiles();
        vm.SelectAllCommand.Execute(null);
        
        // Act
        vm.DeselectAllCommand.Execute(null);
        
        // Assert
        vm.SelectedFileCount.Should().Be(0);
    }
    
    [Fact]
    public async Task TotalTokens_AfterCopy_ShouldMatchAggregatedContent()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadSampleFiles();
        vm.RootNodeViewModel.Children[0].IsSelected = true;
        
        // Act
        await vm.CopyToClipboardCommand.ExecuteAsync(null);
        
        // Assert
        vm.TotalTokens.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task SanitizedValueCount_WhenContentHasSecrets_ShouldBeGreaterThanZero()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadTutorialProject(); // Has 34 secrets
        vm.SelectAllCommand.Execute(null);
        
        // Act
        await vm.CopyToClipboardCommand.ExecuteAsync(null);
        
        // Assert
        vm.SanitizedValueCount.Should().BeGreaterThan(30);
    }
    
    [Fact]
    public async Task SanitizedValueCount_WhenClearSession_ShouldReset()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadTutorialProject();
        vm.SelectAllCommand.Execute(null);
        await vm.CopyToClipboardCommand.ExecuteAsync(null);
        
        // Pre-condition
        vm.SanitizedValueCount.Should().BeGreaterThan(0);
        
        // Act
        vm.ClearSessionCommand.Execute(null);
        
        // Assert
        vm.SanitizedValueCount.Should().Be(0);
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public void SelectedFileCount_WithVariousSelections_ShouldMatchActual(
        int filesToSelect, int expected)
    {
        // Arrange
        var vm = CreateViewModel();
        vm.LoadSampleFiles(); // Has 5 files
        
        // Act
        for (int i = 0; i < filesToSelect; i++)
        {
            vm.GetFileAtIndex(i).IsSelected = true;
        }
        
        // Assert
        vm.SelectedFileCount.Should().Be(expected);
    }
}
```

---

## 📋 TEST CHECKLIST

### **Counter Update Scenarios:**
- [ ] Check single file → SelectedFileCount = 1
- [ ] Check multiple files → SelectedFileCount increments
- [ ] Uncheck file → SelectedFileCount decrements
- [ ] SelectAll → SelectedFileCount = total file count
- [ ] DeselectAll → SelectedFileCount = 0
- [ ] Copy files → TotalTokens updates
- [ ] Copy with secrets → SanitizedValueCount > 0
- [ ] Copy without secrets → SanitizedValueCount = 0
- [ ] Clear session → SanitizedValueCount = 0
- [ ] Undo selection → SelectedFileCount reverts
- [ ] Redo selection → SelectedFileCount forward

### **Edge Cases:**
- [ ] Select folder (should select all children)
- [ ] Deselect folder (should deselect all children)
- [ ] Mixed selection (some checked, some not)
- [ ] Empty folder → all counts = 0
- [ ] Large file selection → token count accuracy
- [ ] Binary files → excluded from counts

---

## 🎯 IMMEDIATE ACTIONS

1. **Create comprehensive counter tests**
2. **Fix SelectedFileCount to be live** (not just on copy)
3. **Optionally make TotalTokens live** (estimated)
4. **Ensure all tests pass**

---

**Next: Implement fixes and tests**


