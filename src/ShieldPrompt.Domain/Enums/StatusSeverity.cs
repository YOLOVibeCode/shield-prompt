namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Severity level for status messages.
/// </summary>
public enum StatusSeverity
{
    /// <summary>
    /// Informational message (neutral).
    /// </summary>
    Info,

    /// <summary>
    /// Success message (operation completed successfully).
    /// </summary>
    Success,

    /// <summary>
    /// Warning message (something might need attention).
    /// </summary>
    Warning,

    /// <summary>
    /// Error message (operation failed).
    /// </summary>
    Error
}
