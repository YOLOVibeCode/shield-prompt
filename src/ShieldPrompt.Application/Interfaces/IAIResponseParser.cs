using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for parsing AI responses to extract file operations.
/// ISP-compliant: focused on AI response parsing only.
/// </summary>
public interface IAIResponseParser
{
    /// <summary>
    /// Parses AI response to identify file updates.
    /// Handles ChatGPT, Claude, and other AI response formats.
    /// </summary>
    ParsedAIResponse Parse(
        string aiResponse, 
        IEnumerable<FileNode> originalFiles);
}

/// <summary>
/// Result of parsing an AI response.
/// </summary>
public record ParsedAIResponse(
    IReadOnlyList<FileUpdate> Updates,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Represents a file operation extracted from AI response.
/// </summary>
public record FileUpdate(
    string FilePath,
    string Content,
    FileUpdateType Type,
    int EstimatedLinesChanged);

/// <summary>
/// Type of file operation.
/// </summary>
public enum FileUpdateType
{
    Create,
    Update,
    Delete
}

