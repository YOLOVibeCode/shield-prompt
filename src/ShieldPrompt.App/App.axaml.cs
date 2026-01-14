using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.App.Views;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Infrastructure.Persistence;
using AvaloniaApplication = Avalonia.Application;

namespace ShieldPrompt.App;

public partial class App : AvaloniaApplication
{
    public static ServiceProvider? Services { get; private set; }

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
            
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
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

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
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