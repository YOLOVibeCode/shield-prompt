using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ShieldPrompt.App.ViewModels.V2;

namespace ShieldPrompt.App.Views.V2;

public partial class MainWindowV2 : Window
{
    public MainWindowV2()
    {
        InitializeComponent();
    }

    private void OnPreviewClick(object? sender, PointerPressedEventArgs e)
    {
        // Copy preview content to clipboard when clicked
        if (DataContext is MainWindowV2ViewModel vm)
        {
            vm.CopyPreviewCommand.Execute(null);
        }
    }
}

