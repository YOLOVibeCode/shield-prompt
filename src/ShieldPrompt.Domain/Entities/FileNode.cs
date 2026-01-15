namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents a file or directory node in the file tree.
/// </summary>
public class FileNode
{
    private readonly List<FileNode> _children;

    public FileNode(string path, string name, bool isDirectory)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsDirectory = isDirectory;
        _children = [];
    }

    /// <summary>
    /// Full path to the file or directory.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Name of the file or directory (without path).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether this node represents a directory.
    /// </summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Whether this file/directory is selected for inclusion in the prompt.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// The text content of the file (empty for directories).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Child nodes (files/directories within this directory).
    /// </summary>
    public IReadOnlyList<FileNode> Children => _children.AsReadOnly();

    /// <summary>
    /// File extension including the dot (e.g., ".cs"), or empty string for directories.
    /// </summary>
    public string Extension
    {
        get
        {
            if (IsDirectory)
                return string.Empty;

            var dotIndex = Name.LastIndexOf('.');
            return dotIndex >= 0 ? Name[dotIndex..] : string.Empty;
        }
    }

    /// <summary>
    /// Adds a child node to this directory.
    /// </summary>
    public void AddChild(FileNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
    }
}

