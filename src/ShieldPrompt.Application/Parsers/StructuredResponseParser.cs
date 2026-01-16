using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Parsers;

/// <summary>
/// Parses structured LLM responses to extract file operations.
/// Supports multiple format types with automatic detection.
/// </summary>
public class StructuredResponseParser : IStructuredResponseParser
{
    public async Task<ParseResult> ParseAsync(
        string llmResponse,
        ResponseFormat expectedFormat = ResponseFormat.Auto,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(llmResponse))
        {
            return new ParseResult(
                Success: true,
                Analysis: string.Empty,
                Operations: Array.Empty<FileOperation>(),
                Warnings: Array.Empty<ParseWarning>(),
                DetectedFormat: ResponseFormat.PlainText);
        }
        
        // Auto-detect format if requested
        var formatToUse = expectedFormat == ResponseFormat.Auto
            ? DetectFormats(llmResponse).FirstOrDefault(ResponseFormat.HybridXmlMarkdown)
            : expectedFormat;
        
        return formatToUse switch
        {
            ResponseFormat.HybridXmlMarkdown => await ParseHybridXmlMarkdownAsync(llmResponse, ct),
            ResponseFormat.PureXml => await ParsePureXmlAsync(llmResponse, ct),
            ResponseFormat.StructuredMarkdown => await ParseStructuredMarkdownAsync(llmResponse, ct),
            ResponseFormat.Json => await ParseJsonAsync(llmResponse, ct),
            ResponseFormat.PlainText => await ParsePlainTextAsync(llmResponse, ct),
            _ => new ParseResult(
                Success: false,
                Analysis: null,
                Operations: Array.Empty<FileOperation>(),
                Warnings: Array.Empty<ParseWarning>(),
                DetectedFormat: formatToUse,
                ErrorMessage: $"Unsupported format: {formatToUse}")
        };
    }
    
    public bool CanParse(string content, ResponseFormat format)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;
        
        return format switch
        {
            ResponseFormat.HybridXmlMarkdown => content.Contains("<code_changes") || content.Contains("<shieldprompt"),
            ResponseFormat.PureXml => content.TrimStart().StartsWith("<?xml") || content.Contains("<shieldprompt_request"),
            ResponseFormat.StructuredMarkdown => Regex.IsMatch(content, @"###\s+(UPDATE|CREATE|DELETE):", RegexOptions.IgnoreCase),
            ResponseFormat.Json => content.TrimStart().StartsWith("{") && content.Contains("shieldprompt"),
            ResponseFormat.PlainText => true, // Can always attempt plain text parsing
            _ => false
        };
    }
    
    public IReadOnlyList<ResponseFormat> DetectFormats(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Array.Empty<ResponseFormat>();
        
        var detectedFormats = new List<ResponseFormat>();
        
        // Check for Hybrid XML-in-Markdown (most common)
        if ((content.Contains("<code_changes") || content.Contains("<shieldprompt")) && !content.TrimStart().StartsWith("<?xml"))
        {
            detectedFormats.Add(ResponseFormat.HybridXmlMarkdown);
        }
        
        // Check for Pure XML
        if (content.TrimStart().StartsWith("<?xml") || 
            (content.Contains("<shieldprompt_request") || content.Contains("<shieldprompt_response")))
        {
            detectedFormats.Add(ResponseFormat.PureXml);
        }
        
        // Check for Structured Markdown
        if (Regex.IsMatch(content, @"###\s+(UPDATE|CREATE|DELETE):", RegexOptions.IgnoreCase))
        {
            detectedFormats.Add(ResponseFormat.StructuredMarkdown);
        }
        
        // Check for JSON
        if (content.TrimStart().StartsWith("{") && content.Contains("shieldprompt"))
        {
            detectedFormats.Add(ResponseFormat.Json);
        }
        
        // Plain text as fallback
        if (detectedFormats.Count == 0)
        {
            detectedFormats.Add(ResponseFormat.PlainText);
        }
        
        return detectedFormats;
    }
    
    private async Task<ParseResult> ParseHybridXmlMarkdownAsync(string llmResponse, CancellationToken ct)
    {
        await Task.CompletedTask; // For async consistency
        
        var warnings = new List<ParseWarning>();
        
        // Step 1: Strip markdown XML fences if present (```xml ... ```)
        var xmlContent = ExtractXmlFromMarkdown(llmResponse);
        
        // Step 2: Find XML start and extract analysis section
        var xmlStartIndex = GetXmlStartIndex(xmlContent);
        string? analysis = null;
        
        if (xmlStartIndex > 0)
        {
            analysis = xmlContent.Substring(0, xmlStartIndex).Trim();
        }
        else if (xmlStartIndex < 0)
        {
            // No XML found, treat entire content as analysis
            return new ParseResult(
                Success: true,
                Analysis: llmResponse,
                Operations: Array.Empty<FileOperation>(),
                Warnings: Array.Empty<ParseWarning>(),
                DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        }
        
        // Step 3: Extract XML island
        var (rootTag, xmlEndIndex) = GetXmlEndIndex(xmlContent, xmlStartIndex);
        if (xmlEndIndex < 0)
        {
            // Check for self-closing tag
            var selfClosingMatch = Regex.Match(xmlContent.Substring(xmlStartIndex), @"<(code_changes|shieldprompt)[^>]*/>", RegexOptions.IgnoreCase);
            if (selfClosingMatch.Success)
            {
                // Empty response tag - no operations
                return new ParseResult(
                    Success: true,
                    Analysis: analysis,
                    Operations: Array.Empty<FileOperation>(),
                    Warnings: warnings,
                    DetectedFormat: ResponseFormat.HybridXmlMarkdown);
            }
            
            return new ParseResult(
                Success: false,
                Analysis: analysis,
                Operations: Array.Empty<FileOperation>(),
                Warnings: warnings,
                DetectedFormat: ResponseFormat.HybridXmlMarkdown,
                ErrorMessage: "Unclosed XML response tag");
        }
        
        var xmlBlock = xmlContent.Substring(xmlStartIndex, xmlEndIndex - xmlStartIndex + rootTag.Length + 3);
        
        // Step 4: Parse XML
        try
        {
            var operations = xmlBlock.Contains("<code_changes", StringComparison.OrdinalIgnoreCase)
                ? ParseCodeChangesXml(xmlBlock, warnings)
                : ParseShieldPromptXml(xmlBlock, warnings);
            
            return new ParseResult(
                Success: true,
                Analysis: analysis,
                Operations: operations,
                Warnings: warnings,
                DetectedFormat: ResponseFormat.HybridXmlMarkdown);
        }
        catch (XmlException ex)
        {
            return new ParseResult(
                Success: false,
                Analysis: analysis,
                Operations: Array.Empty<FileOperation>(),
                Warnings: warnings,
                DetectedFormat: ResponseFormat.HybridXmlMarkdown,
                ErrorMessage: $"XML parsing error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Extracts XML content from markdown code fences if present.
    /// Handles ```xml ... ``` blocks and returns the first <shieldprompt> block found.
    /// </summary>
    private string ExtractXmlFromMarkdown(string content)
    {
        // Pattern: ```xml ... ```
        var markdownFencePattern = @"```xml\s*(.*?)\s*```";
        var matches = Regex.Matches(content, markdownFencePattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        // Find the first match that contains <code_changes> or <shieldprompt>
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var extractedXml = match.Groups[1].Value;
                if (extractedXml.Contains("<code_changes", StringComparison.OrdinalIgnoreCase) ||
                    extractedXml.Contains("<shieldprompt", StringComparison.OrdinalIgnoreCase))
                {
                    // Return the analysis section + extracted XML
                    var analysisBeforeFence = content.Substring(0, match.Index);
                    var analysisAfterFence = content.Substring(match.Index + match.Length);
                    return analysisBeforeFence + extractedXml + analysisAfterFence;
                }
            }
        }
        
        // No markdown fence found, return original content
        return content;
    }
    
    private List<FileOperation> ParseShieldPromptXml(string xmlContent, List<ParseWarning> warnings)
    {
        var operations = new List<FileOperation>();
        
        var doc = XDocument.Parse(xmlContent);
        var fileElements = doc.Descendants("file");
        
        foreach (var fileElement in fileElements)
        {
            var op = ParseFileElement(fileElement, warnings);
            if (op != null)
            {
                operations.Add(op);
            }
        }
        
        return operations;
    }

    private List<FileOperation> ParseCodeChangesXml(string xmlContent, List<ParseWarning> warnings)
    {
        var operations = new List<FileOperation>();
        var doc = XDocument.Parse(xmlContent);
        var fileElements = doc.Descendants("changed_file");

        foreach (var fileElement in fileElements)
        {
            var op = ParseChangedFileElement(fileElement, warnings);
            if (op != null)
            {
                operations.Add(op);
            }
        }

        return operations;
    }

    private FileOperation? ParseChangedFileElement(XElement fileElement, List<ParseWarning> warnings)
    {
        var pathValue = GetElementValue(fileElement, "file_path");
        var summaryValue = GetElementValue(fileElement, "file_summary");
        var operationValue = GetElementValue(fileElement, "file_operation");
        var contentValue = GetElementValue(fileElement, "file_code");

        if (string.IsNullOrWhiteSpace(operationValue))
        {
            warnings.Add(new ParseWarning("Missing <file_operation> element on changed_file", null, null));
            return null;
        }

        if (string.IsNullOrWhiteSpace(pathValue))
        {
            warnings.Add(new ParseWarning("Missing <file_path> element on changed_file", null, null));
            return null;
        }

        pathValue = HttpUtility.HtmlDecode(pathValue);

        if (string.IsNullOrWhiteSpace(summaryValue))
        {
            warnings.Add(new ParseWarning($"Missing <file_summary> for {pathValue}", pathValue, null));
            summaryValue = "No summary provided";
        }
        else
        {
            summaryValue = HttpUtility.HtmlDecode(summaryValue);
        }

        // Security check: Validate path
        ValidateFilePath(pathValue, warnings);

        var operationType = operationValue.Trim().ToUpperInvariant() switch
        {
            "UPDATE" => FileOperationType.Update,
            "CREATE" => FileOperationType.Create,
            "DELETE" => FileOperationType.Delete,
            "PARTIAL_UPDATE" => FileOperationType.PartialUpdate,
            _ => FileOperationType.Update
        };

        int? startLine = null;
        int? endLine = null;

        var startLineValue = GetElementValue(fileElement, "start_line");
        var endLineValue = GetElementValue(fileElement, "end_line");

        if (!string.IsNullOrWhiteSpace(startLineValue) && int.TryParse(startLineValue, out var start))
        {
            startLine = start;
        }

        if (!string.IsNullOrWhiteSpace(endLineValue) && int.TryParse(endLineValue, out var end))
        {
            endLine = end;
        }

        if (startLine.HasValue && endLine.HasValue)
        {
            operationType = FileOperationType.PartialUpdate;
        }

        // Decode HTML entities in content
        if (!string.IsNullOrWhiteSpace(contentValue))
        {
            contentValue = HttpUtility.HtmlDecode(contentValue);
        }

        if (string.IsNullOrWhiteSpace(contentValue))
        {
            contentValue = null;
        }

        return new FileOperation(
            Type: operationType,
            Path: pathValue,
            Content: contentValue,
            Reason: summaryValue,
            StartLine: startLine,
            EndLine: endLine);
    }
    
    private FileOperation? ParseFileElement(XElement fileElement, List<ParseWarning> warnings)
    {
        // Get required attributes (case-insensitive)
        var opAttr = GetAttributeValue(fileElement, "op");
        var pathAttr = GetAttributeValue(fileElement, "path");
        var reasonAttr = GetAttributeValue(fileElement, "reason");
        
        if (string.IsNullOrWhiteSpace(opAttr))
        {
            warnings.Add(new ParseWarning("Missing 'op' attribute on file element", null, null));
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(pathAttr))
        {
            warnings.Add(new ParseWarning("Missing 'path' attribute on file element", null, null));
            return null;
        }
        
        // Decode HTML entities in path and reason
        pathAttr = HttpUtility.HtmlDecode(pathAttr);
        
        if (string.IsNullOrWhiteSpace(reasonAttr))
        {
            warnings.Add(new ParseWarning($"Missing 'reason' attribute for {pathAttr}", pathAttr, null));
            // Continue parsing but with warning
            reasonAttr = "No reason provided";
        }
        else
        {
            reasonAttr = HttpUtility.HtmlDecode(reasonAttr);
        }
        
        // Security check: Validate path
        ValidateFilePath(pathAttr, warnings);
        
        // Parse operation type (case-insensitive)
        var operationType = opAttr.ToLowerInvariant() switch
        {
            "update" => FileOperationType.Update,
            "create" => FileOperationType.Create,
            "delete" => FileOperationType.Delete,
            _ => FileOperationType.Update
        };
        
        // Check for partial update (line numbers)
        int? startLine = null;
        int? endLine = null;
        
        var startLineAttr = GetAttributeValue(fileElement, "start_line");
        var endLineAttr = GetAttributeValue(fileElement, "end_line");
        
        if (!string.IsNullOrWhiteSpace(startLineAttr) && int.TryParse(startLineAttr, out var start))
        {
            startLine = start;
        }
        
        if (!string.IsNullOrWhiteSpace(endLineAttr) && int.TryParse(endLineAttr, out var end))
        {
            endLine = end;
        }
        
        // If has line numbers, it's a partial update
        if (startLine.HasValue && endLine.HasValue)
        {
            operationType = FileOperationType.PartialUpdate;
        }
        
        // Get content (from CDATA or element value)
        string? content = fileElement.Value.Trim();
        
        // Decode HTML entities in content
        if (!string.IsNullOrWhiteSpace(content))
        {
            content = HttpUtility.HtmlDecode(content);
        }
        
        // If content is empty, set to null
        if (string.IsNullOrWhiteSpace(content))
        {
            content = null;
        }
        
        var operation = new FileOperation(
            Type: operationType,
            Path: pathAttr,
            Content: content,
            Reason: reasonAttr,
            StartLine: startLine,
            EndLine: endLine);
        
        return operation;
    }
    
    /// <summary>
    /// Gets attribute value case-insensitively.
    /// </summary>
    private string? GetAttributeValue(XElement element, string attributeName)
    {
        // Try exact match first
        var attr = element.Attribute(attributeName);
        if (attr != null)
        {
            return attr.Value;
        }
        
        // Try case-insensitive
        attr = element.Attributes()
            .FirstOrDefault(a => string.Equals(a.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase));
        
        return attr?.Value;
    }

    private static string? GetElementValue(XElement parent, string elementName)
    {
        var element = parent.Elements()
            .FirstOrDefault(e => string.Equals(e.Name.LocalName, elementName, StringComparison.OrdinalIgnoreCase));

        return element?.Value.Trim();
    }

    private static int GetXmlStartIndex(string xmlContent)
    {
        var codeChangesIndex = xmlContent.IndexOf("<code_changes", StringComparison.OrdinalIgnoreCase);
        var shieldPromptIndex = xmlContent.IndexOf("<shieldprompt", StringComparison.OrdinalIgnoreCase);

        if (codeChangesIndex < 0) return shieldPromptIndex;
        if (shieldPromptIndex < 0) return codeChangesIndex;

        return Math.Min(codeChangesIndex, shieldPromptIndex);
    }

    private static (string rootTag, int endIndex) GetXmlEndIndex(string xmlContent, int startIndex)
    {
        var codeChangesEnd = xmlContent.IndexOf("</code_changes>", startIndex, StringComparison.OrdinalIgnoreCase);
        var shieldPromptEnd = xmlContent.IndexOf("</shieldprompt>", startIndex, StringComparison.OrdinalIgnoreCase);

        if (codeChangesEnd >= 0 && (shieldPromptEnd < 0 || codeChangesEnd < shieldPromptEnd))
        {
            return ("code_changes", codeChangesEnd);
        }

        return ("shieldprompt", shieldPromptEnd);
    }
    
    /// <summary>
    /// Validates file path for security issues (path traversal, absolute paths, etc.).
    /// </summary>
    private void ValidateFilePath(string path, List<ParseWarning> warnings)
    {
        // Check for path traversal attempts
        if (path.Contains(".."))
        {
            warnings.Add(new ParseWarning(
                $"Security warning: Path '{path}' contains path traversal (..) - This may be unsafe",
                path,
                null));
        }
        
        // Check for absolute paths (Unix-style)
        if (path.StartsWith("/") && !path.StartsWith("//"))
        {
            warnings.Add(new ParseWarning(
                $"Security warning: Absolute path '{path}' detected - Should be relative to workspace",
                path,
                null));
        }
        
        // Check for Windows absolute paths
        if (Regex.IsMatch(path, @"^[A-Za-z]:\\"))
        {
            warnings.Add(new ParseWarning(
                $"Security warning: Windows absolute path '{path}' detected - Should be relative to workspace",
                path,
                null));
        }
        
        // Check for UNC paths
        if (path.StartsWith("\\\\") || path.StartsWith("//"))
        {
            warnings.Add(new ParseWarning(
                $"Security warning: UNC path '{path}' detected - Not allowed",
                path,
                null));
        }
    }
    
    private Task<ParseResult> ParsePureXmlAsync(string llmResponse, CancellationToken ct)
    {
        // TODO: Implement Pure XML parsing in next iteration
        return Task.FromResult(new ParseResult(
            Success: false,
            Analysis: null,
            Operations: Array.Empty<FileOperation>(),
            Warnings: Array.Empty<ParseWarning>(),
            DetectedFormat: ResponseFormat.PureXml,
            ErrorMessage: "Pure XML format not yet implemented"));
    }
    
    private Task<ParseResult> ParseStructuredMarkdownAsync(string llmResponse, CancellationToken ct)
    {
        // TODO: Implement Structured Markdown parsing in next iteration
        return Task.FromResult(new ParseResult(
            Success: false,
            Analysis: null,
            Operations: Array.Empty<FileOperation>(),
            Warnings: Array.Empty<ParseWarning>(),
            DetectedFormat: ResponseFormat.StructuredMarkdown,
            ErrorMessage: "Structured Markdown format not yet implemented"));
    }
    
    private Task<ParseResult> ParseJsonAsync(string llmResponse, CancellationToken ct)
    {
        // TODO: Implement JSON parsing in next iteration
        return Task.FromResult(new ParseResult(
            Success: false,
            Analysis: null,
            Operations: Array.Empty<FileOperation>(),
            Warnings: Array.Empty<ParseWarning>(),
            DetectedFormat: ResponseFormat.Json,
            ErrorMessage: "JSON format not yet implemented"));
    }
    
    private Task<ParseResult> ParsePlainTextAsync(string llmResponse, CancellationToken ct)
    {
        // Plain text: Just return as analysis, no operations
        return Task.FromResult(new ParseResult(
            Success: true,
            Analysis: llmResponse,
            Operations: Array.Empty<FileOperation>(),
            Warnings: Array.Empty<ParseWarning>(),
            DetectedFormat: ResponseFormat.PlainText));
    }
}

