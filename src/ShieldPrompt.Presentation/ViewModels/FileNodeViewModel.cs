using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Presentation.ViewModels;

/// <summary>
/// View model for a file or directory node in the tree.
/// </summary>
public partial class FileNodeViewModel : ObservableObject
{
    private readonly FileNode _model;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private int _tokenCount;

    public FileNodeViewModel(FileNode model)
    {
        _model = model;
        _isSelected = model.IsSelected;
        
        Children = new ObservableCollection<FileNodeViewModel>(
            model.Children.Select(child => new FileNodeViewModel(child)));
    }

    public string Name => _model.Name;
    public string Path => _model.Path;
    public bool IsDirectory => _model.IsDirectory;
    public string Extension => _model.Extension;

    public ObservableCollection<FileNodeViewModel> Children { get; }

    /// <summary>
    /// Icon for the tree item based on type.
    /// </summary>
    public string Icon => IsDirectory 
        ? (IsExpanded ? "📂" : "📁")
        : GetFileIcon();

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
            _ => "📄"
        };
    }

    /// <summary>
    /// Updates the model's IsSelected property.
    /// </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        _model.IsSelected = value;
        
        // If selecting a directory, select all children
        if (IsDirectory && value)
        {
            foreach (var child in Children)
            {
                child.IsSelected = true;
            }
        }
    }
}

