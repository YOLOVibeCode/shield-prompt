# ShieldPrompt - Deployment & Distribution Architecture

**Version:** 1.0  
**Date:** January 14, 2026  
**Status:** Architecture Design  

---

## 🎯 Deployment Goals

### **Primary Objectives:**
1. **Automated Build** - CI/CD pipeline triggered by git tags
2. **Multi-Platform Artifacts** - Windows MSI, macOS DMG, Linux AppImage/deb/rpm
3. **Easy Installation** - One-click install for end users
4. **Auto-Update** - Seamless updates without reinstall
5. **Signed Binaries** - Code signing for Windows/macOS trust
6. **Version Control** - Semantic versioning with git tags

### **Success Criteria:**
- ✅ Tag push → Automated build → Release published in <15 minutes
- ✅ User downloads installer → Double-click → App runs
- ✅ Auto-update notification → One click → Latest version
- ✅ Zero manual steps in release process

---

## 📋 Distribution Strategy

### **Platform-Specific Packaging:**

```
┌─────────────────────────────────────────────────────────────────┐
│                    Git Tag (v1.0.0)                             │
│                           │                                     │
│                           ▼                                     │
│                  GitHub Actions Workflow                        │
│                           │                                     │
│         ┌─────────────────┼─────────────────┐                  │
│         ▼                 ▼                 ▼                  │
│    Windows Build     macOS Build      Linux Build             │
│         │                 │                 │                  │
│         ▼                 ▼                 ▼                  │
│   .msi + .exe        .dmg + .pkg    .AppImage + .deb + .rpm   │
│         │                 │                 │                  │
│         └─────────────────┴─────────────────┘                  │
│                           │                                     │
│                           ▼                                     │
│                  GitHub Release Created                         │
│              (with changelog, checksums, signatures)            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Platform-Specific Architectures

### **1. Windows Distribution**

#### **Installer: MSI (Recommended)**
**Technology:** WiX Toolset v4 or Advanced Installer

**Features:**
- ✅ System-wide installation (`C:\Program Files\ShieldPrompt`)
- ✅ Start Menu shortcuts
- ✅ Desktop shortcut (optional)
- ✅ File associations (.shieldprompt files)
- ✅ Add to PATH
- ✅ Uninstaller
- ✅ Per-user or per-machine install
- ✅ Code signing with Authenticode

**Package Structure:**
```
ShieldPrompt-1.0.0-win-x64.msi
├── Program Files/
│   └── ShieldPrompt/
│       ├── ShieldPrompt.exe          # Main executable (self-contained)
│       ├── appsettings.json          # Configuration
│       └── Uninstall.exe             # Uninstaller
│
├── Start Menu/
│   └── ShieldPrompt.lnk
│
└── Registry/
    ├── HKLM\Software\ShieldPrompt    # Version info
    └── Uninstall entry               # Add/Remove Programs
```

**Alternative: ClickOnce**
- Web-based installation
- Auto-update built-in
- Easier for enterprise deployment via Group Policy

**Alternative: MSIX**
- Microsoft Store distribution
- Sandboxed security
- Automatic updates
- Preferred for Windows 10/11

---

### **2. macOS Distribution**

#### **Installer: DMG + PKG**

**DMG (Drag & Drop):**
```
ShieldPrompt-1.0.0-osx-arm64.dmg
└── Contents:
    ├── ShieldPrompt.app/
    │   └── Contents/
    │       ├── MacOS/
    │       │   └── ShieldPrompt       # Executable
    │       ├── Resources/
    │       │   └── icon.icns
    │       └── Info.plist
    │
    ├── Applications (symlink)         # For drag-to-install
    └── .background/                   # Pretty background image
        └── installer-bg.png
```

**PKG (System Installer):**
- For enterprise MDM deployment (Jamf, Intune)
- Installs to `/Applications`
- Creates LaunchAgent for updates

**Code Signing:**
- Apple Developer ID required ($99/year)
- Notarization required for Catalina+
- Prevents "Unidentified Developer" warning

**Universal Binary:**
```bash
# Build for both Intel and Apple Silicon
dotnet publish -r osx-x64 --self-contained -o publish/osx-x64
dotnet publish -r osx-arm64 --self-contained -o publish/osx-arm64
lipo -create -output ShieldPrompt \
  publish/osx-x64/ShieldPrompt \
  publish/osx-arm64/ShieldPrompt
