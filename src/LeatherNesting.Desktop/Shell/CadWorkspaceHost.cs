using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Views;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Compact CAD controls contributed to the shell's existing centre host.</summary>
public sealed class CadWorkspaceHost : Grid
{
    private readonly CadHostState _state;
    private readonly TextBlock _fileName = new() { FontSize = 10, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _status = new() { FontSize = 10, Foreground = AppTheme.TodoAmber };
    private readonly CadEvidenceCanvas _drawing = new();

    public CadWorkspaceHost(CadHostState state, Action? requestImport = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        FileOperationButtons = BuildFileRow(requestImport);
        DrawingToolButtons = BuildToolRow();
        Canvas = new Border
        {
            Background = AppTheme.CanvasBlack,
            Child = new Grid { Children = { _drawing, BuildAxes(), _status } },
        };
        _status.Margin = new Thickness(6);
        _status.VerticalAlignment = VerticalAlignment.Bottom;

        RowDefinitions = RowDefinitions.Parse("24,25,*");
        Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Background = AppTheme.PanelSurface,
            Children = { FileOperationButtons[0], FileOperationButtons[1], FileOperationButtons[2], FileOperationButtons[3], _fileName, FileOperationButtons[5] },
        });
        Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Background = AppTheme.HeaderSurface,
            Children = { DrawingToolButtons[0], DrawingToolButtons[1], DrawingToolButtons[2], DrawingToolButtons[3], DrawingToolButtons[4] },
        });
        Children.Add(Canvas);
        Grid.SetRow(Children[1], 1);
        Grid.SetRow(Canvas, 2);

        _state.Changed += (_, _) => Refresh();
        Refresh();
    }

    public IReadOnlyList<Button> FileOperationButtons { get; }

    public IReadOnlyList<Button> DrawingToolButtons { get; }

    public Border Canvas { get; }

    private IReadOnlyList<Button> BuildFileRow(Action? requestImport)
    {
        var newFile = FileButton("新建文件", () => _state.ReportUnsupported("新建文件"));
        var open = FileButton("打开文件", requestImport ?? (() => _state.ReportUnsupported("打开文件对话框")));
        var saveAs = FileButton("另存为", () => _state.ReportUnsupported("另存为"));
        var replace = FileButton("替换皮料", () => _state.ReportUnsupported("替换皮料"));
        var name = FileButton("未打开文件", () => { });
        name.IsVisible = false;
        var close = FileButton("关闭", _state.Clear);
        return [newFile, open, saveAs, replace, name, close];
    }

    private IReadOnlyList<Button> BuildToolRow() =>
    [
        ToolButton("范围缩放", "⌗", () => _drawing.Refit()),
        ToolButton("绘制多段线", "／", () => _state.ReportUnsupported("绘制多段线")),
        ToolButton("绘制矩形", "□", () => _state.ReportUnsupported("绘制矩形")),
        ToolButton("选择", "↖", () => _state.ReportUnsupported("CAD 选择")),
        ToolButton("删除", "×", () => _state.ReportUnsupported("删除对象")),
    ];

    private static Button FileButton(string label, Action action)
    {
        var button = new Button
        {
            Content = label,
            FontSize = 10,
            MinWidth = 0,
            Padding = new Thickness(6, 1),
            CornerRadius = new CornerRadius(0),
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button ToolButton(string tip, string mark, Action action)
    {
        var button = new Button
        {
            Content = mark,
            Width = 24,
            Height = 24,
            Padding = new Thickness(1),
            CornerRadius = new CornerRadius(0),
        };
        ToolTip.SetTip(button, tip);
        button.Click += (_, _) => action();
        return button;
    }

    private static Control BuildAxes() => new TextBlock
    {
        Text = "+X\n│\n└── +Y",
        Foreground = AppTheme.MaterialBoundary,
        FontSize = 9,
        Margin = new Thickness(8, 7),
        IsHitTestVisible = false,
    };

    private void Refresh()
    {
        _fileName.Text = $"  {_state.FileName}  ";
        _status.Text = _state.StatusMessage;
        _drawing.SetData(_state.Loops);
    }
}
