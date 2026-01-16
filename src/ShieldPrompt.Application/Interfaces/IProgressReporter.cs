using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Reports progress for long-running operations.
/// ISP-compliant: 4 members for progress reporting only.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Gets the current status.
    /// </summary>
    StatusInfo CurrentStatus { get; }

    /// <summary>
    /// Event fired when status changes.
    /// </summary>
    event EventHandler<StatusInfo>? StatusChanged;

    /// <summary>
    /// Reports progress for an operation.
    /// </summary>
    /// <param name="current">Current progress value.</param>
    /// <param name="total">Total value.</param>
    /// <param name="description">Optional description.</param>
    void ReportProgress(int current, int total, string? description = null);

    /// <summary>
    /// Clears the current progress and returns to ready state.
    /// </summary>
    void ClearProgress();
}
