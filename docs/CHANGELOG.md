# Changelog

All notable changes to ShieldPrompt will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Windows MSI installer with WiX
- macOS DMG with custom background
- Code signing for Windows and macOS
- Homebrew cask formula
- Chocolatey package
- Audit logging to SQLite

## [1.1.0] - 2026-01-14

### Added

#### **🎉 MAJOR: Auto File Updates from AI** (THE KILLER FEATURE!)
The complete automation loop is now closed! No more manual file editing!

- **AI Response Parser** - Extracts file operations from ChatGPT/Claude responses
  - Supports ChatGPT markdown format (`**Program.cs**`)
  - Supports Claude format (`` `Program.cs` ``)
  - Supports comment format (`// File: Program.cs`)
  - Auto-maps to original files when no markers present
  - Detects new file creation vs updates
  
- **File Writer Service** - Safely applies file changes
  - Creates new files with content
  - Updates existing files
  - Creates nested directories automatically
  - Deletes files (with explicit permission)
  - Automatic backups before ANY modification
  - Complete backup/restore capability
  - Path validation (security)
  - Atomic operations where possible
  
- **"Apply to Files" Button** in Paste & Restore dialog
  - One-click file updates
  - Shows preview of files that will be modified
  - Displays file type (Create/Update/Delete)
  - Shows estimated lines changed
  - Progress indicators
  - Success/error messages
  - Integrates with undo/redo

#### **Live Counters**
- **SelectedFileCount** now updates instantly when files checked/unchecked
- **TotalTokens** shows estimated count in real-time
- No more waiting for copy to see numbers!
- Event-driven updates (efficient)

#### **Professional Menu Bar**
- **File Menu:** Open, Refresh, Copy, Paste & Restore, Exit
- **Edit Menu:** Undo, Redo, Select All, Deselect All
- **View Menu:** Toggle Shield/Toolbar/Status Bar, Font Size controls
- **Tools Menu:** Load Tutorial (Ctrl+T), Clear Session, Diagnostics, View Logs
- **Help Menu:** User Guide (F1), Tutorial, Shortcuts, GitHub, Updates, About
- All keyboard shortcuts work from menu

#### **Tutorial Project**
- Added `samples/tutorial-project/` with realistic sample code
- 3 C# files + 1 JSON config with 34 fake secrets
- Step-by-step walkthrough guide
- Expected outputs for verification
- Verified metrics: 2,381 tokens, 34 secrets detected
- One-click load via Tools → Load Tutorial (Ctrl+T)

#### **User Guide**
- Complete usage documentation (`docs/USER_GUIDE.md`)
- Installation instructions (Windows/Mac/Linux)
- Quick start guide
- Workflow explanations
- Keyboard shortcuts reference
- Troubleshooting section
- Ready for screenshots (structure in place)

#### **Undo Confirmation for File Changes**
- Ctrl+Z after "Apply to Files" shows confirmation
- Warning message: "⚠️ This will undo AI file changes: X files modified"
- User can confirm or cancel
- Prevents accidental restoration
- Restores from automatic backup on confirm

### Improved

#### **Enhanced Paste & Restore Dialog**
- Now shows file update preview list
- Displays which files will be modified
- Shows operation type (Create/Update/Delete)
- Shows estimated lines changed
- Dynamic button text: "Apply to X File(s)"

#### **Better Status Messages**
- Real-time file count
- Live token estimates
- Sanitization counts
- Clear progress indicators
- Detailed diagnostics

### Technical

#### **New Interfaces (ISP-Compliant):**
- `IAIResponseParser` (1 method) - Parse AI responses
- `IFileWriterService` (4 methods) - File operations
- `IUndoableAction` enhanced with `RequiresConfirmation` and `ConfirmationMessage`
- `IUndoRedoManager` enhanced with `PeekUndo()`

#### **New Services:**
- `AIResponseParser` - Smart response parsing
- `FileWriterService` - Safe file operations
- `FileUpdateAction` - Undoable file changes

#### **Test Suite Growth:**
- **Total: 268 tests** (was 180)
- Added 88 new tests (+48%)
- All TDD (tests first)
- All passing ✅

#### **Test Breakdown:**
- AI Response Parser: 10 tests
- File Writer Service: 12 tests
- File System Manipulation: 27 tests
- File Update Action: 6 tests
- Live Counter Tests: 18 tests
- Paste/Restore Workflow: 9 tests
- Tutorial Integration: 6 tests

### Security

- Path validation prevents directory traversal attacks
- Automatic backups prevent data loss
- Undo confirmation for important operations
- Fail-fast validation (all-or-nothing)
- Backup manifests for audit trail

### Performance

- Event-driven counter updates (no polling)
- Efficient recursive algorithms
- Background token counting
- Cached tokenizers

## [1.0.3] - 2026-01-14

### Fixed
- **AppImage Desktop File Location** - Fixed "Desktop file not found" error
  - Root cause: appimagetool expects desktop file at AppDir root
  - Previously only copied to AppDir/usr/share/applications/
  - Solution: Copy desktop file + icon to AppDir root as well
  - **Result:** AppImage packaging now follows standard conventions

### Technical
- Added desktop file to AppDir root (required by appimagetool)
- Added icon to AppDir root for proper AppImage metadata
- All 180 tests still passing
- FUSE workaround from v1.0.2 still in effect

