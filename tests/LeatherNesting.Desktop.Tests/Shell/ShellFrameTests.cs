using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class ShellFrameTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-001")]
    public void Shell_keeps_the_image_27_five_region_geometry_in_one_body()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        Assert.Equal([GridUnitType.Star, GridUnitType.Star, GridUnitType.Star],
            shell.BodyGrid.ColumnDefinitions.Select(column => column.Width.GridUnitType));
        Assert.Equal([13d, 74d, 13d],
            shell.BodyGrid.ColumnDefinitions.Select(column => column.Width.Value));

        Assert.Equal([20d, 60d, 20d],
            shell.LeftRail.RowDefinitions.Select(row => row.Height.Value));
        Assert.Equal([62d, 38d],
            shell.RightRail.RowDefinitions.Select(row => row.Height.Value));

        Assert.Equal((0, 0), Position(shell.OrderGroupHost));
        Assert.Equal((0, 1), Position(shell.PieceListHost));
        Assert.Equal((0, 2), Position(shell.ProgressSummaryHost));
        Assert.Equal((0, 0), Position(shell.LayoutCandidateHost));
        Assert.Equal((0, 1), Position(shell.OutputInformationHost));
        Assert.Equal((1, 0), Position(shell.CanvasSurface));
        Assert.Same(shell.BodyGrid, Assert.IsType<Grid>(shell.BodyGrid.Parent).Children[0]);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-002")]
    public void Shell_hosts_are_compact_bordered_and_evidence_labelled_without_claiming_live_results()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        Assert.Equal("订单组", shell.OrderGroupHost.Title);
        Assert.Equal("裁片列表 · DEMO", shell.PieceListHost.Title);
        Assert.Equal("进度汇总 · DEMO", shell.ProgressSummaryHost.Title);
        Assert.Equal("CAD 参数", shell.LayoutCandidateHost.Title);
        Assert.Equal("排版输出信息 · DEMO", shell.OutputInformationHost.Title);

        Assert.All(shell.PersistentPaneHosts, host =>
        {
            Assert.Equal(new Thickness(1), host.BorderThickness);
            Assert.Equal(AppTheme.ClassicBorder, host.BorderBrush);
            Assert.Equal(AppTheme.ClassicHeaderHeight, host.Header.Height);
            Assert.True(host.Padding.Left <= 4);
        });
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-003")]
    public void Center_workspace_is_a_black_canvas_with_left_and_bottom_rulers()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        Assert.Equal(AppTheme.CadCanvasBackground, shell.CanvasSurface.Background);
        Assert.Equal(22, shell.VerticalRuler.Width);
        Assert.Equal(20, shell.HorizontalRuler.Height);
        Assert.Equal(0, Grid.GetColumn(shell.VerticalRuler));
        Assert.Equal(0, Grid.GetRow(shell.VerticalRuler));
        Assert.Equal(1, Grid.GetColumn(shell.HorizontalRuler));
        Assert.Equal(1, Grid.GetRow(shell.HorizontalRuler));
        Assert.Equal(1, Grid.GetColumn(shell.CanvasSurface));
        Assert.Equal(0, Grid.GetRow(shell.CanvasSurface));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-004")]
    public void Shell_top_and_status_rows_keep_product_identity_and_operator_fields()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        Assert.Equal("LeatherNesting 卷料智能排样系统", shell.TopCommands.ProductTitle.Text);
        Assert.DoesNotContain("AXTNester", shell.TopCommands.ProductTitle.Text);
        Assert.Equal("单位：米(m)", shell.TopCommands.UnitSelector.SelectedItem);
        Assert.Equal("演示员", shell.TopCommands.OperatorText.Text);
        Assert.Contains("DEMO", shell.StatusDemoText.Text);
        Assert.Equal(AppTheme.StatusBarHeight, shell.StatusBar.Height);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-005")]
    public void Center_canvas_does_not_embed_the_full_M03_workbench_inside_the_shell_frame()
    {
        var viewModel = DesktopComposition.CreateShellViewModel();
        var shell = new AppShellView(viewModel);

        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.IsType<CadCanvasView>(shell.WorkspaceContent.Content);
        Assert.False(shell.WorkspaceContent.IsVisible);
        Assert.NotSame(shell.WorkspaceContent, shell.CanvasSurface.Child);
        Assert.IsNotType<CadCanvasView>(shell.CanvasSurface.Child);
        Assert.DoesNotContain(Descendants(shell.CanvasSurface), control => control is ScrollViewer);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-006")]
    public void Left_rail_strip_is_a_persistent_left_edge_column_and_the_rail_starts_expanded()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        Assert.NotNull(shell.LeftRailStrip);
        Assert.NotNull(shell.LeftRailToggle);
        Assert.False(shell.IsLeftRailCollapsed);
        Assert.True(shell.LeftRail.IsVisible);
        Assert.Equal(13, shell.LeftRailColumn.Width.Value);
        Assert.Equal(GridUnitType.Star, shell.LeftRailColumn.Width.GridUnitType);
        Assert.Equal("◀", Assert.IsType<TextBlock>(shell.LeftRailToggle.Content).Text);
        Assert.Equal((0, 1), (Grid.GetColumn(shell.LeftRailStrip), Grid.GetRow(shell.LeftRailStrip)));
        Assert.DoesNotContain(Descendants(shell.BodyGrid), control => ReferenceEquals(control, shell.LeftRailStrip));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-007")]
    public void Left_rail_toggle_collapses_the_rail_to_the_left_edge_and_restores_it()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        shell.ToggleLeftRail();

        Assert.True(shell.IsLeftRailCollapsed);
        Assert.False(shell.LeftRail.IsVisible);
        Assert.Equal(0, shell.LeftRailColumn.Width.Value);
        Assert.Equal("▶", Assert.IsType<TextBlock>(shell.LeftRailToggle.Content).Text);

        shell.ToggleLeftRail();

        Assert.False(shell.IsLeftRailCollapsed);
        Assert.True(shell.LeftRail.IsVisible);
        Assert.Equal(13, shell.LeftRailColumn.Width.Value);
        Assert.Equal(GridUnitType.Star, shell.LeftRailColumn.Width.GridUnitType);
        Assert.Equal("◀", Assert.IsType<TextBlock>(shell.LeftRailToggle.Content).Text);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "FRAME-008")]
    public void Top_and_status_bars_span_the_whole_window_so_the_workspace_column_is_not_squeezed()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());
        var layout = Assert.IsType<Grid>(shell.Content);
        var workspace = Assert.IsType<Grid>(layout.Children[2]);

        // 外层 Grid 是 Auto(细条),* 两列：顶栏与状态栏必须横跨两列，
        // 否则会被 Auto 列（14px 细条）吃掉宽度，把 * 列（bodyLayer）挤没。
        Assert.Equal(2, Grid.GetColumnSpan(shell.TopCommands));
        Assert.Equal(2, Grid.GetColumnSpan(shell.StatusBar));
        Assert.Same(shell.BodyGrid, workspace.Children[0]);
        Assert.Equal(1, Grid.GetColumn(workspace));
        Assert.Equal(GridUnitType.Star, layout.ColumnDefinitions[1].Width.GridUnitType);
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (var child in root.GetVisualChildren().OfType<Control>())
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }

    private static (int Column, int Row) Position(Control control) =>
        (Grid.GetColumn(control), Grid.GetRow(control));
}
