using System.Text.RegularExpressions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for creating, applying, and managing presets.
/// </summary>
public class PresetService : IPresetService
{
    private readonly IPresetRepository _repository;

    public PresetService(IPresetRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public PromptPreset CreateFromSession(PromptSession session, string name)
    {
        return PromptPreset.Create(name, session.WorkspaceId) with
        {
            ExplicitFilePaths = session.SelectedFilePaths.ToList(),
            CustomInstructions = session.CustomInstructions,
            RoleId = session.SelectedRoleId,
            ModelId = session.SelectedModelId
        };
    }

    /// <inheritdoc />
    public async Task<PresetApplication> ApplyToSessionAsync(
        PromptPreset preset,
        PromptSession session,
        FileNode rootNode,
        CancellationToken ct = default)
    {
        var warnings = new List<string>();
        var selectedPaths = new HashSet<string>();
        var notFound = 0;

        // Apply explicit file paths
        foreach (var path in preset.ExplicitFilePaths)
        {
            if (FindFileNode(rootNode, path) is { } node)
            {
                node.IsSelected = true;
                selectedPaths.Add(path);
            }
            else
            {
                notFound++;
                warnings.Add($"File not found: {path}");
            }
        }

        // Apply file patterns
        foreach (var pattern in preset.FilePatterns)
        {
            var regex = GlobToRegex(pattern);
            var matches = GetAllFiles(rootNode).Where(f => regex.IsMatch(f.Path) || regex.IsMatch(f.Name));

            foreach (var match in matches)
            {
                match.IsSelected = true;
                selectedPaths.Add(match.Path);
            }
        }

        return new PresetApplication(
            FilesSelected: selectedPaths.Count,
            FilesNotFound: notFound,
            Warnings: warnings);
    }

    /// <inheritdoc />
    public async Task RecordUsageAsync(string presetId, CancellationToken ct = default)
    {
        var preset = await _repository.GetByIdAsync(presetId, ct);
        if (preset is null) return;

        await _repository.SaveAsync(preset with
        {
            LastUsed = DateTime.UtcNow,
            UsageCount = preset.UsageCount + 1
        }, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromptPreset>> GetPinnedPresetsAsync(
        string? workspaceId,
        CancellationToken ct = default)
    {
        var global = await _repository.GetGlobalPresetsAsync(ct);
        var pinned = global.Where(p => p.IsPinned).ToList();

        if (workspaceId is not null)
        {
            var workspace = await _repository.GetWorkspacePresetsAsync(workspaceId, ct);
            pinned.AddRange(workspace.Where(p => p.IsPinned));
        }

        return pinned.OrderByDescending(p => p.UsageCount).ToList();
    }

    /// <summary>
    /// Finds a file node by path in the tree.
    /// </summary>
    private static FileNode? FindFileNode(FileNode root, string path)
    {
        if (root.Path == path) return root;
        foreach (var child in root.Children)
        {
            if (FindFileNode(child, path) is { } found)
                return found;
        }
        return null;
    }

    /// <summary>
    /// Gets all file nodes (non-directories) from the tree.
    /// </summary>
    private static IEnumerable<FileNode> GetAllFiles(FileNode node)
    {
        if (!node.IsDirectory) yield return node;
        foreach (var child in node.Children)
            foreach (var file in GetAllFiles(child))
                yield return file;
    }

    /// <summary>
    /// Converts a glob pattern to a regex.
    /// </summary>
    private static Regex GlobToRegex(string glob)
    {
        var pattern = "^" + Regex.Escape(glob)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/\\\\]*")
            .Replace("\\?", ".") + "$";
        return new Regex(pattern, RegexOptions.IgnoreCase);
    }
}
