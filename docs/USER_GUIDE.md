# ShieldPrompt User Guide

> Complete guide to using ShieldPrompt for secure AI-assisted development

---

## 🎓 New User? Start Here!

**Want to learn by doing?** We've created a hands-on tutorial:

👉 **[Interactive Tutorial](../samples/tutorial-project/README_TUTORIAL.md)** - 5-minute walkthrough with sample files

The tutorial includes:
- Sample code with fake secrets (safe to practice with)
- Step-by-step instructions
- What to expect at each step
- Perfect for first-time users!

---

## 📥 Installation

### Windows

**Download:** [ShieldPrompt.exe](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)

1. Download `ShieldPrompt.exe` or `ShieldPrompt-win-x64-portable.zip`
2. For portable: Extract ZIP to any folder
3. For exe: Double-click to run
4. (Optional) Move to `C:\Program Files\ShieldPrompt\` for permanent installation

**First-time:** Windows may show a security warning. Click "More info" → "Run anyway"

---

### macOS

**Download:** [Universal Binary](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)

1. Download `ShieldPrompt-osx-universal.zip`
2. Extract to Applications folder
3. Right-click → Open (first time only - bypasses Gatekeeper)
4. Subsequent launches: just double-click

**Troubleshooting:** If blocked, go to System Preferences → Security → Allow

---

### Linux

**Download:** [AppImage](https://github.com/YOLOVibeCode/shield-prompt/releases/latest) or [DEB](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)

**Option 1: AppImage (Universal)**
```bash
wget https://github.com/YOLOVibeCode/shield-prompt/releases/latest/download/ShieldPrompt-*-linux-x64.AppImage
chmod +x ShieldPrompt-*.AppImage
./ShieldPrompt-*.AppImage
```

**Option 2: DEB Package (Debian/Ubuntu)**
```bash
wget https://github.com/YOLOVibeCode/shield-prompt/releases/latest/download/ShieldPrompt-*-amd64.deb
sudo dpkg -i ShieldPrompt-*-amd64.deb
shieldprompt
```

---

## 🚀 Quick Start (3 Steps)

### Step 1: Open Your Project

1. Launch ShieldPrompt
2. Click **"📁 Open Folder"** (or press `Ctrl+O`)
3. Select your project folder
4. Done! ShieldPrompt remembers this folder

**Tip:** ShieldPrompt auto-excludes: `node_modules`, `.git`, binaries, etc.

---

### Step 2: Copy (With Automatic Protection)

1. **Select files** - Check boxes next to files you want to share
2. **Preview protection** (optional) - Click 🛡️ Shield button
3. **Copy** - Click "Copy" or press `Ctrl+C`
4. **Verify** - Status bar shows: "✅ Copied X files | 🔐 Y values masked"

**What just happened:**
- ShieldPrompt detected all sensitive data (DB names, IPs, API keys, SSNs, etc.)
- Replaced with safe aliases (DATABASE_0, IP_ADDRESS_0, etc.)
- Stored mappings in encrypted memory
- Copied SAFE content to clipboard

**You can now paste in ChatGPT safely!**

---

### Step 3: Restore (Get Your Code Back)

1. **Get AI help** - Paste in ChatGPT, get response, copy it
2. **Paste & Restore** - Click button or press `Ctrl+V` in ShieldPrompt
3. **Review** - See preview of what will be restored
4. **Copy Restored** - Click button to get final code
5. **Use** - Paste in your IDE - values are restored!

**Perfect round-trip:** Original → Safe → AI → Restored ✅

---

## 🛡️ Shield Preview (See Protection Before Copying)

**Want to see what's being protected?**

1. Select your files
2. Click the **🛡️ Shield** button in preview panel
3. Panel expands showing:

```
🛡️ Protected 12 sensitive values

