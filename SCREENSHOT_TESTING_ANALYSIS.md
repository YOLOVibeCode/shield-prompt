# Screenshot Testing & Tutorial Analysis

**Date:** January 14, 2026  
**Goal:** Create automated screenshots + interactive tutorial for user guide  
**Status:** Analysis & Recommendations

---

## 🎯 REQUIREMENTS (From User)

1. **Avalonia screenshot testing capabilities** - Automated UI capture
2. **Sample data provider** - Dead easy sample pages  
3. **Walkthrough tutorial** - Interactive guide when users clone repo

---

## 📚 AVALONIA HEADLESS TESTING (Research)

### **Available Technology:**

#### **Avalonia.Headless** (Official Package)
- **Purpose:** Run Avalonia UI tests without a window/display
- **Use Case:** Automated testing, screenshot generation in CI/CD
- **Availability:** NuGet package `Avalonia.Headless`
- **Integration:** Works with xUnit, NUnit

#### **Avalonia.Headless.XUnit**
- **Purpose:** xUnit integration for headless tests
- **Features:** 
  - Render UI to bitmap
  - Simulate user interactions
  - Capture screenshots
  - No window manager needed

---

## 🎨 SCREENSHOT AUTOMATION APPROACH

### **Recommended Architecture:**

```
tests/
└── ShieldPrompt.Tests.Screenshots/          # NEW project
    ├── ShieldPrompt.Tests.Screenshots.csproj
    ├── Fixtures/
    │   └── SampleProjectFixture.cs           # Provides sample data
    ├── Scenarios/
    │   ├── 01_Installation_Screenshots.cs    # Installation flow
    │   ├── 02_FirstLaunch_Screenshots.cs     # First-time setup
    │   ├── 03_BasicWorkflow_Screenshots.cs   # Copy workflow
    │   ├── 04_ShieldPreview_Screenshots.cs   # Shield panel
    │   └── 05_PasteRestore_Screenshots.cs    # Restore dialog
    ├── Helpers/
    │   └── ScreenshotHelper.cs               # Capture & save utilities
    └── SampleData/
        └── tutorial-project/                 # Sample codebase
            ├── Program.cs                    # With fake secrets
            ├── Database.cs                   # With fake DB names
            └── Config.json                   # With fake IPs
```

---

## 🎮 INTERACTIVE TUTORIAL APPROACH

### **Option A: Tutorial Mode in App (Recommended)**

**Implementation:**
```csharp
// In MainWindowViewModel
public bool IsTutorialMode { get; set; }

[RelayCommand]
private async Task StartTutorialAsync()
{
    // 1. Load bundled sample project
    var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "tutorial-project");
    
    // 2. Show tutorial overlay
    IsTutorialMode = true;
    
    // 3. Step-by-step guided tour
    await ShowTutorialStep("Welcome! Let's learn ShieldPrompt in 2 minutes...");
    await ShowTutorialStep("Step 1: This is the file tree. Check files you want to share.");
    await ShowTutorialStep("Step 2: Click Copy. Notice the status bar shows masked values.");
    // ... etc.
}
```

**Features:**
- ✅ Interactive overlay with highlights
- ✅ Click "Next" to progress through steps
- ✅ Uses real UI with sample data
- ✅ User learns by doing
- ✅ No separate documentation needed

**User Experience:**
```
First Launch → "🎓 Want a 2-minute tutorial?" 
               [Yes, teach me!] [Skip]
               
Tutorial → Highlights areas, explains features, guides user
           [Previous] [Next] [Skip Tutorial]
           
End → "🎉 Tutorial complete! Now try with your own code!"
```

---

### **Option B: Sample Project with README (Simpler)**

**Implementation:**
```
samples/
└── tutorial-project/
    ├── README.md                  # Step-by-step walkthrough
    ├── src/
    │   ├── Program.cs             # Contains fake secrets
    │   ├── Database.cs            # Fake DB names
    │   ├── ApiClient.cs           # Fake API keys
    │   └── Config.json            # Fake credentials
    └── expected-output/
        ├── step1-sanitized.txt    # What ChatGPT should see
        └── step2-restored.txt     # Final result
```

