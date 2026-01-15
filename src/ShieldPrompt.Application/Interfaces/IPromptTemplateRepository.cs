using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for loading prompt templates.
/// ISP: Single responsibility - only template retrieval (≤5 methods).
/// </summary>
public interface IPromptTemplateRepository
{
    /// <summary>
    /// Gets all available templates
    /// </summary>
    IReadOnlyList<PromptTemplate> GetAllTemplates();
    
    /// <summary>
    /// Gets a specific template by ID
    /// </summary>
    PromptTemplate? GetById(string templateId);
}

/// <summary>
/// Composes the final prompt from template + files + options.
/// ISP: Single responsibility - prompt composition (≤3 methods).
/// </summary>
public interface IPromptComposer
{
    /// <summary>
    /// Composes a complete prompt from template, files, and options
    /// </summary>
    ComposedPrompt Compose(
        PromptTemplate template,
        IEnumerable<FileNode> files,
        PromptOptions options);
}

