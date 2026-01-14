using System.Text;
using System.Xml;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Formatters;

/// <summary>
/// Formats prompts as XML (RepoPrompt style).
/// </summary>
public class XmlFormatter : IPromptFormatter
{
    public string FormatName => "XML";

    public string Format(IEnumerable<FileNode> files, string? taskDescription = null)
    {
        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false
        };

        using (var writer = XmlWriter.Create(sb, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("project_context");

            if (!string.IsNullOrWhiteSpace(taskDescription))
            {
                writer.WriteElementString("description", taskDescription);
            }

            writer.WriteStartElement("files");

            foreach (var file in files.Where(f => !f.IsDirectory))
            {
                writer.WriteStartElement("file");
                writer.WriteAttributeString("path", file.Path);

                writer.WriteStartElement("content");

                if (File.Exists(file.Path))
                {
                    try
                    {
                        var content = File.ReadAllText(file.Path);
                        writer.WriteCData(content);
                    }
                    catch
                    {
                        writer.WriteCData("Error reading file");
                    }
                }

                writer.WriteEndElement(); // content
                writer.WriteEndElement(); // file
            }

            writer.WriteEndElement(); // files
            writer.WriteEndElement(); // project_context
            writer.WriteEndDocument();
        }

        return sb.ToString();
    }
}