**README.md content:**
```markdown
# ShieldPrompt Tutorial Project

## Step 1: Open This Folder
1. Launch ShieldPrompt
2. Click "Open Folder"
3. Select `samples/tutorial-project`

## Step 2: Select Files
Check these files:
- [x] Program.cs
- [x] Database.cs
- [x] ApiClient.cs

## Step 3: Click Shield Preview 🛡️
You should see:
- ProductionDB → DATABASE_0
- 192.168.1.50 → IP_ADDRESS_0
- AKIAIOSFODNN7 → AWS_KEY_0

## Step 4: Copy
Press Ctrl+C. Check status bar.

## Step 5: Open ChatGPT
Paste the content. Notice all secrets are safe!

## Step 6: (Simulated) Paste AI Response
Copy the content from `expected-output/step1-sanitized.txt`
Paste into "Paste & Restore" dialog
See the restoration preview

## Step 7: Copy Restored
Click "Copy Restored"
Compare with `expected-output/step2-restored.txt`

✅ Tutorial complete!
```

**Pros:**
- ✅ Simple to implement (no UI changes)
- ✅ Works with current codebase
- ✅ Educational sample data
- ✅ Can verify expected output

**Cons:**
- Requires users to read and follow manually
- Not as interactive

---

## 📸 SCREENSHOT GENERATION RECOMMENDATIONS

### **Recommended Stack:**

```xml
<!-- Add to tests/ShieldPrompt.Tests.Screenshots/ShieldPrompt.Tests.Screenshots.csproj -->
<ItemGroup>
  <PackageReference Include="Avalonia.Headless.XUnit" Version="11.3.11" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.11" />
  <PackageReference Include="SkiaSharp" Version="2.88.7" />
  <PackageReference Include="xunit" Version="2.9.3" />
</ItemGroup>
```

### **Screenshot Test Example:**

```csharp
// tests/ShieldPrompt.Tests.Screenshots/Scenarios/01_FirstLaunch_Screenshots.cs
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Xunit;

[AvaloniaFact]
public async Task Capture_FirstLaunch_Screenshot()
{
    // Arrange - Create window in headless mode
    var window = new MainWindow();
    window.Width = 1280;
    window.Height = 800;
    
    // Load sample data
    var viewModel = window.DataContext as MainWindowViewModel;
    await viewModel.LoadSampleProjectAsync();
    
    // Act - Render and capture
    var bitmap = window.CaptureRenderedFrame();
    
    // Save
    bitmap.Save("docs/images/user-guide/01-first-launch.png");
    
    // Assert (optional)
    Assert.NotNull(bitmap);
}

[AvaloniaFact]
public async Task Capture_FileSelection_Screenshot()
{
    var window = new MainWindow();
    var vm = window.DataContext as MainWindowViewModel;
    
    // Arrange - Load sample, select specific files
    await vm.LoadSampleProjectAsync();
    vm.FileNodes[0].IsSelected = true;
    vm.FileNodes[1].IsSelected = true;
    
    // Capture
    var bitmap = window.CaptureRenderedFrame();
    bitmap.Save("docs/images/user-guide/03-file-selection.png");
}

[AvaloniaFact]
public async Task Capture_ShieldPreview_Screenshot()
{
    var window = new MainWindow();
    var vm = window.DataContext as MainWindowViewModel;
    
    // Arrange
    await vm.LoadSampleProjectAsync();
    vm.FileNodes[0].IsSelected = true;
    await vm.ShowShieldPreviewAsync(); // Expand shield panel
    
    // Capture
    var bitmap = window.CaptureRenderedFrame();
    bitmap.Save("docs/images/user-guide/07-shield-preview.png");
}
```

---

## 📦 SAMPLE PROJECT DATA PROVIDER

### **Recommended Structure:**

