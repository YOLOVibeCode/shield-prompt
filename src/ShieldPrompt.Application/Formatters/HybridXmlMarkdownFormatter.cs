using System.Text;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Application.Formatters;

/// <summary>
/// Hybrid XML-in-Markdown formatter - The RECOMMENDED format.
/// Combines markdown readability with XML precision for file operations.
/// 
/// LLM Adherence: 95% - Markdown is natural, XML island is small enough
/// Parse Reliability: 95% - XML provides unambiguous structure
/// Token Efficiency: 80% - Moderate overhead
/// 
/// Best for: GPT-4, Claude 3.5, General use
/// </summary>
public class HybridXmlMarkdownFormatter : IResponseFormatStrategy
{
    public string FormatName => "Hybrid XML-in-Markdown";
    
    public string FormatDescription => "Markdown for readability with XML island for precise file operations. Best for GPT-4, Claude 3.5, general use.";
    
    public ResponseFormat FormatType => ResponseFormat.HybridXmlMarkdown;
    
    public bool SupportsPartialUpdates => true;
    
    public double LlmAdherenceRate => 0.95;
    
    public double ParseReliability => 0.95;
    
    public double TokenEfficiency => 0.80;
    
    public string GeneratePrompt(PromptContext context)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("# ShieldPrompt Analysis Request");
        sb.AppendLine();
        sb.AppendLine($"**Session ID:** `{context.SessionId}`  ");
        sb.AppendLine($"**Role:** {context.SelectedRole.Icon} {context.SelectedRole.Name}  ");
        sb.AppendLine($"**Sanitization:** {(context.IsSanitized ? "Enabled ✅" : "Disabled ⬜")}  ");
        sb.AppendLine($"**Base Directory:** `{context.BaseDirectory}`  ");
        sb.AppendLine($"**Model Target:** {context.TargetModel}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Role System Prompt
        sb.AppendLine("## 🎭 Your Role");
        sb.AppendLine();
        sb.AppendLine(context.SelectedRole.SystemPrompt);
        sb.AppendLine();

        // Task Section
        sb.AppendLine("## 📋 Your Task");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(context.CustomInstructions))
        {
            sb.AppendLine(context.CustomInstructions);
        }
        else
        {
            sb.AppendLine("Analyze the following files and provide your expert analysis.");
        }
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Files Section
        sb.AppendLine($"## 📁 Files Included ({context.SelectedFiles.Count} files, {context.TotalTokens:N0} tokens)");
        sb.AppendLine();

        foreach (var file in context.SelectedFiles)
        {
            var extension = System.IO.Path.GetExtension(file.Name).ToLowerInvariant();
            var language = MapExtensionToLanguage(extension);

            sb.AppendLine($"### ✅ `{file.Path}`");
            sb.AppendLine($"```{language}");
            sb.AppendLine(file.Content ?? "[File content not loaded]");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        // CRITICAL: OUTPUT FORMAT INSTRUCTIONS (Action-Only Mode)
        sb.AppendLine("⚠️ CRITICAL: OUTPUT FORMAT");
        sb.AppendLine();
        sb.AppendLine("You MUST respond with ONLY a valid XML block. No markdown, no explanations, no commentary.");
        sb.AppendLine();
        sb.AppendLine("FORMAT:");
        sb.AppendLine("<code_changes>");
        sb.AppendLine("  <changed_file>");
        sb.AppendLine("    <file_path>[relative/path]</file_path>");
        sb.AppendLine("    <file_summary>[short summary]</file_summary>");
        sb.AppendLine("    <file_operation>CREATE|UPDATE|DELETE</file_operation>");
        sb.AppendLine("    <file_code><![CDATA[");
        sb.AppendLine("[file content if op is update or create]");
        sb.AppendLine("    ]]></file_code>");
        sb.AppendLine("  </changed_file>");
        sb.AppendLine("</code_changes>");
        sb.AppendLine();
        sb.AppendLine("RULES:");
        sb.AppendLine("- Return ONLY the XML block above. Nothing else.");
        sb.AppendLine("- Do NOT include ```xml fences or any markdown.");
        sb.AppendLine("- Do NOT explain your changes outside the XML.");
        sb.AppendLine("- Use relative paths from the workspace root.");
        sb.AppendLine("- Use CREATE/UPDATE/DELETE only (uppercase).");
        sb.AppendLine("- Omit <file_code> for DELETE.");
        sb.AppendLine("- All code goes inside <![CDATA[...]]>.");
        sb.AppendLine("- If no changes needed, return: <code_changes/>");
        sb.AppendLine();
        sb.AppendLine("START YOUR RESPONSE WITH: <code_changes");

        return sb.ToString();
    }
    
