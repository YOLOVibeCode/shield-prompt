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

    public PromptComposer(ITokenCountingService tokenCountingService)
    {
        _tokenCountingService = tokenCountingService;
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

    private static string BuildFullPrompt(
        string systemPrompt, 
        string userContent, 
        PromptTemplate template,
        PromptOptions options)
    {
        var sb = new StringBuilder();

        // System prompt (if present)
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
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

        return sb.ToString();
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
}