```

---

### **3. Linux Distribution**

#### **AppImage (Recommended for Users)**
**Why:** Single file, runs on any distro, no installation required

```
ShieldPrompt-1.0.0-linux-x64.AppImage
├── AppRun                     # Launcher script
├── ShieldPrompt.desktop       # Desktop entry
├── icon.png                   # Application icon
└── usr/
    ├── bin/
    │   └── ShieldPrompt       # Executable
    └── lib/
        └── ... (dependencies)
```

**Usage:**
```bash
chmod +x ShieldPrompt-1.0.0-linux-x64.AppImage
./ShieldPrompt-1.0.0-linux-x64.AppImage
```

#### **DEB Package (Debian/Ubuntu)**
```
ShieldPrompt-1.0.0-amd64.deb
├── DEBIAN/
│   ├── control               # Package metadata
│   ├── postinst              # Post-install script
│   └── prerm                 # Pre-remove script
│
├── usr/
│   ├── bin/
│   │   └── shieldprompt → ../lib/ShieldPrompt/ShieldPrompt
│   ├── lib/
│   │   └── ShieldPrompt/     # Application files
│   └── share/
│       ├── applications/
│       │   └── shieldprompt.desktop
│       └── icons/
│           └── hicolor/256x256/apps/shieldprompt.png
```

**Install:**
```bash
sudo dpkg -i ShieldPrompt-1.0.0-amd64.deb
sudo apt-get install -f  # Fix dependencies
```

#### **RPM Package (Fedora/RHEL/CentOS)**
```
ShieldPrompt-1.0.0-x86_64.rpm
└── Similar structure to DEB
```

**Install:**
```bash
sudo rpm -i ShieldPrompt-1.0.0-x86_64.rpm
# OR
sudo dnf install ShieldPrompt-1.0.0-x86_64.rpm
```

---

## 🔧 Build & Release Automation

### **GitHub Actions Workflow**

**File:** `.github/workflows/release.yml`

```yaml
name: Release Build

on:
  push:
    tags:
      - 'v*.*.*'  # Triggers on v1.0.0, v1.2.3, etc.

