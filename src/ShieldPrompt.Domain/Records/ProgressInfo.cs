namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Progress information for long-running operations.
/// </summary>
/// <param name="Current">Current progress value.</param>
/// <param name="Total">Total value (0 or negative for indeterminate).</param>
/// <param name="Description">Optional description of the current operation.</param>
public record ProgressInfo(
    int Current,
    int Total,
    string? Description = null)
{
    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;

    /// <summary>
    /// Whether the progress is indeterminate (total unknown).
    /// </summary>
    public bool IsIndeterminate => Total <= 0;

    /// <summary>
    /// Whether the operation is complete.
    /// </summary>
    public bool IsComplete => Total > 0 && Current >= Total;

    /// <summary>
    /// Creates an indeterminate progress.
    /// </summary>
    public static ProgressInfo Indeterminate(string? description = null) =>
        new(0, 0, description);
}
