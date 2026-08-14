using Avalonia.Controls;
using Avalonia.Styling;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.Shell;

namespace LeatherNesting.Desktop.Views;

/// <summary>Application entry window. Hosts the demo shell; module pages own their own UI.</summary>
public sealed class MainWindow : Window
{
    public static ThemeVariant WorkstationThemeVariant { get; } = ThemeVariant.Light;

    public MainWindow()
    {
        Title = "Leather Nesting";
        MinWidth = 1024;
        MinHeight = 640;
        Width = 1366;
        Height = 768;
        RequestedThemeVariant = WorkstationThemeVariant;
        Content = new AppShellView(DesktopComposition.CreateShellViewModel());
    }
}
