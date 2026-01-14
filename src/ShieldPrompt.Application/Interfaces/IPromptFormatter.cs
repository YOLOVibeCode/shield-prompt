using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Formats aggregated file contents into a structured prompt.
/// ISP-compliant: single responsibility - formatting only.
/// </summary>
public interface IPromptFormatter
{
    /// <summary>
    /// Formats the given files into a prompt string.
    /// </summary>
    string Format(IEnumerable<FileNode> files, string? taskDescription = null);

    /// <summary>
    /// Name of this format (e.g., "XML", "Markdown").
    /// </summary>
    string FormatName { get; }
}

