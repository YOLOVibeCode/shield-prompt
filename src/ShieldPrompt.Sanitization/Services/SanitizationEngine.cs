using System.Text.RegularExpressions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Sanitization.Services;

/// <summary>
/// Engine for sanitizing sensitive data by replacing with aliases.
/// </summary>
public class SanitizationEngine(
    IPatternRegistry patternRegistry,
    IMappingSession session,
    IAliasGenerator aliasGenerator) : ISanitizationEngine
{
    public SanitizationResult Sanitize(string content, SanitizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(content))
        {
            return new SanitizationResult(content, false, Array.Empty<SanitizationMatch>());
        }

        var matches = new List<SanitizationMatch>();
        var result = content;

        // Get enabled patterns, ordered by priority (highest first)
        var patterns = patternRegistry.GetPatterns()
            .Where(p => ShouldProcessPattern(p, options))
            .OrderByDescending(p => p.Priority)
            .ToList();

        foreach (var pattern in patterns)
        {
            try
            {
                var regex = pattern.CreateRegex();
                var regexMatches = regex.Matches(result);

                // Process matches in reverse order to maintain string indices
                foreach (Match match in regexMatches.Cast<Match>().Reverse())
                {
                    var originalValue = match.Value;

                    // Check if we already have an alias for this value
                    var alias = session.GetAlias(originalValue);
                    if (alias == null)
                    {
                        // Generate new alias
                        alias = aliasGenerator.Generate(pattern.Category);
                        session.AddMapping(originalValue, alias, pattern.Category);
                    }

                    // Replace in content
                    result = result.Remove(match.Index, match.Length)
                                   .Insert(match.Index, alias);

                    // Record the match
                    matches.Add(new SanitizationMatch(
                        originalValue,
                        alias,
                        pattern.Category,
                        pattern.Name,
                        match.Index,
                        match.Length));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // Pattern timed out - skip it for safety
                continue;
            }
        }

        var wasSanitized = matches.Count > 0;
        return new SanitizationResult(result, wasSanitized, matches);
    }

    private static bool ShouldProcessPattern(Pattern pattern, SanitizationOptions options)
    {
        // Check category-specific options
        return pattern.Category switch
        {
            // Infrastructure patterns
            PatternCategory.Database or
            PatternCategory.Server or
            PatternCategory.IPAddress or
            PatternCategory.Hostname or
            PatternCategory.ConnectionString or
            PatternCategory.FilePath => options.EnableInfrastructure,

            // PII patterns
            PatternCategory.SSN or
            PatternCategory.CreditCard or
            PatternCategory.APIKey or
            PatternCategory.AWSKey or
            PatternCategory.GitHubToken or
            PatternCategory.OpenAIKey or
            PatternCategory.AnthropicKey or
            PatternCategory.SlackToken or
            PatternCategory.AzureKey or
            PatternCategory.PrivateKey or
            PatternCategory.Password or
            PatternCategory.BearerToken => options.EnablePII,

            // Custom patterns
            PatternCategory.Custom => options.EnableCustomPatterns,

            // Default - include
            _ => true
        };
    }
}

