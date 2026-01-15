# ShieldPrompt 🛡️

> **Secure Copy/Paste for ChatGPT/Claude with Automatic Data Protection**

<!-- AUTO-INDEX-SECTION -->
<!--
🤖 AI ASSISTANT - PROJECT NAVIGATION GUIDE
============================================

To fully understand and work with this project, follow this systematic approach:

📚 STEP 1 - READ DOCUMENTATION: docs/INDEX.md
   This is your PRIMARY source for understanding the project:
   • Complete index of ALL project documentation
   • Organized by category (specifications, guides, architecture, etc.)
   • Read specifications FIRST to understand system design
   • Then read guides for implementation details
   • Every document is indexed with descriptions and word counts
   • This is the authoritative source - do not search randomly

🔧 STEP 2 - UNDERSTAND AVAILABLE SCRIPTS: scripts/INDEX.md
   This index shows you ALL tools and automation:
   • Categorized by purpose (setup, build, test, deploy, etc.)
   • Includes usage examples and requirements
   • Marked with executable status and required arguments
   • Shows which language each script uses

   ⚡ SCRIPT EXECUTION ORDER:
   1. Setup scripts - Run FIRST for project initialization
   2. Build scripts - Compile/build the project
   3. Test scripts - Verify functionality
   4. Deploy scripts - Deploy to environments

🎯 COMPLETE WORKFLOW:
   1. Read docs/INDEX.md to understand WHAT the project does
   2. Read scripts/INDEX.md to understand HOW to work with it
   3. Follow specifications → guides → implementation order
   4. Use scripts in order: setup → build → test → deploy

💡 KEY PRINCIPLES:
   • Both indexes are auto-generated and always up-to-date
   • Never search randomly - use the indexes as navigation
   • Read documentation before writing code
   • Check scripts before running manual commands
   • Word counts help prioritize what to read first

✅ AFTER READING BOTH INDEXES:
   You will have complete knowledge of:
   • System architecture and design decisions
   • Implementation details and best practices
   • All available automation and tooling
   • Proper setup, build, test, and deployment procedures

============================================
-->

## 📚 Documentation & Scripts

**Quick Links:**
- 📖 **[Documentation Index](docs/INDEX.md)** - Complete project documentation
- 🔧 **[Scripts Index](scripts/INDEX.md)** - All available scripts and tools

<!-- AUTO-INDEX-SECTION -->

## The Problem

Developers need AI help but can't use agentic tools (Copilot, Cursor) due to security policies. When they copy code to ChatGPT manually, they accidentally leak:

```
❌ Database names      → ProductionDB
❌ API keys            → AKIAIOSFODNN7EXAMPLE  
❌ IP addresses        → 192.168.1.50
❌ Credentials         → password="secret123"
❌ SSNs, credit cards, private keys...
```

**One leaked secret = potential data breach**

## The Solution

ShieldPrompt **automatically sanitizes** before copying to AI, then **restores** when you paste back:

```
Your Code              What AI Sees           Restored Code
─────────────         ───────────────         ─────────────
ProductionDB     →    DATABASE_0         →    ProductionDB
192.168.1.50     →    IP_ADDRESS_0       →    192.168.1.50
AKIAIOSFO...     →    AWS_KEY_0          →    AKIAIOSFO...
```

**Safe to share. Safe to use. Safe to paste back.**

---

## 🚀 Quick Start

### Installation

