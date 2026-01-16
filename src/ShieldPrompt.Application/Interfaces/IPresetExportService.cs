using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for exporting and importing presets.
/// ISP-compliant: 2 methods for export/import only.
/// </summary>
public interface IPresetExportService
{
    /// <summary>
    /// Exports a preset to JSON format for sharing.
    /// </summary>
    string ExportToJson(PromptPreset preset);

    /// <summary>
    /// Imports a preset from JSON format.
    /// Returns null if JSON is invalid.
    /// </summary>
    PromptPreset? ImportFromJson(string json);
}
