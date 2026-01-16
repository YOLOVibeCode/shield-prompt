using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ShieldPrompt.App.ViewModels;

/// <summary>
/// ViewModel for Output Format Settings page.
/// Manages user preferences for LLM response formats and auto-apply behavior.
/// </summary>
public partial class OutputFormatSettingsViewModel : ObservableObject
{
    private readonly IOutputFormatSettingsRepository _settingsRepository;
    
    public ObservableCollection<FormatOption> AvailableFormats { get; } = new();
    
    [ObservableProperty]
    private FormatOption? _selectedFormat;
    
    [ObservableProperty]
    private bool _enablePartialUpdates;
    
    [ObservableProperty]
    private bool _enableAutoApply;
    
    // Auto-apply mode radio buttons
    public bool IsPreviewThenPrompt
    {
        get => AutoApplyBehavior == AutoApplyMode.PreviewThenPrompt;
        set { if (value) AutoApplyBehavior = AutoApplyMode.PreviewThenPrompt; }
    }
    
    public bool IsCountdownWithBackup
    {
        get => AutoApplyBehavior == AutoApplyMode.CountdownWithBackup;
        set { if (value) AutoApplyBehavior = AutoApplyMode.CountdownWithBackup; }
    }
    
    public bool IsImmediateApply
    {
        get => AutoApplyBehavior == AutoApplyMode.ImmediateApply;
        set { if (value) AutoApplyBehavior = AutoApplyMode.ImmediateApply; }
    }
    
    [ObservableProperty]
    private AutoApplyMode _autoApplyBehavior;
    
    // Conflict resolution radio buttons
    public bool IsPromptUser
    {
        get => ConflictStrategy == ConflictResolutionStrategy.PromptUser;
        set { if (value) ConflictStrategy = ConflictResolutionStrategy.PromptUser; }
    }
    
    public bool IsAlwaysSkip
    {
        get => ConflictStrategy == ConflictResolutionStrategy.AlwaysSkip;
        set { if (value) ConflictStrategy = ConflictResolutionStrategy.AlwaysSkip; }
    }
    
    public bool IsAlwaysOverwrite
    {
        get => ConflictStrategy == ConflictResolutionStrategy.AlwaysOverwrite;
        set { if (value) ConflictStrategy = ConflictResolutionStrategy.AlwaysOverwrite; }
    }
    
    public bool IsAutoMerge
    {
        get => ConflictStrategy == ConflictResolutionStrategy.AutoMerge;
        set { if (value) ConflictStrategy = ConflictResolutionStrategy.AutoMerge; }
    }
    
    [ObservableProperty]
    private ConflictResolutionStrategy _conflictStrategy;
    
    [ObservableProperty]
    private bool _includeSessionMetadata;
    
    [ObservableProperty]
    private bool _includeTokenCounts;
    
    [ObservableProperty]
    private bool _includeTimestamps;
    
    [ObservableProperty]
    private int _maxResponseSizeMB;
    
    [ObservableProperty]
    private int _parserTimeoutSeconds;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private string _formatPreview = string.Empty;
    
    public OutputFormatSettingsViewModel(IOutputFormatSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
        
        // Initialize available formats
        InitializeFormats();
        
        // Load settings
        _ = LoadSettingsAsync();
    }
    
    private void InitializeFormats()
    {
        var hybridFormatter = new HybridXmlMarkdownFormatter();
        
        AvailableFormats.Add(new FormatOption(
            Format: ResponseFormat.HybridXmlMarkdown,
            Name: hybridFormatter.FormatName,
            Description: hybridFormatter.FormatDescription,
            IsRecommended: true,
            LlmAdherence: hybridFormatter.LlmAdherenceRate,
            ParseReliability: hybridFormatter.ParseReliability,
            TokenEfficiency: hybridFormatter.TokenEfficiency,
            SupportsPartialUpdates: hybridFormatter.SupportsPartialUpdates,
            BestFor: "GPT-4, Claude 3.5, General use",
            Formatter: hybridFormatter));
        
        // TODO: Add other formatters when implemented
        AvailableFormats.Add(new FormatOption(
            Format: ResponseFormat.PureXml,
            Name: "Pure XML (Enterprise Mode)",
            Description: "Full XML structure with schema validation. Best for automated workflows.",
            IsRecommended: false,
            LlmAdherence: 0.70,
            ParseReliability: 1.0,
            TokenEfficiency: 0.50,
            SupportsPartialUpdates: true,
            BestFor: "Enterprise, API integrations",
            Formatter: null)); // Not implemented yet
        
        AvailableFormats.Add(new FormatOption(
            Format: ResponseFormat.StructuredMarkdown,
            Name: "Structured Markdown (Aider-Style)",
            Description: "Natural markdown with special markers. Most token-efficient.",
            IsRecommended: false,
            LlmAdherence: 0.85,
            ParseReliability: 0.75,
            TokenEfficiency: 0.95,
            SupportsPartialUpdates: false,
            BestFor: "Token efficiency, smaller models",
            Formatter: null)); // Not implemented yet
        
        AvailableFormats.Add(new FormatOption(
            Format: ResponseFormat.Json,
            Name: "JSON (API Mode)",
            Description: "Strict JSON schema for programmatic use.",
            IsRecommended: false,
            LlmAdherence: 0.75,
            ParseReliability: 1.0,
            TokenEfficiency: 0.70,
            SupportsPartialUpdates: true,
            BestFor: "APIs, programmatic workflows",
            Formatter: null)); // Not implemented yet
        
        AvailableFormats.Add(new FormatOption(
            Format: ResponseFormat.PlainText,
            Name: "Plain Text (Minimal)",
            Description: "No special syntax. Maximum compatibility, minimum reliability.",
            IsRecommended: false,
            LlmAdherence: 0.95,
            ParseReliability: 0.50,
            TokenEfficiency: 1.0,
            SupportsPartialUpdates: false,
            BestFor: "Experiments, maximum compatibility",
            Formatter: null)); // Not implemented yet
    }
    
    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsRepository.LoadAsync();
            
