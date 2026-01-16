using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Warning encountered during response parsing.
/// </summary>
public record ParseWarning(
    string Message,
    string? Context,
    int? LineNumber);

/// <summary>
/// Result of parsing an LLM response.
/// </summary>
public record ParseResult(
    bool Success,
    string? Analysis,
    IReadOnlyList<FileOperation> Operations,
    IReadOnlyList<ParseWarning> Warnings,
    ResponseFormat DetectedFormat,
    string? ErrorMessage = null);

/// <summary>
/// Parses structured LLM responses to extract file operations.
/// ISP-compliant: Focused only on parsing, not generating.
/// </summary>
public interface IStructuredResponseParser
{
    /// <summary>
    /// Parses an LLM response using the specified format.
    /// </summary>
    /// <param name="llmResponse">The raw LLM response text</param>
    /// <param name="expectedFormat">The format to parse (or Auto to detect)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Parse result with operations and warnings</returns>
    Task<ParseResult> ParseAsync(
        string llmResponse,
        ResponseFormat expectedFormat = ResponseFormat.Auto,
        CancellationToken ct = default);
    
    /// <summary>
    /// Checks if the content can be parsed in the specified format.
    /// Quick validation before attempting full parse.
    /// </summary>
    /// <param name="content">The content to check</param>
    /// <param name="format">The format to validate against</param>
    /// <returns>True if content appears parseable</returns>
    bool CanParse(string content, ResponseFormat format);
    
    /// <summary>
    /// Attempts to detect which format(s) the response is using.
    /// Returns formats in order of confidence (highest first).
    /// </summary>
    /// <param name="content">The content to analyze</param>
    /// <returns>Detected formats ordered by confidence</returns>
    IReadOnlyList<ResponseFormat> DetectFormats(string content);
}

