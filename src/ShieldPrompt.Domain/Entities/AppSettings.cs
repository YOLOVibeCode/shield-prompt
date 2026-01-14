namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Application settings that persist between sessions.
/// </summary>
public record AppSettings
{
    /// <summary>
    /// Last folder opened by the user.
    /// </summary>
    public string? LastFolderPath { get; init; }

    /// <summary>
    /// Last selected output format name.
    /// </summary>
    public string? LastFormatName { get; init; }

    /// <summary>
    /// Last selected model name.
    /// </summary>
    public string? LastModelName { get; init; }

    /// <summary>
    /// Last selected file paths (for restoring selection).
    /// </summary>
    public IReadOnlyList<string> LastSelectedFiles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Window position and size.
    /// </summary>
    public WindowSettings Window { get; init; } = new();
}

/// <summary>
/// Window position and size settings.
/// </summary>
public record WindowSettings
{
    public double Left { get; init; } = 100;
    public double Top { get; init; } = 100;
    public double Width { get; init; } = 1200;
    public double Height { get; init; } = 700;
    public bool IsMaximized { get; init; }
}

