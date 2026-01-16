using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.App.ViewModels.V2;
using ShieldPrompt.App.Views;
using ShieldPrompt.App.Views.V2;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Application.Factories;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Infrastructure.Persistence;
using ShieldPrompt.Infrastructure.Services;
using AvaloniaApplication = Avalonia.Application;

namespace ShieldPrompt.App;

public partial class App : AvaloniaApplication
{
    public static ServiceProvider? Services { get; private set; }

    /// <summary>
    /// Feature flag to enable v2 UI. Set SHIELDPROMPT_V2=1 to use new UI.
    /// </summary>
    private static bool UseV2UI => Environment.GetEnvironmentVariable("SHIELDPROMPT_V2") == "1";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            
            if (UseV2UI)
            {
                // === NEW v2 UI ===
                var v2ViewModel = Services.GetRequiredService<MainWindowV2ViewModel>();
                desktop.MainWindow = new MainWindowV2
                {
                    DataContext = v2ViewModel
                };
            }
            else
            {
                // === Legacy v1 UI ===
                var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure services
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        // Application services
        services.AddSingleton<IFileAggregationService, FileAggregationService>();
        services.AddSingleton<ITokenCountingService, TokenCountingService>();
        services.AddSingleton<IUndoRedoManager, UndoRedoManager>();

        // Sanitization services
        services.AddSingleton<ShieldPrompt.Sanitization.Interfaces.IPatternRegistry, ShieldPrompt.Sanitization.Services.PatternRegistry>();
        services.AddScoped<ShieldPrompt.Sanitization.Interfaces.IMappingSession, ShieldPrompt.Sanitization.Services.MappingSession>();
        services.AddTransient<ShieldPrompt.Sanitization.Interfaces.IAliasGenerator, ShieldPrompt.Sanitization.Services.AliasGenerator>();
        services.AddScoped<ShieldPrompt.Sanitization.Interfaces.ISanitizationEngine, ShieldPrompt.Sanitization.Services.SanitizationEngine>();
        services.AddScoped<ShieldPrompt.Sanitization.Interfaces.IDesanitizationEngine, ShieldPrompt.Sanitization.Services.DesanitizationEngine>();

        // Initialize pattern registry with built-in patterns
        var registry = new ShieldPrompt.Sanitization.Services.PatternRegistry();
        foreach (var pattern in ShieldPrompt.Sanitization.Patterns.BuiltInPatterns.GetAll())
        {
            registry.AddPattern(pattern);
        }
        services.AddSingleton<ShieldPrompt.Sanitization.Interfaces.IPatternRegistry>(registry);
        
        // File manipulation services
        services.AddSingleton<Application.Interfaces.IAIResponseParser, Application.Services.AIResponseParser>();
        services.AddSingleton<Application.Interfaces.IFileWriterService, Application.Services.FileWriterService>();

        // Prompt template services
        services.AddSingleton<Application.Interfaces.IPromptTemplateRepository, Infrastructure.Services.YamlPromptTemplateRepository>();
        services.AddSingleton<Application.Interfaces.IXmlPromptBuilder, Application.Services.XmlPromptBuilder>();
        services.AddSingleton<Application.Interfaces.IPromptComposer, Application.Services.PromptComposer>();

        // Role repository
        services.AddSingleton<Application.Interfaces.IRoleRepository, Infrastructure.Services.YamlRoleRepository>();
        
        // Custom role repository
        services.AddSingleton<Application.Interfaces.ICustomRoleRepository, Infrastructure.Services.YamlCustomRoleRepository>();

        // Format metadata repository
        services.AddSingleton<Application.Interfaces.IFormatMetadataRepository, Infrastructure.Services.StaticFormatMetadataRepository>();

        // Layout State Persistence
        services.AddSingleton<Infrastructure.Interfaces.ILayoutStateRepository, Infrastructure.Services.JsonLayoutStateRepository>();

        // Output Format Settings
        services.AddSingleton<Application.Interfaces.IOutputFormatSettingsRepository, Infrastructure.Repositories.JsonOutputFormatSettingsRepository>();
        services.AddSingleton<Application.Interfaces.IStructuredResponseParser, Application.Parsers.StructuredResponseParser>();
        
        // Clipboard service
        services.AddSingleton<Infrastructure.Interfaces.IClipboardService, Infrastructure.Services.ClipboardServiceWrapper>();

        // === Phase 4: Git Integration ===
        services.AddSingleton<IGitRepositoryService, GitRepositoryService>();
        services.AddSingleton<IGitStatusProvider, GitStatusProvider>();
        services.AddSingleton<IGitIgnoreService, GitIgnoreService>();

        // === Phase 5: Stored Prompts/Presets ===
        services.AddSingleton<IPresetRepository, JsonPresetRepository>();
        services.AddSingleton<IPresetService, PresetService>();
        services.AddSingleton<IPresetExportService, PresetExportService>();

        // === Phase 6: Apply Mode Enhancement ===
        services.AddSingleton<IDiffService, DiffService>();
        services.AddSingleton<IBackupService, FileBackupService>();
        services.AddSingleton<IFileApplyService, FileApplyService>();

        // === Action System (for reliable undo/redo) ===
        services.AddSingleton<IFileActionFactory, FileActionFactory>();
        services.AddSingleton<IUndoRedoManager, UndoRedoManager>();

        // === Phase 7: Status Bar & Polish ===
        services.AddSingleton<StatusReporter>();
        services.AddSingleton<IStatusMessageReporter>(sp => sp.GetRequiredService<StatusReporter>());
        services.AddSingleton<IProgressReporter>(sp => sp.GetRequiredService<StatusReporter>());
        services.AddSingleton<IStatusBarService, StatusBarService>();

        // ViewModels - v1 (Legacy)
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<RoleEditorViewModel>();
        services.AddSingleton<OutputFormatSettingsViewModel>();
        services.AddSingleton<LlmResponseViewModel>();

        // ViewModels - v2 (New UI)
        services.AddTransient<MainWindowV2ViewModel>();
        services.AddTransient<StatusBarViewModel>();
        services.AddTransient<ApplyDashboardViewModel>();

        // Workspace Repository (new for v2)
        services.AddSingleton<IWorkspaceRepository, Infrastructure.Services.JsonWorkspaceRepository>();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}