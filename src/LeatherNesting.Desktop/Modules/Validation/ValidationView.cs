using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Validation;

/// <summary>M10 validation, approval, and quality-report demonstration page.</summary>
public sealed class ValidationView : UserControl
{
    private readonly ValidationViewModel _viewModel = new();
    private readonly StackPanel _summary = new() { Orientation = Orientation.Horizontal, Spacing = 10 };
    private readonly StackPanel _issues = new() { Spacing = 6 };
    private readonly StackPanel _rules = new() { Spacing = 6 };
    private readonly TextBlock _approvalStatus = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _exportStatus = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _report = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _actionMessage = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly Button _approve = new() { Content = "提交审批（TODO）" };
    private readonly Button _productionExport = new() { Content = "生成生产报告 / PDF（TODO）" };

    public ValidationView()
    {
        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var scenarioSelector = new ComboBox
        {
            Width = 220,
            ItemsSource = _viewModel.Scenarios,
            SelectedItem = _viewModel.Scenario,
        };
        scenarioSelector.SelectionChanged += (_, _) =>
        {
            if (scenarioSelector.SelectedItem is ValidationDemoScenario scenario)
            {
                _viewModel.SelectScenario(scenario.Id);
                Refresh();
            }
        };

        _approve.Click += (_, _) => { _viewModel.RequestApproval(); RefreshAction(); };
        _productionExport.Click += (_, _) => { _viewModel.RequestProductionExport(); RefreshAction(); };

        var approvalActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                _approve,
                _productionExport,
            },
        };

        var top = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = "演示场景", VerticalAlignment = VerticalAlignment.Center, Foreground = AppTheme.TextMuted },
                scenarioSelector,
                new TodoBadge("DEMO · 切换场景仅改变 M10 模块内存状态"),
            },
        };

        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 14,
                Children =
                {
                    new TextBlock { Text = "校验、审批与质量报告", FontSize = 22, FontWeight = FontWeight.Bold },
                    new TodoBadge("TODO · 真实全量校验、豁免签名、审批持久化和 PDF 生成均未接入"),
                    top,
                    Section("问题汇总", _summary),
                    Section("问题清单 · 对象 / 规则 / 建议", _issues),
                    Section("校验规则说明", _rules),
                    Section("审批与生产出口", new StackPanel { Spacing = 8, Children = { _approvalStatus, _exportStatus, approvalActions, _actionMessage } }),
                    ReportSection(),
                },
            },
        };
    }

    private Control ReportSection() => new Border
    {
        Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF4, 0xD8)),
        BorderBrush = AppTheme.TodoAmber,
        BorderThickness = new Thickness(2),
        Padding = new Thickness(14),
        Child = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "DEMO · 质量报告预览", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = AppTheme.TodoAmber },
                _report,
            },
        },
    };

    private void Refresh()
    {
        _summary.Children.Clear();
        _summary.Children.Add(CountCard("阻断", _viewModel.Scenario.BlockingCount, Brushes.IndianRed));
        _summary.Children.Add(CountCard("警告", _viewModel.Scenario.WarningCount, AppTheme.TodoAmber));
        _summary.Children.Add(CountCard("提示", _viewModel.Scenario.InformationCount, AppTheme.Accent));

        _issues.Children.Clear();
        foreach (var issue in _viewModel.Issues)
            _issues.Children.Add(IssueRow(issue));

        _rules.Children.Clear();
        foreach (var rule in _viewModel.Rules)
            _rules.Children.Add(new Border
            {
                Background = AppTheme.Surface,
                BorderBrush = AppTheme.SurfaceBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Child = new TextBlock { Text = $"{rule.Id} · {rule.Name} · {rule.Scope}\n{rule.Description}\n影响：{rule.ProductionImpact}", TextWrapping = TextWrapping.Wrap },
            });

        _approvalStatus.Text = _viewModel.ApprovalStatus;
        _exportStatus.Text = _viewModel.ProductionExportStatus;
        _approve.IsEnabled = _viewModel.Scenario.CanApprove;
        _productionExport.IsEnabled = _viewModel.Scenario.CanExportForProduction;
        _report.Text = _viewModel.BuildReportPreview();
        RefreshAction();
    }

    private Control IssueRow(ValidationIssue issue)
    {
        var locate = new Button { Content = "定位对象（TODO）", VerticalAlignment = VerticalAlignment.Center };
        locate.Click += (_, _) => { _viewModel.Locate(issue); RefreshAction(); };
        var detail = new TextBlock
        {
            Text = $"{SeverityLabel(issue.Severity)} · {issue.ObjectName} [{issue.ObjectId}]\n规则：{issue.RuleId} · {issue.RuleName}\n问题：{issue.Message}\n建议：{issue.Suggestion}",
            TextWrapping = TextWrapping.Wrap,
        };
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"), ColumnSpacing = 12, Children = { detail, locate } };
        Grid.SetColumn(locate, 1);
        return new Border
        {
            Background = AppTheme.Surface,
            BorderBrush = SeverityBrush(issue.Severity),
            BorderThickness = new Thickness(3, 1, 1, 1),
            Padding = new Thickness(10),
            Child = grid,
        };
    }

    private void RefreshAction() => _actionMessage.Text = _viewModel.ActionMessage ?? string.Empty;

    private static Control CountCard(string label, int count, IBrush accent) => new Border
    {
        Background = AppTheme.Surface,
        BorderBrush = accent,
        BorderThickness = new Thickness(1),
        Padding = new Thickness(14, 8),
        MinWidth = 120,
        Child = new TextBlock { Text = $"{label}  {count}", FontSize = 16, FontWeight = FontWeight.Bold },
    };

    private static Control Section(string title, Control content) => new StackPanel
    {
        Spacing = 8,
        Children = { new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold }, content },
    };

    private static string SeverityLabel(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Blocking => "阻断",
        ValidationSeverity.Warning => "警告",
        ValidationSeverity.Information => "提示",
        _ => throw new ArgumentOutOfRangeException(nameof(severity)),
    };

    private static IBrush SeverityBrush(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Blocking => Brushes.IndianRed,
        ValidationSeverity.Warning => AppTheme.TodoAmber,
        ValidationSeverity.Information => AppTheme.Accent,
        _ => throw new ArgumentOutOfRangeException(nameof(severity)),
    };
}