```csharp
// tests/ShieldPrompt.Tests.Screenshots/Fixtures/SampleProjectFixture.cs
public class SampleProjectFixture
{
    public static string CreateTutorialProject()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "ShieldPrompt-Tutorial");
        
        // Create realistic sample files with OBVIOUS fake data
        File.WriteAllText(Path.Combine(tempPath, "Program.cs"), @"
public class Program
{
    // TUTORIAL: These are FAKE secrets for learning - not real!
    private static string DatabaseName = ""ProductionDB"";
    private static string ServerIP = ""192.168.1.50"";
    private static string ApiKey = ""AKIAIOSFODNN7EXAMPLE"";
    private static string AdminSSN = ""123-45-6789"";
    
    public static void Main()
    {
        Console.WriteLine(""Connected to "" + DatabaseName);
    }
}
");
        
        File.WriteAllText(Path.Combine(tempPath, "Database.cs"), @"
public class Database
{
    public string ConnectionString => 
        $""Server={ServerIP};Database={DatabaseName};User=admin;Password=Demo123"";
}
");
        
        return tempPath;
    }
}
```

### **Usage in ViewModel:**

```csharp
// src/ShieldPrompt.App/ViewModels/MainWindowViewModel.cs
[RelayCommand]
private async Task LoadTutorialProjectAsync()
{
    // Check if samples/ folder exists
    var samplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "tutorial-project");
    
    if (Directory.Exists(samplesPath))
    {
        await LoadFolderAsync(samplesPath);
        StatusMessage = "📚 Tutorial project loaded! Follow the steps below.";
        
        // Auto-select recommended files
        foreach (var node in FileNodes.Where(n => n.Name.EndsWith(".cs")))
        {
            node.IsSelected = true;
        }
    }
    else
    {
        StatusMessage = "⚠️ Tutorial project not found. Clone from GitHub to get samples.";
    }
}
```

---

## 🎓 INTERACTIVE TUTORIAL RECOMMENDATIONS

### **Approach 1: In-App Tutorial Overlay (Best UX)**

**Implementation Pattern:**
```csharp
public class TutorialStep
{
    public string Title { get; init; }
    public string Description { get; init; }
    public string? HighlightElementId { get; init; }  // Which UI element to highlight
    public Action? AutoAction { get; init; }          // Optional auto-action
}

public class TutorialManager
{
    private List<TutorialStep> _steps = new()
    {
        new() { 
            Title = "Welcome to ShieldPrompt!",
            Description = "Let's learn how to safely share code with AI in 2 minutes.",
            HighlightElementId = null
        },
        new() { 
            Title = "Step 1: File Selection",
            Description = "Check the boxes next to files you want to share with ChatGPT.",
            HighlightElementId = "FileTreeView",
            AutoAction = () => { /* Auto-select sample files */ }
        },
        new() { 
            Title = "Step 2: See Protection",
            Description = "Click the Shield button to see what will be protected.",
            HighlightElementId = "ShieldButton"
        },
        // ... more steps
    };
}
```

**UI Overlay:**
```xml
<!-- Semi-transparent overlay with tutorial callout -->
<Border Background="#DD000000" IsVisible="{Binding IsTutorialActive}">
  <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center"
              Background="White" Padding="40" CornerRadius="8">
    <TextBlock Text="{Binding TutorialStep.Title}" FontSize="24" FontWeight="Bold"/>
    <TextBlock Text="{Binding TutorialStep.Description}" 
               TextWrapping="Wrap" MaxWidth="600" Margin="0,20"/>
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
      <Button Content="Previous" Command="{Binding PreviousTutorialStepCommand}"/>
      <Button Content="Next" Command="{Binding NextTutorialStepCommand}"/>
      <Button Content="Skip Tutorial" Command="{Binding SkipTutorialCommand}"/>
    </StackPanel>
  </StackPanel>
</Border>
```

---

### **Approach 2: Sample Project with Guided README (Simpler)**

**Folder Structure:**
```
samples/
└── tutorial-project/
    ├── README_TUTORIAL.md         # Step-by-step guide
    ├── src/
    │   ├── Program.cs             # Fake secrets
    │   ├── Database.cs            # Fake DB config
    │   └── ApiClient.cs           # Fake API keys
    ├── screenshots/               # Pre-captured screenshots
    │   ├── step1.png
    │   ├── step2.png
    │   └── step3.png
    └── expected-outputs/
        ├── sanitized.txt          # What ChatGPT should see
        └── restored.txt           # Final result
```

**Benefits:**
- ✅ Works immediately when cloned
- ✅ No code changes needed
- ✅ Easy to maintain
- ✅ Can include screenshots in sample folder itself

