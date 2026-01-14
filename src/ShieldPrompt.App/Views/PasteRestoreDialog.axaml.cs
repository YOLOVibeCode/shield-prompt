using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShieldPrompt.App.Views;

public partial class PasteRestoreDialog : Window
{
    public PasteRestoreDialog()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

