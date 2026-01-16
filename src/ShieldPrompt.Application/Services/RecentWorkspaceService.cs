using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for managing recent workspace history.
/// </summary>
public class RecentWorkspaceService : IRecentWorkspaceService
{
    private readonly IWorkspaceRepository _repository;

    public RecentWorkspaceService(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Workspace>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        var all = await _repository.GetAllAsync(ct);
        return all.Take(count).ToList();
    }

    public async Task RecordOpenedAsync(string workspaceId, CancellationToken ct = default)
    {
        await _repository.UpdateLastOpenedAsync(workspaceId, ct);
    }

    public async Task ClearHistoryAsync(CancellationToken ct = default)
    {
        var all = await _repository.GetAllAsync(ct);
        foreach (var ws in all)
        {
            await _repository.DeleteAsync(ws.Id, ct);
        }
    }

    public async Task RemoveFromHistoryAsync(string workspaceId, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(workspaceId, ct);
    }
}

