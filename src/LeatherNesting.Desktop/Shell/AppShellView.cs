using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Workspace;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Demo shell: left nav, top command bar, center workspace, right inspector, bottom status bar.</summary>
public sealed class AppShellView : UserControl
{
    private readonly AppShellViewModel _viewModel;
    private readonly ContentControl _content = new();
    private readonly TextBlock _inspectorText = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _projectText = new();
    private readonly TextBlock _orderText = new();
    private readonly TextBlock _statusText = new();
    private readonly TextBlock _selectedObjectText = new();
    private readonly TextBlock _statusProjectText = new();
    private readonly TextBlock _statusVersionText = new();
    private readonly TextBlock _demoHintText = new() { Foreground = AppTheme.TodoAmber };

    public AppShellView() : this(DesktopComposition.CreateShellViewModel())
    {
    }

    public AppShellView(AppShellViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Content = BuildLayout();
        _viewModel.SnapshotChanged += (_, snapshot) => RefreshSnapshot(snapshot);
        ShowModule(_viewModel.Modules[0]);
        RefreshSnapshot(_viewModel.Snapshot);
    }

    private Control BuildLayout()
    {
        var grid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("220,*,300"),
            RowDefinitions = RowDefinitions.Parse("Auto,*,Auto"),
            Background = AppTheme.WorkspaceBackground,
        };

        var nav = BuildNav();
        Grid.SetRow(nav, 0); Grid.SetRowSpan(nav, 3); Grid.SetColumn(nav, 0);

        var topBar = BuildTopBar();
        Grid.SetRow(topBar, 0); Grid.SetColumn(topBar, 1); Grid.SetColumnSpan(topBar, 2);

        Grid.SetRow(_content, 1); Grid.SetColumn(_content, 1);

        var inspector = BuildInspector();
        Grid.SetRow(inspector, 1); Grid.SetColumn(inspector, 2);

        var statusBar = BuildStatusBar();
        Grid.SetRow(statusBar, 2); Grid.SetColumn(statusBar, 1); Grid.SetColumnSpan(statusBar, 2);

        grid.Children.Add(nav);
        grid.Children.Add(topBar);
        grid.Children.Add(_content);
        grid.Children.Add(inspector);
        grid.Children.Add(statusBar);
        return grid;
    }

    private Control BuildNav()
    {
        var stack = new StackPanel { Spacing = 2, Margin = new Thickness(8) };
        foreach (var group in _viewModel.Modules.GroupBy(m => m.Group))
        {
            stack.Children.Add(new TextBlock
            {
                Text = group.Key,
                Foreground = AppTheme.TextMuted,
                FontSize = 12,
                Margin = new Thickness(8, 10, 8, 2),
            });
            foreach (var module in group)
            {
                var label = module.HasRealLogic ? module.Title : $"{module.Title} · TODO";
                var button = new Button
                {
                    Content = label,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                };
                button.Click += (_, _) => ShowModule(module);
                stack.Children.Add(button);
            }
        }
        return new Border
        {
            Background = AppTheme.NavBackground,
            Child = new ScrollViewer { Content = stack },
        };
    }

    private Control BuildTopBar()
    {
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(12) };
        bar.Children.Add(CommandButton("导入", () => ShowModule(_viewModel.Modules.Single(module => module.Id == "M02"))));
        foreach (var label in new[] { "新建", "打开", "保存", "运行", "停止", "取消", "导出" })
            bar.Children.Add(CommandButton(label, () => _viewModel.ShowTodo(label)));
        return new Border { Background = AppTheme.Surface, Child = bar };
    }

    private Button CommandButton(string label, Action onClick)
    {
        var button = new Button { Content = label };
        button.Click += (_, _) => onClick();
        return button;
    }

    private Control BuildInspector()
    {
        var panel = new StackPanel { Spacing = 8, Margin = new Thickness(16), Background = AppTheme.Surface };
        panel.Children.Add(new TextBlock { Text = "检查器", FontWeight = FontWeight.Bold });
        panel.Children.Add(_projectText);
        panel.Children.Add(_orderText);
        panel.Children.Add(_statusText);
        panel.Children.Add(_selectedObjectText);
        panel.Children.Add(_inspectorText);
        return new Border { Background = AppTheme.Surface, BorderBrush = AppTheme.SurfaceBorder, BorderThickness = new Thickness(1, 0, 0, 0), Child = panel };
    }

    private Control BuildStatusBar()
    {
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Margin = new Thickness(12, 6) };
        bar.Children.Add(_statusProjectText);
        bar.Children.Add(_statusVersionText);
        bar.Children.Add(_demoHintText);
        return new Border { Background = AppTheme.NavBackground, Child = bar };
    }

    private void ShowModule(ModuleDescriptor module)
    {
        _viewModel.Select(module);
        _content.Content = _viewModel.CurrentView;
    }

    private void RefreshSnapshot(WorkspaceSnapshot snapshot)
    {
        var project = snapshot.CurrentProject;
        _projectText.Text = $"项目：{project?.Name ?? "未打开"}";
        _orderText.Text = $"编号：{project?.ProjectNumber ?? "—"}";
        _statusText.Text = $"状态：{project?.Status ?? "—"}";
        _selectedObjectText.Text = $"选择：{snapshot.SelectedObjectId ?? "无"}";
        _inspectorText.Text = snapshot.TodoHint ?? string.Empty;
        _statusProjectText.Text = $"项目：{project?.Name ?? "未打开"}";
        _statusVersionText.Text = $"状态：{project?.Status ?? "—"}";
        _demoHintText.Text = snapshot.DemoHint ?? string.Empty;
    }
}
