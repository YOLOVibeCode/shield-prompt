using System.Text.RegularExpressions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Parser for extracting file operations from AI responses.
/// Handles ChatGPT, Claude, and other AI formats.
/// </summary>
public class AIResponseParser : IAIResponseParser
{
    // Matches markdown code blocks with optional file headers
    private static readonly Regex CodeBlockPattern = new(
        @"```(?<lang>\w+)?\s*\n(?<content>.*?)```",
        RegexOptions.Singleline | RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));
    
    // Matches file headers: **Filename.cs**, `Filename.cs`, or // File: Filename.cs
    private static readonly Regex FileHeaderPattern = new(
        @"(?:\*\*|``)([^\*`\s]+\.(?:cs|json|txt|md|xml|yaml|yml|config|csproj))(?:\*\*|``)|(?://|#)\s*File:\s*([^\n]+\.(?:cs|json|txt|md))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromSeconds(1));
    
    // Detects "new file" indicators
    private static readonly Regex NewFilePattern = new(
        @"\(new file\)|\bnew file\b|create.*file",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ParsedAIResponse Parse(string aiResponse, IEnumerable<FileNode> originalFiles)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            return new ParsedAIResponse(Array.Empty<FileUpdate>(), Array.Empty<string>());

        var updates = new List<FileUpdate>();
        var warnings = new List<string>();
        var originalFileList = originalFiles.ToList();
        
        // Extract all code blocks
        var codeBlocks = ExtractCodeBlocks(aiResponse);
        
        if (codeBlocks.Count == 0)
        {
            warnings.Add("No code blocks found in AI response");
            return new ParsedAIResponse(updates, warnings);
        }
        
        // Try to match code blocks to files
        for (int i = 0; i < codeBlocks.Count; i++)
        {
            var (content, startPos) = codeBlocks[i];
            
            // Strategy 1: Look for file header before this code block
            var filePath = ExtractFilePathBeforePosition(aiResponse, startPos);
            
            // Strategy 2: Extract from comment inside code block
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = ExtractFilePathFromComment(content);
            }
            
            // Strategy 3: Map to original files in order
            if (string.IsNullOrEmpty(filePath) && i < originalFileList.Count)
            {
                filePath = Path.GetFileName(originalFileList[i].Path);
            }
            
            // Strategy 4: Use generic name
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = $"UpdatedFile{i + 1}.cs";
                warnings.Add($"Could not determine file name for code block {i + 1}, using {filePath}");
            }
            
            // Determine if this is a new file or update
            var type = DetermineUpdateType(filePath, originalFileList, aiResponse, startPos);
            
            // Count non-empty lines for preview
            var lines = content.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            
            updates.Add(new FileUpdate(
                filePath,
                content.Trim(),
                type,
                lines));
        }
        
        return new ParsedAIResponse(updates, warnings);
    }

    private List<(string Content, int StartPosition)> ExtractCodeBlocks(string response)
    {
        var blocks = new List<(string, int)>();
        var matches = CodeBlockPattern.Matches(response);
        
        foreach (Match match in matches)
        {
            if (match.Success)
            {
                var content = match.Groups["content"].Value;
                blocks.Add((content, match.Index));
            }
        }
        
        return blocks;
    }

    private string? ExtractFilePathBeforePosition(string response, int position)
    {
        // Look at text before the code block (up to 500 chars back)
        var start = Math.Max(0, position - 500);
        var textBefore = response.Substring(start, position - start);
        
        var matches = FileHeaderPattern.Matches(textBefore);
        if (matches.Count > 0)
        {
            var lastMatch = matches[matches.Count - 1];
            return lastMatch.Groups[1].Value != string.Empty 
                ? lastMatch.Groups[1].Value 
                : lastMatch.Groups[2].Value;
        }
        
        return null;
    }

    private string? ExtractFilePathFromComment(string content)
    {
        // Look for // File: path/to/file.cs at start of content
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            var firstLine = lines[0].Trim();
            if (firstLine.StartsWith("//") || firstLine.StartsWith("#"))
            {
                var match = Regex.Match(firstLine, @"File:\s*(.+\.(?:cs|json|txt|md))");
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
        }
        
        return null;
    }

    private FileUpdateType DetermineUpdateType(
        string filePath, 
        List<FileNode> originalFiles,
        string response,
        int position)
    {
        // Check if file was in original set
        var existsInOriginal = originalFiles.Any(f => 
            Path.GetFileName(f.Path).Equals(filePath, StringComparison.OrdinalIgnoreCase) ||
            f.Path.EndsWith(filePath, StringComparison.OrdinalIgnoreCase));
        
        if (!existsInOriginal)
        {
            return FileUpdateType.Create;
        }
        
        // Check for "new file" indicators near this position
        var contextStart = Math.Max(0, position - 200);
        var contextEnd = Math.Min(response.Length, position + 200);
        var context = response.Substring(contextStart, contextEnd - contextStart);
        
        if (NewFilePattern.IsMatch(context))
        {
            return FileUpdateType.Create;
        }
        
        return FileUpdateType.Update;
    }
}

