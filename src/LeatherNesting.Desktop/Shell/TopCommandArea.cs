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
    private readonly Action<ShellMenuCommand> _activateMenu;

    public TopCommandArea(Action<ShellToolbarCommand> activateToolbar, Action<ShellMenuCommand> activateMenu)
    {
        ArgumentNullException.ThrowIfNull(activateToolbar);
        _activateMenu = activateMenu ?? throw new ArgumentNullException(nameof(activateMenu));

        MenuItems = ShellTopMenu.Labels.Select(CreateMenuItem).ToArray();
        CommandButtons = ShellToolbar.Commands
            .Select(command => new ShellToolbarButton(command, activateToolbar))
            .ToArray();

        var menu = new Menu
        {
            Height = AppTheme.MenuBarHeight,
            Background = AppTheme.MenuSurface,
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
            Background = AppTheme.ToolbarSurface,
        };

        ProductTitle = new TextBlock
        {
            Text = "LeatherNesting 卷料智能排样系统",
            Foreground = AppTheme.TitleText,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
        };
        UnitSelector = new ComboBox
        {
            ItemsSource = new[] { "单位：米(m)", "单位：毫米(mm)" },
            SelectedIndex = 0,
            Width = 126,
            Height = 28,
            VerticalAlignment = VerticalAlignment.Center,
        };
        OperatorText = new TextBox
        {
            Text = "演示员",
            FontSize = 12,
            Width = 92,
            Height = 28,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var operatorArea = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
            Children =
            {
                new TextBlock
                {
                    Text = "操作员：",
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                OperatorText,
            },
        };

        var titleBar = new Border
        {
            Height = AppTheme.TitleBarHeight,
            Background = AppTheme.ApplicationTitle,
            Child = ProductTitle,
        };
        var toolbarRow = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,Auto,Auto"),
            Children = { ToolbarScrollViewer, UnitSelector, operatorArea },
        };
        Grid.SetColumn(UnitSelector, 1);
        Grid.SetColumn(operatorArea, 2);

        Background = AppTheme.ToolbarSurface;
        BorderBrush = AppTheme.ClassicBorderNeutral;
        BorderThickness = new Thickness(0, 0, 0, 1);
        Child = new StackPanel
        {
            Children = { titleBar, menu, toolbarRow },
        };
    }

    public IReadOnlyList<MenuItem> MenuItems { get; }

    public IReadOnlyList<ShellToolbarButton> CommandButtons { get; }

    /// <summary>「设置>订单窗口」可勾选菜单项，shell 用它同步左侧栏显隐状态。</summary>
    public MenuItem? OrderWindowToggle { get; private set; }

    public ScrollViewer ToolbarScrollViewer { get; }

    public TextBlock ProductTitle { get; }

    public ComboBox UnitSelector { get; }

    public TextBox OperatorText { get; }

    private MenuItem CreateMenuItem(string label)
    {
        var entries = ShellTopMenu.EntriesFor(label);

        return new MenuItem
        {
            Header = label,
            Foreground = AppTheme.PrimaryText,
            ItemsSource = entries is null
                ? new Control[]
                {
                    new MenuItem
                    {
                        Header = ShellTopMenu.PlaceholderText,
                        IsEnabled = false,
                        MinWidth = 140,
                    },
                }
                : entries.Select(CreateMenuEntry).ToArray(),
        };
    }

    private Control CreateMenuEntry(ShellMenuEntry entry) => entry switch
    {
        ShellMenuCommand command => CreateCommandItem(command),
        ShellMenuSeparator => new Separator(),
        ShellMenuSubmenu submenu => CreateSubmenuItem(submenu),
        _ => throw new ArgumentOutOfRangeException(nameof(entry)),
    };

    private MenuItem CreateCommandItem(ShellMenuCommand command)
    {
        var item = new MenuItem
        {
            Header = command.Label,
            Foreground = AppTheme.PrimaryText,
            IsEnabled = command.IsEnabled,
        };
        if (command.Launch == ShellCommandLaunch.ToggleOrderWindow)
        {
            // 「设置>订单窗口」是勾选式开关：勾选=左侧栏显示，取消=隐藏。
            item.ToggleType = MenuItemToggleType.CheckBox;
            item.IsChecked = true;
            OrderWindowToggle = item;
        }
        item.Click += (_, _) => _activateMenu(command);
        return item;
    }

    private MenuItem CreateSubmenuItem(ShellMenuSubmenu submenu)
    {
        var item = new MenuItem
        {
            Header = submenu.Label,
            Foreground = AppTheme.PrimaryText,
        };
        item.ItemsSource = submenu.Children.Select(CreateMenuEntry).ToArray();
        return item;
    }
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
                    Foreground = AppTheme.PrimaryText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            },
        };
        Click += (_, _) => activate(command);
    }

    public ShellToolbarCommand Descriptor { get; }

    public ToolbarIcon Icon { get; }
}
