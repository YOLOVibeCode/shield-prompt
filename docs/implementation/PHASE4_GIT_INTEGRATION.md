# Phase 4: Git Integration Implementation Specification

**Phase ID:** PHASE-4  
**Priority:** P1 (High Value Feature)  
**Estimated Effort:** 3-4 days  
**Prerequisites:** Phase 1, Phase 3  
**Status:** PENDING

---

## 1. Executive Summary

Git Integration enhances the file tree with version control awareness. Users can see modified/added/deleted files, filter by git status, and leverage git context for smarter prompt generation.

---

## 2. Feature Requirements

### 2.1 Core Features

| Feature | Description | Priority |
|---------|-------------|----------|
| Git Status Display | Show file status icons (M/A/D/?) | P0 |
| Status Filtering | Filter tree by status (modified, staged, etc.) | P1 |
| Branch Display | Show current branch in status bar | P0 |
| Diff Preview | Show git diff in preview (optional) | P2 |
| Ignore Patterns | Respect .gitignore | P0 |
| Changed Files Quick Select | Button to select all modified files | P1 |

### 2.2 Git Status Indicators

| Status | Icon | Color | Description |
|--------|------|-------|-------------|
| Modified | M | Yellow | File modified but not staged |
| Staged | S | Green | File staged for commit |
| Added | A | Green | New file (untracked) |
| Deleted | D | Red | File deleted |
| Renamed | R | Blue | File renamed |
| Untracked | ? | Gray | Not tracked by git |
| Ignored | ⊘ | Dark Gray | In .gitignore |

---

## 3. Domain Model Specification

### 3.1 GitFileStatus Enum (NEW)

**File:** `src/ShieldPrompt.Domain/Enums/GitFileStatus.cs`

```csharp
namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Git status for a file.
/// </summary>
[Flags]
public enum GitFileStatus
{
    None = 0,
    Modified = 1,
    Staged = 2,
    Added = 4,
    Deleted = 8,
    Renamed = 16,
    Untracked = 32,
    Ignored = 64,
    Conflict = 128
}
```

### 3.2 GitRepositoryInfo Record (NEW)

**File:** `src/ShieldPrompt.Domain/Records/GitRepositoryInfo.cs`

```csharp
namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Information about a git repository.
/// </summary>
public record GitRepositoryInfo(
    string RootPath,
    string CurrentBranch,
    string? RemoteUrl,
    bool HasUncommittedChanges,
    int ModifiedFileCount,
    int StagedFileCount,
    int UntrackedFileCount);
```

### 3.3 FileNode Enhancement

**File:** `src/ShieldPrompt.Domain/Entities/FileNode.cs` (MODIFY)

```csharp
// Add to existing FileNode class:

/// <summary>
/// Git status of this file. None if not in a git repository.
/// </summary>
public GitFileStatus GitStatus { get; set; } = GitFileStatus.None;

/// <summary>
/// Whether this file is ignored by .gitignore.
/// </summary>
public bool IsGitIgnored => GitStatus.HasFlag(GitFileStatus.Ignored);
```

---

## 4. Interface Specifications (ISP-Compliant)

### 4.1 IGitStatusProvider (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IGitStatusProvider.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Provides git status information for files.
/// Follows ISP - git status queries only.
/// </summary>
public interface IGitStatusProvider
{
    /// <summary>
    /// Gets the git status for a file.
    /// </summary>
    GitFileStatus GetFileStatus(string filePath);
    
    /// <summary>
    /// Gets all modified files in a directory.
    /// </summary>
    IEnumerable<string> GetModifiedFiles(string directoryPath);
    
    /// <summary>
    /// Checks if a path is inside a git repository.
    /// </summary>
    bool IsInGitRepository(string path);
    
    /// <summary>
    /// Gets the repository root path for a given path.
    /// </summary>
    string? GetRepositoryRoot(string path);
}
```

### 4.2 IGitRepositoryService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IGitRepositoryService.cs`

