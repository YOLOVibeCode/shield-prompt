# ShieldPrompt Release Guide

**For:** Release managers, DevOps engineers  
**Purpose:** Step-by-step guide to create and publish releases

---

## 🎯 Release Philosophy

**Principle:** **Tag → Test → Build → Publish → Verify**

- Git tags trigger everything automatically
- All tests MUST pass before build
- Multi-platform builds run in parallel
- Release published automatically
- Manual verification before announcement

---

## 📋 Pre-Release Checklist

### **Code Quality:**
- [ ] All 180 tests passing locally
- [ ] No compiler warnings
- [ ] No linter errors
- [ ] Code formatted (`dotnet format`)
- [ ] Performance benchmarks met

### **Documentation:**
- [ ] CHANGELOG.md updated
- [ ] README.md current
- [ ] Version bumped in VERSION file
- [ ] Breaking changes documented (if any)

### **Testing:**
- [ ] Tested on Windows 10/11
- [ ] Tested on macOS 12+
- [ ] Tested on Ubuntu 22.04+
- [ ] Tested with .NET 10 runtime
- [ ] Cross-platform file paths verified

---

## 🚀 Creating a Release

### **Step 1: Prepare Release Branch**

```bash
# Start from develop branch
git checkout develop
git pull origin develop

# Create release branch
git checkout -b release/v1.0.0
```

### **Step 2: Update Version**

**Files to update:**

1. **VERSION**
   ```
   1.0.0
   ```

2. **CHANGELOG.md**
   ```markdown
   ## [1.0.0] - 2026-01-14
   
   ### Added
   - Feature 1
   - Feature 2
   
   ### Changed
   - Improvement 1
   
   ### Fixed
   - Bug fix 1
   ```

3. **src/ShieldPrompt.App/ShieldPrompt.App.csproj**
   ```xml
   <PropertyGroup>
     <Version>1.0.0</Version>
     <AssemblyVersion>1.0.0.0</AssemblyVersion>
     <FileVersion>1.0.0.0</FileVersion>
   </PropertyGroup>
   ```

### **Step 3: Commit Version Bump**

```bash
git add VERSION CHANGELOG.md src/ShieldPrompt.App/ShieldPrompt.App.csproj
git commit -m "chore: bump version to 1.0.0"
git push origin release/v1.0.0
```

### **Step 4: Final Testing**

```bash
# Run full test suite
dotnet test

# Verify test count
dotnet test --logger "console;verbosity=detailed" | grep "Passed:"
# Should show: Passed! - Failed: 0, Passed: 180

# Build release configuration
dotnet build -c Release

# Test installers locally (optional but recommended)
dotnet publish -c Release -r win-x64 --self-contained -o test-publish
./test-publish/ShieldPrompt.exe  # Test on Windows
```

### **Step 5: Merge to Main**

```bash
# Merge release branch to main
git checkout main
git pull origin main
git merge release/v1.0.0 --no-ff -m "Release v1.0.0"
```

### **Step 6: Create and Push Tag**

```bash
# Create annotated tag
git tag -a v1.0.0 -m "ShieldPrompt v1.0.0

Release Highlights:
- Visual protection preview with shield panel
- Intelligent undo/redo system
- Settings persistence and auto-restore
- 14 enterprise-grade sanitization patterns
- Three output formats (Plain/Markdown/XML)

Technical:
- 180 tests (100% passing)
- Clean Architecture compliance
- ISP-compliant interfaces
- Cross-platform (Windows/macOS/Linux)

Security:
- Zero-knowledge architecture
- In-memory session management
- Secure disposal
- HIPAA/GDPR/PCI-DSS ready

Documentation:
- Complete specification (1,147 lines)
- Enterprise use cases
- Executive summary
- Deployment architecture"

# Push main and tag
git push origin main
git push origin v1.0.0
```

**⚡ This triggers GitHub Actions automatically!**

### **Step 7: Monitor Build**

```bash
# Watch GitHub Actions at:
# https://github.com/YOLOVibeCode/shield-prompt/actions

# Builds will:
1. Run all 180 tests ✅
2. Build Windows (x64, ARM64)
3. Build macOS (universal binary)
4. Build Linux (AppImage, DEB, RPM)
5. Generate checksums
6. Create GitHub Release
7. Upload all artifacts

# Expected duration: 10-15 minutes
```

