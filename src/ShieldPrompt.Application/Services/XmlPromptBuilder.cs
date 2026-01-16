using ShieldPrompt.Application.Interfaces;
using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Type-safe XML builder for prompt generation.
/// Uses XDocument/XElement to guarantee well-formed XML with proper escaping.
/// </summary>
public class XmlPromptBuilder : IXmlPromptBuilder
{
    /// <summary>
    /// Builds the complete response contract XML with examples of all operation types.
    /// </summary>
    public string BuildResponseContract()
    {
        var doc = new XDocument(
            new XComment(" LLM Response Contract - Action-Only Mode "),
            new XElement("code_changes",
                new XAttribute("version", "1.0"),
                CreateFileExample("UPDATE", "src/Example.cs", "Updated logic with error handling", 
                    "public class Example {\n    // Full file contents here\n    public void Method() { }\n}"),
                CreateFileExample("CREATE", "src/NewFeature.cs", "Created new feature class",
                    "public class NewFeature {\n    // New file contents\n}"),
                CreateFileExample("DELETE", "src/Deprecated.cs", "Removed deprecated class")));
        
        return doc.ToString(SaveOptions.DisableFormatting);
    }
    
    /// <summary>
    /// Creates a single file example XML element with proper structure.
    /// </summary>
    public XElement CreateFileExample(string operation, string path, string summary, string? code = null)
    {
        var element = new XElement("changed_file",
            new XElement("file_path", path),
            new XElement("file_summary", summary),
            new XElement("file_operation", operation.ToUpperInvariant()));
        
        // Only add file_code if code is provided (DELETE operations don't have code)
        if (code != null)
        {
            element.Add(new XElement("file_code", new XCData(code)));
        }
        
        return element;
    }
    
    /// <summary>
    /// Appends XML response instructions to an existing prompt.
    /// </summary>
    public string AppendResponseInstructions(string existingPrompt)
    {
        var sb = new StringBuilder();
        sb.Append(existingPrompt);
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("⚠️ CRITICAL: OUTPUT FORMAT");
        sb.AppendLine();
        sb.AppendLine("You MUST respond ONLY with valid XML and nothing else.");
        sb.AppendLine();
        sb.AppendLine("FORMAT:");
        sb.AppendLine("<code_changes>");
        sb.AppendLine("  <changed_file>");
        sb.AppendLine("    <file_path>{relative/path}</file_path>");
        sb.AppendLine("    <file_summary>{short summary}</file_summary>");
        sb.AppendLine("    <file_operation>CREATE|UPDATE|DELETE</file_operation>");
        sb.AppendLine("    <file_code><![CDATA[{full file contents}]]></file_code>");
        sb.AppendLine("  </changed_file>");
        sb.AppendLine("</code_changes>");
        sb.AppendLine();
        sb.AppendLine("RULES:");
        sb.AppendLine("- Respond ONLY with valid XML. No markdown, no explanations, no commentary.");
        sb.AppendLine("- One <changed_file> per file.");
        sb.AppendLine("- For DELETE: omit <file_code>.");
        sb.AppendLine("- Use relative paths from repo root.");
        sb.AppendLine("- If no changes needed, return: <code_changes/>");
        sb.AppendLine();
        sb.AppendLine("EXAMPLE OUTPUT:");
        sb.AppendLine(BuildResponseContract());
        sb.AppendLine();
        sb.AppendLine("START YOUR RESPONSE WITH: <code_changes");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Validates that an XML string is well-formed.
    /// </summary>
    public bool IsWellFormedXml(string xml, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            errorMessage = "XML string is null or empty";
            return false;
        }
        
        try
        {
            XDocument.Parse(xml);
            errorMessage = null;
            return true;
        }
        catch (XmlException ex)
        {
            errorMessage = $"XML parsing error at line {ex.LineNumber}, position {ex.LinePosition}: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Unexpected error: {ex.Message}";
            return false;
        }
    }
}

