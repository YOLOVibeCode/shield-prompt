using ShieldPrompt.Domain.Enums;

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

