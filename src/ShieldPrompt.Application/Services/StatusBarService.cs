using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Provides aggregated status bar data.
/// </summary>
public class StatusBarService : IStatusBarService
{
    private readonly DateTime _sessionStart;
    private int _selectedCount;
    private int _totalCount;
    private int _usedTokens;

    public StatusBarService()
    {
        _sessionStart = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public FileSelectionSummary GetFileSelectionSummary()
    {
        return new FileSelectionSummary(_selectedCount, _totalCount);
    }

    /// <inheritdoc />
    public TokenUsageSummary GetTokenUsageSummary(int contextLimit)
    {
        var percentage = contextLimit > 0 ? (double)_usedTokens / contextLimit * 100 : 0;
        return new TokenUsageSummary(_usedTokens, contextLimit, percentage);
    }

    /// <inheritdoc />
    public SessionSummary GetSessionInfo()
    {
        var duration = DateTime.UtcNow - _sessionStart;
        return new SessionSummary(duration, _sessionStart);
    }

    /// <inheritdoc />
    public void UpdateFileSelection(int selectedCount, int totalCount)
    {
        _selectedCount = selectedCount;
        _totalCount = totalCount;
    }

    /// <inheritdoc />
    public void UpdateTokenUsage(int usedTokens)
    {
        _usedTokens = usedTokens;
    }
}