---

## 🔧 IMPLEMENTATION RECOMMENDATIONS

### **Phase 1: Sample Tutorial Project (Quick Win - 30 minutes)**

**Create:**
1. `samples/tutorial-project/` - Sample codebase with fake secrets
2. `samples/tutorial-project/README_TUTORIAL.md` - Step-by-step walkthrough
3. `samples/tutorial-project/EXPECTED.md` - What you should see at each step

**Benefits:**
- ✅ Users can try immediately
- ✅ No UI changes needed
- ✅ Works with current app
- ✅ Educational fake data

**Sample fake data:**
```csharp
// samples/tutorial-project/src/Program.cs
public class Program
{
    // 🎓 TUTORIAL NOTE: These are FAKE credentials for learning!
    // Real ShieldPrompt will detect and protect these patterns.
    
    private const string DbName = "ProductionDB";              // Will become DATABASE_0
    private const string ServerIp = "192.168.1.50";           // Will become IP_ADDRESS_0
    private const string AwsKey = "AKIAIOSFODNN7EXAMPLE";     // Will become AWS_KEY_0
    private const string SampleSsn = "123-45-6789";           // Will become SSN_0
    
    public static void Main()
    {
        Console.WriteLine($"Connecting to {DbName} at {ServerIp}");
        Console.WriteLine($"Using AWS key: {AwsKey}");
    }
}
```

---

### **Phase 2: Automated Screenshots (Medium - 2 hours)**

**Setup:**
1. Create `tests/ShieldPrompt.Tests.Screenshots/` project
2. Add `Avalonia.Headless.XUnit` package
3. Create screenshot test fixtures
4. Run tests to generate images → `docs/images/user-guide/`

**Benefits:**
- ✅ Repeatable
- ✅ Can regenerate on UI changes
- ✅ CI-friendly
- ✅ Consistent quality

**Example Test:**
```csharp
[AvaloniaTheory]
[InlineData("01-empty-state", false)]
[InlineData("02-files-loaded", true)]
public async Task Capture_MainWindow_States(string filename, bool loadSample)
{
    // Arrange
    var app = AvaloniaApp.GetTestApplication();
    var window = new MainWindow { Width = 1280, Height = 800 };
    
    if (loadSample)
    {
        var vm = window.DataContext as MainWindowViewModel;
        await vm.LoadFolderAsync(SampleProjectFixture.TutorialPath);
    }
    
    // Act - Render
    await Task.Delay(100); // Allow UI to settle
    var bitmap = window.CaptureRenderedFrame();
    
    // Save
    var outputPath = $"docs/images/user-guide/{filename}.png";
    bitmap.Save(outputPath);
    
    Console.WriteLine($"✅ Saved: {outputPath}");
}
```

---

### **Phase 3: In-App Tutorial Mode (Advanced - 4 hours)**

**Features:**
- Interactive overlay system
- Step-by-step guided tour
- Highlights UI elements
- Auto-progresses through workflow
- Can skip or restart

**Best for:**
- ✅ Consumer apps (high polish)
- ✅ Complex workflows
- ✅ Non-technical users

**Complexity:**
- Moderate (requires UI overlay system)
- Worth it for better onboarding

---

## 📊 COMPARISON MATRIX

| Approach | Time | Maintenance | UX | Best For |
|----------|------|-------------|-----|----------|
| **Sample Project + README** | 30 min | Low | Good | Quick start |
| **Automated Screenshots** | 2 hours | Medium | Great | Documentation |
| **In-App Tutorial** | 4 hours | High | Excellent | Consumer polish |

---

## 🎯 RECOMMENDED IMPLEMENTATION PLAN

### **Immediate (Do First):**

1. **Create Sample Tutorial Project** (30 minutes)
   ```
   samples/tutorial-project/
   ├── README_TUTORIAL.md
   ├── src/
   │   ├── Program.cs      (with fake DB names, IPs, API keys)
   │   ├── Database.cs     (with connection strings)
   │   └── Config.json     (with credentials)
   └── EXPECTED.md         (what to expect at each step)
   ```

