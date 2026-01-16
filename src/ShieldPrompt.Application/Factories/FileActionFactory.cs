using ShieldPrompt.Application.Actions;
using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Factories;

/// <summary>
/// Factory for creating IFileAction instances from FileOperation records.
/// </summary>
public class FileActionFactory : IFileActionFactory
{
    public async Task<IFileAction> CreateAsync(
        FileOperation operation,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var absolutePath = Path.Combine(workspaceRoot, operation.Path);

        return operation.Type switch
        {
            FileOperationType.Create => new FileCreateAction(
                absolutePath,
                operation.Content ?? string.Empty),

            FileOperationType.Update => new FileUpdateAction(
                absolutePath,
                await ReadCurrentContentAsync(absolutePath, ct),
                operation.Content ?? string.Empty),

            FileOperationType.Delete => new FileDeleteAction(
                absolutePath,
                await ReadCurrentContentAsync(absolutePath, ct)),

            _ => throw new NotSupportedException(
                $"Operation type {operation.Type} is not supported by FileActionFactory")
        };
    }

    private static async Task<string> ReadCurrentContentAsync(string path, CancellationToken ct)
    {
        return File.Exists(path)
            ? await File.ReadAllTextAsync(path, ct)
            : string.Empty;
    }
}

