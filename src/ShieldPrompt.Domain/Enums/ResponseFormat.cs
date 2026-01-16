namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Defines the available response formats for LLM output.
/// Each format optimizes for different LLM behaviors and use cases.
/// </summary>
public enum ResponseFormat
{
    /// <summary>
    /// Markdown for readability with XML island for precise file operations.
    /// Best for: GPT-4, Claude 3.5, General use.
    /// LLM Adherence: 95%, Parse Reliability: 95%, Token Efficiency: 80%
    /// </summary>
    HybridXmlMarkdown = 0,
    
    /// <summary>
    /// Pure XML structure with full schema validation.
    /// Best for: Enterprise workflows, API integrations.
    /// LLM Adherence: 70%, Parse Reliability: 100%, Token Efficiency: 50%
    /// </summary>
    PureXml = 1,
    
    /// <summary>
    /// Natural markdown with special markers (Aider-style).
    /// Best for: Token efficiency, smaller models.
    /// LLM Adherence: 85%, Parse Reliability: 75%, Token Efficiency: 95%
    /// </summary>
    StructuredMarkdown = 2,
    
    /// <summary>
    /// JSON format with strict schema.
    /// Best for: API integrations, programmatic use.
    /// LLM Adherence: 75%, Parse Reliability: 100%, Token Efficiency: 70%
    /// </summary>
    Json = 3,
    
    /// <summary>
    /// Minimal plain text format.
    /// Best for: Quick iterations, maximum compatibility.
    /// LLM Adherence: 95%, Parse Reliability: 50%, Token Efficiency: 100%
    /// </summary>
    PlainText = 4,
    
    /// <summary>
    /// Automatically detect format from response content.
    /// </summary>
    Auto = 99
}

