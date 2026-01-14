namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Categories of sensitive data patterns that can be detected and sanitized.
/// </summary>
public enum PatternCategory
{
    // Infrastructure patterns
    Database,
    Server,
    IPAddress,
    Hostname,
    ConnectionString,
    FilePath,
    
    // Critical PII patterns
    SSN,
    CreditCard,
    APIKey,
    AWSKey,
    GitHubToken,
    OpenAIKey,
    AnthropicKey,
    SlackToken,
    AzureKey,
    PrivateKey,
    Password,
    BearerToken,
    
    // Extended patterns
    Email,
    Phone,
    URL,
    
    // User-defined
    Custom
}
