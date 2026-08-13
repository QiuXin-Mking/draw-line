using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace LeatherNesting.Desktop.DesignSystem;

/// <summary>Stable, compact host used by the persistent workstation rails.</summary>
public sealed class ClassicPaneHost : Border
{
    private readonly TextBlock _titleText;
    private readonly ContentControl _contentHost;

    public ClassicPaneHost(string title, Control? content = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _titleText = new TextBlock
        {
            Text = title,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = AppTheme.TextPrimary,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0),
        };
        Header = new Border
        {
            Height = AppTheme.ClassicHeaderHeight,
            Background = AppTheme.ClassicHeaderBackground,
            BorderBrush = AppTheme.ClassicBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = _titleText,
        };
        _contentHost = new ContentControl { Content = content };

        Background = AppTheme.ClassicPanelBackground;
        BorderBrush = AppTheme.ClassicBorder;
        BorderThickness = new Thickness(1);
        Padding = new Thickness(0);
        Child = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("Auto,*"),
            Children = { Header, _contentHost },
        };
        Grid.SetRow(_contentHost, 1);
    }

    public string Title
    {
        get => _titleText.Text ?? string.Empty;
        set => _titleText.Text = value;
    }

    public Border Header { get; }

    public Control? HostedContent
    {
        get => _contentHost.Content as Control;
        set => _contentHost.Content = value;
    }
}
