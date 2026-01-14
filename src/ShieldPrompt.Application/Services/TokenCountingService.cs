using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.ValueObjects;
using TiktokenSharp;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for counting tokens in content using AI model tokenizers.
/// </summary>
public class TokenCountingService : ITokenCountingService
{
    private readonly Dictionary<string, TikToken> _tokenizers = new();
    private readonly object _lock = new();

    public int CountTokens(string content, string encoding = "cl100k_base")
    {
        if (string.IsNullOrEmpty(content))
            return 0;

        var tokenizer = GetOrCreateTokenizer(encoding);
        var tokens = tokenizer.Encode(content);
        return tokens.Count;
    }

    public async Task<TokenCount> CountFileTokensAsync(FileNode file, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.IsDirectory)
            return new TokenCount(0);

        if (!File.Exists(file.Path))
            return new TokenCount(0);

        try
        {
            var content = await File.ReadAllTextAsync(file.Path, ct).ConfigureAwait(false);
            var count = CountTokens(content);
            return new TokenCount(count);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Can't read file - return 0
            return new TokenCount(0);
        }
    }

    public bool ExceedsLimit(int tokens, ModelProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        // Reserve percentage of context for AI response
        var availableTokens = profile.ContextLimit * (1.0 - profile.ReservedForResponse);
        return tokens > availableTokens;
    }

    private TikToken GetOrCreateTokenizer(string encoding)
    {
        lock (_lock)
        {
            if (!_tokenizers.TryGetValue(encoding, out var tokenizer))
            {
                tokenizer = TikToken.GetEncoding(encoding);
                _tokenizers[encoding] = tokenizer;
            }
            return tokenizer;
        }
    }
}

