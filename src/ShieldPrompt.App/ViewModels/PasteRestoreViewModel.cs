using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Sanitization.Interfaces;
using TextCopy;

namespace ShieldPrompt.App.ViewModels;

public partial class PasteRestoreViewModel : ViewModelBase
{
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly IMappingSession _session;

    [ObservableProperty]
    private string _pastedContent = string.Empty;

    [ObservableProperty]
    private string _restoredContent = string.Empty;

    [ObservableProperty]
    private int _aliasCount;

    [ObservableProperty]
    private string _previewSummary = string.Empty;

    public PasteRestoreViewModel(
        IDesanitizationEngine desanitizationEngine,
        IMappingSession session)
    {
        _desanitizationEngine = desanitizationEngine;
        _session = session;
    }

    public ObservableCollection<AliasMapping> DetectedAliases { get; } = new();
    public ObservableCollection<string> ChangeSummary { get; } = new();

    [RelayCommand]
    private async Task PasteFromClipboardAsync()
    {
        var clipboardText = await ClipboardService.GetTextAsync();
        if (!string.IsNullOrEmpty(clipboardText))
        {
            PastedContent = clipboardText;
            RestoreAliases();
        }
    }

    [RelayCommand]
    private void RestoreAliases()
    {
        if (string.IsNullOrEmpty(PastedContent))
            return;

        // Restore using session mappings
        RestoredContent = _desanitizationEngine.Desanitize(PastedContent, _session);

        // Detect which aliases were found
        DetectedAliases.Clear();
        ChangeSummary.Clear();

        foreach (var (alias, original) in _session.GetAllMappings())
        {
            if (PastedContent.Contains(alias))
            {
                var count = CountOccurrences(PastedContent, alias);
                DetectedAliases.Add(new AliasMapping(alias, original, count));
                ChangeSummary.Add($"Will replace {count}x: {alias} → {original}");
            }
        }

        AliasCount = DetectedAliases.Count;
        PreviewSummary = AliasCount > 0
            ? $"🔓 Ready to restore {AliasCount} sensitive values from AI response"
            : "ℹ️ No aliases detected in pasted content";
    }

    private static int CountOccurrences(string content, string value)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(value))
            return 0;

        var count = 0;
        var index = 0;
        while ((index = content.IndexOf(value, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    [RelayCommand]
    private async Task CopyRestoredAsync()
    {
        await ClipboardService.SetTextAsync(RestoredContent);
    }

    partial void OnPastedContentChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            RestoreAliases();
        }
    }
}

public record AliasMapping(string Alias, string Original, int Occurrences = 1);