env:
  DOTNET_VERSION: '10.0.x'
  APP_VERSION: ${{ github.ref_name }}

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Run tests
        run: dotnet test --no-restore --verbosity normal
      
      - name: Publish Windows x64
        run: |
          dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
            -c Release \
            -r win-x64 \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:IncludeNativeLibrariesForSelfExtract=true \
            -p:Version=${{ env.APP_VERSION }} \
            -o publish/win-x64
      
      - name: Publish Windows ARM64
        run: |
          dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
            -c Release \
            -r win-arm64 \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:IncludeNativeLibrariesForSelfExtract=true \
            -p:Version=${{ env.APP_VERSION }} \
            -o publish/win-arm64
      
      - name: Create MSI Installer
        run: |
          # Install WiX toolset
          dotnet tool install --global wix
          
          # Build MSI
          cd installers/windows
          wix build -arch x64 -o ../../publish/ShieldPrompt-${{ env.APP_VERSION }}-win-x64.msi
      
      - name: Sign Windows Binaries
        if: env.WINDOWS_CERT_PASSWORD != ''
        env:
          WINDOWS_CERT_PASSWORD: ${{ secrets.WINDOWS_CERT_PASSWORD }}
        run: |
          # Sign with Authenticode
          signtool sign /f certificate.pfx /p ${{ env.WINDOWS_CERT_PASSWORD }} \
            /tr http://timestamp.digicert.com /td sha256 \
            publish/win-x64/ShieldPrompt.exe
      
      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: windows-build
          path: publish/ShieldPrompt-*.msi

  build-macos:
    runs-on: macos-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Run tests
        run: dotnet test --verbosity normal
      
      - name: Publish macOS ARM64
        run: |
          dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
            -c Release \
            -r osx-arm64 \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:Version=${{ env.APP_VERSION }} \
            -o publish/osx-arm64
      
      - name: Publish macOS x64 (Intel)
        run: |
          dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
            -c Release \
            -r osx-x64 \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:Version=${{ env.APP_VERSION }} \
            -o publish/osx-x64
      
      - name: Create Universal Binary
        run: |
          lipo -create -output publish/ShieldPrompt \
            publish/osx-x64/ShieldPrompt \
            publish/osx-arm64/ShieldPrompt
      
      - name: Create .app Bundle
        run: |
          mkdir -p "ShieldPrompt.app/Contents/MacOS"
          mkdir -p "ShieldPrompt.app/Contents/Resources"
          
          cp publish/ShieldPrompt "ShieldPrompt.app/Contents/MacOS/"
          cp installers/macos/Info.plist "ShieldPrompt.app/Contents/"
          cp installers/macos/icon.icns "ShieldPrompt.app/Contents/Resources/"
      
      - name: Sign macOS App
        if: env.APPLE_CERT_PASSWORD != ''
        env:
          APPLE_CERT_PASSWORD: ${{ secrets.APPLE_CERT_PASSWORD }}
          APPLE_TEAM_ID: ${{ secrets.APPLE_TEAM_ID }}
        run: |
          # Import certificate
          security import certificate.p12 -P ${{ env.APPLE_CERT_PASSWORD }}
          
          # Sign app
          codesign --deep --force --verify --verbose \
            --sign "Developer ID Application: Your Company (TEAM_ID)" \
            ShieldPrompt.app
          
          # Notarize (required for macOS 10.15+)
          xcrun notarytool submit ShieldPrompt.app \
            --apple-id ${{ secrets.APPLE_ID }} \
            --password ${{ secrets.APPLE_APP_PASSWORD }} \
            --team-id ${{ env.APPLE_TEAM_ID }} \
            --wait
      
      - name: Create DMG
        run: |
          brew install create-dmg
          
          create-dmg \
            --volname "ShieldPrompt" \
            --volicon "installers/macos/icon.icns" \
            --window-pos 200 120 \
            --window-size 800 400 \
            --icon-size 100 \
            --icon "ShieldPrompt.app" 200 190 \
            --hide-extension "ShieldPrompt.app" \
            --app-drop-link 600 185 \
            --background "installers/macos/dmg-background.png" \
            "publish/ShieldPrompt-${{ env.APP_VERSION }}-osx-universal.dmg" \
            "ShieldPrompt.app"
      
      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: macos-build
          path: publish/ShieldPrompt-*.dmg

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Run tests
        run: dotnet test --verbosity normal
      
      - name: Publish Linux x64
        run: |
          dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
            -c Release \
            -r linux-x64 \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:Version=${{ env.APP_VERSION }} \
            -o publish/linux-x64
      
      - name: Create AppImage
        run: |
          # Download appimagetool
          wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
          chmod +x appimagetool-x86_64.AppImage
          
          # Create AppDir structure
          mkdir -p AppDir/usr/bin
          mkdir -p AppDir/usr/share/applications
          mkdir -p AppDir/usr/share/icons/hicolor/256x256/apps
          
          # Copy files
          cp publish/linux-x64/ShieldPrompt AppDir/usr/bin/
          cp installers/linux/shieldprompt.desktop AppDir/usr/share/applications/
          cp installers/linux/icon.png AppDir/usr/share/icons/hicolor/256x256/apps/shieldprompt.png
          cp installers/linux/AppRun AppDir/
          
          # Build AppImage
          ./appimagetool-x86_64.AppImage AppDir \
            publish/ShieldPrompt-${{ env.APP_VERSION }}-linux-x64.AppImage
      
      - name: Create DEB Package
        run: |
          mkdir -p deb-package/DEBIAN
          mkdir -p deb-package/usr/bin
          mkdir -p deb-package/usr/lib/shieldprompt
          mkdir -p deb-package/usr/share/applications
          mkdir -p deb-package/usr/share/icons
          
          # Copy executable
          cp publish/linux-x64/ShieldPrompt deb-package/usr/lib/shieldprompt/
          ln -s ../lib/shieldprompt/ShieldPrompt deb-package/usr/bin/shieldprompt
          
          # Copy desktop entry
          cp installers/linux/shieldprompt.desktop deb-package/usr/share/applications/
          cp installers/linux/icon.png deb-package/usr/share/icons/
          
          # Create control file
          cat > deb-package/DEBIAN/control << EOF
          Package: shieldprompt
          Version: ${{ env.APP_VERSION }}
          Section: utils
          Priority: optional
          Architecture: amd64
          Maintainer: ShieldPrompt Team <team@shieldprompt.io>
          Description: Secure AI prompt generation with automatic data protection
           ShieldPrompt is a desktop application that enables safe use of AI coding
           assistants like ChatGPT by automatically sanitizing sensitive data before
           copying and restoring it after pasting.
          EOF
          
          # Build DEB
          dpkg-deb --build deb-package \
            publish/ShieldPrompt-${{ env.APP_VERSION }}-amd64.deb
      
      - name: Create RPM Package
        run: |
          sudo apt-get update
          sudo apt-get install -y rpm
          
          # Create RPM build structure
          mkdir -p ~/rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}
          
          # Create spec file (see below for full spec)
          cp installers/linux/shieldprompt.spec ~/rpmbuild/SPECS/
          
          # Copy source
          cp -r publish/linux-x64 ~/rpmbuild/SOURCES/
          
          # Build RPM
          rpmbuild -bb ~/rpmbuild/SPECS/shieldprompt.spec
          
          # Copy output
          cp ~/rpmbuild/RPMS/x86_64/shieldprompt-*.rpm \
            publish/ShieldPrompt-${{ env.APP_VERSION }}-x86_64.rpm
      
      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: linux-build
          path: |
            publish/ShieldPrompt-*.AppImage
            publish/ShieldPrompt-*.deb
            publish/ShieldPrompt-*.rpm

  create-release:
    needs: [build-windows, build-macos, build-linux]
    runs-on: ubuntu-latest
    steps:
      - name: Download all artifacts
        uses: actions/download-artifact@v4
      
      - name: Generate checksums
        run: |
          cd windows-build && sha256sum * > SHA256SUMS
          cd ../macos-build && sha256sum * > SHA256SUMS
          cd ../linux-build && sha256sum * > SHA256SUMS
      
      - name: Generate changelog
        id: changelog
        run: |
          # Extract changelog from git commits since last tag
          git log $(git describe --tags --abbrev=0 HEAD^)..HEAD \
            --pretty=format:"- %s" > CHANGELOG.txt
      
      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          name: ShieldPrompt ${{ env.APP_VERSION }}
          body_path: CHANGELOG.txt
          draft: false
          prerelease: false
          files: |
            windows-build/*
            macos-build/*
            linux-build/*
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

---

## 📦 Installer File Structures

### **Windows WiX Configuration**

**File:** `installers/windows/ShieldPrompt.wxs`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package 
    Name="ShieldPrompt"
    Version="$(var.Version)"
    Manufacturer="YOLOVibeCode"
    UpgradeCode="YOUR-GUID-HERE"
    Language="1033">
    
    <MajorUpgrade 
      DowngradeErrorMessage="A newer version is already installed." 
      AllowSameVersionUpgrades="yes" />
    
    <Feature Id="MainApplication" Title="ShieldPrompt" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
      <ComponentGroupRef Id="StartMenuShortcuts" />
    </Feature>
    
    <StandardDirectory Id="ProgramFiles6432Folder">
      <Directory Id="INSTALLFOLDER" Name="ShieldPrompt">
        <Component Id="MainExecutable">
          <File 
            Id="ShieldPromptExe" 
            Source="../../publish/win-x64/ShieldPrompt.exe" 
            KeyPath="yes">
            <Shortcut 
              Id="StartMenuShortcut"
              Directory="ProgramMenuFolder"
              Name="ShieldPrompt"
              Description="Secure AI Prompt Generation"
              WorkingDirectory="INSTALLFOLDER"
              Icon="AppIcon.exe" />
          </File>
        </Component>
        
        <Component Id="Configuration">
          <File Source="../../config/default-patterns.yaml" />
        </Component>
      </Directory>
    </StandardDirectory>
    
    <Icon Id="AppIcon.exe" SourceFile="../../src/ShieldPrompt.App/Assets/icon.ico" />
    
    <Property Id="ARPPRODUCTICON" Value="AppIcon.exe" />
    <Property Id="ARPURLINFOABOUT" Value="https://github.com/YOLOVibeCode/shield-prompt" />
  </Package>
</Wix>
```

### **macOS Info.plist**

**File:** `installers/macos/Info.plist`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>ShieldPrompt</string>
    
    <key>CFBundleDisplayName</key>
    <string>ShieldPrompt</string>
    
    <key>CFBundleIdentifier</key>
    <string>com.yolovibecode.shieldprompt</string>
    
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    
    <key>CFBundleExecutable</key>
    <string>ShieldPrompt</string>
    
    <key>CFBundleIconFile</key>
    <string>icon.icns</string>
    
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    
    <key>NSHighResolutionCapable</key>
    <true/>
    
    <key>NSRequiresAquaSystemAppearance</key>
    <false/>
</dict>
</plist>
```

### **Linux Desktop Entry**

**File:** `installers/linux/shieldprompt.desktop`

```ini
[Desktop Entry]
Type=Application
Name=ShieldPrompt
GenericName=Secure AI Prompt Generator
Comment=Sanitize code before sharing with AI
Exec=shieldprompt %F
Icon=shieldprompt
Terminal=false
Categories=Development;Utility;Security;
Keywords=ai;chatgpt;security;sanitization;prompt;
StartupWMClass=ShieldPrompt
```

### **Linux RPM Spec**

**File:** `installers/linux/shieldprompt.spec`

```spec
Name:           shieldprompt
Version:        1.0.0
Release:        1%{?dist}
Summary:        Secure AI prompt generation with automatic data protection

License:        MIT
URL:            https://github.com/YOLOVibeCode/shield-prompt
Source0:        %{name}-%{version}.tar.gz

BuildRequires:  dotnet-sdk-10.0
Requires:       dotnet-runtime-10.0

%description
ShieldPrompt is a desktop application that enables safe use of AI coding
assistants like ChatGPT by automatically sanitizing sensitive data before
copying and restoring it after pasting.

%prep
%setup -q

%build
dotnet publish -c Release -r linux-x64 --self-contained

%install
mkdir -p %{buildroot}%{_bindir}
mkdir -p %{buildroot}%{_libdir}/shieldprompt
mkdir -p %{buildroot}%{_datadir}/applications
mkdir -p %{buildroot}%{_datadir}/icons/hicolor/256x256/apps

cp publish/linux-x64/ShieldPrompt %{buildroot}%{_libdir}/shieldprompt/
ln -s %{_libdir}/shieldprompt/ShieldPrompt %{buildroot}%{_bindir}/shieldprompt

cp installers/linux/shieldprompt.desktop %{buildroot}%{_datadir}/applications/
cp installers/linux/icon.png %{buildroot}%{_datadir}/icons/hicolor/256x256/apps/shieldprompt.png

%files
%license LICENSE
%doc README.md
%{_bindir}/shieldprompt
%{_libdir}/shieldprompt/
%{_datadir}/applications/shieldprompt.desktop
%{_datadir}/icons/hicolor/256x256/apps/shieldprompt.png

%changelog
* Wed Jan 14 2026 ShieldPrompt Team <team@shieldprompt.io> - 1.0.0-1
- Initial release
- 14 sanitization patterns
- Visual protection preview
- Intelligent undo/redo
```

---

## 🔄 Auto-Update Architecture

### **Strategy: GitHub Releases as Update Server**

**Flow:**
```
1. App checks GitHub API for latest release on startup
2. Compares current version with latest
3. If update available → Shows notification
4. User clicks "Update" → Downloads installer
5. Closes app → Runs installer → Reopens updated version
```

**Implementation:**

**File:** `src/ShieldPrompt.Infrastructure/Services/UpdateChecker.cs`

```csharp
public interface IUpdateChecker
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task<string> DownloadUpdateAsync(UpdateInfo update, IProgress<int> progress);
}

public record UpdateInfo(
    string Version,
    string DownloadUrl,
    string ReleaseNotes,
    DateTime PublishedAt,
    long SizeBytes);

public class GitHubUpdateChecker : IUpdateChecker
{
    private const string RepoOwner = "YOLOVibeCode";
    private const string RepoName = "shield-prompt";
    private readonly HttpClient _httpClient;

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        var currentVersion = Assembly.GetEntryAssembly()!
            .GetName().Version!;

        var url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var release = await response.Content
            .ReadFromJsonAsync<GitHubRelease>();

        var latestVersion = Version.Parse(release.TagName.TrimStart('v'));
        
        if (latestVersion > currentVersion)
        {
            var asset = GetAssetForPlatform(release.Assets);
            return new UpdateInfo(
                release.TagName,
                asset.BrowserDownloadUrl,
                release.Body,
                release.PublishedAt,
                asset.Size);
        }

        return null; // Already on latest
    }
}
```

**UI Integration:**

```csharp
// In MainWindowViewModel
[ObservableProperty]
private UpdateInfo? _availableUpdate;

[ObservableProperty]
private bool _showUpdateNotification;

private async Task CheckForUpdatesAsync()
{
    var update = await _updateChecker.CheckForUpdatesAsync();
    if (update != null)
    {
        AvailableUpdate = update;
        ShowUpdateNotification = true;
    }
}

[RelayCommand]
private async Task InstallUpdateAsync()
{
    var installerPath = await _updateChecker
        .DownloadUpdateAsync(AvailableUpdate!, new Progress<int>(p => 
        {
            StatusText = $"Downloading update... {p}%";
        }));

    // Close app and run installer
    Process.Start(installerPath);
    Application.Current?.Shutdown();
}
```

---

## 🏷️ Versioning Strategy

### **Semantic Versioning (semver.org):**

**Format:** `MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]`

**Examples:**
```
v1.0.0          - Initial release
v1.0.1          - Bug fix
v1.1.0          - New feature (formatters)
v1.2.0          - New feature (undo/redo)
v2.0.0          - Breaking change (API redesign)
v1.0.0-alpha.1  - Pre-release
v1.0.0-beta.2   - Beta release
v1.0.0-rc.1     - Release candidate
v1.0.0+20260114 - Build metadata
```

### **Git Tag Strategy:**

**Branching Model:**
```
main            - Production releases only (v1.0.0, v1.1.0)
develop         - Integration branch
feature/*       - Feature branches
release/*       - Release preparation
hotfix/*        - Emergency fixes
```

**Release Process:**
```bash
# 1. Create release branch from develop
git checkout develop
git pull
git checkout -b release/v1.0.0

# 2. Update version numbers
# Update src/ShieldPrompt.App/ShieldPrompt.App.csproj
# Update CHANGELOG.md

# 3. Commit version bump
git commit -am "chore: bump version to 1.0.0"

# 4. Merge to main
git checkout main
git merge release/v1.0.0 --no-ff

# 5. Create tag
git tag -a v1.0.0 -m "Release version 1.0.0

Features:
- Visual protection preview
- Intelligent undo/redo
- Settings persistence
- 14 sanitization patterns

Tests: 180/180 passing"

# 6. Push tag (triggers CI/CD)
git push origin main
git push origin v1.0.0

# 7. Merge back to develop
git checkout develop
git merge main
git push origin develop
```

---

## 📋 Release Checklist Template

### **Pre-Release:**
- [ ] All tests passing (180/180)
- [ ] Version bumped in `.csproj`
- [ ] CHANGELOG.md updated
- [ ] Documentation reviewed
- [ ] Security audit complete
- [ ] Performance benchmarks met
- [ ] Cross-platform testing done

### **Release:**
- [ ] Create release branch
- [ ] Version commit
- [ ] Merge to main
- [ ] Create git tag
- [ ] Push tag (triggers builds)
- [ ] Monitor GitHub Actions
- [ ] Verify artifacts generated

### **Post-Release:**
- [ ] Test installers on all platforms
- [ ] Update website/docs
- [ ] Announce on social media
- [ ] Notify enterprise customers
- [ ] Monitor for issues
- [ ] Close milestone

---

## 🔐 Code Signing Requirements

### **Windows (Authenticode):**
**Required:** Code signing certificate from trusted CA
- DigiCert, Sectigo, GlobalSign
- Cost: $200-$800/year
- EV certificate recommended ($300-$500/year)

**Process:**
```bash
signtool sign /f certificate.pfx /p PASSWORD \
  /tr http://timestamp.digicert.com /td sha256 \
  /fd sha256 \
  ShieldPrompt.exe
```

### **macOS (Apple Developer ID):**
**Required:** Apple Developer Program membership
- Cost: $99/year
- Developer ID Application certificate

**Process:**
```bash
# Sign
codesign --deep --force --verify --verbose \
  --sign "Developer ID Application: Your Name (TEAM_ID)" \
  --options runtime \
  ShieldPrompt.app

# Notarize (required for macOS 10.15+)
xcrun notarytool submit ShieldPrompt.app \
  --apple-id apple@example.com \
  --password @keychain:AC_PASSWORD \
  --team-id TEAM_ID \
  --wait

# Staple
xcrun stapler staple ShieldPrompt.app
```

### **Linux:**
**Optional:** GPG signing for package verification

```bash
# Sign DEB
dpkg-sig --sign builder ShieldPrompt-1.0.0-amd64.deb

# Sign RPM
rpm --addsign ShieldPrompt-1.0.0-x86_64.rpm
```

---

## 📊 Distribution Channels

### **Primary: GitHub Releases**
- Automatic via GitHub Actions
- Free hosting
- Version tracking
- Download analytics
- Checksums included

### **Secondary Channels:**

#### **Windows:**
- [ ] Microsoft Store (MSIX submission)
- [ ] Chocolatey package manager
- [ ] WinGet repository
- [ ] Scoop bucket

#### **macOS:**
- [ ] Homebrew cask
  ```bash
  brew install --cask shieldprompt
  ```

#### **Linux:**
- [ ] Snap Store
- [ ] Flathub (Flatpak)
- [ ] AUR (Arch User Repository)

---

## 🎯 Deployment Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                   Developer Workflow                            │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
                   Code Changes Merged
                            │
                            ▼
                    Ready for Release?
                            │
                    YES     │
                            ▼
              git tag -a v1.0.0 -m "Release"
                            │
                            ▼
                   git push origin v1.0.0
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                   GitHub Actions Triggered                       │
├─────────────────────────────────────────────────────────────────┤
│  1. Checkout code                                               │
│  2. Run all tests (180)           ← MUST PASS                   │
│  3. Build for all platforms       ← Parallel builds             │
│  4. Create installers             ← Platform-specific           │
│  5. Sign binaries                 ← Code signing                │
│  6. Generate checksums            ← Security                    │
│  7. Create GitHub Release         ← Automatic                   │
│  8. Upload artifacts              ← MSI, DMG, AppImage, etc.    │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     GitHub Release Created                       │
├─────────────────────────────────────────────────────────────────┤
│  • ShieldPrompt-1.0.0-win-x64.msi                               │
│  • ShieldPrompt-1.0.0-osx-universal.dmg                         │
│  • ShieldPrompt-1.0.0-linux-x64.AppImage                        │
│  • ShieldPrompt-1.0.0-amd64.deb                                 │
│  • ShieldPrompt-1.0.0-x86_64.rpm                                │
│  • SHA256SUMS                                                   │
│  • Release notes                                                │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
                    Users Download
                            │
                    ┌───────┴────────┐
                    ▼                ▼
              One-Click         Auto-Update
               Install          Notification
```

---

## 🎯 Enterprise Deployment

### **For IT Departments:**

**Option 1: MSI Silent Install (Windows)**
```powershell
# Silent install for all users
msiexec /i ShieldPrompt-1.0.0-win-x64.msi /quiet /qn

# Via Group Policy
# Deploy through GPO Software Installation
```

**Option 2: DMG/PKG via MDM (macOS)**
```bash
# Install via Jamf Pro, Intune, or Workspace ONE
sudo installer -pkg ShieldPrompt-1.0.0-osx-universal.pkg -target /
```

**Option 3: DEB via APT Repository**
```bash
# Host internal APT repository
# Add to sources.list
echo "deb https://packages.company.com/shieldprompt stable main" | \
  sudo tee /etc/apt/sources.list.d/shieldprompt.list

# Install
sudo apt update
sudo apt install shieldprompt
```

---

## 📊 Monitoring & Analytics

### **Telemetry (Optional, Opt-In Only):**

**Metrics to Track:**
- Version distribution
- Platform usage (Windows/Mac/Linux)
- Feature usage (format types, sanitization categories)
- Error rates
- Update adoption rate

**Implementation:**
```csharp
public interface ITelemetryService
{
    Task TrackEventAsync(string eventName, Dictionary<string, string> properties);
    Task TrackExceptionAsync(Exception ex);
}

// Privacy-first: Only anonymous metrics
// User can disable in settings
// No PII ever collected
```

**Tools:**
- Application Insights (Azure)
- Google Analytics (events only)
- Self-hosted Matomo
- Or none (privacy-first default)

---

## 🎁 Release Assets Structure

### **GitHub Release v1.0.0:**

```
📦 ShieldPrompt v1.0.0
├── 📄 Release Notes
│   └── Changelog, breaking changes, migration guide
│
├── 💾 Windows
│   ├── ShieldPrompt-1.0.0-win-x64.msi          (signed)
│   ├── ShieldPrompt-1.0.0-win-arm64.msi        (signed)
│   └── ShieldPrompt-1.0.0-win-x64-portable.zip (no install)
│
├── 🍎 macOS
│   ├── ShieldPrompt-1.0.0-osx-universal.dmg    (signed, notarized)
│   └── ShieldPrompt-1.0.0-osx-universal.pkg    (for MDM)
│
├── 🐧 Linux
│   ├── ShieldPrompt-1.0.0-linux-x64.AppImage   (universal)
│   ├── ShieldPrompt-1.0.0-amd64.deb            (Debian/Ubuntu)
│   └── ShieldPrompt-1.0.0-x86_64.rpm           (Fedora/RHEL)
│
├── 🔐 Verification
│   ├── SHA256SUMS                               (checksums)
│   └── SHA256SUMS.sig                           (GPG signature)
│
└── 📚 Documentation
    ├── README.md
    ├── CHANGELOG.md
    └── INSTALL.md
```

---

## 🎯 Installation Instructions

### **Windows:**
```
1. Download ShieldPrompt-1.0.0-win-x64.msi
2. Double-click to run installer
3. Follow wizard (Next → Next → Install)
4. Find "ShieldPrompt" in Start Menu
5. Done!
```

**Or via Package Manager:**
```powershell
# Chocolatey (after package submission)
choco install shieldprompt

# WinGet (after package submission)
winget install YOLOVibeCode.ShieldPrompt
```

### **macOS:**
```
1. Download ShieldPrompt-1.0.0-osx-universal.dmg
2. Double-click to mount
3. Drag ShieldPrompt.app to Applications folder
4. Open from Applications or Spotlight
5. Done!
```

**Or via Homebrew:**
```bash
# After cask submission
brew install --cask shieldprompt
```

### **Linux:**

**AppImage (Any Distro):**
```bash
# Download
wget https://github.com/YOLOVibeCode/shield-prompt/releases/download/v1.0.0/ShieldPrompt-1.0.0-linux-x64.AppImage

# Make executable
chmod +x ShieldPrompt-1.0.0-linux-x64.AppImage

# Run
./ShieldPrompt-1.0.0-linux-x64.AppImage
```

**Debian/Ubuntu:**
```bash
wget https://github.com/YOLOVibeCode/shield-prompt/releases/download/v1.0.0/ShieldPrompt-1.0.0-amd64.deb
sudo dpkg -i ShieldPrompt-1.0.0-amd64.deb
```

**Fedora/RHEL:**
```bash
wget https://github.com/YOLOVibeCode/shield-prompt/releases/download/v1.0.0/ShieldPrompt-1.0.0-x86_64.rpm
sudo rpm -i ShieldPrompt-1.0.0-x86_64.rpm
```

---

## 📋 Continuous Deployment Phases

### **Phase 1: Manual Releases (Current)**
- Developer creates tag manually
- GitHub Actions builds automatically
- Manual testing before publish

### **Phase 2: Automated Pre-Releases**
- Every merge to develop → Pre-release build
- Auto-deployed to test channel
- Beta testers get automatic updates

### **Phase 3: Continuous Deployment**
- Hotfix branches → Patch releases
- Feature branches → Beta releases
- Main branch → Stable releases
- Fully automated with approval gates

---

## 🎯 First Release Plan

### **Release v1.0.0 - "Shield Launch"**

**Date:** January 2026  
**Focus:** Core features, stability, documentation

**Steps:**
1. Final testing on all platforms
2. Create release branch
3. Version bump to 1.0.0
4. Update CHANGELOG.md
5. Create git tag
6. Push tag → Triggers builds
7. Verify all artifacts
8. Publish release
9. Announce!

**Deliverables:**
- 5 installer types
- Complete documentation
- 180 tests passing
- Code signing complete
- Auto-update ready

---

## 📞 Support & Maintenance

### **Update Cadence:**

| Type | Frequency | Examples |
|------|-----------|----------|
| **Patch** | As needed | Bug fixes, security patches |
| **Minor** | Monthly | New features, UX improvements |
| **Major** | Yearly | Breaking changes, redesigns |

### **Long-Term Support:**
- v1.x.x supported for 2 years
- Security patches for critical issues
- Community-driven bug fixes

---

## 🎊 Summary

**ShieldPrompt is ready for automated deployment with:**

✅ **Multi-platform builds** (Windows/Mac/Linux)  
✅ **Professional installers** (MSI/DMG/AppImage/DEB/RPM)  
✅ **Code signing** (Authenticode, Apple Developer ID)  
✅ **Auto-update system** (GitHub Releases API)  
✅ **Semantic versioning** (Git tag-driven)  
✅ **CI/CD pipeline** (GitHub Actions)  
✅ **Distribution channels** (Direct, package managers)  
✅ **Enterprise deployment** (Silent install, MDM)  

**Next Step:** Create the GitHub Actions workflow files and installer configurations!

---

**Ready for ENGINEER to implement the automation!** 🚀

ROLE: architect STRICT=true
