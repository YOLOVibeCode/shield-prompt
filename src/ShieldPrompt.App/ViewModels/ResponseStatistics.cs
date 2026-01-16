using ShieldPrompt.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ShieldPrompt.App.ViewModels;

/// <summary>
/// Statistics about a parsed LLM response.
/// Provides summary metrics for the dashboard UI.
/// </summary>
public record ResponseStatistics(
    int TotalOperations,
    int UpdateCount,
    int CreateCount,
    int DeleteCount,
    int WarningCount,
    int EstimatedLinesAffected,
    ResponseFormat DetectedFormat,
    DateTime ParsedAt)
{
    /// <summary>
    /// Gets whether this response has any warnings.
    /// </summary>
    public bool HasWarnings => WarningCount > 0;
    
    /// <summary>
    /// Gets whether this response contains destructive operations (deletes).
    /// </summary>
    public bool HasDestructiveOperations => DeleteCount > 0;
    
    /// <summary>
    /// Gets whether this response is empty (no operations).
    /// </summary>
    public bool IsEmpty => TotalOperations == 0;
    
    /// <summary>
    /// Gets a human-readable summary of the statistics.
    /// </summary>
    public string SummaryText
    {
        get
        {
            if (IsEmpty)
                return "No operations found in response.";
            
            var parts = new List<string>();
            
            if (UpdateCount > 0)
                parts.Add($"{UpdateCount} update{(UpdateCount == 1 ? "" : "s")}");
            if (CreateCount > 0)
                parts.Add($"{CreateCount} create{(CreateCount == 1 ? "" : "s")}");
            if (DeleteCount > 0)
                parts.Add($"{DeleteCount} delete{(DeleteCount == 1 ? "" : "s")}");
            
            return $"{TotalOperations} operations: {string.Join(", ", parts)}";
        }
    }
    
    /// <summary>
    /// Creates empty statistics (for initialization).
    /// </summary>
    public static ResponseStatistics Empty => new(
        TotalOperations: 0,
        UpdateCount: 0,
        CreateCount: 0,
        DeleteCount: 0,
        WarningCount: 0,
        EstimatedLinesAffected: 0,
        DetectedFormat: ResponseFormat.HybridXmlMarkdown,
        ParsedAt: DateTime.UtcNow);
}

