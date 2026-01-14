using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Sanitization.Services;

/// <summary>
/// Generates unique aliases for sensitive values based on pattern category.
/// </summary>
public class AliasGenerator : IAliasGenerator
{
    private readonly Dictionary<PatternCategory, int> _counters = new();

    public string Generate(PatternCategory category)
    {
        var count = _counters.GetValueOrDefault(category, 0);
        _counters[category] = count + 1;

        return category switch
        {
            PatternCategory.Database => $"DATABASE_{count}",
            PatternCategory.Server => $"SERVER_{count}",
            PatternCategory.IPAddress => $"IP_ADDRESS_{count}",
            PatternCategory.Hostname => $"HOSTNAME_{count}",
            PatternCategory.ConnectionString => $"CONN_STRING_{count}",
            PatternCategory.FilePath => $"FILE_PATH_{count}",
            PatternCategory.SSN => $"SSN_{count}",
            PatternCategory.CreditCard => $"CREDIT_CARD_{count}",
            PatternCategory.APIKey => $"API_KEY_{count}",
            PatternCategory.AWSKey => $"AWS_KEY_{count}",
            PatternCategory.GitHubToken => $"GITHUB_TOKEN_{count}",
            PatternCategory.OpenAIKey => $"OPENAI_KEY_{count}",
            PatternCategory.AnthropicKey => $"ANTHROPIC_KEY_{count}",
            PatternCategory.SlackToken => $"SLACK_TOKEN_{count}",
            PatternCategory.AzureKey => $"AZURE_KEY_{count}",
            PatternCategory.PrivateKey => $"PRIVATE_KEY_{count}",
            PatternCategory.Password => $"PASSWORD_{count}",
            PatternCategory.BearerToken => $"BEARER_TOKEN_{count}",
            PatternCategory.Email => $"EMAIL_{count}",
            PatternCategory.Phone => $"PHONE_{count}",
            PatternCategory.URL => $"URL_{count}",
            PatternCategory.Custom => $"CUSTOM_{count}",
            _ => $"VALUE_{count}"
        };
    }

    public void Reset()
    {
        _counters.Clear();
    }
}

