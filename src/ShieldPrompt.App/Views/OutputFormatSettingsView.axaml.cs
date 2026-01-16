using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShieldPrompt.App.Views;

public partial class OutputFormatSettingsView : UserControl
{
    public OutputFormatSettingsView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

