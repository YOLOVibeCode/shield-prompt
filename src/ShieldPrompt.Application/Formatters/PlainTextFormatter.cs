using System.Text;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Formatters;

/// <summary>
/// Formats prompts as plain text with file separators.
/// </summary>
public class PlainTextFormatter : IPromptFormatter
{
    public string FormatName => "Plain Text";

    public string Format(IEnumerable<FileNode> files, string? taskDescription = null)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(taskDescription))
        {
            sb.AppendLine("TASK:");
            sb.AppendLine(taskDescription);
            sb.AppendLine();
            sb.AppendLine("FILES:");
            sb.AppendLine();
        }

        foreach (var file in files.Where(f => !f.IsDirectory))
        {
            sb.AppendLine($"=== FILE: {file.Path} ===");
            
            if (File.Exists(file.Path))
            {
                try
                {
                    var content = File.ReadAllText(file.Path);
                    sb.AppendLine(content);
                }
                catch
                {
                    sb.AppendLine("[Error reading file]");
                }
            }
            
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

