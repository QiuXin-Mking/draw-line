using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.GeometryRepair;

/// <summary>M04 contour issue list, repair tools, visual difference preview, and session controls.</summary>
public sealed class GeometryRepairView : UserControl
{
    private readonly GeometryRepairViewModel _viewModel = new();
    private readonly StackPanel _issueRows = new() { Spacing = 6 };
    private readonly StackPanel _selectedIssue = new() { Spacing = 4 };
    private readonly RepairPreviewCanvas _preview = new() { MinHeight = 330 };
    private readonly TextBlock _state = new() { FontWeight = FontWeight.Bold };
    private readonly TextBlock _difference = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _feedback = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Button _commit = new() { Content = "提交到会话" };
    private readonly Button _cancel = new() { Content = "取消预览" };
    private readonly Button _undo = new() { Content = "撤销" };
    private readonly Button _redo = new() { Content = "重做" };

    public GeometryRepairView()
    {
        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        _commit.Click += (_, _) => { _viewModel.CommitPreview(); Refresh(); };
        _cancel.Click += (_, _) => { _viewModel.CancelPreview(); Refresh(); };
        _undo.Click += (_, _) => { _viewModel.Undo(); Refresh(); };
        _redo.Click += (_, _) => { _viewModel.Redo(); Refresh(); };

        var tools = new StackPanel { Spacing = 12 };
        foreach (var group in _viewModel.ToolGroups)
            tools.Children.Add(BuildToolGroup(group));

        var left = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                Section("轮廓问题表", _issueRows),
                Section("选中对象", Card(_selectedIssue)),
                Section("修复工具", tools),
            },
        };

        var legend = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                Legend("原始", new SolidColorBrush(Color.FromRgb(0x8C, 0x98, 0xA5))),
                Legend("沿用", new SolidColorBrush(Color.FromRgb(0x55, 0xAA, 0xF5))),
                Legend("新增", new SolidColorBrush(Color.FromRgb(0x42, 0xC7, 0x7A))),
                Legend("冲突", Brushes.IndianRed),
            },
        };
        var previewCard = Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "选择预览 · 原始几何永不覆盖", FontWeight = FontWeight.Bold },
                legend,
                _preview,
            },
        });
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { _commit, _cancel, _undo, _redo },
        };
        var right = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                previewCard,
                Section("预览差异", Card(new StackPanel { Spacing = 6, Children = { _state, _difference } })),
                Section("操作状态", Card(new StackPanel { Spacing = 8, Children = { actions, _feedback } })),
                TodoActions(),
            },
        };
        var body = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("360,*"),
            ColumnSpacing = 14,
            Children = { left, right },
        };
        Grid.SetColumn(right, 1);

        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "轮廓诊断与几何修复", FontSize = 22, FontWeight = FontWeight.Bold },
                    new TodoBadge("TODO · 节点/剪断画布手势、批量修复和项目持久化尚未接入"),
                    body,
                },
            },
        };
    }

    private Control BuildToolGroup(RepairToolGroup group)
    {
        var buttons = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var tool in group.Tools)
        {
            var button = new Button
            {
                Content = tool.IsConnected ? tool.Label : $"{tool.Label} · TODO",
                Margin = new Thickness(0, 0, 6, 6),
            };
            ToolTip.SetTip(button, tool.Description);
            button.Click += (_, _) => { _viewModel.Preview(tool.Action); Refresh(); };
            buttons.Children.Add(button);
        }
        return Card(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock { Text = group.Name, FontWeight = FontWeight.Bold },
                buttons,
            },
        });
    }

    private Control TodoActions()
    {
        var batch = new Button { Content = "批量修复 · TODO" };
        batch.Click += (_, _) => { _viewModel.InvokeTodo(RepairTodoAction.BatchRepair); Refresh(); };
        var persist = new Button { Content = "写入项目版本 · TODO" };
        persist.Click += (_, _) => { _viewModel.InvokeTodo(RepairTodoAction.PersistToProject); Refresh(); };
        return Section("未接入能力", Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TodoBadge(),
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { batch, persist } },
            },
        }));
    }

    private void Refresh()
    {
        _issueRows.Children.Clear();
        foreach (var issue in _viewModel.Issues)
        {
            var button = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Content = new TextBlock
                {
                    Text = $"{SeverityLabel(issue.Severity)}  {issue.ObjectId} · {issue.ObjectName}\n{issue.Kind} · {issue.Detail}",
                    TextWrapping = TextWrapping.Wrap,
                },
            };
            button.Click += (_, _) => { _viewModel.SelectIssue(issue.ObjectId); RefreshSelectedIssue(); };
            _issueRows.Children.Add(button);
        }
        RefreshSelectedIssue();

        _preview.SetData(_viewModel.BeforeLoops, _viewModel.CurrentLoops);
        _state.Text = $"状态：{_viewModel.StateLabel}";
        var difference = _viewModel.Difference;
        _difference.Text =
            $"轮廓：{difference.BeforeLoopCount} → {difference.AfterLoopCount}\n" +
            $"曲线：{difference.BeforeCurveCount} → {difference.AfterCurveCount}（新增 {difference.AddedCurveCount} / 移除 {difference.RemovedCurveCount}）\n" +
            $"面积：{difference.BeforeAreaSquareMillimetres:F2} → {difference.AfterAreaSquareMillimetres:F2} mm²\n" +
            $"拓扑：{difference.TopologyChange}";
        _feedback.Text = _viewModel.Feedback;
        _feedback.Foreground = _viewModel.Feedback.Contains("TODO", StringComparison.Ordinal) ? AppTheme.TodoAmber : AppTheme.TextMuted;
        _commit.IsEnabled = _viewModel.CanCommit;
        _cancel.IsEnabled = _viewModel.CanCancel;
        _undo.IsEnabled = _viewModel.CanUndo;
        _redo.IsEnabled = _viewModel.CanRedo;
    }

    private void RefreshSelectedIssue()
    {
        var issue = _viewModel.SelectedIssue;
        _selectedIssue.Children.Clear();
        _selectedIssue.Children.Add(new TextBlock { Text = $"{issue.ObjectName} [{issue.ObjectId}]", FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap });
        _selectedIssue.Children.Add(new TextBlock { Text = $"问题：{issue.Kind}\n{issue.Detail}\n建议：{issue.Suggestion}", Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap });
    }

    private static Control Legend(string text, IBrush brush) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = 5,
        Children =
        {
            new Border { Width = 18, Height = 4, Background = brush, VerticalAlignment = VerticalAlignment.Center },
            new TextBlock { Text = text },
        },
    };

    private static Control Section(string title, Control content) => new StackPanel
    {
        Spacing = 7,
        Children = { new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold }, content },
    };

    private static Border Card(Control child) => new()
    {
        Background = AppTheme.Surface,
        BorderBrush = AppTheme.SurfaceBorder,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(4),
        Padding = new Thickness(10),
        Child = child,
    };

    private static string SeverityLabel(RepairIssueSeverity severity) => severity switch
    {
        RepairIssueSeverity.Blocking => "阻断",
        RepairIssueSeverity.Warning => "警告",
        RepairIssueSeverity.Information => "提示",
        _ => throw new ArgumentOutOfRangeException(nameof(severity)),
    };
}
