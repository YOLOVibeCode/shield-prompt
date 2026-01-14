using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.ValueObjects;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for counting tokens in content using AI model tokenizers.
/// ISP-compliant: focused on token counting only.
/// </summary>
public interface ITokenCountingService
{
    /// <summary>
    /// Counts tokens in the given content using the specified encoding.
    /// </summary>
    int CountTokens(string content, string encoding = "cl100k_base");

    /// <summary>
    /// Counts tokens for a file node by reading its contents.
    /// </summary>
    Task<TokenCount> CountFileTokensAsync(FileNode file, CancellationToken ct = default);

    /// <summary>
    /// Checks if the token count exceeds the model's available limit.
    /// </summary>
    bool ExceedsLimit(int tokens, ModelProfile profile);
}

