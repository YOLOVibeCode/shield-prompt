using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Composes the final prompt from template + files + options.
/// Implements ISP - single responsibility: prompt composition only.
/// </summary>
public class PromptComposer : IPromptComposer
{
    private readonly ITokenCountingService _tokenCountingService;
    private readonly IXmlPromptBuilder _xmlPromptBuilder;

    public PromptComposer(
        ITokenCountingService tokenCountingService,
        IXmlPromptBuilder xmlPromptBuilder)
    {
        _tokenCountingService = tokenCountingService;
        _xmlPromptBuilder = xmlPromptBuilder;
    }

    public ComposedPrompt Compose(
        PromptTemplate template,
        IEnumerable<FileNode> files,
        PromptOptions options)
    {
        var fileList = files.ToList();
        var warnings = new List<string>();

        // 1. Build system prompt with variable substitution
        var systemPrompt = SubstituteVariables(
            template.SystemPrompt, 
            fileList, 
            options);

        // 2. Build user content (file listings)
        var userContent = BuildUserContent(fileList, options);

        // 3. Build full prompt (system + user)
        var fullPrompt = BuildFullPrompt(systemPrompt, userContent, template, options);

        // 4. Count tokens
        var estimatedTokens = _tokenCountingService.CountTokens(fullPrompt);

        // 5. Generate warnings
        if (template.RequiresCustomInput && string.IsNullOrWhiteSpace(options.CustomInstructions))
        {
            warnings.Add("⚠️ This template requires custom instructions. Please provide context.");
        }

        if (fileList.Count == 0)
        {
            warnings.Add("⚠️ No files selected. The prompt will not include any code.");
        }

        return new ComposedPrompt(
            SystemPrompt: systemPrompt,
            UserContent: userContent,
            FullPrompt: fullPrompt,
            EstimatedTokens: estimatedTokens,
            Warnings: warnings);
    }

    private static string SubstituteVariables(
        string systemPrompt, 
        List<FileNode> files, 
        PromptOptions options)
    {
        var result = systemPrompt;

        // Substitute {custom_instructions}
        if (result.Contains("{custom_instructions}"))
        {
            var instructions = options.CustomInstructions ?? "(No specific instructions provided)";
            result = result.Replace("{custom_instructions}", instructions);
        }

        // Substitute {file_count}
        result = result.Replace("{file_count}", files.Count.ToString());

        // Substitute {language} - detect from file extensions
        var language = DetectPrimaryLanguage(files);
        result = result.Replace("{language}", language);

        return result;
    }

