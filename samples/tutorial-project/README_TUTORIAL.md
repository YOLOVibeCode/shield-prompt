# 🎓 ShieldPrompt Tutorial - Learn by Doing!

**Time Required:** 5 minutes  
**What You'll Learn:** How to safely share code with ChatGPT using ShieldPrompt

---

## ⚠️ IMPORTANT: This Uses FAKE Data!

All files in this folder contain **FAKE credentials for educational purposes**:
- ❌ Not real database names
- ❌ Not real API keys  
- ❌ Not real IP addresses
- ❌ Not real SSNs or credit cards

**This is a safe learning environment!**

---

## 🚀 Tutorial Steps

### Step 1: Launch ShieldPrompt

```bash
# From the shield-prompt repository root:
dotnet run --project src/ShieldPrompt.App
```

Or if you downloaded a release, just double-click the app.

---

### Step 2: Open This Tutorial Folder

1. Click **"📁 Open Folder"** button (or press `Ctrl+O`)
2. Navigate to: `samples/tutorial-project/`
3. Click **"Select Folder"**

**What you should see:**
- File tree showing `src/` and `config/` folders
- 4 files total:
  - `src/Program.cs`
  - `src/DatabaseService.cs`
  - `src/ApiClient.cs`
  - `config/appsettings.json`

---

### Step 3: Select Files to Share

**Check these boxes:**
- ✅ `src/Program.cs`
- ✅ `src/DatabaseService.cs`
- ✅ `config/appsettings.json`

**What you should see:**
- Status bar shows token count (e.g., "~500 tokens")
- Selected files highlighted

---

### Step 4: 🛡️ See What Will Be Protected (Optional)

1. Click the **🛡️ Shield** button in the preview panel
2. You should see something like:

```
🛡️ Protected 34 sensitive values

🗄️ ProductionDB                     → DATABASE_0
🗄️ CustomerData                     → DATABASE_1
🌐 192.168.1.50                    → IP_ADDRESS_0
🌐 10.0.0.25                       → IP_ADDRESS_1
🌐 172.16.0.100                    → IP_ADDRESS_2
🔑 AKIAIOSFODNN7EXAMPLE            → AWS_KEY_0
🔑 sk-proj-Tutorial...             → OPENAI_KEY_0 (detected as password)
🔑 sk-ant-api03-Tutorial...        → ANTHROPIC_KEY_0 (detected as password)
🔑 ghp_TutorialFake...             → GITHUB_TOKEN_0
🆔 123-45-6789                     → SSN_0
🆔 987-65-4321                     → SSN_1
💳 4111-1111-1111-1111             → CREDIT_CARD_0
🔐 P@ssw0rd123!                    → PASSWORD_0
🔐 MySecretPassword123             → PASSWORD_1
🔐 SuperSecret123                  → PASSWORD_2
🔗 Server=ProductionDB;Database=... → CONNECTION_STRING_0
🌐 db.internal.company.com         → HOSTNAME_0
... and 17 more values

(Total: 4 databases, 4 IPs, 10 passwords, 4 SSNs, 2 credit cards, 6 connection strings, 2 AWS keys, 2 GitHub tokens)
```

**This preview shows you EXACTLY what will be protected before you copy!**

---

### Step 5: Copy (With Automatic Protection)

1. Click **"Copy"** button (or press `Ctrl+C`)

**What you should see:**
- Status bar: `"✅ Copied 3 files | 🔐 34 values masked | 2,381 tokens"`
- Clipboard now contains SAFE content

**Note:** The exact numbers may vary slightly depending on formatting, but you should see:
- **~2,300-2,400 tokens** (substantial content)
- **~30-35 sensitive values masked** (comprehensive protection)

---

### Step 6: See What ChatGPT Would Receive

1. Open any text editor (Notepad, TextEdit, VS Code)
2. Paste (`Ctrl+V` or `Cmd+V`)

**You should see:**
```
All database names → DATABASE_0
All IP addresses → IP_ADDRESS_0, IP_ADDRESS_1, IP_ADDRESS_2
All API keys → AWS_KEY_0, OPENAI_KEY_0, ANTHROPIC_KEY_0
All SSNs → SSN_0, SSN_1
All passwords → PASSWORD_0, PASSWORD_1, PASSWORD_2
```

**✅ SAFE to share with ChatGPT!** No real secrets exposed!

---

### Step 7: Simulate ChatGPT Response

For this tutorial, we'll pretend ChatGPT suggested improvements.

