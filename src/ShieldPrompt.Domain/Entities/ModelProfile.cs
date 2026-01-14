namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents an AI model with its token limits and configuration.
/// </summary>
public record ModelProfile(
    string Name,
    string DisplayName,
    int ContextLimit,
    string TokenizerEncoding,
    double ReservedForResponse);

/// <summary>
/// Predefined model profiles for common AI models.
/// </summary>
public static class ModelProfiles
{
    public static readonly ModelProfile GPT4o = new(
        "gpt-4o",
        "GPT-4o",
        128_000,
        "cl100k_base",
        0.25);

    public static readonly ModelProfile GPT4oMini = new(
        "gpt-4o-mini",
        "GPT-4o Mini",
        128_000,
        "cl100k_base",
        0.25);

    public static readonly ModelProfile Claude35Sonnet = new(
        "claude-3.5-sonnet",
        "Claude 3.5 Sonnet",
        200_000,
        "cl100k_base",
        0.25);

    public static readonly ModelProfile Claude3Opus = new(
        "claude-3-opus",
        "Claude 3 Opus",
        200_000,
        "cl100k_base",
        0.25);

    public static readonly ModelProfile Gemini25Pro = new(
        "gemini-2.5-pro",
        "Gemini 2.5 Pro",
        1_000_000,
        "cl100k_base",
        0.25);

    public static readonly ModelProfile DeepSeekV3 = new(
        "deepseek-v3",
        "DeepSeek V3",
        128_000,
        "cl100k_base",
        0.25);

    public static IReadOnlyList<ModelProfile> All { get; } = new[]
    {
        GPT4o,
        GPT4oMini,
        Claude35Sonnet,
        Claude3Opus,
        Gemini25Pro,
        DeepSeekV3
    };
}