2. **Manual Screenshots for USER_GUIDE.md** (45 minutes)
   - Run app
   - Load tutorial project
   - Capture 10-15 key screenshots
   - Save to `docs/images/user-guide/`

3. **Write USER_GUIDE.md** (30 minutes)
   - Embed screenshots
   - Step-by-step instructions
   - Link from README.md

**Total: ~90 minutes for complete user guide**

---

### **Future Enhancement:**

4. **Automated Screenshot Tests** (when UI stabilizes)
   - Add `Avalonia.Headless.XUnit`
   - Create screenshot test project
   - Generate images on demand
   - Run in CI to verify UI doesn't break

5. **Interactive Tutorial Mode** (for v2.0)
   - In-app guided tour
   - Overlay system
   - Progressive disclosure
   - Gamification (optional)

---

## 📝 SAMPLE USER_GUIDE.MD OUTLINE

```markdown
# ShieldPrompt User Guide

> Learn to use ShieldPrompt in 5 minutes

## Quick Tutorial

**Want to try it?** Load the tutorial project:
1. Clone this repo
2. Open `samples/tutorial-project` in ShieldPrompt
3. Follow the steps in `samples/tutorial-project/README_TUTORIAL.md`

---

## Installation

### Windows
![Windows Installation](images/user-guide/install-windows.png)
[Step-by-step instructions]

### macOS  
![macOS Installation](images/user-guide/install-macos.png)
[Step-by-step instructions]

### Linux
![Linux Installation](images/user-guide/install-linux.png)
[Step-by-step instructions]

---

## First-Time Setup

![First Launch](images/user-guide/first-launch.png)

1. Launch ShieldPrompt
2. Click "Open Folder"
3. Select your project
4. Done!

---

## Basic Workflow

### 1. Select Files
![File Selection](images/user-guide/file-selection.png)

### 2. Copy (Protected)
![Copy Button](images/user-guide/copy-button.png)

### 3. Use with ChatGPT
![ChatGPT](images/user-guide/chatgpt-paste.png)

### 4. Paste & Restore
![Restore Dialog](images/user-guide/restore-dialog.png)

---

## Advanced Features
[Screenshots for shield preview, formats, models, undo/redo]

---

## Troubleshooting
[Common issues with solutions]
```

---

## ✅ FINAL RECOMMENDATIONS

### **Start With (Minimum Viable Documentation):**

1. ✅ **Sample Tutorial Project** - 30 minutes
   - Create `samples/tutorial-project/`
   - Add fake secrets (obvious educational data)
   - Write walkthrough README

2. ✅ **Manual Screenshots** - 45 minutes
   - Capture 10-15 key screenshots
   - Save to `docs/images/user-guide/`
   - Professional quality

3. ✅ **USER_GUIDE.md** - 30 minutes
   - Write step-by-step guide
   - Embed screenshots
   - Link from main README

**Total: ~90 minutes for production-ready user guide**

---

### **Future (When Time Allows):**

4. ⏭️ **Automated Screenshot Tests** - 2 hours
   - `Avalonia.Headless.XUnit` integration
   - Regenerate on UI changes
   - CI integration

5. ⏭️ **In-App Tutorial** - 4 hours
   - Interactive overlay
   - Guided tour
   - Better onboarding UX

---

## 🎯 NEXT STEPS (If Approved)

1. Create `samples/tutorial-project/` structure
2. Write sample code with educational fake secrets
3. Create tutorial README
4. Run app and capture screenshots manually
5. Write `docs/USER_GUIDE.md` with embedded images
6. Update `docs/INDEX.md` and main `README.md`
7. Commit and push

**This gives users an immediate, hands-on learning experience!**

```text
[code]
Edge cases handled: Headless rendering on different platforms, screenshot file sizes, fake vs. real-looking data, tutorial completion tracking, different screen resolutions.
Tests written: None yet (analysis phase).
Security considerations: Tutorial sample data must be OBVIOUSLY fake (not real-looking secrets); mark with "TUTORIAL" comments; never use patterns that could leak real data.
Potential refactors (not implemented): Video walkthrough generation, GIF animations, interactive Jupyter-style tutorials, screenshot diffing for UI regression testing.
```

ROLE: engineer STRICT=true
