using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Action that desanitizes content by restoring original values from aliases.
/// Supports undo to restore sanitized content.
/// </summary>
public class DesanitizeAction : ISanitizationAction
{
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly IMappingSession _session;
    private string _sanitizedContent;
    private string _currentContent;

    public Guid Id { get; } = Guid.NewGuid();
    public string Description => "Desanitize content";
    public string CurrentContent => _currentContent;
    public SanitizationOperationType OperationType => SanitizationOperationType.Desanitize;

    /// <summary>
    /// Creates a desanitize action.
    /// </summary>
    /// <param name="sanitizedContent">Sanitized content to restore.</param>
    /// <param name="desanitizationEngine">Engine to perform desanitization.</param>
    /// <param name="session">Mapping session containing alias-to-original mappings.</param>
    public DesanitizeAction(
        string sanitizedContent,
        IDesanitizationEngine desanitizationEngine,
        IMappingSession session)
    {
        _sanitizedContent = sanitizedContent ?? throw new ArgumentNullException(nameof(sanitizedContent));
        _currentContent = sanitizedContent;
        _desanitizationEngine = desanitizationEngine ?? throw new ArgumentNullException(nameof(desanitizationEngine));
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// Executes desanitization, restoring original values from aliases.
    /// </summary>
    public Task<ActionResult> ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            var result = _desanitizationEngine.Desanitize(_sanitizedContent, _session);
            _currentContent = result;

            return Task.FromResult(new ActionResult(
                Success: true,
                Metadata: new Dictionary<string, object>
                {
                    ["MappingCount"] = _session.GetAllMappings().Count
                }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ActionResult(false, $"Desanitization failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Undoes desanitization, restoring sanitized content.
    /// </summary>
    public Task<ActionResult> UndoAsync(CancellationToken ct = default)
    {
        try
        {
            _currentContent = _sanitizedContent;
            return Task.FromResult(new ActionResult(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ActionResult(false, $"Undo desanitization failed: {ex.Message}"));
        }
    }
}

