using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Sanitization.Patterns;

/// <summary>
/// Built-in detection patterns for sensitive data.
/// Based on OpenCode Enterprise Shield patterns.
/// </summary>
public static class BuiltInPatterns
{
    /// <summary>
    /// Gets all 14 built-in patterns.
    /// </summary>
    public static IEnumerable<Pattern> GetAll()
    {
        yield return ServerDatabaseNames;
        yield return PrivateIPAddresses;
        yield return ConnectionStrings;
        yield return WindowsFilePaths;
        yield return InternalHostnames;
        yield return SocialSecurityNumbers;
        yield return CreditCards;
        yield return AWSKeys;
        yield return GitHubTokens;
        yield return OpenAIKeys;
        yield return AnthropicKeys;
        yield return PrivateKeys;
        yield return PasswordsInCode;
        yield return JWTTokens;
    }

    // ==================== INFRASTRUCTURE PATTERNS ====================

    /// <summary>
    /// Pattern 1: Server/Database Names
    /// Matches: ProductionDB, DevServer, TestDB, staging-mysql, etc.
    /// </summary>
    public static Pattern ServerDatabaseNames => new(
        "Server/Database Names",
        @"(?i)(prod|production|staging|dev|development|test|qa|uat)[-_]?(db|database|server|host|sql|mysql|postgres|postgresql|mongo|mongodb|redis|elastic|elasticsearch)",
        PatternCategory.Database)
    {
        Priority = 150
    };

    /// <summary>
    /// Pattern 2: IP Addresses (RFC 1918 Private Ranges)
    /// Matches: 10.x.x.x, 172.16-31.x.x, 192.168.x.x
    /// </summary>
    public static Pattern PrivateIPAddresses => new(
        "Private IP Addresses",
        @"\b(10\.\d{1,3}\.\d{1,3}\.\d{1,3}|172\.(1[6-9]|2\d|3[01])\.\d{1,3}\.\d{1,3}|192\.168\.\d{1,3}\.\d{1,3})\b",
        PatternCategory.IPAddress)
    {
        Priority = 140
    };

    /// <summary>
    /// Pattern 3: Connection Strings
    /// Matches: Server=...; Data Source=...;
    /// </summary>
    public static Pattern ConnectionStrings => new(
        "Connection Strings",
        @"(?i)(server|host|data source|datasource)\s*=\s*[^;]+;",
        PatternCategory.ConnectionString)
    {
        Priority = 130
    };

    /// <summary>
    /// Pattern 4: Windows File Paths
    /// Matches: C:\Users\..., \\server\share\...
    /// </summary>
    public static Pattern WindowsFilePaths => new(
        "Windows File Paths",
        @"(?i)([a-z]:\\[^\s<>:""\|\?\*]+|\\\\[a-z0-9_.$●-]+\\[^\s<>:""\|\?\*]+)",
        PatternCategory.FilePath)
    {
        Priority = 120
    };

    /// <summary>
    /// Pattern 5: Internal Hostnames
    /// Matches: db.internal.company.com, api.corp.local
    /// </summary>
    public static Pattern InternalHostnames => new(
        "Internal Hostnames",
        @"\b[a-z0-9][-a-z0-9]*\.(internal|local|corp|intra|private)\.[a-z]{2,}\b",
        PatternCategory.Hostname)
    {
        Priority = 110
    };

    // ==================== CRITICAL PII PATTERNS ====================

    /// <summary>
    /// Pattern 6: US Social Security Numbers
    /// Matches: 123-45-6789, 123 45 6789, 123456789
    /// </summary>
    public static Pattern SocialSecurityNumbers => new(
        "Social Security Numbers",
        @"\b\d{3}[-\s]?\d{2}[-\s]?\d{4}\b",
        PatternCategory.SSN)
    {
        Priority = 200 // High priority - critical PII
    };

    /// <summary>
    /// Pattern 7: Credit Card Numbers
    /// Matches: Visa, MasterCard, Amex, Discover (basic pattern - Luhn validation recommended)
    /// </summary>
    public static Pattern CreditCards => new(
        "Credit Card Numbers",
        @"\b(?:4\d{3}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}|5[1-5]\d{2}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}|3[47]\d{2}[\s-]?\d{6}[\s-]?\d{5}|6(?:011|5\d{2})[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4})\b",
        PatternCategory.CreditCard)
    {
        Priority = 190
    };

    /// <summary>
    /// Pattern 8: AWS Access Keys
    /// Matches: AKIAIOSFODNN7EXAMPLE
    /// </summary>
    public static Pattern AWSKeys => new(
        "AWS Access Keys",
        @"AKIA[0-9A-Z]{16}",
        PatternCategory.AWSKey)
    {
        Priority = 180
    };

    /// <summary>
    /// Pattern 9: GitHub Personal Access Tokens
    /// Matches: ghp_..., gho_..., ghu_..., ghs_..., ghr_...
    /// </summary>
    public static Pattern GitHubTokens => new(
        "GitHub Tokens",
        @"\bgh[pousr]_[A-Za-z0-9_]{36,}\b",
        PatternCategory.GitHubToken)
    {
        Priority = 170
    };

    /// <summary>
    /// Pattern 10: OpenAI API Keys
    /// Matches: sk-...48 characters
    /// </summary>
    public static Pattern OpenAIKeys => new(
        "OpenAI API Keys",
        @"\bsk-[A-Za-z0-9]{48}\b",
        PatternCategory.OpenAIKey)
    {
        Priority = 160
    };

    /// <summary>
    /// Pattern 11: Anthropic API Keys
    /// Matches: sk-ant-...88+ characters
    /// </summary>
    public static Pattern AnthropicKeys => new(
        "Anthropic API Keys",
        @"sk-ant-[A-Za-z0-9-]{88,}",
        PatternCategory.AnthropicKey)
    {
        Priority = 155
    };

    /// <summary>
    /// Pattern 12: Private Keys (PEM format)
    /// Matches: -----BEGIN RSA PRIVATE KEY-----, etc.
    /// </summary>
    public static Pattern PrivateKeys => new(
        "Private Keys",
        @"-----BEGIN (?:RSA |EC |DSA |OPENSSH )?PRIVATE KEY-----",
        PatternCategory.PrivateKey)
    {
        Priority = 210 // Highest priority - extremely sensitive
    };

    /// <summary>
    /// Pattern 13: Passwords in Code
    /// Matches: password = "...", apiKey: "...", etc.
    /// </summary>
    public static Pattern PasswordsInCode => new(
        "Passwords in Code",
        @"(?i)(password|passwd|pwd|secret|api_key|apikey|auth_token|authtoken)\s*[=:]\s*[""']?[^\s""']{8,}[""']?",
        PatternCategory.Password)
    {
        Priority = 185
    };

    /// <summary>
    /// Pattern 14: JWT Bearer Tokens
    /// Matches: eyJhbGciOiJI...
    /// </summary>
    public static Pattern JWTTokens => new(
        "JWT Bearer Tokens",
        @"\beyJ[A-Za-z0-9-_]+\.eyJ[A-Za-z0-9-_]+\.[A-Za-z0-9-_.+/=]+\b",
        PatternCategory.BearerToken)
    {
        Priority = 175
    };
}