**Copy this simulated response:**
```csharp
// Improved version suggested by AI
public class Program
{
    // Use configuration instead of hardcoded values
    private static string DatabaseName = Configuration["Database:Name"] ?? "DATABASE_0";
    private static string ServerIp = Configuration["Database:Host"] ?? "IP_ADDRESS_0";
    private static string AwsAccessKey = Configuration["Api:AWS:Key"] ?? "AWS_KEY_0";
    
    public static void Main(string[] args)
    {
        Console.WriteLine($"Database: {DatabaseName}");
        Console.WriteLine($"Server: {ServerIp}");
        // Better! Configuration instead of hardcoded values
    }
}
```

---

### Step 8: Paste & Restore

1. Click **"Paste & Restore"** button in ShieldPrompt (or press `Ctrl+V`)
2. Paste the simulated ChatGPT response
3. You should see a dialog showing:

```
🔓 Ready to restore 3 sensitive values

🔓 DATABASE_0 → ProductionDB (2x in content)
🔓 IP_ADDRESS_0 → 192.168.1.50 (2x in content)
🔓 AWS_KEY_0 → AKIAIOSFODNN7EXAMPLE (2x in content)

Preview:
[Shows the code with real values restored]
```

4. Click **"Copy Restored"**

---

### Step 9: Verify Restoration

1. Paste into text editor
2. You should see:
   - `DATABASE_0` → `ProductionDB` ✅
   - `IP_ADDRESS_0` → `192.168.1.50` ✅
   - `AWS_KEY_0` → `AKIAIOSFODNN7EXAMPLE` ✅

**Perfect! Your code now has the REAL values back and is ready to use!**

---

## 🎉 Tutorial Complete!

### What You Learned:

1. ✅ How to load a project in ShieldPrompt
2. ✅ How to select files for sharing
3. ✅ How to preview what will be protected (Shield button)
4. ✅ How automatic sanitization works on copy
5. ✅ What ChatGPT receives (safe aliases)
6. ✅ How to restore real values from AI responses
7. ✅ Round-trip workflow (original → safe → AI → restored)

### Key Takeaways:

- 🛡️ **Zero configuration needed** - Protection is automatic
- 🔐 **14 patterns detected** - Databases, IPs, API keys, SSNs, credit cards, etc.
- 🔄 **Perfect restoration** - Original values always preserved
- 👁️ **Full visibility** - See what's protected before copying
- ⚡ **Fast workflow** - 3 keystrokes (Ctrl+O, Ctrl+C, Ctrl+V)

---

## 🚀 Next Steps

### Try with Your Own Code:

1. Click **"📁 Open Folder"** again
2. Select your actual project
3. Select files you want AI help with
4. Use the same workflow you just learned!

### Explore Advanced Features:

- **Format Selection** - Try Markdown, XML, or Plain Text
- **Model Selection** - Switch between GPT-4o, Claude, Gemini
- **Undo/Redo** - Press `Ctrl+Z` to undo, `Ctrl+Y` to redo
- **Keyboard Shortcuts** - Everything has shortcuts!

---

## 💡 Pro Tips

### Before Copying:
- ✅ Always click 🛡️ Shield to verify what's being masked
- ✅ Check token count doesn't exceed model limit
- ✅ Use Markdown format for best ChatGPT results

### In ChatGPT:
- ✅ Ask specific questions ("refactor this", "explain this function")
- ✅ AI can understand aliases (DATABASE_0 is still meaningful)
- ✅ Copy AI's full response

### After Pasting:
- ✅ Review restoration preview before clicking "Copy Restored"
- ✅ Use "Apply to Files" to update files automatically (future feature)
- ✅ Undo if needed (`Ctrl+Z`)

---

## 🆘 Troubleshooting

### "No sensitive values detected"
- This is GOOD if your code has no secrets!
- Tutorial files are designed to trigger detections
- With real code, seeing zero can mean you're already secure

### "Token count seems high"
- Deselect large files
- Choose fewer files per request
- Switch to a model with larger context (Claude 200K, Gemini 2M)

### "Restoration doesn't match"
- Make sure you copied the FULL ChatGPT response
- Check that aliases in response match what was sanitized
- Unknown aliases are left unchanged (safe default)

---

## 📚 Learn More

- [Full User Guide](../../docs/USER_GUIDE.md) - Complete documentation
- [Security Patterns](../../docs/SECURITY_PATTERNS.md) - All 14 patterns explained  
- [Use Cases](../../docs/USE_CASES.md) - Enterprise scenarios
- [Specification](../../docs/SPECIFICATION.md) - Technical deep dive

---

**🎊 Congratulations! You're now ready to use ShieldPrompt safely!**

*Your secrets stay secret. Your productivity goes up.* 🛡️