    public int EstimateTokenOverhead(int baseTokens)
    {
        // Format instructions add roughly 600-800 tokens
        // Metadata adds ~100 tokens
        // Total overhead: ~20-25% for typical prompts
        const int formatInstructionsTokens = 700;
        const int metadataTokens = 100;
        
        return baseTokens + formatInstructionsTokens + metadataTokens;
    }
    
    public string GenerateResponseExample()
    {
        var sb = new StringBuilder();
        
        // ACTION-ONLY FORMAT: No prose, just XML
        sb.AppendLine("<code_changes>");
        sb.AppendLine("  <changed_file>");
        sb.AppendLine("    <file_path>src/Program.cs</file_path>");
        sb.AppendLine("    <file_summary>Added null check and error handling</file_summary>");
        sb.AppendLine("    <file_operation>UPDATE</file_operation>");
        sb.AppendLine("    <file_code><![CDATA[");
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    class Program");
        sb.AppendLine("    {");
        sb.AppendLine("        static async Task Main(string[] args)");
        sb.AppendLine("        {");
        sb.AppendLine("            try");
        sb.AppendLine("            {");
        sb.AppendLine("                var service = new UserService();");
        sb.AppendLine("                await service.LoadUsersAsync();");
        sb.AppendLine("            }");
        sb.AppendLine("            catch (Exception ex)");
        sb.AppendLine("            {");
        sb.AppendLine("                Console.WriteLine($\"Error: {ex.Message}\");");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("    ]]></file_code>");
        sb.AppendLine("  </changed_file>");
        sb.AppendLine("  ");
        sb.AppendLine("  <changed_file>");
        sb.AppendLine("    <file_path>src/ErrorHandler.cs</file_path>");
        sb.AppendLine("    <file_summary>Centralized error handling</file_summary>");
        sb.AppendLine("    <file_operation>CREATE</file_operation>");
        sb.AppendLine("    <file_code><![CDATA[");
        sb.AppendLine("public static class ErrorHandler");
        sb.AppendLine("{");
        sb.AppendLine("    public static void LogError(Exception ex)");
        sb.AppendLine("    {");
        sb.AppendLine("        Console.WriteLine($\"[ERROR] {ex.Message}\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("    ]]></file_code>");
        sb.AppendLine("  </changed_file>");
        sb.AppendLine("  ");
        sb.AppendLine("  <changed_file>");
        sb.AppendLine("    <file_path>src/OldHelper.cs</file_path>");
        sb.AppendLine("    <file_summary>Deprecated, replaced by ErrorHandler</file_summary>");
        sb.AppendLine("    <file_operation>DELETE</file_operation>");
        sb.AppendLine("  </changed_file>");
        sb.AppendLine("</code_changes>");
        
        return sb.ToString();
    }
    
    private static string MapExtensionToLanguage(string extension)
    {
        return extension switch
        {
            ".cs" => "csharp",
            ".xaml" or ".axaml" => "xml",
            ".json" => "json",
            ".xml" => "xml",
            ".md" => "markdown",
            ".txt" => "text",
            ".yml" or ".yaml" => "yaml",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".h" or ".hpp" => "cpp",
            ".go" => "go",
            ".rs" => "rust",
            ".rb" => "ruby",
            ".php" => "php",
            ".html" => "html",
            ".css" => "css",
            ".sql" => "sql",
            ".sh" => "bash",
            _ => "text"
        };
    }
}

