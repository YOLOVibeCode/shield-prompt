using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Options for controlling sanitization behavior.
/// </summary>
public record SanitizationOptions
{
    public bool EnableInfrastructure { get; init; } = true;
    public bool EnablePII { get; init; } = true;
    public bool EnableCustomPatterns { get; init; } = true;
    public PolicyMode Mode { get; init; } = PolicyMode.SanitizedOnly;
}