**ISP Compliance:** 4 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Provides git repository information.
/// Follows ISP - repository metadata only.
/// </summary>
public interface IGitRepositoryService
{
    /// <summary>
    /// Gets repository information for a path.
    /// </summary>
    Task<GitRepositoryInfo?> GetRepositoryInfoAsync(string path, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the current branch name.
    /// </summary>
    string? GetCurrentBranch(string repositoryPath);
    
    /// <summary>
    /// Gets the list of branches.
    /// </summary>
    IEnumerable<string> GetBranches(string repositoryPath);
    
    /// <summary>
    /// Refreshes git status cache.
    /// </summary>
    Task RefreshStatusAsync(string repositoryPath, CancellationToken ct = default);
}
```

### 4.3 IGitIgnoreService (NEW)

**File:** `src/ShieldPrompt.Application/Interfaces/IGitIgnoreService.cs`

**ISP Compliance:** 3 methods ✅

```csharp
namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Handles .gitignore pattern matching.
/// Follows ISP - ignore pattern matching only.
/// </summary>
public interface IGitIgnoreService
{
    /// <summary>
    /// Checks if a path should be ignored.
    /// </summary>
    bool ShouldIgnore(string filePath, string repositoryRoot);
    
    /// <summary>
    /// Gets all ignore patterns for a repository.
    /// </summary>
    IEnumerable<string> GetIgnorePatterns(string repositoryRoot);
    
    /// <summary>
    /// Reloads ignore patterns from .gitignore files.
    /// </summary>
    void ReloadPatterns(string repositoryRoot);
}
```

---

## 5. Service Implementations

### 5.1 GitStatusProvider

**File:** `src/ShieldPrompt.Infrastructure/Services/GitStatusProvider.cs`

```csharp
namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// Git status provider using command-line git.
/// </summary>
public class GitStatusProvider : IGitStatusProvider
{
    private readonly Dictionary<string, Dictionary<string, GitFileStatus>> _statusCache = new();

    public GitFileStatus GetFileStatus(string filePath)
    {
        var repoRoot = GetRepositoryRoot(filePath);
        if (repoRoot is null) return GitFileStatus.None;
        
        if (!_statusCache.TryGetValue(repoRoot, out var cache))
        {
            cache = LoadStatusCache(repoRoot);
            _statusCache[repoRoot] = cache;
        }
        
        var relativePath = Path.GetRelativePath(repoRoot, filePath);
        return cache.TryGetValue(relativePath, out var status) ? status : GitFileStatus.None;
    }

    public IEnumerable<string> GetModifiedFiles(string directoryPath)
    {
        var repoRoot = GetRepositoryRoot(directoryPath);
        if (repoRoot is null) yield break;
        
        var result = RunGitCommand(repoRoot, "diff --name-only");
        foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            yield return Path.Combine(repoRoot, line.Trim());
        }
    }

    public bool IsInGitRepository(string path)
    {
        return GetRepositoryRoot(path) is not null;
    }

    public string? GetRepositoryRoot(string path)
    {
        var current = Path.GetDirectoryName(path) ?? path;
        while (!string.IsNullOrEmpty(current))
        {
            if (Directory.Exists(Path.Combine(current, ".git")))
                return current;
            current = Path.GetDirectoryName(current);
        }
        return null;
    }

    private Dictionary<string, GitFileStatus> LoadStatusCache(string repoRoot)
    {
        var cache = new Dictionary<string, GitFileStatus>();
        var result = RunGitCommand(repoRoot, "status --porcelain");
        
        foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length < 4) continue;
            
            var statusChars = line[..2];
            var filePath = line[3..].Trim();
            
            cache[filePath] = ParseStatus(statusChars);
        }
        
        return cache;
    }

    private static GitFileStatus ParseStatus(string statusChars)
    {
        var status = GitFileStatus.None;
        
        // Index status (first char)
        switch (statusChars[0])
        {
            case 'M': status |= GitFileStatus.Staged | GitFileStatus.Modified; break;
            case 'A': status |= GitFileStatus.Staged | GitFileStatus.Added; break;
            case 'D': status |= GitFileStatus.Staged | GitFileStatus.Deleted; break;
            case 'R': status |= GitFileStatus.Staged | GitFileStatus.Renamed; break;
        }
        
        // Working tree status (second char)
        switch (statusChars[1])
        {
            case 'M': status |= GitFileStatus.Modified; break;
            case 'D': status |= GitFileStatus.Deleted; break;
            case '?': status |= GitFileStatus.Untracked; break;
            case '!': status |= GitFileStatus.Ignored; break;
        }
        
        return status;
    }

    private static string RunGitCommand(string workingDir, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process is null) return string.Empty;
        
        return process.StandardOutput.ReadToEnd();
    }
}
```

---

## 6. ViewModel Specification

### 6.1 GitStatusViewModel (NEW)

**File:** `src/ShieldPrompt.App/ViewModels/V2/GitStatusViewModel.cs`

```csharp
namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel for git status display in status bar.
/// </summary>
public partial class GitStatusViewModel : ObservableObject
{
    private readonly IGitRepositoryService _gitService;
    
