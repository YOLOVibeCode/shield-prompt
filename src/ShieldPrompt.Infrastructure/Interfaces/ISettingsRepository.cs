using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Infrastructure.Interfaces;

/// <summary>
/// Repository for persisting application settings.
/// ISP-compliant: focused on settings persistence only.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Loads settings from disk, or returns defaults if not found.
    /// </summary>
    Task<AppSettings> LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    Task SaveAsync(AppSettings settings, CancellationToken ct = default);
}