### **Step 8: Verify Release**

Once GitHub Actions completes:

```bash
# Go to releases page
open https://github.com/YOLOVibeCode/shield-prompt/releases

# Verify:
- [ ] Release created with correct tag
- [ ] All platform installers present
- [ ] SHA256SUMS file included
- [ ] Release notes correct
- [ ] Download links work
```

### **Step 9: Test Installers**

Download and test each installer:

**Windows:**
```powershell
# Download ShieldPrompt-1.0.0-win-x64.msi
# Double-click → Install
# Launch from Start Menu
# Verify app works
```

**macOS:**
```bash
# Download ShieldPrompt-1.0.0-osx-universal.dmg
# Open DMG → Drag to Applications
# Launch ShieldPrompt.app
# Verify app works
```

**Linux:**
```bash
# Download ShieldPrompt-1.0.0-linux-x64.AppImage
chmod +x ShieldPrompt-1.0.0-linux-x64.AppImage
./ShieldPrompt-1.0.0-linux-x64.AppImage
# Verify app works
```

### **Step 10: Merge Back to Develop**

```bash
# Merge main back to develop
git checkout develop
git merge main
git push origin develop
```

### **Step 11: Announce Release**

- [ ] Update project website
- [ ] Post on Twitter/X
- [ ] Post on Reddit (r/dotnet, r/programming)
- [ ] Submit to Product Hunt
- [ ] Write blog post
- [ ] Email enterprise contacts
- [ ] Update documentation site

---

## 🔥 Hotfix Process

For urgent bug fixes:

```bash
# Create hotfix branch from main
git checkout main
git checkout -b hotfix/v1.0.1

# Fix bug
# ... make changes ...
git commit -am "fix: critical security issue in pattern matching"

# Update version
echo "1.0.1" > VERSION
# Update CHANGELOG.md

# Commit version bump
git commit -am "chore: bump version to 1.0.1"

# Merge to main
git checkout main
git merge hotfix/v1.0.1 --no-ff

# Tag
git tag -a v1.0.1 -m "Hotfix v1.0.1: Critical security patch"

# Push (triggers build)
git push origin main
git push origin v1.0.1

# Merge to develop
git checkout develop
git merge main
git push origin develop
```

---

## 🎯 Version Numbering Guide

### **MAJOR.MINOR.PATCH**

**MAJOR (v2.0.0):** Breaking changes
- Changed interface contracts
- Removed features
- Incompatible settings format
- Requires migration

**MINOR (v1.1.0):** New features (backward compatible)
- New output format
- New detection pattern
- New UI feature
- Enhanced UX

**PATCH (v1.0.1):** Bug fixes
- Security patches
- Bug fixes
- Performance improvements
- Documentation updates

### **Pre-Release Suffixes:**
```
v1.0.0-alpha.1   - Early testing
v1.0.0-beta.1    - Feature complete, testing
v1.0.0-rc.1      - Release candidate
v1.0.0           - Stable release
```

---

## 📦 Artifact Naming Convention

**Format:** `ShieldPrompt-{VERSION}-{PLATFORM}-{ARCH}.{EXT}`

**Examples:**
```
ShieldPrompt-1.0.0-win-x64.msi
ShieldPrompt-1.0.0-win-arm64.msi
ShieldPrompt-1.0.0-osx-universal.dmg
ShieldPrompt-1.0.0-linux-x64.AppImage
ShieldPrompt-1.0.0-amd64.deb
ShieldPrompt-1.0.0-x86_64.rpm
```

**Portable Versions:**
```
ShieldPrompt-1.0.0-win-x64-portable.zip
ShieldPrompt-1.0.0-osx-universal.zip
ShieldPrompt-1.0.0-linux-x64.tar.gz
```

---

## 🔐 Code Signing Guide

### **Windows Code Signing:**

**Requirements:**
- Code signing certificate (.pfx file)
- Certificate password
- Timestamp server URL

**Setup GitHub Secret:**
```bash
# Add to GitHub Repository Secrets:
WINDOWS_CERT_PASSWORD=<your-certificate-password>
WINDOWS_CERT_BASE64=<base64-encoded-certificate>
```

