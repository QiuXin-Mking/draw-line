using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Administration;

/// <summary>M12 rule library, audit, permission, and system-settings demonstration page.</summary>
public sealed class AdministrationView : UserControl
{
    private readonly AdministrationViewModel _viewModel = new();
    private readonly StackPanel _presetDetail = new() { Spacing = 6 };
    private readonly StackPanel _auditTimeline = new() { Spacing = 6 };
    private readonly StackPanel _permissionMatrix = new() { Spacing = 4 };
    private readonly TextBlock _permissionExplanation = new() { Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _adapterExplanation = new() { Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _actionFeedback = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _settingsFeedback = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly Button _editRule = new() { Content = "编辑 / 发布规则（TODO）" };
    private readonly Button _registerAdapter = new() { Content = "注册外部适配器（TODO）" };

    public AdministrationView()
    {
        Content = BuildLayout();
        RefreshPreset();
        RefreshAudit();
        RefreshPermissions();
    }

    private Control BuildLayout()
    {
        _editRule.Click += (_, _) => { _viewModel.RequestRuleWrite(); RefreshPermissions(); };
        _registerAdapter.Click += (_, _) => { _viewModel.RequestAdapterRegistration(); RefreshPermissions(); };

        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock { Text = "规则库、审计、权限与系统设置", FontSize = 22, FontWeight = FontWeight.Bold },
                    new TodoBadge(AdministrationViewModel.TodoInventory),
                    Section("预设库与版本", BuildPresetLibrary()),
                    Section("审计时间线", BuildAudit()),
                    Section("角色权限矩阵", BuildPermissions()),
                    Section("系统设置 · 仅内存 DEMO", BuildSettings()),
                },
            },
        };
    }

    private Control BuildPresetLibrary()
    {
        var cards = new StackPanel { Spacing = 6 };
        foreach (var preset in _viewModel.Presets)
        {
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = $"{AdministrationViewModel.CategoryLabel(preset.Category)} · {preset.Name} · 项目 {preset.ProjectSnapshot.Version} / 最新 {preset.Latest.Version}",
            };
            button.Click += (_, _) => { _viewModel.SelectPreset(preset.Id); RefreshPreset(); };
            cards.Children.Add(button);
        }

        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("2*,3*"), ColumnSpacing = 14, Children = { cards, Panel(_presetDetail) } };
        Grid.SetColumn(grid.Children[1], 1);
        return grid;
    }

    private Control BuildAudit()
    {
        var filter = new ComboBox
        {
            Width = 160,
            ItemsSource = Enum.GetValues<AuditCategory>(),
            SelectedItem = AuditCategory.All,
        };
        filter.SelectionChanged += (_, _) =>
        {
            if (filter.SelectedItem is AuditCategory category)
            {
                _viewModel.SetAuditFilter(category);
                RefreshAudit();
            }
        };
        return new StackPanel { Spacing = 8, Children =
        {
            new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children =
            {
                new TextBlock { Text = "筛选", VerticalAlignment = VerticalAlignment.Center, Foreground = AppTheme.TextMuted }, filter,
                new TextBlock { Text = "DEMO · 时间线未读取或写入真实日志", VerticalAlignment = VerticalAlignment.Center, Foreground = AppTheme.TodoAmber },
            }},
            _auditTimeline,
        }};
    }

    private Control BuildPermissions()
    {
        var role = new ComboBox
        {
            Width = 180,
            ItemsSource = Enum.GetValues<RoleKind>(),
            SelectedItem = _viewModel.SelectedRole,
        };
        role.SelectionChanged += (_, _) =>
        {
            if (role.SelectedItem is RoleKind selected)
            {
                _viewModel.SelectRole(selected);
                RefreshPermissions();
            }
        };
        return new StackPanel { Spacing = 8, Children =
        {
            new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children =
            {
                new TextBlock { Text = "演示当前角色", VerticalAlignment = VerticalAlignment.Center, Foreground = AppTheme.TextMuted }, role,
                new TodoBadge("TODO · 内存角色切换，未执行权限认证或保存"),
            }},
            _permissionMatrix,
            new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { _editRule, _registerAdapter } },
            _permissionExplanation,
            _adapterExplanation,
            _actionFeedback,
        }};
    }

    private Control BuildSettings()
    {
        var unit = Combo(["mm", "inch"], _viewModel.Settings.Unit);
        var tolerance = new TextBox { Text = _viewModel.Settings.Tolerance, Width = 120 };
        var autoSave = new CheckBox { Content = "自动保存（演示）", IsChecked = _viewModel.Settings.AutoSave };
        var theme = Combo(["跟随系统", "浅色", "深色"], _viewModel.Settings.Theme);
        var logLevel = Combo(["精简", "常规", "诊断"], _viewModel.Settings.LogLevel);
        var update = new Button { Content = "应用到内存 DEMO（TODO）", HorizontalAlignment = HorizontalAlignment.Left };
        update.Click += (_, _) =>
        {
            _viewModel.UpdateSettings(
                unit.SelectedItem?.ToString() ?? "mm",
                tolerance.Text ?? string.Empty,
                autoSave.IsChecked == true,
                theme.SelectedItem?.ToString() ?? "跟随系统",
                logLevel.SelectedItem?.ToString() ?? "常规");
            _settingsFeedback.Text = _viewModel.SettingsFeedback;
        };

        _settingsFeedback.Text = _viewModel.SettingsFeedback;
        return Panel(new StackPanel { Spacing = 8, Children =
        {
            Field("单位", unit),
            Field("几何容差", tolerance),
            Field("自动保存", autoSave),
            Field("主题", theme),
            Field("日志级别", logLevel),
            update,
            _settingsFeedback,
            new TextBlock { Text = "主题切换不会修改全局 AppTheme；自动保存不会启动定时器；日志设置不会创建文件。", Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap },
        }});
    }

    private void RefreshPreset()
    {
        var preset = _viewModel.SelectedPreset;
        _presetDetail.Children.Clear();
        _presetDetail.Children.Add(new TextBlock { Text = $"{preset.Id} · {preset.Name}", FontSize = 16, FontWeight = FontWeight.Bold });
        _presetDetail.Children.Add(new TextBlock { Text = _viewModel.PresetComparison, TextWrapping = TextWrapping.Wrap });
        _presetDetail.Children.Add(new TextBlock { Text = "版本列表", FontWeight = FontWeight.Bold });
        foreach (var version in preset.Versions)
            _presetDetail.Children.Add(new TextBlock { Text = $"{version.Version} · {version.PublishedAt} · {version.Author}\n{version.Summary}", TextWrapping = TextWrapping.Wrap });
        _presetDetail.Children.Add(new TextBlock { Text = "TODO · 切换版本只查看演示数据；规则写入和项目升级均未接入。", Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap });
    }

    private void RefreshAudit()
    {
        _auditTimeline.Children.Clear();
        foreach (var entry in _viewModel.FilteredAuditEvents)
        {
            _auditTimeline.Children.Add(Panel(new TextBlock
            {
                Text = $"{entry.Time} · {entry.Actor} · {AdministrationViewModel.AuditCategoryLabel(entry.Category)}\n{entry.Action} · {entry.Target}\n{entry.Result}",
                TextWrapping = TextWrapping.Wrap,
            }));
        }
    }

    private void RefreshPermissions()
    {
        _permissionMatrix.Children.Clear();
        _permissionMatrix.Children.Add(MatrixRow("能力", "操作员", "工艺工程师", "审核员", "管理员", true));
        foreach (var row in _viewModel.Permissions)
            _permissionMatrix.Children.Add(MatrixRow(row.Capability, Mark(row.Operator), Mark(row.ProcessEngineer), Mark(row.Reviewer), Mark(row.Administrator), false));

        _editRule.IsEnabled = _viewModel.CanEditRules;
        _registerAdapter.IsEnabled = _viewModel.CanManageAdapters;
        _permissionExplanation.Text = _viewModel.RuleActionExplanation;
        _adapterExplanation.Text = _viewModel.AdapterActionExplanation;
        _actionFeedback.Text = _viewModel.ActionFeedback;
    }

    private static Control MatrixRow(string capability, string operatorValue, string engineer, string reviewer, string administrator, bool header)
    {
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("2*,*,*,*,*"), ColumnSpacing = 6 };
        var values = new[] { capability, operatorValue, engineer, reviewer, administrator };
        for (var index = 0; index < values.Length; index++)
        {
            var text = new TextBlock { Text = values[index], FontWeight = header ? FontWeight.Bold : FontWeight.Normal, TextWrapping = TextWrapping.Wrap };
            Grid.SetColumn(text, index);
            grid.Children.Add(text);
        }
        return Panel(grid);
    }

    private static string Mark(bool allowed) => allowed ? "允许" : "禁用";

    private static ComboBox Combo(IReadOnlyList<string> items, string selected) => new()
    {
        Width = 160,
        ItemsSource = items,
        SelectedItem = selected,
    };

    private static Control Field(string label, Control control) => new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children =
    {
        new TextBlock { Text = label, Width = 100, VerticalAlignment = VerticalAlignment.Center, Foreground = AppTheme.TextMuted }, control,
    }};

    private static Border Panel(Control content) => new()
    {
        Background = AppTheme.Surface,
        BorderBrush = AppTheme.SurfaceBorder,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(4),
        Padding = new Thickness(10),
        Child = content,
    };

    private static Control Section(string title, Control content) => new StackPanel { Spacing = 8, Children =
    {
        new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold }, content,
    }};
}
