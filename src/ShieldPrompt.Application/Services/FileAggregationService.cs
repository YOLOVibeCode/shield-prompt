using System.Text;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for loading and aggregating file contents from directories.
/// </summary>
public class FileAggregationService : IFileAggregationService
{
    private static readonly HashSet<string> BinaryExtensions =
    [
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx",
        ".zip", ".tar", ".gz", ".rar", ".7z",
        ".mp3", ".mp4", ".avi", ".mov", ".wav",
        ".ttf", ".otf", ".woff", ".woff2",
        ".exe", ".dll", ".so", ".dylib"
    ];

    private static readonly HashSet<string> ExcludedDirectories =
    [
        "node_modules", ".git", ".svn", ".hg",
        "__pycache__", ".venv", "venv",
        "bin", "obj", "dist", "build",
        ".next", ".nuxt"
    ];

    private static readonly HashSet<string> ExcludedFiles =
    [
        "package-lock.json", "yarn.lock", "pnpm-lock.yaml", "Cargo.lock"
    ];

    private bool IsHiddenFile(string path)
    {
        var fileName = Path.GetFileName(path);
        return !string.IsNullOrEmpty(fileName) && fileName.StartsWith('.');
    }

    public async Task<FileNode> LoadDirectoryAsync(string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var directoryInfo = new DirectoryInfo(path);
        var rootNode = new FileNode(directoryInfo.FullName, directoryInfo.Name, true);

        await LoadDirectoryRecursiveAsync(rootNode, directoryInfo, ct).ConfigureAwait(false);

        return rootNode;
    }

    public async Task<string> AggregateContentsAsync(IEnumerable<FileNode> files, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(files);

        var fileList = files.Where(f => !f.IsDirectory).ToList();
        if (fileList.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        foreach (var file in fileList)
        {
            ct.ThrowIfCancellationRequested();

            if (!File.Exists(file.Path))
                continue;

            sb.AppendLine($"=== FILE: {file.Path} ===");
            
            try
            {
                var content = await File.ReadAllTextAsync(file.Path, ct).ConfigureAwait(false);
                sb.AppendLine(content);
                sb.AppendLine();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                sb.AppendLine($"[Error reading file: {ex.Message}]");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public bool IsBinaryFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var extension = Path.GetExtension(path).ToLowerInvariant();
        return BinaryExtensions.Contains(extension);
    }

    public bool IsExcluded(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var fileName = Path.GetFileName(path);
        
        // Exclude hidden files/directories (starting with .)
        if (IsHiddenFile(path))
            return true;
        
        if (ExcludedFiles.Contains(fileName))
            return true;

        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(part => ExcludedDirectories.Contains(part));
    }

    private async Task LoadDirectoryRecursiveAsync(
        FileNode parentNode,
        DirectoryInfo directory,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // Load subdirectories
        try
        {
            foreach (var subDir in directory.GetDirectories())
            {
                if (IsExcluded(subDir.FullName))
                    continue;

                var childNode = new FileNode(subDir.FullName, subDir.Name, true);
                parentNode.AddChild(childNode);

                await LoadDirectoryRecursiveAsync(childNode, subDir, ct).ConfigureAwait(false);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }

        // Load files
        try
        {
            foreach (var file in directory.GetFiles())
            {
                if (IsExcluded(file.FullName) || IsBinaryFile(file.FullName))
                    continue;

                var fileNode = new FileNode(file.FullName, file.Name, false);
                parentNode.AddChild(fileNode);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip files we can't access
        }
    }
}

