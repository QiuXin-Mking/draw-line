using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;

namespace LeatherNesting.Desktop.Shell;

/// <summary>High-density image-10/21 CAD property controls for the shell's right host.</summary>
public sealed class CadPropertyPane : ScrollViewer
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _checks = new(StringComparer.Ordinal);

    public CadPropertyPane(CadHostState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        var panel = new StackPanel { Spacing = 1, Margin = new Thickness(2) };

        AddButton(panel, "自动组合", state);
        AddButton(panel, "全部拆解", state);
        AddButton(panel, "内缩生成线", state);
        AddValue(panel, "内缩值", "-8.00");
        AddValue(panel, "尖角处理", "圆形");
        AddChoice(panel, "内部", false);
        AddChoice(panel, "外部", true);
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
        AddButton(panel, "清除选择", state);
        AddButton(panel, "做圆", state);
        AddSizeRange(panel, "0.0", "1.5", state);
        AddSizeRange(panel, "1.6", "2.0", state);
        AddResize(panel, state);
        AddButton(panel, "颜色线", state);

        Content = panel;
    }

    public IReadOnlyList<string> FieldLabels { get; private set; } = [];

    public string Value(string label) => _values[label];

    public bool IsChecked(string label) => _checks[label];

    private void AddLabel(string label) => FieldLabels = FieldLabels.Append(label).ToArray();

    private void AddButton(Panel panel, string label, CadHostState state)
    {
        AddLabel(label);
        var button = new Button { Content = label, FontSize = 10, Height = 22, Padding = new Thickness(4, 1), CornerRadius = new CornerRadius(0) };
        button.Click += (_, _) => state.ReportUnsupported(label);
        panel.Children.Add(button);
    }

    private void AddValue(Panel panel, string label, string value)
    {
        AddLabel(label);
        _values[label] = value;
        panel.Children.Add(Row(new TextBlock { Text = label, FontSize = 10 }, new TextBox { Text = value, FontSize = 10, Height = 22, Padding = new Thickness(2, 0) }));
    }

    private void AddChoice(Panel panel, string label, bool selected)
    {
        AddLabel(label);
        _checks[label] = selected;
        panel.Children.Add(new RadioButton { Content = label, IsChecked = selected, FontSize = 10, GroupName = "inset-side" });
    }

    private void AddCheck(Panel panel, string label, bool selected)
    {
        AddLabel(label);
        _checks[label] = selected;
        panel.Children.Add(new CheckBox { Content = label, IsChecked = selected, FontSize = 10 });
    }

    private void AddLineCheck(Panel panel, string label, bool selected, string colorIndex, Color color)
    {
        AddLabel(label);
        _checks[label] = selected;
        panel.Children.Add(Row(
            new CheckBox { Content = label, IsChecked = selected, FontSize = 10 },
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
                new CheckBox { Content = label, IsChecked = selected, FontSize = 10 },
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
        select.Click += (_, _) => state.ReportUnsupported("按尺寸选择");
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
        resize.Click += (_, _) => state.ReportUnsupported("调整大小");
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

    private static TextBox MiniBox(string text) => new() { Text = text, Width = 40, Height = 20, FontSize = 9, Padding = new Thickness(1, 0) };
}
