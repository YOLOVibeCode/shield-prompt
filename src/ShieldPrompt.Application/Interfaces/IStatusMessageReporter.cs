namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Reports status messages to the UI.
/// ISP-compliant: 4 methods for message reporting only.
/// </summary>
public interface IStatusMessageReporter
{
    /// <summary>
    /// Reports an informational message.
    /// </summary>
    void ReportInfo(string message);

    /// <summary>
    /// Reports a success message.
    /// </summary>
    void ReportSuccess(string message);

    /// <summary>
    /// Reports a warning message.
    /// </summary>
    void ReportWarning(string message);

    /// <summary>
    /// Reports an error message.
    /// </summary>
    void ReportError(string message);
}
