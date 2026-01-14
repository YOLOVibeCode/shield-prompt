using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Application.Services.UndoRedo;

/// <summary>
/// Undoable action for sanitization operations.
/// Stores both original and sanitized content for undo/redo.
/// </summary>
public class SanitizationAction : IUndoableAction
{
    private readonly string _originalContent;
    private readonly SanitizationResult _sanitizationResult;
    private readonly IMappingSession _session;
    private readonly Action<string> _applyContent;
    private readonly Dictionary<string, string> _savedMappings;

    public SanitizationAction(
        string originalContent,
        SanitizationResult sanitizationResult,
        IMappingSession session,
        Action<string> applyContent)
    {
        _originalContent = originalContent;
        _sanitizationResult = sanitizationResult;
        _session = session;
        _applyContent = applyContent;
        Timestamp = DateTime.UtcNow;

        // Save current mappings for restore
        _savedMappings = new Dictionary<string, string>(session.GetAllMappings());
    }

    public string Description => _sanitizationResult.WasSanitized
        ? $"Sanitize ({_sanitizationResult.TotalMatches} values masked)"
        : "Sanitize (no changes)";

    public DateTime Timestamp { get; }

    public Task ExecuteAsync()
    {
        // Apply sanitized content
        _applyContent(_sanitizationResult.SanitizedContent);

        // Restore mappings
        foreach (var (alias, original) in _savedMappings)
        {
            var match = _sanitizationResult.Matches.FirstOrDefault(m => m.Alias == alias);
            if (match != null)
            {
                _session.AddMapping(original, alias, match.Category);
            }
        }

        return Task.CompletedTask;
    }

    public Task UndoAsync()
    {
        // Restore original unsanitized content
        _applyContent(_originalContent);

        // Clear mappings added by this action
        _session.Clear();

        return Task.CompletedTask;
    }

    public bool CanMergeWith(IUndoableAction other) => false; // Sanitization actions don't merge

    public IUndoableAction MergeWith(IUndoableAction other) =>
        throw new NotSupportedException("Sanitization actions cannot be merged");
}

