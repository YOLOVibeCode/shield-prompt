using ShieldPrompt.Domain.Records;

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