    [ObservableProperty]
    private bool _isGitRepository;
    
    [ObservableProperty]
    private string _currentBranch = string.Empty;
    
    [ObservableProperty]
    private int _modifiedFileCount;
    
    [ObservableProperty]
    private int _stagedFileCount;
    
    [ObservableProperty]
    private bool _hasUncommittedChanges;

    public GitStatusViewModel(IGitRepositoryService gitService)
    {
        _gitService = gitService;
    }

    public string BranchDisplay => IsGitRepository 
        ? $"⎇ {CurrentBranch}" 
        : string.Empty;

    public string StatusDisplay => IsGitRepository
        ? $"{ModifiedFileCount}M {StagedFileCount}S"
        : string.Empty;

    public async Task RefreshAsync(string workspacePath)
    {
        var info = await _gitService.GetRepositoryInfoAsync(workspacePath);
        
        if (info is null)
        {
            IsGitRepository = false;
            return;
        }
        
        IsGitRepository = true;
        CurrentBranch = info.CurrentBranch;
        ModifiedFileCount = info.ModifiedFileCount;
        StagedFileCount = info.StagedFileCount;
        HasUncommittedChanges = info.HasUncommittedChanges;
    }
}
```

### 6.2 FileNodeViewModel Enhancement

**File:** `src/ShieldPrompt.App/ViewModels/V2/FileNodeViewModel.cs` (MODIFY)

```csharp
// Add to existing FileNodeViewModel:

[ObservableProperty]
private GitFileStatus _gitStatus;

public string GitStatusIcon => GitStatus switch
{
    _ when GitStatus.HasFlag(GitFileStatus.Staged) => "●",
    _ when GitStatus.HasFlag(GitFileStatus.Modified) => "○",
    _ when GitStatus.HasFlag(GitFileStatus.Added) => "+",
    _ when GitStatus.HasFlag(GitFileStatus.Deleted) => "-",
    _ when GitStatus.HasFlag(GitFileStatus.Untracked) => "?",
    _ when GitStatus.HasFlag(GitFileStatus.Ignored) => "⊘",
    _ => string.Empty
};

public IBrush GitStatusColor => GitStatus switch
{
    _ when GitStatus.HasFlag(GitFileStatus.Staged) => Brushes.LightGreen,
    _ when GitStatus.HasFlag(GitFileStatus.Modified) => Brushes.Yellow,
    _ when GitStatus.HasFlag(GitFileStatus.Added) => Brushes.LightGreen,
    _ when GitStatus.HasFlag(GitFileStatus.Deleted) => Brushes.Red,
    _ when GitStatus.HasFlag(GitFileStatus.Untracked) => Brushes.Gray,
    _ when GitStatus.HasFlag(GitFileStatus.Ignored) => Brushes.DarkGray,
    _ => Brushes.Transparent
};
```

---

## 7. Test Specifications (TDD)

### 7.1 GitStatusProvider Tests

**File:** `tests/ShieldPrompt.Tests.Unit/Infrastructure/Services/GitStatusProviderTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.Infrastructure.Services;

public class GitStatusProviderTests
{
    [Fact]
    public void GetRepositoryRoot_WithGitRepo_ReturnsRoot()
    {
        // Arrange
        var provider = new GitStatusProvider();
        var testPath = Path.Combine(Path.GetTempPath(), "test-git-repo");
        Directory.CreateDirectory(Path.Combine(testPath, ".git"));

        try
        {
            // Act
            var root = provider.GetRepositoryRoot(Path.Combine(testPath, "subdir", "file.cs"));

            // Assert
            root.Should().Be(testPath);
        }
        finally
        {
            Directory.Delete(testPath, true);
        }
    }

