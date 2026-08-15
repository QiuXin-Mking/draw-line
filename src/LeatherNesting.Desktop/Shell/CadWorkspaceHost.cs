using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Desktop.Views;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Compact CAD controls contributed to the shell's existing centre host.</summary>
public sealed class CadWorkspaceHost : Grid
{
    private readonly CadHostState _state;
    private readonly Action<ShellMenuCommand> _activateContext;
    private readonly TextBlock _fileName = new() { FontSize = 10, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _status = new() { FontSize = 10, Foreground = AppTheme.TodoAmber, IsHitTestVisible = false };
    private readonly TextBlock _coordinates = new()
    {
        Foreground = AppTheme.CadCoordinateText,
        FontSize = 11,
        Margin = new Thickness(8, 7),
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        IsHitTestVisible = false,
        Text = string.Empty,
    };
    private bool _hasRefittedData;

    public CadWorkspaceHost(CadHostState state, Action? requestImport = null, Action<ShellMenuCommand>? activateContext = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _activateContext = activateContext ?? (_ => { });
        Drawing = new CanvasView
        {
            CanvasBrush = AppTheme.CanvasBlack,
            OuterContourPen = new Pen(AppTheme.GeometryOuterContour, 1.5),
            InternalContourPen = new Pen(AppTheme.GeometryInternalLine, 1.5),
            SelectionPen = new Pen(AppTheme.ClassicFocus, 3),
        };
        Drawing.OnClick = point =>
        {
            if (_state.Workbench.CanPreview)
                _state.Workbench.SelectPiece(point);
        };
        Drawing.OnDrag = delta =>
        {
            if (_state.Workbench.CanPreview && _state.Workbench.SelectedLoopId is not null)
                _state.Workbench.MoveSelected(delta);
        };
        Drawing.ContextMenu = BuildContextMenu();
        Drawing.PointerMoved += OnDrawingPointerMoved;
        Drawing.PointerExited += OnDrawingPointerExited;
        FileOperationButtons = BuildFileRow(requestImport);
        DrawingToolButtons = BuildToolRow();
        Axes = new CadOriginAxes(Drawing) { IsHitTestVisible = false };
        Canvas = new Border
        {
            Background = AppTheme.CanvasBlack,
            Child = new Grid { Children = { Drawing, Axes, _coordinates, _status } },
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

    public CanvasView Drawing { get; }

    public CadOriginAxes Axes { get; }

    /// <summary>Top-left coordinate readout text (empty when the pointer is outside the canvas).</summary>
    public string CoordinateText => _coordinates.Text ?? string.Empty;

    private ContextMenu BuildContextMenu()
    {
        var items = new List<MenuItem>();
        foreach (var entry in ShellContextMenu.Entries)
        {
            if (entry is not ShellMenuCommand command)
                continue;
            var item = new MenuItem
            {
                Header = command.Label,
                Foreground = AppTheme.PrimaryText,
                IsEnabled = command.IsEnabled,
            };
            item.Click += (_, _) => _activateContext(command);
            items.Add(item);
        }

        return new ContextMenu { ItemsSource = items };
    }

    /// <summary>Updates the coordinate readout for a model-space pointer position.</summary>
    public void UpdateCoordinates(Point2D model)
    {
        _coordinates.Text = $"X {model.X:F2} mm · Y {model.Y:F2} mm";
    }

    private void OnDrawingPointerMoved(object? sender, PointerEventArgs e)
    {
        UpdateCoordinates(Drawing.ToModel(e.GetPosition(Drawing)));
    }

    private void OnDrawingPointerExited(object? sender, PointerEventArgs e)
    {
        _coordinates.Text = string.Empty;
    }

    private IReadOnlyList<Button> BuildFileRow(Action? requestImport)
    {
        var newFile = UnsupportedFileButton("新建文件");
        var open = requestImport is null
            ? UnsupportedFileButton("打开文件")
            : FileButton("打开文件", requestImport);
        var saveAs = UnsupportedFileButton("另存为");
        var replace = UnsupportedFileButton("替换皮料");
        var name = FileButton("未打开文件", () => { });
        name.IsVisible = false;
        var close = FileButton("关闭", _state.Clear);
        return [newFile, open, saveAs, replace, name, close];
    }

    private IReadOnlyList<Button> BuildToolRow() =>
    [
        ToolButton("范围缩放", "⌗", Drawing.Refit),
        UnsupportedToolButton("绘制多段线", "／"),
        UnsupportedToolButton("绘制矩形", "□"),
        ToolButton("选择", "↖", () =>
        {
            _state.Workbench.SelectTool(CadToolMode.Select);
            _state.ReportError("选择模式：单击选中轮廓，拖动创建移动预览。");
        }),
        UnsupportedToolButton("删除", "×"),
    ];

    private static Button UnsupportedFileButton(string label)
    {
        var button = FileButton(label, () => { });
        button.IsEnabled = false;
        ToolTip.SetTip(button, $"{label} · {TodoBadge.StandardText}");
        return button;
    }

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

    private static Button UnsupportedToolButton(string tip, string mark)
    {
        var button = ToolButton($"{tip} · {TodoBadge.StandardText}", mark, () => { });
        button.IsEnabled = false;
        return button;
    }

    private void Refresh()
    {
        _fileName.Text = $"  {_state.FileName}  ";
        _status.Text = _state.StatusMessage;
        Drawing.SelectedLoopId = _state.Workbench.SelectedLoopId;
        Drawing.SetData(_state.Loops, refit: !_hasRefittedData);
        DrawingToolButtons[3].IsEnabled = _state.Workbench.CanPreview && _state.Loops.Count > 0;
        _hasRefittedData = _state.Loops.Count > 0;
    }
}
