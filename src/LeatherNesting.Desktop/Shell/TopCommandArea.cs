using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Traditional two-level desktop menu and icon command surface.</summary>
public sealed class TopCommandArea : Border
{
    public TopCommandArea(Action<ShellToolbarCommand> activate)
    {
        ArgumentNullException.ThrowIfNull(activate);

        MenuItems = ShellTopMenu.Labels.Select(CreateMenuItem).ToArray();
        CommandButtons = ShellToolbar.Commands
            .Select(command => new ShellToolbarButton(command, activate))
            .ToArray();

        var menu = new Menu
        {
            Height = AppTheme.MenuBarHeight,
            Background = AppTheme.MenuBackground,
            ItemsSource = MenuItems,
        };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Margin = new Thickness(4, 2),
        };
        foreach (var button in CommandButtons)
            toolbar.Children.Add(button);

        ToolbarScrollViewer = new ScrollViewer
        {
            Content = toolbar,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Height = AppTheme.ToolbarHeight,
            Background = AppTheme.ToolbarBackground,
        };

        Background = AppTheme.ToolbarBackground;
        BorderBrush = AppTheme.ToolbarBorder;
        BorderThickness = new Thickness(0, 0, 0, 1);
        Child = new StackPanel
        {
            Children = { menu, ToolbarScrollViewer },
        };
    }

    public IReadOnlyList<MenuItem> MenuItems { get; }

    public IReadOnlyList<ShellToolbarButton> CommandButtons { get; }

    public ScrollViewer ToolbarScrollViewer { get; }

    private static MenuItem CreateMenuItem(string label) => new()
    {
        Header = label,
        ItemsSource = new[]
        {
            new MenuItem
            {
                Header = ShellTopMenu.PlaceholderText,
                IsEnabled = false,
            },
        },
    };
}

public sealed class ShellToolbarButton : Button
{
    public ShellToolbarButton(ShellToolbarCommand command, Action<ShellToolbarCommand> activate)
    {
        Descriptor = command ?? throw new ArgumentNullException(nameof(command));
        ArgumentNullException.ThrowIfNull(activate);

        Icon = ToolbarIconFactory.Create(command.Icon);
        Width = AppTheme.ToolbarButtonWidth;
        Height = AppTheme.ToolbarHeight - 6;
        Padding = new Thickness(5, 3);
        Background = Brushes.Transparent;
        BorderBrush = Brushes.Transparent;
        BorderThickness = new Thickness(1);
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
        Content = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 2,
            Children =
            {
                Icon,
                new TextBlock
                {
                    Text = command.Label,
                    FontSize = 12,
                    Foreground = AppTheme.TextPrimary,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            },
        };
        Click += (_, _) => activate(command);
    }

    public ShellToolbarCommand Descriptor { get; }

    public ToolbarIcon Icon { get; }
}