    [Fact]
    public void GetRepositoryRoot_WithoutGitRepo_ReturnsNull()
    {
        var provider = new GitStatusProvider();
        var result = provider.GetRepositoryRoot("/some/non/git/path");
        result.Should().BeNull();
    }

    [Fact]
    public void IsInGitRepository_WithGitRepo_ReturnsTrue()
    {
        var provider = new GitStatusProvider();
        var testPath = Path.Combine(Path.GetTempPath(), "test-git-repo-2");
        Directory.CreateDirectory(Path.Combine(testPath, ".git"));

        try
        {
            provider.IsInGitRepository(testPath).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(testPath, true);
        }
    }
}
```

### 7.2 ViewModel Tests

**File:** `tests/ShieldPrompt.Tests.Unit/ViewModels/V2/GitStatusViewModelTests.cs`

```csharp
namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

public class GitStatusViewModelTests
{
    private readonly IGitRepositoryService _gitService;
    private readonly GitStatusViewModel _sut;

    public GitStatusViewModelTests()
    {
        _gitService = Substitute.For<IGitRepositoryService>();
        _sut = new GitStatusViewModel(_gitService);
    }

    [Fact]
    public async Task RefreshAsync_WithGitRepo_SetsProperties()
    {
        // Arrange
        var info = new GitRepositoryInfo(
            "/repo",
            "main",
            "https://github.com/test/repo",
            HasUncommittedChanges: true,
            ModifiedFileCount: 3,
            StagedFileCount: 1,
            UntrackedFileCount: 2);
        _gitService.GetRepositoryInfoAsync("/repo", Arg.Any<CancellationToken>())
            .Returns(info);

        // Act
        await _sut.RefreshAsync("/repo");

        // Assert
        _sut.IsGitRepository.Should().BeTrue();
        _sut.CurrentBranch.Should().Be("main");
        _sut.ModifiedFileCount.Should().Be(3);
        _sut.StagedFileCount.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_WithoutGitRepo_SetsIsGitRepositoryFalse()
    {
        _gitService.GetRepositoryInfoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GitRepositoryInfo?)null);

        await _sut.RefreshAsync("/not-a-repo");

        _sut.IsGitRepository.Should().BeFalse();
    }

    [Fact]
    public void BranchDisplay_WithGitRepo_ShowsBranch()
    {
        _sut.IsGitRepository = true;
        _sut.CurrentBranch = "feature/test";

        _sut.BranchDisplay.Should().Contain("feature/test");
    }
}
```

---

## 8. Implementation Checklist

### 8.1 Domain Layer

- [ ] Create `GitFileStatus` enum
- [ ] Create `GitRepositoryInfo` record
- [ ] Add `GitStatus` property to `FileNode`
- [ ] Write unit tests
- [ ] Run tests: `dotnet test --filter "FullyQualifiedName~Git"`

### 8.2 Application Layer

- [ ] Create `IGitStatusProvider` interface
- [ ] Create `IGitRepositoryService` interface
- [ ] Create `IGitIgnoreService` interface
- [ ] Write interface documentation

### 8.3 Infrastructure Layer

- [ ] Implement `GitStatusProvider`
- [ ] Implement `GitRepositoryService`
- [ ] Implement `GitIgnoreService`
- [ ] Write unit tests (TDD)

### 8.4 Presentation Layer

- [ ] Create `GitStatusViewModel`
- [ ] Enhance `FileNodeViewModel` with git status
- [ ] Update `FileTreePanel.axaml` with status display
- [ ] Update `StatusBar.axaml` with branch display
- [ ] Write ViewModel tests (TDD)

### 8.5 Integration

- [ ] Register services in DI
- [ ] Add "Select Modified Files" button
- [ ] Add git status filter dropdown
- [ ] End-to-end testing

---

## 9. Acceptance Criteria

| Criterion | Verification |
|-----------|--------------|
| Modified files show M indicator | Manual test |
| Staged files show green indicator | Manual test |
| Branch name displays in status bar | Manual test |
| .gitignore files are respected | Manual test |
| "Select Modified" button works | Manual test |
| All unit tests pass | `dotnet test` |

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-15 | Architect | Initial specification |

