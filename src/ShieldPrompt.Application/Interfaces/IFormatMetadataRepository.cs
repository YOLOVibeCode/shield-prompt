using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for retrieving format metadata (pros/cons).
/// ISP-compliant: Only read operations for format information.
/// </summary>
public interface IFormatMetadataRepository
{
    /// <summary>
    /// Gets all available format metadata.
    /// </summary>
    IReadOnlyList<FormatMetadata> GetAllFormats();
    
    /// <summary>
    /// Gets metadata for a specific format by ID.
    /// </summary>
    /// <param name="formatId">The format identifier (e.g., "markdown", "xml").</param>
    /// <returns>The format metadata if found, null otherwise.</returns>
    FormatMetadata? GetById(string formatId);
}

