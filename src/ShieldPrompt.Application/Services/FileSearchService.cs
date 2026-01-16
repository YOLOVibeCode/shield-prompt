using System.Text.RegularExpressions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for advanced file searching.
/// </summary>
public class FileSearchService : IFileSearchService
{
    public IEnumerable<FileNode> SearchByPattern(FileNode root, string pattern, bool isRegex = false)
    {
        var allFiles = GetAllFiles(root);
        
        if (isRegex)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return allFiles.Where(f => regex.IsMatch(f.Path) || regex.IsMatch(f.Name));
        }
        else
        {
            // Convert glob pattern to regex
            var regexPattern = GlobToRegex(pattern);
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            return allFiles.Where(f => regex.IsMatch(f.Path) || regex.IsMatch(f.Name));
        }
    }

    public async Task<IEnumerable<FileNode>> SearchByContentAsync(FileNode root, string query, CancellationToken ct = default)
    {
        var allFiles = GetAllFiles(root);
        var results = new List<FileNode>();

        foreach (var file in allFiles)
        {
            ct.ThrowIfCancellationRequested();
            
            if (string.IsNullOrEmpty(file.Content))
                continue;

            if (file.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(file);
            }
        }

        return results;
    }

    public IEnumerable<FileNode> FilterByExtension(FileNode root, IEnumerable<string> extensions)
    {
        var extensionSet = extensions.Select(e => e.StartsWith('.') ? e : $".{e}")
            .Select(e => e.ToLowerInvariant())
            .ToHashSet();

        return GetAllFiles(root)
            .Where(f => extensionSet.Contains(f.Extension.ToLowerInvariant()));
    }

    public IEnumerable<FileNode> FilterByName(FileNode root, string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return GetAllFiles(root);

        var query = searchQuery.ToLowerInvariant();
        return GetAllFiles(root)
            .Where(f => f.Name.ToLowerInvariant().Contains(query) ||
                       f.Path.ToLowerInvariant().Contains(query));
    }

    private static IEnumerable<FileNode> GetAllFiles(FileNode node)
    {
        if (!node.IsDirectory)
        {
            yield return node;
        }

        foreach (var child in node.Children)
        {
            foreach (var file in GetAllFiles(child))
            {
                yield return file;
            }
        }
    }

    private static string GlobToRegex(string glob)
    {
        // Convert glob pattern to regex
        // ** matches any number of directories
        // * matches any characters except /
        // ? matches single character except /
        var pattern = "^" + Regex.Escape(glob)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", ".")
            + "$";
        return pattern;
    }
}

