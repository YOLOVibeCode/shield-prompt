# ShieldPrompt 🛡️

> **Secure Copy/Paste for ChatGPT/Claude with Automatic Data Protection**

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

- **[Security Patterns](docs/SECURITY_PATTERNS.md)** - All 14 detection patterns explained
- **[Use Cases](docs/USE_CASES.md)** - Enterprise scenarios & examples
- **[User Guide](docs/USER_GUIDE.md)** - Complete usage instructions
- **[Technical Spec](SPECIFICATION.md)** - Full architecture & implementation
- **[Why ShieldPrompt?](docs/WHY_SHIELDPROMPT.md)** - Enterprise vs. Agentic AI comparison

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

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 📞 Support

- **Issues:** [GitHub Issues](https://github.com/YOLOVibeCode/shield-prompt/issues)
- **Discussions:** [GitHub Discussions](https://github.com/YOLOVibeCode/shield-prompt/discussions)
- **Security:** [Security Advisories](https://github.com/YOLOVibeCode/shield-prompt/security/advisories)

---

**ShieldPrompt** - Safe AI assistance without the security risks.

*Download: [Latest Release](https://github.com/YOLOVibeCode/shield-prompt/releases/latest)*

