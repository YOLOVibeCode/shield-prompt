using System.Collections.Generic;
using System.Linq;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// Static implementation of format metadata repository.
/// Provides pros/cons for each prompt format based on research.
/// </summary>
public class StaticFormatMetadataRepository : IFormatMetadataRepository
{
    private static readonly List<FormatMetadata> _formats = new()
    {
        new FormatMetadata(
            Id: "markdown",
            Name: "Markdown",
            Description: "Natural, human-readable format with syntax highlighting support",
            Pros: new[]
            {
                "Native LLM understanding - trained extensively on markdown",
                "Human-readable and easy to scan",
                "Excellent syntax highlighting in code blocks",
                "Balances structure with readability",
                "Widely supported across tools",
                "Great for mixed content (text + code)"
            },
            Cons: new[]
            {
                "Less rigid structure than XML/JSON",
                "May require post-processing for complex parsing",
                "Ambiguity in edge cases (nested lists, tables)"
            },
            RecommendedFor: "Most AI interactions, code analysis, general prompts",
            Icon: "📝"),

        new FormatMetadata(
            Id: "xml",
            Name: "XML",
            Description: "Highly structured format with schema validation support",
            Pros: new[]
            {
                "Extremely structured and machine-parseable",
                "Schema validation (XSD) ensures correctness",
                "Clear hierarchical relationships",
                "Self-documenting with attributes",
                "Excellent for complex, nested data"
            },
            Cons: new[]
            {
                "Verbose syntax increases token count",
                "Less natural for LLMs compared to markdown",
                "Harder for humans to read/write",
                "Requires more careful formatting"
            },
            RecommendedFor: "Complex data structures, strict validation needs, API schemas",
            Icon: "📋"),

        new FormatMetadata(
            Id: "json",
            Name: "JSON",
            Description: "Lightweight data interchange format, machine-friendly",
            Pros: new[]
            {
                "Structured and machine-parseable",
                "Compact compared to XML",
                "Universal programming language support",
                "Clear key-value relationships",
                "Easy to validate with JSON Schema"
            },
            Cons: new[]
            {
                "Not ideal for prose or explanations",
                "Code blocks require escaping (strings)",
                "Less readable for long content",
                "Limited support for comments"
            },
            RecommendedFor: "Structured data exchange, API responses, configuration",
            Icon: "🔧"),

        new FormatMetadata(
            Id: "plaintext",
            Name: "Plain Text",
            Description: "Simple, unformatted text without markup",
            Pros: new[]
            {
                "Simplest possible format",
                "No syntax overhead",
                "Maximum flexibility",
                "Universal compatibility"
            },
            Cons: new[]
            {
                "No structure or hierarchy",
                "No syntax highlighting",
                "Ambiguous boundaries between sections",
                "Harder for LLMs to parse context",
                "Manual formatting required"
            },
            RecommendedFor: "Quick notes, simple queries, minimal context",
            Icon: "📄")
    };

    public IReadOnlyList<FormatMetadata> GetAllFormats()
    {
        return _formats.AsReadOnly();
    }

    public FormatMetadata? GetById(string formatId)
    {
        return _formats.FirstOrDefault(f => 
            f.Id.Equals(formatId, StringComparison.OrdinalIgnoreCase));
    }
}