    private static string BuildUserContent(List<FileNode> files, PromptOptions options)
    {
        if (files.Count == 0)
            return "(No files provided)";

        var sb = new StringBuilder();
        sb.AppendLine("## Files for Analysis");
        sb.AppendLine();

        foreach (var file in files)
        {
            // File header
            if (options.IncludeFilePaths)
            {
                sb.AppendLine($"### `{file.Path}`");
            }
            else
            {
                sb.AppendLine($"### `{file.Name}`");
            }
            sb.AppendLine();

            // File content
            var extension = System.IO.Path.GetExtension(file.Name).TrimStart('.');
            var language = MapExtensionToLanguage(extension);
            
            sb.AppendLine($"```{language}");
            
            if (options.IncludeLineNumbers)
            {
                var lines = file.Content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    sb.AppendLine($"{i + 1,4} | {lines[i]}");
                }
            }
            else
            {
                sb.AppendLine(file.Content);
            }
            
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string BuildFullPrompt(
        string systemPrompt, 
        string userContent, 
        PromptTemplate template,
        PromptOptions options)
    {
        // NEW: Use BuildFullPromptWithXml for type-safe XML generation
        return BuildFullPromptWithXml(systemPrompt, userContent, template, options);
    }

    private static string DetectPrimaryLanguage(List<FileNode> files)
    {
        if (files.Count == 0)
            return "unknown";

        // Count extensions
        var extensionCounts = files
            .GroupBy(f => System.IO.Path.GetExtension(f.Name).TrimStart('.').ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .ToList();

        if (extensionCounts.Count == 0)
            return "unknown";

        var mostCommonExtension = extensionCounts[0].Key;
        return MapExtensionToLanguage(mostCommonExtension);
    }

    private static string MapExtensionToLanguage(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            "cs" => "csharp",
            "ts" => "typescript",
            "js" => "javascript",
            "jsx" => "jsx",
            "tsx" => "tsx",
            "py" => "python",
            "java" => "java",
            "cpp" or "cc" or "cxx" => "cpp",
            "c" => "c",
            "h" or "hpp" => "cpp",
            "go" => "go",
            "rs" => "rust",
            "rb" => "ruby",
            "php" => "php",
            "swift" => "swift",
            "kt" or "kts" => "kotlin",
            "scala" => "scala",
            "sql" => "sql",
            "sh" or "bash" => "bash",
            "ps1" => "powershell",
            "yaml" or "yml" => "yaml",
            "json" => "json",
            "xml" => "xml",
            "html" => "html",
            "css" => "css",
            "scss" or "sass" => "scss",
            "md" => "markdown",
            _ => extension
        };
    }

    private static void AppendResponseFormatContract(StringBuilder sb)
    {
        // NOTE: This method is now deprecated in favor of XmlPromptBuilder.AppendResponseInstructions()
        // Keeping minimal implementation for backward compatibility during migration.
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Response Format (Action-Only XML)");
        sb.AppendLine();
        sb.AppendLine("Respond ONLY with valid XML and nothing else.");
        sb.AppendLine();
        sb.AppendLine("<code_changes>");
        sb.AppendLine("  <changed_file>");
        sb.AppendLine("    <file_path>{relative/path}</file_path>");
        sb.AppendLine("    <file_summary>{short summary}</file_summary>");
        sb.AppendLine("    <file_operation>CREATE|UPDATE|DELETE</file_operation>");
        sb.AppendLine("    <file_code><![CDATA[{full file contents}]]></file_code>");
        sb.AppendLine("  </changed_file>");
        sb.AppendLine("</code_changes>");
        sb.AppendLine();
        sb.AppendLine("Rules:");
        sb.AppendLine("1) No markdown, no explanations, no extra text.");
        sb.AppendLine("2) One <changed_file> per file.");
        sb.AppendLine("3) For DELETE: omit <file_code>.");
        sb.AppendLine("4) Use relative paths from repo root.");
        sb.AppendLine("5) If unsure, return empty <code_changes/>.");
        sb.AppendLine();
        sb.AppendLine("START YOUR RESPONSE WITH: <code_changes");
    }
    
    /// <summary>
    /// Builds the full prompt with XML response instructions using XmlPromptBuilder.
    /// This is the NEW, type-safe implementation that replaces string concatenation.
    /// </summary>
    private string BuildFullPromptWithXml(
        string systemPrompt, 
        string userContent, 
        PromptTemplate template,
        PromptOptions options)
    {
        var sb = new StringBuilder();

        // Role system prompt (if selected)
        if (options.SelectedRole != null)
        {
            sb.AppendLine("# AI Role");
            sb.AppendLine();
            sb.AppendLine($"**You are acting as: {options.SelectedRole.Name} {options.SelectedRole.Icon}**");
            sb.AppendLine();
            sb.AppendLine(options.SelectedRole.SystemPrompt);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // Template system prompt (if present)
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            sb.AppendLine("# Task");
            sb.AppendLine();
            sb.AppendLine(systemPrompt);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // Focus areas (if selected)
        if (options.SelectedFocusAreas?.Count > 0)
        {
            sb.AppendLine("**Focus Areas:**");
            foreach (var area in options.SelectedFocusAreas)
            {
                sb.AppendLine($"- {area}");
            }
            sb.AppendLine();
        }

        // Custom instructions (if provided and not already in system prompt)
        if (!string.IsNullOrWhiteSpace(options.CustomInstructions) 
            && !systemPrompt.Contains("{custom_instructions}"))
        {
            sb.AppendLine("**Additional Context:**");
            sb.AppendLine(options.CustomInstructions);
            sb.AppendLine();
        }

        // User content (files)
        sb.Append(userContent);

        // NEW: Use XmlPromptBuilder for type-safe XML response format
        var promptWithoutResponseInstructions = sb.ToString();
        return _xmlPromptBuilder.AppendResponseInstructions(promptWithoutResponseInstructions);
    }
}