🗄️ ProductionDB         → DATABASE_0
🌐 192.168.1.50        → IP_ADDRESS_0
🔑 AKIAIOSFO...        → AWS_KEY_0
🆔 123-45-6789         → SSN_0
💳 4111-1111-...       → CREDIT_CARD_0
```

**Benefits:**
- See exactly what will be masked
- Verify sensitive data was detected
- Make informed decision before copying

---

## ⚡ Advanced Features

### Format Selection

Choose output format for AI:

- **Markdown** (Recommended) - Code blocks, best for ChatGPT
- **XML** - RepoPrompt-style, structured
- **Plain Text** - Simple file separators

**How:** Dropdown next to Copy button

---

### Model Selection

Select AI model for accurate token counting:

- **GPT-4o** - 128,000 tokens
- **Claude 3.5 Sonnet** - 200,000 tokens  
- **Gemini 2.5 Pro** - 2,000,000 tokens
- **DeepSeek V3** - 64,000 tokens

**How:** Dropdown in toolbar

**Benefit:** See real-time token count, get warned if exceeding limit

---

### Undo / Redo

Made a mistake? No problem!

- **Undo** - `Ctrl+Z` - Undo last action
- **Redo** - `Ctrl+Y` - Redo action

**Works for:**
- File selections
- Folder changes  
- Sanitization operations

**Smart batching:** Multiple quick selections = one undo action

---

## ⌨️ Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| **Open Folder** | `Ctrl+O` |
| **Copy** | `Ctrl+C` |
| **Paste & Restore** | `Ctrl+V` |
| **Undo** | `Ctrl+Z` |
| **Redo** | `Ctrl+Y` |
| **Select All** | `Ctrl+A` |
| **Deselect All** | `Ctrl+D` |

---

## 🔍 What Gets Protected (14 Patterns)

### Infrastructure
1. ✅ Database/Server Names - `ProductionDB`, `staging-mysql`
2. ✅ Private IP Addresses - `192.168.x.x`, `10.x.x.x`, `172.16-31.x.x`
3. ✅ Connection Strings - `Server=prod;User=admin;Password=...`
4. ✅ File Paths - `C:\Users\...`, `\\server\share\...`
5. ✅ Internal Hostnames - `db.internal.company.com`

### Credentials & PII
6. ✅ AWS Access Keys - `AKIAIOSFODNN7...`
7. ✅ OpenAI API Keys - `sk-proj-...`
8. ✅ Anthropic API Keys - `sk-ant-...`
9. ✅ GitHub Tokens - `ghp_...`, `gho_...`
10. ✅ Private Keys - `-----BEGIN RSA PRIVATE KEY-----`
11. ✅ Passwords in Code - `password = "secret"`
12. ✅ JWT Tokens - `eyJhbGciOiJIUzI1NiI...`
13. ✅ Social Security Numbers - `123-45-6789`
14. ✅ Credit Cards - Visa, MasterCard, Amex (with Luhn validation)

**All patterns tested with real-world examples!**

---

## 🆘 Troubleshooting

### App Won't Launch

**Windows:**
- Right-click → "Run as Administrator"
- Check Windows Defender hasn't blocked it

**macOS:**
- System Preferences → Security & Privacy → Allow
- Try right-click → Open (instead of double-click)

**Linux:**
- Verify executable: `chmod +x ShieldPrompt*.AppImage`
- Check FUSE is installed: `sudo apt install fuse libfuse2`

---

### No Files Showing

- Verify folder permissions (must be readable)
- Check if all files are in excluded list
- Try different folder

---

### Copy Not Working

- Verify files are selected (checkboxes checked)
- Check clipboard permissions (macOS/Linux may need approval)
- Try keyboard shortcut (`Ctrl+C`) instead of button

---

### Restoration Not Matching

- Ensure you copied ChatGPT's FULL response
- Unknown aliases are left unchanged (safe default)
- Check that session is still active (4-hour default timeout)

---

## 💡 Pro Tips

### Before Copying:
- ✅ Use 🛡️ Shield preview to verify detection
- ✅ Check token count doesn't exceed model limit
- ✅ Deselect large files if token count is too high
- ✅ Use Markdown format for best ChatGPT results

### In ChatGPT:
- ✅ Paste the full content
- ✅ Ask specific questions ("refactor", "explain", "optimize")
- ✅ AI understands aliases (DATABASE_0 is meaningful)
- ✅ Copy the FULL response (not just code snippets)

### After Restoring:
- ✅ Review restoration preview before copying
- ✅ Verify all values were restored correctly
- ✅ Use Undo (`Ctrl+Z`) if something unexpected happened

---

## 🧪 Test Your Understanding

Try these exercises with the tutorial project:

1. **Exercise 1:** Copy only `Program.cs` - how many values are masked?
2. **Exercise 2:** Try different output formats - which do you prefer?
3. **Exercise 3:** Use Shield preview - can you find all 15+ sensitive values?
4. **Exercise 4:** Simulate ChatGPT response and restore - does round-trip work?

**Answers:** See `samples/tutorial-project/expected-outputs/`

---

## 📚 Additional Resources

- **[Interactive Tutorial](../samples/tutorial-project/README_TUTORIAL.md)** - Hands-on learning
- **[Use Cases](USE_CASES.md)** - Enterprise scenarios
- **[Security Patterns](SECURITY_PATTERNS.md)** - All patterns explained
- **[Technical Specification](SPECIFICATION.md)** - Architecture deep dive
- **[FAQ](FAQ.md)** - Common questions

---

## 🎓 Learn More

### Understanding ShieldPrompt:
- [Executive Summary](EXECUTIVE_SUMMARY.md) - Business case
- [Why ShieldPrompt?](WHY_SHIELDPROMPT.md) - vs. Agentic AI tools

### For Developers:
- [Contributing Guide](../CONTRIBUTING.md) - How to contribute
- [Architecture](SPECIFICATION.md#architecture) - Clean architecture explained

---

## 📞 Need Help?

- **Questions:** [GitHub Discussions](https://github.com/YOLOVibeCode/shield-prompt/discussions)
- **Issues:** [GitHub Issues](https://github.com/YOLOVibeCode/shield-prompt/issues)
- **Security:** [Security Advisories](https://github.com/YOLOVibeCode/shield-prompt/security/advisories)

---

**🎉 Ready to use ShieldPrompt safely with your real code!**

*Your secrets stay secret. Your compliance stays intact. Your productivity goes up.* 🛡️

