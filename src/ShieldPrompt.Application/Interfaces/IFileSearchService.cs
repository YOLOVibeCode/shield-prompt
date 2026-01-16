using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for advanced file searching.
/// Follows ISP - search operations only.
/// </summary>
public interface IFileSearchService
{
    /// <summary>
    /// Searches files by name pattern (glob or regex).
    /// </summary>
    IEnumerable<FileNode> SearchByPattern(FileNode root, string pattern, bool isRegex = false);
    
    /// <summary>
    /// Searches files by content.
    /// </summary>
    Task<IEnumerable<FileNode>> SearchByContentAsync(FileNode root, string query, CancellationToken ct = default);
    
    /// <summary>
    /// Filters files by extension.
    /// </summary>
    IEnumerable<FileNode> FilterByExtension(FileNode root, IEnumerable<string> extensions);
    
    /// <summary>
    /// Filters files by name (simple contains search).
    /// </summary>
    IEnumerable<FileNode> FilterByName(FileNode root, string searchQuery);
}

