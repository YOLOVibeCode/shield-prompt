using System.Xml.Linq;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Builds XML-based prompt sections using type-safe XML APIs.
/// Follows ISP - XML generation only, max 5 methods.
/// </summary>
public interface IXmlPromptBuilder
{
    /// <summary>
    /// Builds the complete response contract XML with examples.
    /// </summary>
    string BuildResponseContract();
    
    /// <summary>
    /// Creates a single file example XML element.
    /// </summary>
    XElement CreateFileExample(string operation, string path, string summary, string? code = null);
    
    /// <summary>
    /// Appends XML response instructions to an existing prompt.
    /// </summary>
    string AppendResponseInstructions(string existingPrompt);
    
    /// <summary>
    /// Validates that an XML string is well-formed.
    /// </summary>
    bool IsWellFormedXml(string xml, out string? errorMessage);
}

