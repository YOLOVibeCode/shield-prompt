using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Unified status reporter implementing both message and progress reporting.
/// </summary>
public class StatusReporter : IStatusMessageReporter, IProgressReporter
{
    private StatusInfo _currentStatus = StatusInfo.Ready;

    /// <inheritdoc />
    public StatusInfo CurrentStatus => _currentStatus;

    /// <inheritdoc />
    public event EventHandler<StatusInfo>? StatusChanged;

    /// <inheritdoc />
    public void ReportInfo(string message)
    {
        UpdateStatus(StatusInfo.Info(message));
    }

    /// <inheritdoc />
    public void ReportSuccess(string message)
    {
        UpdateStatus(StatusInfo.Success(message));
    }

    /// <inheritdoc />
    public void ReportWarning(string message)
    {
        UpdateStatus(StatusInfo.Warning(message));
    }

    /// <inheritdoc />
    public void ReportError(string message)
    {
        UpdateStatus(StatusInfo.Error(message));
    }

    /// <inheritdoc />
    public void ReportProgress(int current, int total, string? description = null)
    {
        var progress = new ProgressInfo(current, total, description);
        var message = description ?? $"Processing... {progress.Percentage:F0}%";
        UpdateStatus(StatusInfo.Loading(message, progress));
    }

    /// <inheritdoc />
    public void ClearProgress()
    {
        UpdateStatus(StatusInfo.Ready);
    }

    private void UpdateStatus(StatusInfo status)
    {
        _currentStatus = status;
        StatusChanged?.Invoke(this, status);
    }
}
