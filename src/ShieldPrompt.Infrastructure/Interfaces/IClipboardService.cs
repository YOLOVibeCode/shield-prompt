using System.Threading;
using System.Threading.Tasks;

namespace ShieldPrompt.Infrastructure.Interfaces;

/// <summary>
/// Interface for clipboard operations.
/// Wraps TextCopy.ClipboardService for testability.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Gets text from the clipboard.
    /// </summary>
    Task<string?> GetTextAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets text to the clipboard.
    /// </summary>
    Task SetTextAsync(string text, CancellationToken cancellationToken = default);
}

