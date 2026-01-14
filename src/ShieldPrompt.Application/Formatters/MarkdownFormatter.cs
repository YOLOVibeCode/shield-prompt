using System.Text;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Formatters;

/// <summary>
/// Formats prompts as Markdown with code blocks.
/// </summary>
public class MarkdownFormatter : IPromptFormatter
{
    public string FormatName => "Markdown";

    public string Format(IEnumerable<FileNode> files, string? taskDescription = null)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Project Context");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(taskDescription))
        {
            sb.AppendLine("## Task");
            sb.AppendLine(taskDescription);
            sb.AppendLine();
        }

        sb.AppendLine("## Files");
        sb.AppendLine();

        foreach (var file in files.Where(f => !f.IsDirectory))
        {
            sb.AppendLine($"### `{file.Name}`");
            sb.AppendLine();

            var language = GetLanguageHint(file.Extension);
            sb.AppendLine($"```{language}");

            if (File.Exists(file.Path))
            {
                try
                {
                    var content = File.ReadAllText(file.Path);
                    sb.AppendLine(content);
                }
                catch
                {
                    sb.AppendLine("// Error reading file");
                }
            }

            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetLanguageHint(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" => "cpp",
            ".c" => "c",
            ".go" => "go",
            ".rs" => "rust",
            ".rb" => "ruby",
            ".php" => "php",
            ".swift" => "swift",
            ".kt" => "kotlin",
            ".sql" => "sql",
            ".sh" or ".bash" => "bash",
            ".ps1" => "powershell",
            ".html" or ".htm" => "html",
            ".css" => "css",
            ".scss" or ".sass" => "scss",
            ".json" => "json",
            ".xml" => "xml",
            ".yaml" or ".yml" => "yaml",
            ".md" => "markdown",
            _ => ""
        };
    }
}

