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

