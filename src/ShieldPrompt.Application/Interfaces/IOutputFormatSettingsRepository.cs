using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for managing output format settings.
/// ISP-compliant: Focused only on format settings, not general app settings.
/// </summary>
public interface IOutputFormatSettingsRepository
{
    /// <summary>
    /// Loads the current output format settings.
    /// Returns default settings if none exist.
    /// </summary>
    Task<OutputFormatSettings> LoadAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Saves the output format settings.
    /// </summary>
    Task SaveAsync(OutputFormatSettings settings, CancellationToken ct = default);
    
    /// <summary>
    /// Resets settings to default values.
    /// </summary>
    Task ResetToDefaultAsync(CancellationToken ct = default);
}

