using System.Text.Json;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// JSON-based implementation of ISessionRepository.
/// Stores session data in ~/.shieldprompt/sessions/{workspaceId}/
/// </summary>
public class JsonSessionRepository : ISessionRepository
{
    private readonly string _sessionsDirectory;
    private readonly JsonSerializerOptions _options;

    public JsonSessionRepository()
    {
        _sessionsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".shieldprompt",
            "sessions");
        Directory.CreateDirectory(_sessionsDirectory);

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyList<PromptSession>> GetByWorkspaceAsync(string workspaceId, CancellationToken ct = default)
    {
        var workspaceDir = Path.Combine(_sessionsDirectory, workspaceId);
        if (!Directory.Exists(workspaceDir))
        {
            return Array.Empty<PromptSession>();
        }

        var sessions = new List<PromptSession>();
        foreach (var filePath in Directory.EnumerateFiles(workspaceDir, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath, ct);
                var session = JsonSerializer.Deserialize<PromptSession>(json, _options);
                if (session != null)
                {
                    sessions.Add(session);
                }
            }
            catch
            {
                // Skip malformed session files
            }
        }

        return sessions.OrderByDescending(s => s.LastModified).ToList();
    }

    public async Task<PromptSession?> GetByIdAsync(string sessionId, CancellationToken ct = default)
    {
        // Search all workspace directories
        foreach (var workspaceDir in Directory.EnumerateDirectories(_sessionsDirectory))
        {
            var filePath = Path.Combine(workspaceDir, $"{sessionId}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath, ct);
                    return JsonSerializer.Deserialize<PromptSession>(json, _options);
                }
                catch
                {
                    return null;
                }
            }
        }

        return null;
    }

    public async Task SaveAsync(PromptSession session, CancellationToken ct = default)
    {
        var workspaceDir = Path.Combine(_sessionsDirectory, session.WorkspaceId);
        Directory.CreateDirectory(workspaceDir);

        var filePath = Path.Combine(workspaceDir, $"{session.Id}.json");
        var json = JsonSerializer.Serialize(session, _options);
        await File.WriteAllTextAsync(filePath, json, ct);
    }

    public Task DeleteAsync(string sessionId, CancellationToken ct = default)
    {
        // Search all workspace directories
        foreach (var workspaceDir in Directory.EnumerateDirectories(_sessionsDirectory))
        {
            var filePath = Path.Combine(workspaceDir, $"{sessionId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                break;
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteByWorkspaceAsync(string workspaceId, CancellationToken ct = default)
    {
        var workspaceDir = Path.Combine(_sessionsDirectory, workspaceId);
        if (Directory.Exists(workspaceDir))
        {
            Directory.Delete(workspaceDir, true);
        }

        return Task.CompletedTask;
    }
}

