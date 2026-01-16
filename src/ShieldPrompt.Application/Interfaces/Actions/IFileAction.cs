using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Application.Interfaces.Actions;

/// <summary>
/// File-specific action interface. Extends IAction with file context.
/// ISP-compliant: Adds only 2 file-specific members.
/// </summary>
public interface IFileAction : IAction
{
    /// <summary>Absolute path to the affected file.</summary>
    string FilePath { get; }
    
    /// <summary>Type of file operation.</summary>
    FileOperationType OperationType { get; }
}

