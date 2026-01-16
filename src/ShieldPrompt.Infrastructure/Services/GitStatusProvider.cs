using System.Diagnostics;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;

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
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                yield return Path.Combine(repoRoot, trimmed);
            }
        }
    }

    public bool IsInGitRepository(string path)
    {
        return GetRepositoryRoot(path) is not null;
    }

    public string? GetRepositoryRoot(string path)
    {
        var current = File.Exists(path) ? Path.GetDirectoryName(path) ?? path : path;
        while (!string.IsNullOrEmpty(current))
        {
            var gitDir = Path.Combine(current, ".git");
            if (Directory.Exists(gitDir) || File.Exists(gitDir))
                return current;
            var parent = Path.GetDirectoryName(current);
            if (parent == current) break; // Reached root
            current = parent ?? string.Empty;
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
        
        if (statusChars.Length < 2) return status;
        
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
        try
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
            
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
        catch
        {
            // Git not available or error - return empty
            return string.Empty;
        }
    }
}

