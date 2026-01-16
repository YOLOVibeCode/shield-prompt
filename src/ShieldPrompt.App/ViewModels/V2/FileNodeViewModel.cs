using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.App.ViewModels.V2;

/// <summary>
/// ViewModel wrapper for FileNode that provides cascading selection behavior.
/// 
/// Selection Logic:
/// - Selecting a FOLDER selects ALL descendants (files + subfolders)
/// - Deselecting a FOLDER deselects ALL descendants
/// - Selecting ALL children auto-selects the parent
/// - Selecting SOME children shows parent as "indeterminate" (partial)
/// - Selecting NO children auto-deselects the parent
/// </summary>
public partial class FileNodeViewModel : ObservableObject
{
    private readonly FileNode _model;
    private bool _isUpdatingFromParent;
    private bool _isUpdatingFromChildren;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private int _tokenCount;

    /// <summary>
    /// Selection state: null = indeterminate (partial), true = selected, false = not selected
    /// </summary>
    [ObservableProperty]
    private bool? _isChecked = false;

    /// <summary>
    /// Parent node for upward propagation.
    /// </summary>
    public FileNodeViewModel? Parent { get; private set; }

    public FileNodeViewModel(FileNode model, FileNodeViewModel? parent = null)
    {
        _model = model;
        Parent = parent;
        _isChecked = model.IsSelected;

        Children = new ObservableCollection<FileNodeViewModel>(
            model.Children.Select(child => new FileNodeViewModel(child, this)));

        // Subscribe to children changes for upward propagation
        foreach (var child in Children)
        {
            child.PropertyChanged += OnChildPropertyChanged;
        }
    }

    public string Name => _model.Name;
    public string Path => _model.Path;
    public bool IsDirectory => _model.IsDirectory;
    public string Extension => _model.Extension;
    public string Content => _model.Content;
    public GitFileStatus GitStatus => _model.GitStatus;
    public bool IsGitIgnored => _model.IsGitIgnored;

    public ObservableCollection<FileNodeViewModel> Children { get; }

    /// <summary>
    /// Git status indicator icon.
    /// </summary>
    public string GitStatusIcon => GitStatus switch
    {
        var s when s.HasFlag(GitFileStatus.Conflict) => "⚠",
        var s when s.HasFlag(GitFileStatus.Staged) && s.HasFlag(GitFileStatus.Modified) => "SM",
        var s when s.HasFlag(GitFileStatus.Staged) => "S",
        var s when s.HasFlag(GitFileStatus.Modified) => "M",
        var s when s.HasFlag(GitFileStatus.Added) => "A",
        var s when s.HasFlag(GitFileStatus.Deleted) => "D",
        var s when s.HasFlag(GitFileStatus.Renamed) => "R",
        var s when s.HasFlag(GitFileStatus.Untracked) => "?",
        var s when s.HasFlag(GitFileStatus.Ignored) => "I",
        _ => ""
    };

    /// <summary>
    /// Whether to show the git status indicator.
    /// </summary>
    public bool HasGitStatus => GitStatus != GitFileStatus.None && !IsDirectory;

    /// <summary>
    /// Git status color for the indicator.
    /// </summary>
    public IBrush GitStatusColor => GitStatus switch
    {
        var s when s.HasFlag(GitFileStatus.Conflict) => Brushes.Red,
        var s when s.HasFlag(GitFileStatus.Staged) => Brushes.LightGreen,
        var s when s.HasFlag(GitFileStatus.Modified) => Brushes.Orange,
        var s when s.HasFlag(GitFileStatus.Added) => Brushes.LightGreen,
        var s when s.HasFlag(GitFileStatus.Deleted) => Brushes.Red,
        var s when s.HasFlag(GitFileStatus.Renamed) => Brushes.Cyan,
        var s when s.HasFlag(GitFileStatus.Untracked) => Brushes.Gray,
        var s when s.HasFlag(GitFileStatus.Ignored) => Brushes.DarkGray,
        _ => Brushes.Transparent
    };

    /// <summary>
    /// Whether this file is modified (for quick filtering).
    /// </summary>
    public bool IsModified => GitStatus.HasFlag(GitFileStatus.Modified) ||
                               GitStatus.HasFlag(GitFileStatus.Added) ||
                               GitStatus.HasFlag(GitFileStatus.Deleted) ||
                               GitStatus.HasFlag(GitFileStatus.Renamed);

    /// <summary>
    /// Icon for the tree item based on type and expansion state.
    /// </summary>
    public string Icon => IsDirectory
        ? (IsExpanded ? "📂" : "📁")
        : GetFileIcon();

