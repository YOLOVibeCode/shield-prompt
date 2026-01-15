# Screenshot Capture Guide

> How to capture professional screenshots for the user guide

---

## 🎯 Goal

Capture 10-15 high-quality screenshots showing ShieldPrompt in action using the tutorial project.

---

## 📋 Screenshot Checklist

### Installation & Setup (3 screenshots)
- [ ] `01-download-page.png` - GitHub releases page
- [ ] `02-first-launch.png` - App on first launch (empty state)
- [ ] `03-open-folder.png` - Folder selection dialog

### Tutorial Project Loaded (5 screenshots)
- [ ] `04-file-tree.png` - Tutorial files loaded, some selected
- [ ] `05-shield-preview.png` - Shield panel expanded showing masked values
- [ ] `06-copy-success.png` - Status bar after copying
- [ ] `07-paste-restore-dialog.png` - Paste & Restore dialog with preview
- [ ] `08-restoration-preview.png` - Shows what will be restored

### Advanced Features (4 screenshots)
- [ ] `09-format-selection.png` - Format dropdown (Markdown, XML, Plain)
- [ ] `10-model-selection.png` - Model dropdown (GPT-4o, Claude, etc.)
- [ ] `11-undo-redo.png` - Undo/Redo buttons
- [ ] `12-token-count.png` - Status bar showing token count

### ChatGPT Integration (2 screenshots)
- [ ] `13-chatgpt-paste.png` - Content pasted in ChatGPT (sanitized)
- [ ] `14-chatgpt-response.png` - ChatGPT response with aliases

---

## 🎨 Capture Instructions (macOS)

### Before You Start:
1. Build and run the app:
   ```bash
   cd /Users/admin/Dev/YOLOProjects/shield-prompt
   dotnet run --project src/ShieldPrompt.App
   ```

2. Load the tutorial project:
   - Click "Open Folder"
   - Navigate to `samples/tutorial-project/`
   - Click "Select Folder"

### Taking Screenshots:

**Method 1: Region Selection (Recommended)**
```bash
# Press: Cmd+Shift+4
# Drag to select area
# Screenshot saved to Desktop
```

**Method 2: Window Capture**
```bash
# Press: Cmd+Shift+4, then Spacebar
# Click window
# Screenshot saved to Desktop
```

### After Capture:

1. **Move to project:**
   ```bash
   mv ~/Desktop/Screen\ Shot*.png docs/images/user-guide/01-xxx.png
   ```

2. **Rename descriptively:**
   - `01-download-page.png`
   - `02-first-launch.png`
   - etc.

3. **Optimize size** (optional):
   ```bash
   # Using ImageOptim or:
   sips -Z 1200 docs/images/user-guide/*.png
   ```

---

## 📐 Quality Guidelines

### Resolution:
- ✅ **Minimum:** 1280x800
- ✅ **Recommended:** 1920x1080
- ✅ **Max width:** 1200px (for documentation)

### Consistency:
- ✅ Use same window size for all app screenshots
- ✅ Use same theme (light mode recommended for docs)
- ✅ Crop to relevant area (remove unnecessary chrome)

### Content:
- ✅ Use tutorial project files (realistic but fake)
- ✅ Show actual UI states (not mocked)
- ✅ Include relevant UI elements (buttons, status bar, etc.)

### File Size:
- ✅ Target: < 500KB per image
- ✅ Format: PNG (for UI clarity)
- ✅ Optimize before committing

---

## 🎬 Screenshot Scenarios

### Scenario 1: Fresh Install
```
1. Launch app for first time
2. Capture empty state
3. Save as: 02-first-launch.png
```

### Scenario 2: Tutorial Loaded
```
1. Open samples/tutorial-project/
2. Files load in tree
3. Capture full window
4. Save as: 04-file-tree.png
```

### Scenario 3: File Selection
```
1. Check Program.cs
2. Check DatabaseService.cs  
3. Check appsettings.json
4. Capture with checkmarks visible
5. Save as: 04-file-tree.png (update if needed)
```

### Scenario 4: Shield Preview
```
1. With files selected, click Shield button
2. Panel expands showing ~15 masked values
3. Capture expanded shield panel
4. Save as: 05-shield-preview.png
```

### Scenario 5: Copy Success
```
1. Click Copy button
2. Wait for status bar to update
3. Capture showing: "✅ Copied 3 files | 🔐 15 values masked | 487 tokens"
4. Save as: 06-copy-success.png
```

### Scenario 6: Paste Dialog
```
1. Click "Paste & Restore" button
2. Paste some sample content with aliases
3. Capture dialog showing restoration preview
4. Save as: 07-paste-restore-dialog.png
```

---

## 📝 Embedding in USER_GUIDE.md

After capturing, embed in documentation:

```markdown
### Step 1: Open Your Project

![Open Folder Dialog](images/user-guide/03-open-folder.png)

1. Click "Open Folder" button
2. Select your project
3. Click "Select Folder"
```

---

## ✅ Verification

Before committing, verify:

- [ ] All screenshots captured
- [ ] Files properly named (01-, 02-, etc.)
- [ ] Saved to `docs/images/user-guide/`
- [ ] File sizes reasonable (< 500KB each)
- [ ] All embedded in USER_GUIDE.md
- [ ] Images display correctly in Markdown preview

---

## 🚀 Quick Capture Session (45 minutes)

**Recommended flow:**

1. **Setup** (5 min) - Build app, load tutorial project
2. **Capture** (30 min) - Take all 14 screenshots systematically
3. **Organize** (5 min) - Rename, move to docs/images/
4. **Verify** (5 min) - Check all look good

**Pro tip:** Take screenshots in order (01, 02, 03...) following the user guide flow

---

## 🎯 Ready to Capture?

**Run this to get started:**
```bash
cd /Users/admin/Dev/YOLOProjects/shield-prompt
dotnet run --project src/ShieldPrompt.App
# Then: Open samples/tutorial-project/ and start capturing!
```

**Save screenshots to:** `docs/images/user-guide/`

---

**Once screenshots are captured, the USER_GUIDE.md will be complete!**

