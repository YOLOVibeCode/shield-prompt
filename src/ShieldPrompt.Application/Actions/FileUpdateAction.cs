using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Action for updating an existing file.
/// Undo: Restores the original content.
/// </summary>
public class FileUpdateAction : IFileAction
{
    private readonly string _filePath;
    private readonly string _originalContent;
    private readonly string _newContent;

    public Guid Id { get; } = Guid.NewGuid();
    public string Description => $"Update {Path.GetFileName(_filePath)}";
    public string FilePath => _filePath;
    public FileOperationType OperationType => FileOperationType.Update;

    public FileUpdateAction(string filePath, string originalContent, string newContent)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _originalContent = originalContent ?? string.Empty;
        _newContent = newContent ?? string.Empty;
    }

    public async Task<ActionResult> ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            await File.WriteAllTextAsync(_filePath, _newContent, ct);
            return new ActionResult(true);
        }
        catch (Exception ex)
        {
            return new ActionResult(false, $"Failed to update file: {ex.Message}");
        }
    }

    public async Task<ActionResult> UndoAsync(CancellationToken ct = default)
    {
        try
        {
            await File.WriteAllTextAsync(_filePath, _originalContent);
            return new ActionResult(true);
        }
        catch (Exception ex)
        {
            return new ActionResult(false, $"Failed to undo file update: {ex.Message}");
        }
    }
}

