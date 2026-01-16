using System.Diagnostics;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// Service for git repository information.
/// </summary>
public class GitRepositoryService : IGitRepositoryService
{
    public async Task<GitRepositoryInfo?> GetRepositoryInfoAsync(string path, CancellationToken ct = default)
    {
        var repoRoot = GetRepositoryRoot(path);
        if (repoRoot is null) return null;

        var branch = GetCurrentBranch(repoRoot);
        var remoteUrl = GetRemoteUrl(repoRoot);
        var (modified, staged, untracked) = await GetFileCountsAsync(repoRoot, ct);

        return new GitRepositoryInfo(
            RootPath: repoRoot,
            CurrentBranch: branch ?? "unknown",
            RemoteUrl: remoteUrl,
            HasUncommittedChanges: modified > 0 || staged > 0 || untracked > 0,
            ModifiedFileCount: modified,
            StagedFileCount: staged,
            UntrackedFileCount: untracked);
    }

    public string? GetCurrentBranch(string repositoryPath)
    {
        var result = RunGitCommand(repositoryPath, "branch --show-current");
        return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
    }

    public IEnumerable<string> GetBranches(string repositoryPath)
    {
        var result = RunGitCommand(repositoryPath, "branch --list");
        foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var branch = line.Trim().TrimStart('*', ' ');
            if (!string.IsNullOrEmpty(branch))
            {
                yield return branch;
            }
        }
    }

    public Task RefreshStatusAsync(string repositoryPath, CancellationToken ct = default)
    {
        // Clear any caches if needed - for now, just a no-op
        return Task.CompletedTask;
    }

    private static string? GetRepositoryRoot(string path)
    {
        var current = File.Exists(path) ? Path.GetDirectoryName(path) ?? path : path;
        while (!string.IsNullOrEmpty(current))
        {
            var gitDir = Path.Combine(current, ".git");
            if (Directory.Exists(gitDir) || File.Exists(gitDir))
                return current;
            var parent = Path.GetDirectoryName(current);
            if (parent == current) break;
            current = parent ?? string.Empty;
        }
        return null;
    }

    private static string? GetRemoteUrl(string repositoryPath)
    {
        var result = RunGitCommand(repositoryPath, "config --get remote.origin.url");
        return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
    }

    private static async Task<(int modified, int staged, int untracked)> GetFileCountsAsync(string repositoryPath, CancellationToken ct)
    {
        var statusOutput = RunGitCommand(repositoryPath, "status --porcelain");
        var modified = 0;
        var staged = 0;
        var untracked = 0;

        foreach (var line in statusOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            ct.ThrowIfCancellationRequested();
            if (line.Length < 2) continue;

            var indexStatus = line[0];
            var workTreeStatus = line.Length > 1 ? line[1] : ' ';

            if (indexStatus != ' ' && indexStatus != '?')
                staged++;
            if (workTreeStatus == 'M')
                modified++;
            if (workTreeStatus == '?' || indexStatus == '?')
                untracked++;
        }

        return (modified, staged, untracked);
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
            return string.Empty;
        }
    }
}