**Download latest release:**
- **Windows:** [ShieldPrompt.exe](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)
- **macOS:** [Universal Binary](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)
- **Linux:** [AppImage](https://github.com/YOLOVibeCode/shield-prompt/releases/latest) or [DEB](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)

**Or build from source:**
```bash
git clone https://github.com/YOLOVibeCode/shield-prompt.git
cd shield-prompt
dotnet run --project src/ShieldPrompt.App
```

### Usage (3 Steps)

1. **📁 Open Folder** - Select your project  
2. **📋 Copy** (`Ctrl+C`) - Paste to ChatGPT (automatically sanitized!)  
3. **🔄 Paste & Restore** (`Ctrl+V`) - Get safe code back with real values

**That's it!** ShieldPrompt handles all the protection automatically.

### 🎓 New User? Try the Tutorial!

**Learn by doing:** Load `samples/tutorial-project/` and follow the [interactive tutorial](samples/tutorial-project/README_TUTORIAL.md)

- ✅ Hands-on walkthrough (5 minutes)
- ✅ Sample files with fake secrets (safe to practice)
- ✅ See exactly what gets protected
- ✅ Learn the complete workflow

---

---

## 📋 Example Workflow

```bash
# 1. Your code (before)
var connection = "Server=ProductionDB;Password=secret123";
var apiKey = "AKIAIOSFODNN7EXAMPLE";

# 2. Copy with ShieldPrompt → What ChatGPT sees
var connection = "Server=DATABASE_0;Password=PASSWORD_0";
var apiKey = "AWS_KEY_0";

# 3. ChatGPT suggests improvements
var connection = Configuration.GetConnectionString("DATABASE_0");
var apiKey = Configuration["AWS_KEY_0"];

# 4. Paste & Restore → Your final code
var connection = Configuration.GetConnectionString("ProductionDB");
var apiKey = Configuration["AKIAIOSFODNN7EXAMPLE"];
```

**Result:** Better code. Zero secrets leaked. ✅

---

## ✨ Key Features

- **🛡️ 14 Security Patterns** - API keys, SSNs, IPs, databases, credit cards, private keys
- **🔄 Automatic Restore** - Paste AI response → get working code with real values
- **👁️ Protection Preview** - See what's being masked before copying
- **↶↷ Undo/Redo** - Intelligent action history (`Ctrl+Z/Y`)
- **🧠 Remembers Everything** - Auto-loads last folder & selections
- **⚡ Keyboard Shortcuts** - `Ctrl+O/C/V/Z/Y` for everything
- **📊 Token Counting** - Real-time counts for GPT-4o, Claude, Gemini
- **🎯 Zero-Knowledge** - Sensitive data never leaves your machine

[See all features →](docs/FEATURES.md)

---

## 📚 Documentation

- **[User Guide](docs/USER_GUIDE.md)** ⭐ - Complete usage guide with screenshots
- **[Interactive Tutorial](samples/tutorial-project/README_TUTORIAL.md)** ⭐ - Learn by doing (5 min)
- **[Documentation Index](docs/INDEX.md)** - Complete documentation index
- **[Use Cases](docs/USE_CASES.md)** - Enterprise scenarios & examples  
- **[Technical Specification](docs/SPECIFICATION.md)** - Full architecture & implementation

---

## 🏢 For Enterprises

**ShieldPrompt vs. Agentic AI Tools:**

| | Copilot/Cursor | ShieldPrompt |
|-|----------------|--------------|
| **Security Policy** | Often blocked | ✅ Works with copy/paste policies |
| **Developer Control** | AI decides | ✅ Developer controls everything |
| **Data Protection** | Code sent unprotected | ✅ Automatic sanitization |
| **Cost** | $10-20/dev/month | ✅ FREE (MIT License) |

**Perfect for companies that can't or won't approve agentic tools.**

---

## ⚙️ Technology

- **Runtime:** .NET 10.0
- **UI:** Avalonia 11 (Cross-platform)
- **Tests:** 180/180 passing ✅
- **License:** MIT (Free for commercial use)
- **Version:** 1.0.3 (Released Jan 14, 2026)

---

## 🤝 Contributing

Contributions welcome! See [docs/INDEX.md](docs/INDEX.md) for project structure and guidelines.

---

## 📞 Support

- **Issues:** [GitHub Issues](https://github.com/YOLOVibeCode/shield-prompt/issues)
- **Discussions:** [GitHub Discussions](https://github.com/YOLOVibeCode/shield-prompt/discussions)
- **Security:** [Security Advisories](https://github.com/YOLOVibeCode/shield-prompt/security/advisories)

---

**ShieldPrompt** - Safe AI assistance without the security risks.

*Download: [Latest Release](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)*

