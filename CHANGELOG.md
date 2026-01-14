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

