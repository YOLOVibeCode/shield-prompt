namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Result of applying a preset to current session state.
/// </summary>
/// <param name="FilesSelected">Number of files successfully selected.</param>
/// <param name="FilesNotFound">Number of files that could not be found.</param>
/// <param name="Warnings">Any warnings generated during application.</param>
public record PresetApplication(
    int FilesSelected,
    int FilesNotFound,
    IReadOnlyList<string> Warnings)
{
    /// <summary>
    /// Whether any files were successfully selected.
    /// </summary>
    public bool HasSelections => FilesSelected > 0;

    /// <summary>
    /// Whether any files were not found.
    /// </summary>
    public bool HasWarnings => FilesNotFound > 0 || Warnings.Count > 0;

    /// <summary>
    /// Creates an empty result (no files selected, no errors).
    /// </summary>
    public static PresetApplication Empty => new(0, 0, Array.Empty<string>());
}
