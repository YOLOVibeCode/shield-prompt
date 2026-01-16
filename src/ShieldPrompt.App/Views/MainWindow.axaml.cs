using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Infrastructure.Interfaces;

namespace ShieldPrompt.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void HorizontalSplitter_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.FileTreeWidth = LayoutDefaults.FileTreeWidth;
        }
    }

    private void VerticalSplitter_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.PromptBuilderHeightRatio = LayoutDefaults.PromptBuilderHeight;
        }
    }

    private async void LivePreviewBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is Border border)
        {
            // Execute the copy command
            await vm.CopyLivePreviewCommand.ExecuteAsync(null);
            
            // Add flash class for visual feedback
            border.Classes.Add("copyFlash");
            
            // Remove flash class after animation
            await Task.Delay(500);
            border.Classes.Remove("copyFlash");
        }
    }
}