using Avalonia.Controls;
using Avalonia.Layout;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Geometry.Offset;

namespace LeatherNesting.Desktop.Views;

/// <summary>U4 CAD repair and process workbench. Single canvas with mutually exclusive tool modes.</summary>
public sealed class CadWorkbenchView : UserControl
{
    private readonly CadWorkbenchViewModel _viewModel;
    private readonly CanvasView _canvas = new();

    public CadWorkbenchView(CadWorkbenchViewModel viewModel)
    {
        _viewModel = viewModel;
        Content = BuildLayout();
        RefreshCanvas();
        _canvas.OnClick = point =>
        {
            _viewModel.SelectPiece(point);
            _canvas.SelectedLoopId = _viewModel.SelectedLoopId;
            _canvas.InvalidateVisual();
        };
        _canvas.OnDrag = delta =>
        {
            _viewModel.MoveSelected(delta);
            _canvas.SetData(_viewModel.CurrentLoops, refit: false);
        };
    }

    private Control BuildLayout()
    {
        var mainGrid = new Grid
        {
            ColumnDefinitions = new("*"),
            RowDefinitions = new("Auto,*,Auto"),
        };

        // Toolbar
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new(4),
        };

        foreach (var mode in Enum.GetValues<CadToolMode>())
        {
            var button = new Button
            {
                Content = FormatToolMode(mode),
                [!Button.IsEnabledProperty] = new Avalonia.Data.Binding("!IsPreviewing"),
            };
            button.Click += (_, _) => _viewModel.SelectTool(mode);
            toolbar.Children.Add(button);
        }

        var offsetDistance = new TextBox { Text = "1", Width = 70, PlaceholderText = "offset mm" };
        toolbar.Children.Add(offsetDistance);

        mainGrid.Children.Add(toolbar);
        Grid.SetRow(toolbar, 0);

        // Canvas area
        mainGrid.Children.Add(_canvas);
        Grid.SetRow(_canvas, 1);

        // Status bar
        var statusBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new(4),
        };

        var stateLabel = new TextBlock { Text = FormatState(_viewModel.State) };
        statusBar.Children.Add(stateLabel);

        var previewButton = new Button
        {
            Content = "Preview",
            IsEnabled = _viewModel.CanPreview,
        };
        previewButton.Click += (_, _) => { PreviewByTool(offsetDistance); RefreshCanvas(); };
        statusBar.Children.Add(previewButton);

        var commitButton = new Button
        {
            Content = "Commit",
            IsEnabled = _viewModel.CanCommit,
        };
        commitButton.Click += (_, _) => { _viewModel.Commit(); RefreshCanvas(); };
        statusBar.Children.Add(commitButton);

        var cancelButton = new Button
        {
            Content = "Cancel",
            IsEnabled = _viewModel.CanCancel,
        };
        cancelButton.Click += (_, _) => { _viewModel.Cancel(); RefreshCanvas(); };
        statusBar.Children.Add(cancelButton);

        var undoButton = new Button
        {
            Content = "Undo",
            IsEnabled = _viewModel.CanUndo,
        };
        undoButton.Click += (_, _) => { _viewModel.Undo(); RefreshCanvas(); };
        statusBar.Children.Add(undoButton);

        var redoButton = new Button
        {
            Content = "Redo",
            IsEnabled = _viewModel.CanRedo,
        };
        redoButton.Click += (_, _) => { _viewModel.Redo(); RefreshCanvas(); };
        statusBar.Children.Add(redoButton);

        var rotateButton = new Button { Content = "旋转 +15°" };
        rotateButton.Click += (_, _) => { _viewModel.RotateSelected(15); RefreshCanvas(); };
        statusBar.Children.Add(rotateButton);

        mainGrid.Children.Add(statusBar);
        Grid.SetRow(statusBar, 2);

        return mainGrid;
    }

    private void RefreshCanvas() => _canvas.SetData(_viewModel.CurrentLoops);

    private void PreviewByTool(TextBox offsetDistance)
    {
        switch (_viewModel.ToolMode)
        {
            case CadToolMode.BoundaryRepair:
                _viewModel.PreviewClose();
                break;
            case CadToolMode.Offset:
                if (double.TryParse(offsetDistance.Text, out var distance))
                    _viewModel.PreviewOffset(distance, OffsetDirection.Inside);
                break;
        }
    }

    private static string FormatToolMode(CadToolMode mode) => mode switch
    {
        CadToolMode.Select => "选择",
        CadToolMode.BoundaryRepair => "边界修复",
        CadToolMode.Offset => "内缩/外扩",
        CadToolMode.NodeEdit => "节点编辑",
        CadToolMode.Break => "剪断",
        CadToolMode.Notch => "剪口",
        _ => mode.ToString(),
    };

    private static string FormatState(WorkbenchState state) => state switch
    {
        WorkbenchState.Ready => "就绪",
        WorkbenchState.Previewing => "预览中",
        WorkbenchState.Committed => "已提交",
        _ => state.ToString(),
    };
}