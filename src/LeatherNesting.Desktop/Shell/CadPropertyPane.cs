using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Globalization;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Geometry.Offset;

namespace LeatherNesting.Desktop.Shell;

/// <summary>High-density image-10/21 CAD property controls for the shell's right host.</summary>
public sealed class CadPropertyPane : ScrollViewer
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _checks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TextBox> _editors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Button> _actions = new(StringComparer.Ordinal);
    private readonly TextBlock _sessionStatus = new() { FontSize = 9, Foreground = AppTheme.WarningText, TextWrapping = TextWrapping.Wrap };
    private RadioButton? _inside;
    private RadioButton? _outside;

    public CadPropertyPane(CadHostState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Background = AppTheme.PanelSurface;
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        var panel = new StackPanel { Spacing = 1, Margin = new Thickness(2) };

        AddSessionControls(panel, state);

        AddButton(panel, "自动组合", state);
        AddButton(panel, "全部拆解", state);
        AddSupportedButton(panel, "内缩生成线", () => PreviewOffset(state));
        AddValue(panel, "内缩值", "-8.00", enabled: true);
        AddValue(panel, "尖角处理", "圆形");
        _inside = AddChoice(panel, "内部", false, enabled: true);
        _outside = AddChoice(panel, "外部", true, enabled: true);
        AddCheck(panel, "剪口过滤", false);
        AddButton(panel, "修改线颜色", state);
        AddValue(panel, "缩放比例", "1.00");
        AddValue(panel, "曲线精度", "0.01");
        AddValue(panel, "连接容差", "0.05");
        AddValue(panel, "曲线光滑", "0.00");
        AddCheck(panel, "导入时修改线颜色", true);
        AddCheck(panel, "自动调整角度", false);
        AddLineCheck(panel, "所有线", false, "0", Colors.White);
        AddLineCheck(panel, "外部线", true, "0", Colors.White);
        AddLineCheck(panel, "文本", false, "6", Colors.Magenta);
        AddLineCheck(panel, "内部线", true, "3", Colors.LimeGreen);
        AddRangeCheck(panel, "冲孔1", true, "0.50", "1.50", "4", Colors.Cyan);
        AddRangeCheck(panel, "冲孔2", true, "1.60", "5.00", "5", Colors.Blue);
        AddCheck(panel, "自动信息识别", false);
        AddCheck(panel, "显示顺序方向", false);
        AddButton(panel, "选中内线", state);
        AddButton(panel, "选中外线", state);
        AddSupportedButton(panel, "清除选择", state.Workbench.ClearSelection);
        AddButton(panel, "做圆", state);
        AddSizeRange(panel, "0.0", "1.5", state);
        AddSizeRange(panel, "1.6", "2.0", state);
        AddResize(panel, state);
        AddButton(panel, "颜色线", state);

        Content = panel;
        state.Changed += (_, _) => RefreshActions(state);
        RefreshActions(state);
    }

    public IReadOnlyList<string> FieldLabels { get; private set; } = [];

    public string Value(string label) => _editors.TryGetValue(label, out var editor) ? editor.Text ?? string.Empty : _values[label];

    public bool IsChecked(string label) => _checks[label];

    public Button ActionButton(string label) => _actions[label];

    public TextBox Editor(string label) => _editors[label];

    private void AddLabel(string label) => FieldLabels = FieldLabels.Append(label).ToArray();

    private void AddButton(Panel panel, string label, CadHostState state)
    {
        AddLabel(label);
        var button = new Button { Content = label, FontSize = 10, Height = 22, Padding = new Thickness(4, 1), CornerRadius = new CornerRadius(0) };
        button.IsEnabled = false;
        ToolTip.SetTip(button, $"{label} · {TodoBadge.StandardText}");
        _actions[label] = button;
        panel.Children.Add(button);
    }

    private void AddValue(Panel panel, string label, string value, bool enabled = false)
    {
        AddLabel(label);
        _values[label] = value;
        var editor = new TextBox { Text = value, FontSize = 10, Height = 22, Padding = new Thickness(2, 0), IsEnabled = enabled };
        _editors[label] = editor;
        panel.Children.Add(Row(new TextBlock { Text = label, FontSize = 10 }, editor));
    }

    private RadioButton AddChoice(Panel panel, string label, bool selected, bool enabled = false)
    {
        AddLabel(label);
        _checks[label] = selected;
        var choice = new RadioButton { Content = label, IsChecked = selected, FontSize = 10, GroupName = "inset-side", IsEnabled = enabled };
        panel.Children.Add(choice);
        return choice;
    }

    private void AddCheck(Panel panel, string label, bool selected)
    {
        AddLabel(label);
        _checks[label] = selected;
        panel.Children.Add(new CheckBox { Content = label, IsChecked = selected, FontSize = 10, IsEnabled = false });
    }

    private void AddLineCheck(Panel panel, string label, bool selected, string colorIndex, Color color)
    {
        AddLabel(label);
        _checks[label] = selected;
        panel.Children.Add(Row(
            new CheckBox { Content = label, IsChecked = selected, FontSize = 10, IsEnabled = false },
            new Border { Width = 28, Height = 16, Background = new SolidColorBrush(color), Child = new TextBlock { Text = colorIndex, FontSize = 9, TextAlignment = TextAlignment.Center } }));
    }

    private void AddRangeCheck(Panel panel, string label, bool selected, string min, string max, string colorIndex, Color color)
    {
        AddLabel(label);
        _checks[label] = selected;
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                new CheckBox { Content = label, IsChecked = selected, FontSize = 10, IsEnabled = false },
                new TextBlock { Text = "范围", FontSize = 9, VerticalAlignment = VerticalAlignment.Center },
                MiniBox(min), new TextBlock { Text = "–", FontSize = 9 }, MiniBox(max),
                new Border { Width = 20, Height = 16, Background = new SolidColorBrush(color), Child = new TextBlock { Text = colorIndex, FontSize = 9, TextAlignment = TextAlignment.Center } },
            },
        });
    }

    private void AddSizeRange(Panel panel, string min, string max, CadHostState state)
    {
        AddLabel("最小尺寸");
        AddLabel("最大尺寸");
        var select = new Button { Content = "选择", FontSize = 9, Height = 21, Padding = new Thickness(3, 0) };
        select.IsEnabled = false;
        ToolTip.SetTip(select, $"按尺寸选择 · {TodoBadge.StandardText}");
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children = { new TextBlock { Text = "最小尺寸", FontSize = 9 }, MiniBox(min), new TextBlock { Text = "最大尺寸", FontSize = 9 }, MiniBox(max), select },
        });
    }

    private void AddResize(Panel panel, CadHostState state)
    {
        AddLabel("宽");
        AddLabel("高");
        AddLabel("调整大小");
        var resize = new Button { Content = "调整大小", FontSize = 9, Height = 21, Padding = new Thickness(3, 0) };
        resize.IsEnabled = false;
        ToolTip.SetTip(resize, $"调整大小 · {TodoBadge.StandardText}");
        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children = { new TextBlock { Text = "宽", FontSize = 9 }, MiniBox("100.00"), new TextBlock { Text = "高", FontSize = 9 }, MiniBox("100.00"), resize },
        });
    }

    private static Grid Row(Control first, Control second)
    {
        var row = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,76"), Children = { first, second } };
        Grid.SetColumn(second, 1);
        return row;
    }

    private static TextBox MiniBox(string text) => new() { Text = text, Width = 40, Height = 20, FontSize = 9, Padding = new Thickness(1, 0), IsEnabled = false };

    private void AddSessionControls(Panel panel, CadHostState state)
    {
        var controls = new StackPanel { Spacing = 2, Margin = new Thickness(1, 1, 1, 4) };
        controls.Children.Add(new TextBlock { Text = "CAD 会话", FontSize = 10, FontWeight = FontWeight.Bold, Foreground = AppTheme.PrimaryText });
        controls.Children.Add(_sessionStatus);
        AddSupportedButton(controls, "闭合轮廓", () =>
        {
            state.Workbench.SelectTool(CadToolMode.BoundaryRepair);
            state.Workbench.PreviewClose();
        }, addFieldLabel: false);
        AddSupportedButton(controls, "旋转 +15°", () => state.Workbench.RotateSelected(15), addFieldLabel: false);
        AddSupportedButton(controls, "提交到 CAD 会话", state.Workbench.Commit, addFieldLabel: false);
        AddSupportedButton(controls, "取消预览", state.Workbench.Cancel, addFieldLabel: false);
        AddSupportedButton(controls, "撤销", state.Workbench.Undo, addFieldLabel: false);
        AddSupportedButton(controls, "重做", state.Workbench.Redo, addFieldLabel: false);
        panel.Children.Add(new Border
        {
            BorderBrush = AppTheme.ClassicBorderNeutral,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(3),
            Child = controls,
        });
    }

    private void AddSupportedButton(Panel panel, string label, Action action, bool addFieldLabel = true)
    {
        if (addFieldLabel)
            AddLabel(label);
        var button = new Button { Content = label, FontSize = 10, Height = 22, Padding = new Thickness(4, 1), CornerRadius = new CornerRadius(0) };
        button.Click += (_, _) => action();
        _actions[label] = button;
        panel.Children.Add(button);
    }

    private void PreviewOffset(CadHostState state)
    {
        var text = Editor("内缩值").Text;
        if (!TryParseFiniteDistance(text, out var distance) || distance == 0)
        {
            state.ReportError("内缩值必须是非零有限数值（mm）。");
            return;
        }

        state.Workbench.SelectTool(CadToolMode.Offset);
        state.Workbench.PreviewOffset(
            Math.Abs(distance),
            _outside?.IsChecked == true ? OffsetDirection.Outside : OffsetDirection.Inside);
    }

    private static bool TryParseFiniteDistance(string? text, out double distance)
    {
        var parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out distance)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out distance);
        return parsed && double.IsFinite(distance);
    }

    private void RefreshActions(CadHostState state)
    {
        var workbench = state.Workbench;
        var readyWithGeometry = workbench.CanPreview && state.Loops.Count > 0;
        _actions["闭合轮廓"].IsEnabled = readyWithGeometry;
        _actions["内缩生成线"].IsEnabled = readyWithGeometry;
        _actions["旋转 +15°"].IsEnabled = readyWithGeometry && workbench.SelectedLoopId is not null;
        _actions["清除选择"].IsEnabled = !workbench.CanCancel && workbench.SelectedLoopId is not null;
        _actions["提交到 CAD 会话"].IsEnabled = workbench.CanCommit;
        _actions["取消预览"].IsEnabled = workbench.CanCancel;
        _actions["撤销"].IsEnabled = !workbench.CanCancel && workbench.CanUndo;
        _actions["重做"].IsEnabled = !workbench.CanCancel && workbench.CanRedo;
        _editors["内缩值"].IsEnabled = readyWithGeometry;
        if (_inside is not null)
            _inside.IsEnabled = readyWithGeometry;
        if (_outside is not null)
            _outside.IsEnabled = readyWithGeometry;
        _sessionStatus.Text = state.StatusMessage;
    }
}
