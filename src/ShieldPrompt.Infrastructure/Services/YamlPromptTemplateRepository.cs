using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// Loads prompt templates from YAML files.
/// Implements ISP - single responsibility: template loading only.
/// </summary>
public class YamlPromptTemplateRepository : IPromptTemplateRepository
{
    private readonly string _configPath;
    private List<PromptTemplate>? _cachedTemplates;

    public YamlPromptTemplateRepository(string? configPath = null)
    {
        // Default to config/prompt-templates.yaml in application directory
        _configPath = configPath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..", "config", "prompt-templates.yaml");
    }

    public IReadOnlyList<PromptTemplate> GetAllTemplates()
    {
        if (_cachedTemplates != null)
            return _cachedTemplates;

        try
        {
            var fullPath = Path.GetFullPath(_configPath);
            
            if (!File.Exists(fullPath))
            {
                // Return embedded fallback templates if YAML file missing
                _cachedTemplates = GetFallbackTemplates();
                return _cachedTemplates;
            }

            var yaml = File.ReadAllText(fullPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var yamlData = deserializer.Deserialize<YamlTemplateFile>(yaml);
            
            _cachedTemplates = yamlData.Templates
                .Select(t => new PromptTemplate(
                    Id: t.Id ?? "unknown",
                    Name: t.Name ?? "Unknown",
                    Icon: t.Icon ?? "💬",
                    Description: t.Description ?? "",
                    SystemPrompt: t.SystemPrompt ?? "",
                    FocusOptions: t.FocusOptions?.ToArray() ?? Array.Empty<string>(),
                    RequiresCustomInput: t.RequiresCustom,
                    Placeholder: t.Placeholder))
                .ToList();

            return _cachedTemplates;
        }
        catch (Exception ex)
        {
            // Log error and return fallback templates
            Console.Error.WriteLine($"Error loading templates from {_configPath}: {ex.Message}");
            _cachedTemplates = GetFallbackTemplates();
            return _cachedTemplates;
        }
    }

    public PromptTemplate? GetById(string templateId)
    {
        return GetAllTemplates().FirstOrDefault(t => t.Id == templateId);
    }

    private static List<PromptTemplate> GetFallbackTemplates()
    {
        // Embedded fallback templates if YAML file is missing
        return new List<PromptTemplate>
        {
            new PromptTemplate(
                Id: "code_review",
                Name: "Code Review",
                Icon: "🔍",
                Description: "Review code for bugs, security issues, and best practices",
                SystemPrompt: "You are an expert code reviewer. Review the following code for bugs, security vulnerabilities, performance issues, and best practices. Provide specific, actionable feedback.",
                FocusOptions: new[] { "Security", "Performance", "Readability", "Testing" },
                RequiresCustomInput: false),

            new PromptTemplate(
                Id: "debug",
                Name: "Debug Issue",
                Icon: "🐛",
                Description: "Analyze code to find and fix bugs",
                SystemPrompt: "You are a debugging expert. The user is experiencing an issue:\n\n{custom_instructions}\n\nAnalyze the code to identify the root cause, explain why it fails, and provide a fix.",
                FocusOptions: new[] { "Logic Errors", "Runtime Exceptions", "Edge Cases" },
                RequiresCustomInput: true,
                Placeholder: "Describe the error or unexpected behavior..."),

            new PromptTemplate(
                Id: "explain",
                Name: "Explain Code",
                Icon: "📖",
                Description: "Explain code clearly for learning",
                SystemPrompt: "You are a code explainer. Explain this code clearly including: what it does, how it works, key concepts used, and potential gotchas.",
                FocusOptions: new[] { "Architecture", "Algorithms", "Design Patterns" },
                RequiresCustomInput: false),

            new PromptTemplate(
                Id: "generic",
                Name: "Custom Prompt",
                Icon: "💬",
                Description: "Create your own custom prompt",
                SystemPrompt: "",
                FocusOptions: Array.Empty<string>(),
                RequiresCustomInput: true,
                Placeholder: "Enter your instructions for the AI...")
        };
    }

    // YAML deserialization classes
    private class YamlTemplateFile
    {
        public List<YamlTemplate> Templates { get; set; } = new();
    }

    private class YamlTemplate
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public string? SystemPrompt { get; set; }
        public List<string>? FocusOptions { get; set; }
        public bool RequiresCustom { get; set; }
        public string? Placeholder { get; set; }
    }
}

