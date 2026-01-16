namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Defines the type of file operation to perform.
/// </summary>
public enum FileOperationType
{
    /// <summary>
    /// Replace entire file content.
    /// </summary>
    Update,
    
    /// <summary>
    /// Replace specific line range (partial update).
    /// Requires StartLine and EndLine to be specified.
    /// </summary>
    PartialUpdate,
    
    /// <summary>
    /// Create new file.
    /// </summary>
    Create,
    
    /// <summary>
    /// Delete existing file.
    /// </summary>
    Delete,

    /// <summary>
    /// Rename or move a file.
    /// Requires OriginalPath to be specified in FileOperation.
    /// </summary>
    Rename
}

