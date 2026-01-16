using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Defines a file operation extracted from an LLM response.
/// Immutable record for safety and clarity.
/// </summary>
public record FileOperation(
    FileOperationType Type,
    string Path,
    string? Content,
    string Reason,
    int? StartLine = null,
    int? EndLine = null,
    string? OriginalPath = null)
{
    /// <summary>
    /// Validates that the file operation is well-formed.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Path))
            return false;

        if (string.IsNullOrWhiteSpace(Reason))
            return false;

        // Partial updates must have line numbers
        if (Type == FileOperationType.PartialUpdate)
        {
            if (!StartLine.HasValue || !EndLine.HasValue)
                return false;

            if (StartLine.Value < 1 || EndLine.Value < StartLine.Value)
                return false;
        }

        // Update and Create must have content
        if (Type is FileOperationType.Update or FileOperationType.PartialUpdate or FileOperationType.Create)
        {
            if (string.IsNullOrWhiteSpace(Content))
                return false;
        }

        // Rename must have OriginalPath
        if (Type == FileOperationType.Rename)
        {
            if (string.IsNullOrWhiteSpace(OriginalPath))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if this is a destructive operation (delete or overwrite).
    /// </summary>
    public bool IsDestructive => Type is FileOperationType.Delete or FileOperationType.Update;
}

