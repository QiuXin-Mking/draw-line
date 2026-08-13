using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Export;

/// <summary>M11 export package configuration and manifest demonstration.</summary>
public sealed class ExportView : UserControl
{
    private readonly ExportViewModel _viewModel = new();
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _action = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly TextBox _manifest = new() { IsReadOnly = true, AcceptsReturn = true, MinHeight = 245, FontFamily = FontFamily.Parse("Menlo, Consolas") };
    private readonly Button _productionButton = new() { Content = "预览生产交接包" };
    private readonly ComboBox _scenarioPicker = new() { MinWidth = 210 };

    public ExportView()
    {
        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        _scenarioPicker.ItemsSource = _viewModel.Scenarios;
        _scenarioPicker.SelectionChanged += (_, _) =>
        {
            if (_scenarioPicker.SelectedItem is ExportDemoScenario scenario)
            {
                _viewModel.SelectScenario(scenario.Id);
                Refresh();
            }
        };
        _scenarioPicker.SelectedItem = _viewModel.Scenario;

        _productionButton.Click += (_, _) =>
        {
            _viewModel.RequestProductionExport();
            Refresh();
        };

        var outputCards = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var output in _viewModel.OutputOptions)
        {
            var selector = new CheckBox { Content = output.Label, IsChecked = output.IsSelected };
            selector.IsCheckedChanged += (_, _) =>
            {
                _viewModel.SetOutputSelected(output.Id, selector.IsChecked == true);
                Refresh();
            };
            outputCards.Children.Add(new Border
            {
                Background = AppTheme.Surface,
                BorderBrush = AppTheme.SurfaceBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 8, 8),
                Width = 150,
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        selector,
                        new TextBlock { Text = output.Role, Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap },
                    },
                },
            });
        }

        var settings = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("120,*,120,*"),
            RowDefinitions = RowDefinitions.Parse("Auto,Auto,Auto,Auto"),
            ColumnSpacing = 8,
            RowSpacing = 8,
        };
        AddSetting(settings, 0, 0, "输出目录", _viewModel.Settings.Directory);
        AddSetting(settings, 1, 0, "命名模板", _viewModel.Settings.NamingTemplate);
        AddSetting(settings, 2, 0, "单位", _viewModel.Settings.Unit);
        AddSetting(settings, 2, 2, "原点", _viewModel.Settings.Origin);
        AddSetting(settings, 3, 0, "旋转", _viewModel.Settings.Rotation);
        AddSetting(settings, 3, 2, "曲线容差", _viewModel.Settings.CurveTolerance);

        var mappings = new StackPanel { Spacing = 5 };
        foreach (var mapping in _viewModel.LayerMappings)
            mappings.Children.Add(new TextBlock { Text = $"{mapping.Semantic,-10} → {mapping.DxfLayer} · {mapping.LineType}", FontFamily = FontFamily.Parse("Menlo, Consolas") });

        var futureActions = new WrapPanel { Orientation = Orientation.Horizontal };
        AddTodoButton(futureActions, "实际写文件", ExportTodoAction.WriteFiles);
        AddTodoButton(futureActions, "打开目录", ExportTodoAction.OpenOutputDirectory);
        AddTodoButton(futureActions, "外部程序", ExportTodoAction.LaunchExternalProgram);
        AddTodoButton(futureActions, "PLT", ExportTodoAction.ExportPlt);
        AddTodoButton(futureActions, "DWG", ExportTodoAction.ExportDwg);
        AddTodoButton(futureActions, "发送设备", ExportTodoAction.SendToDevice);

        var manifestCard = Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "DEMO manifest 预览", FontWeight = FontWeight.Bold },
                new TextBlock { Text = "包含项目版本、输入指纹、文件角色、图层映射与输出哈希占位。", Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap },
                _manifest,
            },
        });

        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "导出与生产交接", FontSize = 21, FontWeight = FontWeight.Bold },
                    new TodoBadge("TODO · 页面仅生成 DEMO 预览；不写文件、不启动程序、不连接设备"),
                    Card(new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "校验门禁", FontWeight = FontWeight.Bold }, _scenarioPicker, _status, _productionButton, _action } }),
                    Section("输出选择", outputCards),
                    Card(new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "包设置", FontWeight = FontWeight.Bold }, settings, new TextBlock { Text = $"标签：{_viewModel.Settings.LabelContent}", Foreground = AppTheme.TextMuted } } }),
                    Card(new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "DXF 图层 / 线型映射", FontWeight = FontWeight.Bold }, mappings } }),
                    manifestCard,
                    Card(new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "未来适配器 · 均不执行", FontWeight = FontWeight.Bold }, futureActions } }),
                },
            },
        };
    }

    private void Refresh()
    {
        _status.Text = $"{_viewModel.ProductionExportStatus}\n{_viewModel.Scenario.Description}";
        _status.Foreground = _viewModel.CanRequestProductionExport ? Brushes.LightGreen : Brushes.IndianRed;
        _productionButton.IsEnabled = _viewModel.CanRequestProductionExport;
        _action.Text = _viewModel.ActionMessage;
        _manifest.Text = _viewModel.BuildManifestPreview();
    }

    private void AddTodoButton(Panel panel, string label, ExportTodoAction action)
    {
        var button = new Button { Content = $"{label} · TODO", Margin = new Thickness(0, 0, 8, 8) };
        button.Click += (_, _) =>
        {
            _viewModel.InvokeTodo(action);
            Refresh();
        };
        panel.Children.Add(button);
    }

    private static void AddSetting(Grid grid, int row, int column, string label, string value)
    {
        var labelBlock = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Foreground = AppTheme.TextMuted };
        var valueBlock = new TextBlock { Text = value, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        Grid.SetRow(labelBlock, row);
        Grid.SetColumn(labelBlock, column);
        Grid.SetRow(valueBlock, row);
        Grid.SetColumn(valueBlock, column + 1);
        if (row < 2)
            Grid.SetColumnSpan(valueBlock, 3);
        grid.Children.Add(labelBlock);
        grid.Children.Add(valueBlock);
    }

    private static Control Section(string title, Control content) => Card(new StackPanel
    {
        Spacing = 8,
        Children = { new TextBlock { Text = title, FontWeight = FontWeight.Bold }, content },
    });

    private static Border Card(Control child) => new()
    {
        Background = AppTheme.Surface,
        BorderBrush = AppTheme.SurfaceBorder,
        BorderThickness = new Thickness(1),
        Padding = new Thickness(12),
        Child = child,
    };
}
