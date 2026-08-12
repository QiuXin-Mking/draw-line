using Avalonia.Controls;
using Avalonia.Layout;
using LeatherNesting.Desktop.ViewModels;

namespace LeatherNesting.Desktop.Views;

/// <summary>U4 CAD repair and process workbench. Single canvas with mutually exclusive tool modes.</summary>
public sealed class CadWorkbenchView : UserControl
{
    private readonly CadWorkbenchViewModel _viewModel;

    public CadWorkbenchView(CadWorkbenchViewModel viewModel)
    {
        _viewModel = viewModel;
        Content = BuildLayout();
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

        mainGrid.Children.Add(toolbar);
        Grid.SetRow(toolbar, 0);

        // Canvas area (placeholder for Stage 2 — real canvas rendering in later iteration)
        var canvas = new Border
        {
            Background = Avalonia.Media.Brushes.White,
            Child = new TextBlock
            {
                Text = FormatToolMode(_viewModel.ToolMode) + " — 画布（Stage 2 占位）",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        mainGrid.Children.Add(canvas);
        Grid.SetRow(canvas, 1);

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
        previewButton.Click += (_, _) => { /* Triggered by tool action */ };
        statusBar.Children.Add(previewButton);

        var commitButton = new Button
        {
            Content = "Commit",
            IsEnabled = _viewModel.CanCommit,
        };
        commitButton.Click += (_, _) => _viewModel.Commit();
        statusBar.Children.Add(commitButton);

        var cancelButton = new Button
        {
            Content = "Cancel",
            IsEnabled = _viewModel.CanCancel,
        };
        cancelButton.Click += (_, _) => _viewModel.Cancel();
        statusBar.Children.Add(cancelButton);

        var undoButton = new Button
        {
            Content = "Undo",
            IsEnabled = _viewModel.CanUndo,
        };
        undoButton.Click += (_, _) => _viewModel.Undo();
        statusBar.Children.Add(undoButton);

        var redoButton = new Button
        {
            Content = "Redo",
            IsEnabled = _viewModel.CanRedo,
        };
        redoButton.Click += (_, _) => _viewModel.Redo();
        statusBar.Children.Add(redoButton);

        mainGrid.Children.Add(statusBar);
        Grid.SetRow(statusBar, 2);

        return mainGrid;
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