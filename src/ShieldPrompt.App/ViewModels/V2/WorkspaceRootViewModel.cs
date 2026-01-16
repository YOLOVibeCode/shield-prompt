using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel representing a workspace root folder.
/// Multiple workspace roots can be added to concatenate files from different repositories.
/// </summary>
public partial class WorkspaceRootViewModel : ObservableObject
{
    private readonly FileNode _rootNode;

    /// <summary>
    /// The display name for this workspace root (folder name).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The full path to this workspace root.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Whether this is the primary (active) workspace where changes will be applied.
    /// </summary>
    [ObservableProperty]
    private bool _isPrimary;

    /// <summary>
    /// Whether this root is expanded in the tree.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>
    /// The children of this workspace root.
    /// </summary>
    public ObservableCollection<FileNodeViewModel> Children { get; }

    /// <summary>
    /// Icon for the workspace root - shows a star if primary.
    /// </summary>
    public string Icon => IsPrimary ? "⭐" : "📁";

    /// <summary>
    /// Background color to indicate primary status.
    /// </summary>
    public string BackgroundColor => IsPrimary ? "#2d3748" : "Transparent";

    public WorkspaceRootViewModel(FileNode rootNode, bool isPrimary = false)
    {
        _rootNode = rootNode;
        Path = rootNode.Path;
        Name = ExtractFolderName(rootNode.Path);
        IsPrimary = isPrimary;

        // Create children from the root node's children
        Children = new ObservableCollection<FileNodeViewModel>(
            rootNode.Children.Select(child => new FileNodeViewModel(child)));
    }

    /// <summary>
    /// Extracts the folder name from a path, handling trailing slashes and both Unix/Windows paths.
    /// </summary>
    private static string ExtractFolderName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "Unknown";

        // Normalize path separators and remove trailing slashes
        var normalized = path.TrimEnd('/', '\\');

        // Find the last separator
        var lastSep = normalized.LastIndexOfAny(['/', '\\']);

        if (lastSep >= 0 && lastSep < normalized.Length - 1)
        {
            return normalized[(lastSep + 1)..];
        }

        // No separator found, return the whole path (might be just a folder name)
        return string.IsNullOrEmpty(normalized) ? "Unknown" : normalized;
    }

    /// <summary>
    /// Gets all selected files from this workspace root.
    /// </summary>
    public IEnumerable<FileNodeViewModel> GetSelectedFiles()
    {
        return Children.SelectMany(child => child.GetSelectedFiles());
    }

    /// <summary>
    /// Gets the total count of files in this workspace.
    /// </summary>
    public int GetTotalFileCount()
    {
        return Children.Sum(child => CountFilesRecursive(child));
    }

    private static int CountFilesRecursive(FileNodeViewModel node)
    {
        if (!node.IsDirectory)
            return 1;
        return node.Children.Sum(child => CountFilesRecursive(child));
    }

    partial void OnIsPrimaryChanged(bool value)
    {
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(BackgroundColor));
    }
}