### References
- AppImage specification requires desktop file at AppDir root
- Error: "Desktop file not found, aborting"

## [1.0.2] - 2026-01-14

### Fixed
- **Linux AppImage Build Failure** - Fixed FUSE dependency issue
  - Root cause: `appimagetool-x86_64.AppImage` requires FUSE (libfuse.so.2)
  - GitHub Actions runners don't have FUSE installed by default
  - Solution: Extract appimagetool with `--appimage-extract` flag
  - Now uses `./squashfs-root/AppRun` instead of direct AppImage execution
  - **Result:** Linux builds now succeed without FUSE dependency

### Technical
- Analysis performed using GitHub CLI (`gh`) for exact error diagnosis
- Implemented industry-standard AppImage workaround
- No changes to functionality - pure CI/CD fix
- All 180 tests still passing

### References
- Error: `dlopen(): error loading libfuse.so.2`
- Solution documented in: `LINUX_BUILD_FAILURE_ANALYSIS.md`
- AppImage wiki: https://github.com/AppImage/AppImageKit/wiki/FUSE

## [1.0.1] - 2026-01-14

### Fixed
- **GitHub Actions Build Failures** - Fixed executable naming inconsistencies
  - Added AssemblyName property for clean executable names
  - Windows now outputs `ShieldPrompt.exe` (was `ShieldPrompt.App.exe`)
  - macOS/Linux now output `ShieldPrompt` (was `ShieldPrompt.App`)
  
### Added
- **Application Icons** - Created professional shield icon assets
  - macOS: icon.icns (multi-resolution)
  - Linux: icon.png (256x256)
  - SVG source for future modifications
  
### Improved
- **GitHub Actions Workflow** - Enhanced with validation and debugging
  - Added verification steps after each build
  - Added debug output on failures
  - Added chmod +x for Linux executables
  - Improved error messages

### Technical
- Build script updated to use clean executable names
- All 180 tests still passing
- No breaking changes to functionality

## [1.0.0] - 2026-01-14

### Added - Core Features
- File aggregation with recursive directory loading
- Smart exclusions (node_modules, .git, binaries, etc.)
- Binary file detection
- Token counting with TiktokenSharp
- Model profiles (GPT-4o, Claude 3.5, Gemini 2.5, DeepSeek V3)
- Context limit checking and warnings

### Added - Security (14 Patterns)
- Server/Database name detection
- Private IP address detection (RFC 1918)
- Connection string detection
- Windows file path detection
- Internal hostname detection
- Social Security Number (SSN) detection
- Credit card number detection
- AWS access key detection
- GitHub token detection
- OpenAI API key detection
- Anthropic API key detection
- Private key (PEM) detection
- Password in code detection
- JWT bearer token detection

### Added - Sanitization Engine
- Automatic sanitization on copy to clipboard
- Alias generation (DATABASE_0, IP_ADDRESS_0, etc.)
- Secure mapping session (in-memory only)
- Pattern registry with enable/disable
- Priority-based pattern processing
- ReDoS protection (100ms timeout)
- Thread-safe operations

### Added - Desanitization
- Automatic value restoration from aliases
- Round-trip verification
- Occurrence counting
- Mapping lookup with session

### Added - User Interface
- Interactive file tree with checkboxes
- File type icons
- Token count per file
- Folder expand/collapse
- Folder picker dialog
- Three output formats (Plain Text, Markdown, XML)
- Format selection dropdown
- Model selection dropdown
- Status bar with live statistics
- Loading indicators

### Added - Protection Preview
- Visual shield panel (collapsible)
- Before/after sanitization preview
- Category icons (database, IP, keys, PII)
- Grouped by type
- Shows top 5 per category

### Added - Paste & Restore Dialog
- AI response input
- Auto-paste from clipboard
- Detected aliases list
- Occurrence count display
- Restoration preview
- One-click copy restored content

### Added - UX Enhancements
- Settings persistence (JSON)
- Auto-restore last folder on startup
- Auto-restore file selections
- Auto-restore format/model preferences
- Intelligent undo/redo system
- Smart action batching (2-second window)
- Keyboard shortcuts (Ctrl+O/C/V/Z/Y/R, F5)
- Tooltips on all buttons
- Contextual status messages
- Error recovery

### Technical
- Clean Architecture (Domain → Application → Infrastructure)
- Interface Segregation Principle (all interfaces <10 methods)
- Test-Driven Development (180 tests, 100% passing)
- Dependency Injection throughout
- Async/await for all I/O operations
- Thread-safe services
- Null-safe with nullable reference types enabled

### Security
- Zero-knowledge architecture
- In-memory session only (no disk persistence)
- Secure disposal (memory overwrite)
- Session expiry (4-hour default)
- Thread-safe mapping operations
- Fail-secure error handling

### Documentation
- README.md - Quick start and features
- SPECIFICATION.md - Complete technical specification (1,147 lines)
- USE_CASES.md - Enterprise scenarios and compliance
- EXECUTIVE_SUMMARY.md - For decision makers
- DEPLOYMENT_ARCHITECTURE.md - Build and distribution
- .cursorrules - Development best practices (665 lines)
- Multiple phase completion documents
- COMPLETE.md - Final deliverables

## [0.1.0] - 2026-01-14 (Internal)

### Added
- Initial project structure
- Domain models
- Basic UI shell

---

## Version History

- **v1.0.0** (2026-01-14) - Initial public release

