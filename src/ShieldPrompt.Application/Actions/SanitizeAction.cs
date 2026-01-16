using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Action that sanitizes content by replacing sensitive values with aliases.
/// Supports undo to restore original content.
/// </summary>
public class SanitizeAction : ISanitizationAction
{
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly SanitizationOptions _options;
    private string _originalContent;
    private string _currentContent;

    public Guid Id { get; } = Guid.NewGuid();
    public string Description => "Sanitize content";
    public string CurrentContent => _currentContent;
    public SanitizationOperationType OperationType => SanitizationOperationType.Sanitize;

    /// <summary>
    /// Creates a sanitize action.
    /// </summary>
    /// <param name="content">Original content to sanitize.</param>
    /// <param name="options">Sanitization options.</param>
    /// <param name="sanitizationEngine">Engine to perform sanitization.</param>
    public SanitizeAction(
        string content,
        SanitizationOptions options,
        ISanitizationEngine sanitizationEngine)
    {
        _originalContent = content ?? throw new ArgumentNullException(nameof(content));
        _currentContent = content;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sanitizationEngine = sanitizationEngine ?? throw new ArgumentNullException(nameof(sanitizationEngine));
    }

    /// <summary>
    /// Executes sanitization, replacing sensitive values with aliases.
    /// </summary>
    public Task<ActionResult> ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            var result = _sanitizationEngine.Sanitize(_originalContent, _options);
            _currentContent = result.SanitizedContent;

            return Task.FromResult(new ActionResult(
                Success: true,
                Metadata: new Dictionary<string, object>
                {
                    ["WasSanitized"] = result.WasSanitized,
                    ["MatchCount"] = result.TotalMatches
                }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ActionResult(false, $"Sanitization failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Undoes sanitization, restoring original content.
    /// </summary>
    public Task<ActionResult> UndoAsync(CancellationToken ct = default)
    {
        try
        {
            _currentContent = _originalContent;
            return Task.FromResult(new ActionResult(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ActionResult(false, $"Undo sanitization failed: {ex.Message}"));
        }
    }
}

