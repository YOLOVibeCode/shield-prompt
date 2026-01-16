using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// Service for handling .gitignore pattern matching.
/// </summary>
public class GitIgnoreService : IGitIgnoreService
{
    private readonly Dictionary<string, List<string>> _patternCache = new();

    public bool ShouldIgnore(string filePath, string repositoryRoot)
    {
        var patterns = GetIgnorePatterns(repositoryRoot);
        var relativePath = Path.GetRelativePath(repositoryRoot, filePath).Replace('\\', '/');

        foreach (var pattern in patterns)
        {
            if (MatchesPattern(relativePath, pattern))
                return true;
        }

        return false;
    }

    public IEnumerable<string> GetIgnorePatterns(string repositoryRoot)
    {
        if (_patternCache.TryGetValue(repositoryRoot, out var cached))
        {
            return cached;
        }

        ReloadPatterns(repositoryRoot);
        return _patternCache.TryGetValue(repositoryRoot, out var patterns) ? patterns : Array.Empty<string>();
    }

    public void ReloadPatterns(string repositoryRoot)
    {
        var patterns = new List<string>();
        var gitignorePath = Path.Combine(repositoryRoot, ".gitignore");

        if (File.Exists(gitignorePath))
        {
            var lines = File.ReadAllLines(gitignorePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith('#'))
                {
                    patterns.Add(trimmed);
                }
            }
        }

        _patternCache[repositoryRoot] = patterns;
    }

    private static bool MatchesPattern(string path, string pattern)
    {
        // Simple glob matching - can be enhanced later
        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + pattern
                .Replace(".", "\\.")
                .Replace("*", ".*")
                .Replace("?", ".") + "$";
            
            return System.Text.RegularExpressions.Regex.IsMatch(path, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return path.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}