    /// <summary>
    /// Display text for token count.
    /// </summary>
    public string TokenCountDisplay => TokenCount > 0 ? $"({TokenCount:N0})" : string.Empty;

    /// <summary>
    /// Whether this node is effectively selected (for file collection).
    /// Directories return false; only files count.
    /// </summary>
    public bool IsSelected => !IsDirectory && IsChecked == true;

    private string GetFileIcon()
    {
        return Extension.ToLowerInvariant() switch
        {
            ".cs" => "🔷",
            ".xaml" or ".axaml" => "🎨",
            ".json" => "📋",
            ".xml" => "📄",
            ".md" => "📝",
            ".txt" => "📃",
            ".yml" or ".yaml" => "⚙️",
            ".js" or ".ts" => "🟨",
            ".py" => "🐍",
            ".go" => "🔵",
            ".rs" => "🦀",
            _ => "📄"
        };
    }

    /// <summary>
    /// Handles checkbox state changes.
    /// </summary>
    partial void OnIsCheckedChanged(bool? value)
    {
        // Don't recurse if this change came from parent or child updates
        if (_isUpdatingFromParent || _isUpdatingFromChildren)
            return;

        // Update the underlying model
        _model.IsSelected = value == true;

        // If this is a directory, cascade selection to all descendants
        if (IsDirectory && value.HasValue)
        {
            SetChildrenChecked(value.Value);
        }

        // Notify parent to recalculate its state
        NotifyParentOfChange();
        
        // Notify that IsSelected changed
        OnPropertyChanged(nameof(IsSelected));
    }

    /// <summary>
    /// Sets all children to the specified checked state (cascades down).
    /// </summary>
    private void SetChildrenChecked(bool isChecked)
    {
        foreach (var child in Children)
        {
            child._isUpdatingFromParent = true;
            child.IsChecked = isChecked;
            child._model.IsSelected = isChecked;
            
            // Recursively set children
            if (child.IsDirectory)
            {
                child.SetChildrenChecked(isChecked);
            }
            
            child._isUpdatingFromParent = false;
            child.OnPropertyChanged(nameof(IsSelected));
        }
    }

    /// <summary>
    /// Notifies the parent to recalculate its checkbox state based on children.
    /// </summary>
    private void NotifyParentOfChange()
    {
        Parent?.UpdateStateFromChildren();
    }

    /// <summary>
    /// Updates this node's checkbox state based on children states.
    /// Called when a child's state changes.
    /// </summary>
    private void UpdateStateFromChildren()
    {
        if (!IsDirectory || Children.Count == 0)
            return;

        _isUpdatingFromChildren = true;

        var allChecked = Children.All(c => c.IsChecked == true);
        var anyChecked = Children.Any(c => c.IsChecked == true || c.IsChecked == null);

        if (allChecked)
        {
            IsChecked = true;
            _model.IsSelected = true;
        }
        else if (anyChecked)
        {
            IsChecked = null; // Indeterminate
            _model.IsSelected = false;
        }
        else
        {
            IsChecked = false;
            _model.IsSelected = false;
        }

        _isUpdatingFromChildren = false;

        // Continue propagation upward
        Parent?.UpdateStateFromChildren();
    }

    /// <summary>
    /// Handles child property changes for upward propagation.
    /// </summary>
    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsChecked) && !_isUpdatingFromParent)
        {
            UpdateStateFromChildren();
        }
    }

    /// <summary>
    /// Selects this node and all descendants.
    /// </summary>
    public void SelectAll()
    {
        IsChecked = true;
    }

    /// <summary>
    /// Deselects this node and all descendants.
    /// </summary>
    public void DeselectAll()
    {
        IsChecked = false;
    }

    /// <summary>
    /// Gets all selected file nodes (not directories) from this node and descendants.
    /// </summary>
    public IEnumerable<FileNodeViewModel> GetSelectedFiles()
    {
        if (!IsDirectory && IsChecked == true)
        {
            yield return this;
        }

        foreach (var child in Children)
        {
            foreach (var selected in child.GetSelectedFiles())
            {
                yield return selected;
            }
        }
    }

    /// <summary>
    /// Gets all file nodes (not directories) from this node and descendants.
    /// </summary>
    public IEnumerable<FileNodeViewModel> GetAllFiles()
    {
        if (!IsDirectory)
        {
            yield return this;
        }

        foreach (var child in Children)
        {
            foreach (var file in child.GetAllFiles())
            {
                yield return file;
            }
        }
    }
}

