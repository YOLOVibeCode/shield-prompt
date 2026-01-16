namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Information about a git repository.
/// </summary>
public record GitRepositoryInfo(
    string RootPath,
    string CurrentBranch,
    string? RemoteUrl,
    bool HasUncommittedChanges,
    int ModifiedFileCount,
    int StagedFileCount,
    int UntrackedFileCount);