            SelectedFormat = AvailableFormats.FirstOrDefault(f => f.Format == settings.PreferredFormat)
                          ?? AvailableFormats.First();
            EnablePartialUpdates = settings.EnablePartialUpdates;
            EnableAutoApply = settings.EnableAutoApply;
            AutoApplyBehavior = settings.AutoApplyBehavior;
            ConflictStrategy = settings.ConflictStrategy;
            IncludeSessionMetadata = settings.IncludeSessionMetadata;
            IncludeTokenCounts = settings.IncludeTokenCounts;
            IncludeTimestamps = settings.IncludeTimestamps;
            MaxResponseSizeMB = settings.MaxResponseSizeMB;
            ParserTimeoutSeconds = settings.ParserTimeoutSeconds;
            
            UpdateFormatPreview();
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠️ Failed to load settings: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            var settings = new OutputFormatSettings(
                PreferredFormat: SelectedFormat?.Format ?? ResponseFormat.HybridXmlMarkdown,
                EnablePartialUpdates: EnablePartialUpdates,
                EnableAutoApply: EnableAutoApply,
                AutoApplyBehavior: AutoApplyBehavior,
                ConflictStrategy: ConflictStrategy,
                IncludeSessionMetadata: IncludeSessionMetadata,
                IncludeTokenCounts: IncludeTokenCounts,
                IncludeTimestamps: IncludeTimestamps,
                MaxResponseSizeMB: MaxResponseSizeMB,
                ParserTimeoutSeconds: ParserTimeoutSeconds);
            
            await _settingsRepository.SaveAsync(settings);
            
            StatusMessage = "✅ Settings saved successfully!";
            
            // Clear message after 3 seconds
            await Task.Delay(3000);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Failed to save settings: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task RestoreDefaults()
    {
        try
        {
            await _settingsRepository.ResetToDefaultAsync();
            await LoadSettingsAsync();
            
            StatusMessage = "✅ Settings restored to defaults!";
            
            await Task.Delay(3000);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Failed to restore defaults: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void PreviewFormat()
    {
        UpdateFormatPreview();
    }
    
    private void UpdateFormatPreview()
    {
        if (SelectedFormat?.Formatter != null)
        {
            FormatPreview = SelectedFormat.Formatter.GenerateResponseExample();
        }
        else
        {
            FormatPreview = "Preview not available for this format yet.\n\nThis format will be implemented in a future update.";
        }
    }
    
    partial void OnSelectedFormatChanged(FormatOption? value)
    {
        UpdateFormatPreview();
        
        // Auto-disable partial updates if format doesn't support it
        if (value != null && !value.SupportsPartialUpdates)
        {
            EnablePartialUpdates = false;
        }
    }
    
    partial void OnAutoApplyBehaviorChanged(AutoApplyMode value)
    {
        OnPropertyChanged(nameof(IsPreviewThenPrompt));
        OnPropertyChanged(nameof(IsCountdownWithBackup));
        OnPropertyChanged(nameof(IsImmediateApply));
    }
    
    partial void OnConflictStrategyChanged(ConflictResolutionStrategy value)
    {
        OnPropertyChanged(nameof(IsPromptUser));
        OnPropertyChanged(nameof(IsAlwaysSkip));
        OnPropertyChanged(nameof(IsAlwaysOverwrite));
        OnPropertyChanged(nameof(IsAutoMerge));
    }
}

/// <summary>
/// Represents a format option in the settings UI.
/// </summary>
public record FormatOption(
    ResponseFormat Format,
    string Name,
    string Description,
    bool IsRecommended,
    double LlmAdherence,
    double ParseReliability,
    double TokenEfficiency,
    bool SupportsPartialUpdates,
    string BestFor,
    IResponseFormatStrategy? Formatter)
{
    public string DisplayName => IsRecommended ? $"⭐ {Name}" : Name;
    public string LlmAdherenceText => $"{LlmAdherence:P0}";
    public string ParseReliabilityText => $"{ParseReliability:P0}";
    public string TokenEfficiencyText => $"{TokenEfficiency:P0}";
}

