using System.Text.Json;
using System.Text.Json.Serialization;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for exporting and importing presets as JSON.
/// </summary>
public class PresetExportService : IPresetExportService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public string ExportToJson(PromptPreset preset)
    {
        // Export only the shareable properties, excluding usage statistics
        var exportModel = new PresetExportModel
        {
            Name = preset.Name,
            Description = preset.Description,
            Icon = preset.Icon,
            FilePatterns = preset.FilePatterns.ToList(),
            ExplicitFilePaths = preset.ExplicitFilePaths.ToList(),
            CustomInstructions = preset.CustomInstructions,
            RoleId = preset.RoleId,
            ModelId = preset.ModelId,
            IncludeLineNumbers = preset.IncludeLineNumbers
        };

        return JsonSerializer.Serialize(exportModel, SerializerOptions);
    }

    /// <inheritdoc />
    public PromptPreset? ImportFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var exportModel = JsonSerializer.Deserialize<PresetExportModel>(json, SerializerOptions);
            if (exportModel is null || string.IsNullOrWhiteSpace(exportModel.Name))
                return null;

            // Create a new preset with fresh ID and reset statistics
            return new PromptPreset
            {
                Id = Guid.NewGuid().ToString(),
                Name = exportModel.Name,
                Description = exportModel.Description ?? string.Empty,
                Icon = exportModel.Icon ?? "📋",
                CreatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow,
                UsageCount = 0,
                Scope = PresetScope.Global, // Imported presets default to global
                FilePatterns = (IReadOnlyList<string>?)exportModel.FilePatterns ?? Array.Empty<string>(),
                ExplicitFilePaths = (IReadOnlyList<string>?)exportModel.ExplicitFilePaths ?? Array.Empty<string>(),
                CustomInstructions = exportModel.CustomInstructions ?? string.Empty,
                RoleId = exportModel.RoleId,
                ModelId = exportModel.ModelId,
                IncludeLineNumbers = exportModel.IncludeLineNumbers,
                IsPinned = false,
                IsBuiltIn = false
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Internal model for export/import serialization.
    /// Only includes shareable properties.
    /// </summary>
    private class PresetExportModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<string>? FilePatterns { get; set; }
        public List<string>? ExplicitFilePaths { get; set; }
        public string? CustomInstructions { get; set; }
        public string? RoleId { get; set; }
        public string? ModelId { get; set; }
        public bool IncludeLineNumbers { get; set; }
    }
}
