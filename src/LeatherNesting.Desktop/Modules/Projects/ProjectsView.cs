using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Projects;

/// <summary>M01: project &amp; order centre demo page.</summary>
public sealed class ProjectsView : UserControl
{
    private readonly ProjectsViewModel _viewModel;
    private readonly TextBlock _todoMessage = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _versionDetail = new() { Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap };

    public ProjectsView()
        : this(new ProjectsViewModel())
    {
    }

    public ProjectsView(ProjectsViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var scenario = _viewModel.Scenario;
        var root = new ScrollViewer
        {
            Content = new StackPanel { Spacing = 16, Margin = new Thickness(24), Children =
            {
                Section("项目摘要", SummaryCard(scenario)),
                Section("订单信息", OrderInfo(scenario)),
                Section("版本时间线", VersionTimeline(scenario)),
                Section("最近变更", HistoryList(scenario.ChangeHistory)),
                Section("导出历史", HistoryList(scenario.ExportHistory)),
                Section("状态轨迹", StatusTrace()),
                Section("操作", Actions()),
            }},
        };
        return root;
    }

    private static TextBlock SectionHeader(string text) => new() { Text = text, FontSize = 14, FontWeight = FontWeight.Bold };

    private static Control Section(string header, Control body) => new StackPanel { Spacing = 8, Children = { SectionHeader(header), body } };

    private Control SummaryCard(Demo.DemoScenario scenario)
    {
        return new Border
        {
            Background = AppTheme.Surface,
            BorderBrush = AppTheme.SurfaceBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(16),
            Child = new StackPanel { Spacing = 4, Children =
            {
                new TextBlock { Text = scenario.ProjectName, FontSize = 18, FontWeight = FontWeight.Bold },
                Row("项目编号", scenario.ProjectNumber),
                Row("创建人", scenario.Creator),
                Row("状态", scenario.Status),
            }},
        };
    }

    private static Control OrderInfo(Demo.DemoScenario scenario) => new StackPanel { Spacing = 4, Children =
    {
        Row("订单号", scenario.OrderNumber),
        Row("客户", scenario.Customer),
        Row("款号", scenario.StyleNumber),
        Row("交期", scenario.Deadline),
        Row("优先级", scenario.Priority),
        Row("备注", scenario.Notes),
    }};

    private Control VersionTimeline(Demo.DemoScenario scenario)
    {
        var list = new StackPanel { Spacing = 4 };
        foreach (var entry in scenario.VersionHistory)
        {
            var button = new Button
            {
                Content = $"{entry.Version} · {entry.Date}",
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            button.Click += (_, _) =>
            {
                _viewModel.SelectVersion(entry);
                _versionDetail.Text = _viewModel.SelectedVersionDetail;
            };
            list.Children.Add(button);
        }
        list.Children.Add(_versionDetail);
        return list;
    }

    private static Control HistoryList(IReadOnlyList<Demo.HistoryEntry> entries)
    {
        var list = new StackPanel { Spacing = 2 };
        foreach (var entry in entries)
            list.Children.Add(new TextBlock { Text = $"{entry.Date} · {entry.Description}" });
        return list;
    }

    private Control StatusTrace() => new TextBlock
    {
        Text = string.Join(" → ", _viewModel.StatusTrace),
        Foreground = AppTheme.TextMuted,
        TextWrapping = TextWrapping.Wrap,
    };

    private Control Actions()
    {
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(ActionButton("新建", _viewModel.NewProject));
        buttons.Children.Add(ActionButton("复制版本", _viewModel.Duplicate));
        buttons.Children.Add(ActionButton("审批", _viewModel.Approve));
        buttons.Children.Add(ActionButton("恢复", _viewModel.Restore));
        buttons.Children.Add(ActionButton("编辑订单信息", _viewModel.EditOrder));
        return new StackPanel { Spacing = 8, Children = { buttons, _todoMessage } };
    }

    private Button ActionButton(string label, Action action)
    {
        var button = new Button { Content = label };
        button.Click += (_, _) => { action(); _todoMessage.Text = _viewModel.TodoMessage; };
        return button;
    }

    private static Control Row(string label, string value) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = 8,
        Children =
        {
            new TextBlock { Text = label, Foreground = AppTheme.TextMuted, MinWidth = 80 },
            new TextBlock { Text = value },
        },
    };
}
