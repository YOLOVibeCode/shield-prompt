namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Provides aggregated status bar data.
/// ISP-compliant: 4 methods for status aggregation only.
/// </summary>
public interface IStatusBarService
{
    /// <summary>
    /// Gets the current file selection summary.
    /// </summary>
    FileSelectionSummary GetFileSelectionSummary();

    /// <summary>
    /// Gets the current token usage summary.
    /// </summary>
    TokenUsageSummary GetTokenUsageSummary(int contextLimit);

    /// <summary>
    /// Gets the session information.
    /// </summary>
    SessionSummary GetSessionInfo();

    /// <summary>
    /// Updates the file selection counts.
    /// </summary>
    void UpdateFileSelection(int selectedCount, int totalCount);

    /// <summary>
    /// Updates the token usage.
    /// </summary>
    void UpdateTokenUsage(int usedTokens);
}

/// <summary>
/// Summary of file selection state.
/// </summary>
/// <param name="SelectedCount">Number of files selected.</param>
/// <param name="TotalCount">Total number of files available.</param>
public record FileSelectionSummary(int SelectedCount, int TotalCount)
{
    /// <summary>
    /// Formatted display string.
    /// </summary>
    public string Display => $"{SelectedCount}/{TotalCount} files";
}

/// <summary>
/// Summary of token usage.
/// </summary>
/// <param name="UsedTokens">Number of tokens used.</param>
/// <param name="ContextLimit">Maximum context window size.</param>
/// <param name="Percentage">Usage percentage (0-100).</param>
public record TokenUsageSummary(int UsedTokens, int ContextLimit, double Percentage)
{
    /// <summary>
    /// Formatted display string.
    /// </summary>
    public string Display => $"{UsedTokens:N0} / {ContextLimit:N0}";

    /// <summary>
    /// Whether the usage is critical (>95%).
    /// </summary>
    public bool IsCritical => Percentage >= 95;

    /// <summary>
    /// Whether the usage is high (>80%).
    /// </summary>
    public bool IsHigh => Percentage >= 80;
}

/// <summary>
/// Summary of session information.
/// </summary>
/// <param name="Duration">Duration of the current session.</param>
/// <param name="StartTime">When the session started.</param>
public record SessionSummary(TimeSpan Duration, DateTime StartTime)
{
    /// <summary>
    /// Formatted display string.
    /// </summary>
    public string Display => $"Session: {Duration:h\\:mm}";
}
