namespace ShieldPrompt.Application.Interfaces.Actions;

/// <summary>
/// Sanitization-specific action interface.
/// ISP-compliant: Only 2 additional members beyond IAction.
/// </summary>
public interface ISanitizationAction : IAction
{
    /// <summary>
    /// The current content state (either original, sanitized, or desanitized).
    /// </summary>
    string CurrentContent { get; }

    /// <summary>
    /// Type of sanitization operation.
    /// </summary>
    SanitizationOperationType OperationType { get; }
}

/// <summary>
/// Types of sanitization operations.
/// </summary>
public enum SanitizationOperationType
{
    /// <summary>Replace sensitive values with aliases.</summary>
    Sanitize,
    
    /// <summary>Restore original values from aliases.</summary>
    Desanitize
}

