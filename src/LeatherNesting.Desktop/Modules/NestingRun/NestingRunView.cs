using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.NestingRun;

/// <summary>M08 strategy and run-control demonstration. It never starts a production nesting operation.</summary>
public sealed class NestingRunView : UserControl
{
    private readonly NestingRunViewModel _viewModel = new();
    private readonly StackPanel _settings = new() { Spacing = 6 };
    private readonly StackPanel _status = new() { Spacing = 8 };
    private readonly StackPanel _timeline = new() { Spacing = 5 };
    private readonly TextBlock _feedback = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly TextBox _budget = new() { Width = 100 };
    private readonly ComboBox _angles = new() { ItemsSource = new[] { "0° / 180°", "0° / 90° / 180° / 270°", "0°--359°" }, MinWidth = 220 };
    private readonly ComboBox _order = new() { ItemsSource = new[] { "优先级 → 面积", "面积 → 优先级", "大件优先 → 小件填空" }, MinWidth = 220 };
    private readonly TextBox _seed = new() { Width = 100 };
    private readonly CheckBox _fillSmallPieces = new() { Content = "小件填空（TODO 模拟偏好）" };

    public NestingRunView()
    {
        var preset = new ComboBox
        {
            ItemsSource = _viewModel.PresetNames,
            SelectedItem = _viewModel.Settings.Preset,
            MinWidth = 180,
        };
        preset.SelectionChanged += (_, _) =>
        {
            if (preset.SelectedItem is string selected)
                _viewModel.SelectPreset(selected);
            Refresh();
        };

        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock { Text = "排样策略与运行控制", FontSize = 22, FontWeight = FontWeight.Bold },
                    new TodoBadge("TODO · 模拟状态：自动算法、真实计时、取消、进度、校验与方案写入均未接入"),
                    new TextBlock
                    {
                        Text = NestingRunViewModel.ProductionWarning + "；页面仅演示控制语义，不生成生产方案。",
                        Foreground = Brushes.IndianRed,
                        FontWeight = FontWeight.Bold,
                        TextWrapping = TextWrapping.Wrap,
                    },
                    Section("策略预设与设置", new StackPanel { Spacing = 8, Children = { Label("预设"), preset, _settings } }),
                    Section("运行控制", BuildControls()),
                    Section("当前状态与最佳 DEMO 指标", _status),
                    Section("状态机时间线", _timeline),
                },
            },
        };

        Refresh();
    }

    private Control BuildControls()
    {
        var controls = new WrapPanel { Orientation = Orientation.Horizontal };
        controls.Children.Add(Command("1. 准备", _viewModel.Prepare));
        controls.Children.Add(Command("2. 开始", _viewModel.Start));
        controls.Children.Add(Command("3. 发现更优", _viewModel.ReportBetterDemoResult));
        controls.Children.Add(Command("继续运行", _viewModel.ResumeDemoRun));
        controls.Children.Add(Command("完成", _viewModel.Complete));
        controls.Children.Add(Command("停止并保留最佳", _viewModel.Stop));
        controls.Children.Add(Command("取消并回滚", _viewModel.Cancel));
        controls.Children.Add(Command("重置演示", _viewModel.Reset));

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                controls,
                new TextBlock
                {
                    Text = "停止：保留当前最佳完整 DEMO 指标。取消：丢弃本次临时 DEMO 指标并恢复运行前快照。",
                    TextWrapping = TextWrapping.Wrap,
                },
                _feedback,
            },
        };
    }

    private Button Command(string label, Func<bool> action)
    {
        var button = new Button { Content = label, Margin = new Thickness(0, 0, 8, 8) };
        button.Click += (_, _) =>
        {
            action();
            Refresh();
        };
        return button;
    }

    private void Refresh()
    {
        _settings.Children.Clear();
        _budget.Text = _viewModel.Settings.TimeBudgetMinutes.ToString();
        _angles.SelectedItem = _viewModel.Settings.AllowedAngles;
        _order.SelectedItem = _viewModel.Settings.PlacementOrder;
        _seed.Text = _viewModel.Settings.Seed.ToString();
        _fillSmallPieces.IsChecked = _viewModel.Settings.FillSmallPieces;
        _settings.Children.Add(EditorRow("时间预算（分钟）", _budget));
        _settings.Children.Add(EditorRow("允许角度", _angles));
        _settings.Children.Add(EditorRow("排放顺序", _order));
        _settings.Children.Add(EditorRow("确定性种子", _seed));
        _settings.Children.Add(_fillSmallPieces);
        var applySettings = new Button { Content = "应用内存演示设置", HorizontalAlignment = HorizontalAlignment.Left };
        applySettings.Click += (_, _) =>
        {
            _viewModel.UpdateSettings(
                _budget.Text,
                _angles.SelectedItem as string ?? string.Empty,
                _order.SelectedItem as string ?? string.Empty,
                _seed.Text,
                _fillSmallPieces.IsChecked == true);
            _feedback.Text = _viewModel.Feedback;
        };
        _settings.Children.Add(applySettings);
        _settings.Children.Add(new TextBlock { Text = _viewModel.Settings.Disclaimer, Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap });

        _status.Children.Clear();
        _status.Children.Add(Row("状态", $"{_viewModel.StateLabel} · {NestingRunViewModel.SimulatedStatus}"));
        _status.Children.Add(Row("最佳利用率", $"{_viewModel.BestMetrics.UtilizationPercent:0.0}% · DEMO"));
        _status.Children.Add(Row("已放 / 未放", $"{_viewModel.BestMetrics.PlacedPieces} / {_viewModel.BestMetrics.UnplacedPieces} · DEMO"));
        _status.Children.Add(Row("材料张数", $"{_viewModel.BestMetrics.MaterialSheets} · DEMO"));
        _status.Children.Add(Row("候选数 / 已用时间", $"{_viewModel.BestMetrics.CandidateCount} / {_viewModel.BestMetrics.Elapsed}"));
        _status.Children.Add(new TextBlock { Text = _viewModel.OutcomeSummary, Foreground = Brushes.IndianRed, TextWrapping = TextWrapping.Wrap });

        _timeline.Children.Clear();
        foreach (var entry in _viewModel.Timeline)
            _timeline.Children.Add(Row(entry.State, entry.Detail));

        _feedback.Text = _viewModel.Feedback;
    }

    private static TextBlock Label(string text) => new() { Text = text, Foreground = AppTheme.TextMuted };

    private static Control Section(string title, Control content) => new Border
    {
        Background = AppTheme.Surface,
        BorderBrush = AppTheme.SurfaceBorder,
        BorderThickness = new Thickness(1),
        Padding = new Thickness(12),
        Child = new StackPanel
        {
            Spacing = 9,
            Children = { new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold }, content },
        },
    };

    private static Control Row(string label, string value) => new Grid
    {
        ColumnDefinitions = new ColumnDefinitions("150,*"),
        Children =
        {
            new TextBlock { Text = label, Foreground = AppTheme.TextMuted },
            new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap, [Grid.ColumnProperty] = 1 },
        },
    };

    private static Control EditorRow(string label, Control editor)
    {
        Grid.SetColumn(editor, 1);
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,*"),
            Children =
            {
                new TextBlock { Text = label, Foreground = AppTheme.TextMuted, VerticalAlignment = VerticalAlignment.Center },
                editor,
            },
        };
    }
}
