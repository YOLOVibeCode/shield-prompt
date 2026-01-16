using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Represents the current status bar state.
/// </summary>
/// <param name="Message">The status message to display.</param>
/// <param name="Severity">The severity level of the message.</param>
/// <param name="IsLoading">Whether a loading operation is in progress.</param>
/// <param name="Progress">Optional progress information for long-running operations.</param>
public record StatusInfo(
    string Message,
    StatusSeverity Severity,
    bool IsLoading,
    ProgressInfo? Progress = null)
{
    /// <summary>
    /// Default "Ready" status.
    /// </summary>
    public static StatusInfo Ready => new("Ready", StatusSeverity.Info, false);

    /// <summary>
    /// Creates an info status.
    /// </summary>
    public static StatusInfo Info(string message) =>
        new(message, StatusSeverity.Info, false);

    /// <summary>
    /// Creates a success status.
    /// </summary>
    public static StatusInfo Success(string message) =>
        new(message, StatusSeverity.Success, false);

    /// <summary>
    /// Creates a warning status.
    /// </summary>
    public static StatusInfo Warning(string message) =>
        new(message, StatusSeverity.Warning, false);

    /// <summary>
    /// Creates an error status.
    /// </summary>
    public static StatusInfo Error(string message) =>
        new(message, StatusSeverity.Error, false);

    /// <summary>
    /// Creates a loading status with progress.
    /// </summary>
    public static StatusInfo Loading(string message, ProgressInfo? progress = null) =>
        new(message, StatusSeverity.Info, true, progress);
}
