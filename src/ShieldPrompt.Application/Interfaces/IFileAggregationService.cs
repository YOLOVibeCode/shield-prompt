using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for loading and aggregating file contents from directories.
/// ISP-compliant: focused on file aggregation only.
/// </summary>
public interface IFileAggregationService
{
    /// <summary>
    /// Loads a directory tree from the specified path.
    /// </summary>
    Task<FileNode> LoadDirectoryAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Aggregates contents of selected files into a single string.
    /// </summary>
    Task<string> AggregateContentsAsync(IEnumerable<FileNode> files, CancellationToken ct = default);

    /// <summary>
    /// Checks if a file is binary (should be excluded).
    /// </summary>
    bool IsBinaryFile(string path);

    /// <summary>
    /// Checks if a path should be excluded based on rules.
    /// </summary>
    bool IsExcluded(string path);
}

