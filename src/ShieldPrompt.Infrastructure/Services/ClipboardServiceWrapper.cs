using ShieldPrompt.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using TextCopy;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// Clipboard service implementation using TextCopy.
/// </summary>
public class ClipboardServiceWrapper : IClipboardService
{
    public async Task<string?> GetTextAsync(CancellationToken cancellationToken = default)
    {
        return await ClipboardService.GetTextAsync(cancellationToken);
    }
    
    public async Task SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        await ClipboardService.SetTextAsync(text, cancellationToken);
    }
}

