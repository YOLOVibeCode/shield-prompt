namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Policy modes for handling sensitive data.
/// </summary>
public enum PolicyMode
{
    /// <summary>
    /// No sanitization - content passes through unchanged.
    /// Use with caution.
    /// </summary>
    Unrestricted,
    
    /// <summary>
    /// All detected sensitive data is sanitized before output.
    /// This is the default and recommended mode.
    /// </summary>
    SanitizedOnly,
    
    /// <summary>
    /// Content containing any sensitive data is blocked entirely.
    /// Most restrictive mode for high-security environments.
    /// </summary>
    Blocked
}
