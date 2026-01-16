namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Git status for a file.
/// </summary>
[Flags]
public enum GitFileStatus
{
    None = 0,
    Modified = 1,
    Staged = 2,
    Added = 4,
    Deleted = 8,
    Renamed = 16,
    Untracked = 32,
    Ignored = 64,
    Conflict = 128
}