**Local Testing:**
```powershell
# Sign executable
signtool sign /f certificate.pfx /p PASSWORD `
  /tr http://timestamp.digicert.com /td sha256 `
  /fd sha256 `
  ShieldPrompt.exe

# Verify
signtool verify /pa ShieldPrompt.exe
```

### **macOS Code Signing:**

**Requirements:**
- Apple Developer Program ($99/year)
- Developer ID Application certificate
- App-specific password for notarization

**Setup GitHub Secrets:**
```bash
APPLE_CERT_PASSWORD=<certificate-password>
APPLE_ID=<your-apple-id@email.com>
APPLE_APP_PASSWORD=<app-specific-password>
APPLE_TEAM_ID=<your-team-id>
```

**Local Testing:**
```bash
# Sign
codesign --deep --force --verify --verbose \
  --sign "Developer ID Application: Your Name (TEAM_ID)" \
  --options runtime \
  ShieldPrompt.app

# Verify
codesign --verify --deep --strict --verbose=2 ShieldPrompt.app

# Notarize
xcrun notarytool submit ShieldPrompt.zip \
  --apple-id your@email.com \
  --password @keychain:AC_PASSWORD \
  --team-id TEAM_ID \
  --wait

# Staple
xcrun stapler staple ShieldPrompt.app
```

---

## 🎯 Release Template

**GitHub Release Description Template:**

```markdown
## ShieldPrompt v{VERSION}

**The Secure Alternative to Agentic AI Coding Tools**

### 🎯 Highlights

{List 3-5 key features or changes}

### 📥 Installation

Choose your platform:

| Platform | Recommended | Alternative |
|----------|-------------|-------------|
| **Windows 10/11** | [MSI Installer](link) (x64/ARM64) | [Portable ZIP](link) |
| **macOS 12+** | [DMG](link) (Universal) | [ZIP](link) |
| **Linux** | [AppImage](link) (Universal) | [DEB](link) / [RPM](link) |

### 🔐 What's Protected

14 enterprise-grade detection patterns:
- Database names, IPs, credentials
- API keys (AWS, GitHub, OpenAI, Anthropic)
- PII (SSN, credit cards)
- Private keys, passwords, tokens

### ✨ What's New in v{VERSION}

{Changelog excerpt}

### 📊 Technical Details

- **Tests:** 180/180 passing ✅
- **.NET:** 10.0 runtime required
- **Architecture:** Clean Architecture with ISP
- **License:** MIT (Free & Open Source)

### ✅ Verification

Verify download integrity:
```bash
sha256sum -c SHA256SUMS
```

### 🚀 Quick Start

1. Download installer for your platform
2. Install (double-click)
3. Launch ShieldPrompt
4. Press Ctrl+O to open folder
5. Select files, press Ctrl+C
6. Paste in ChatGPT - it's SAFE!

### 📚 Documentation

- [README](link)
- [Use Cases](link)
- [Specification](link)

---

**Full Changelog:** [CHANGELOG.md](link)
```

---

## 🎊 Summary for Architect Review

### **Deployment Architecture Includes:**

✅ **Automated CI/CD**
- GitHub Actions workflow
- Tag-triggered builds
- Multi-platform parallel builds
- Automatic release creation

✅ **Professional Installers**
- Windows: MSI (WiX), MSIX, Portable ZIP
- macOS: DMG with drag-to-install, PKG for MDM
- Linux: AppImage, DEB, RPM

✅ **Code Signing**
- Windows Authenticode
- macOS Developer ID + Notarization
- Linux GPG signing (optional)

✅ **Auto-Update System**
- GitHub Releases as update server
- In-app update notifications
- One-click update download
- Seamless update installation

✅ **Distribution Channels**
- GitHub Releases (primary)
- Package managers (Chocolatey, Homebrew, etc.)
- Enterprise MDM deployment
- Direct download from website

✅ **Version Control**
- Semantic versioning
- Git tag strategy
- Branching model (gitflow)
- Changelog automation

---

## 🎯 Next Steps

**For ENGINEER to implement:**
1. Create actual WiX configuration files
2. Generate macOS icon (.icns)
3. Create Linux icon (.png)
4. Test GitHub Actions workflow
5. Setup code signing certificates
6. Configure secrets in GitHub
7. Test full release process
8. Create first v1.0.0 tag!

**All architecture and strategy defined - ready for implementation!**

---

ROLE: architect STRICT=true

