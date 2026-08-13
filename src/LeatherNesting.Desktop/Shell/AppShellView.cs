using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Workspace;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Demo shell: top command area, flexible workspace, fixed inspector, and bottom status bar.</summary>
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
        ShowModule(_viewModel.Modules.Single(module => module.Id == "M03"));
        RefreshSnapshot(_viewModel.Snapshot);
    }

    public ContentControl WorkspaceContent => _content;

    private Control BuildLayout()
    {
        var grid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,300"),
            RowDefinitions = RowDefinitions.Parse("Auto,*,Auto"),
            Background = AppTheme.WorkspaceBackground,
        };

        var topBar = BuildTopBar();
        Grid.SetRow(topBar, 0); Grid.SetColumn(topBar, 0); Grid.SetColumnSpan(topBar, 2);

        Grid.SetRow(_content, 1); Grid.SetColumn(_content, 0);

        var inspector = BuildInspector();
        Grid.SetRow(inspector, 1); Grid.SetColumn(inspector, 1);

        var statusBar = BuildStatusBar();
        Grid.SetRow(statusBar, 2); Grid.SetColumn(statusBar, 0); Grid.SetColumnSpan(statusBar, 2);

        grid.Children.Add(topBar);
        grid.Children.Add(_content);
        grid.Children.Add(inspector);
        grid.Children.Add(statusBar);
        return grid;
    }

    private Control BuildTopBar()
    {
        return new TopCommandArea(command =>
        {
            _viewModel.ActivateToolbarCommand(command);
            _content.Content = _viewModel.CurrentView;
        });
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
