using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using LeatherNesting.Application;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Modules.Import;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Desktop.Views;
using LeatherNesting.Desktop.Workspace;
using LeatherNesting.Domain;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class CadHostEvidenceTests
{
    [Fact]
    [Trait("TestId", "CAD-HOST-001")]
    public void Fixed_canvas_host_keeps_evidenced_file_and_drawing_tool_order()
    {
        var state = new CadHostState();
        var host = new CadWorkspaceHost(state);

        Assert.Equal(
            ["新建文件", "打开文件", "另存为", "替换皮料", "未打开文件", "关闭"],
            host.FileOperationButtons.Select(button => button.Content));
        Assert.Equal(
            ["范围缩放", $"绘制多段线 · {TodoBadge.StandardText}", $"绘制矩形 · {TodoBadge.StandardText}"],
            host.DrawingToolButtons.Take(3).Select(ToolTip.GetTip));
        Assert.All(host.DrawingToolButtons, button => Assert.NotNull(ToolTip.GetTip(button)));
        Assert.Equal(24, host.DrawingToolButtons[0].Width);
        Assert.Equal(24, host.DrawingToolButtons[0].Height);
        Assert.Same(AppTheme.CadCanvasBackground, host.Canvas.Background);
        Assert.False(host.FileOperationButtons[1].IsEnabled);
        Assert.Contains(TodoBadge.StandardText, ToolTip.GetTip(host.FileOperationButtons[1])?.ToString());
    }

    [Fact]
    [Trait("TestId", "CAD-HOST-001A")]
    public void Cad_import_route_opens_as_an_overlay_without_replacing_the_five_pane_body()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [new TestModule("M02", 2), new TestModule("M03", 3)],
            workspace,
            workspace);
        var shell = new AppShellView(viewModel);

        shell.TopCommands.CommandButtons.Single(button => button.Descriptor.Label == "CAD工具")
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.True(shell.WorkspaceContent.IsVisible);
        Assert.IsType<Border>(shell.WorkspaceContent.Content);
        Assert.Same(shell.BodyGrid, Assert.IsType<Grid>(shell.BodyGrid.Parent).Children[0]);
    }

    [Fact]
    [Trait("TestId", "CAD-HOST-002")]
    public void Unsupported_drawing_tools_are_disabled_and_marked_todo()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        var host = new CadWorkspaceHost(state);

        var rectangle = host.DrawingToolButtons.Single(button => Equals(ToolTip.GetTip(button), $"绘制矩形 · {TodoBadge.StandardText}"));

        Assert.False(rectangle.IsEnabled);
        Assert.Equal("40.DXF", state.FileName);
    }

    [Fact]
    [Trait("TestId", "CAD-HOST-003")]
    public void Right_property_pane_keeps_evidenced_field_order_defaults_and_checks()
    {
        var pane = new CadPropertyPane(new CadHostState());

        Assert.Equal(
            [
                "自动组合", "全部拆解", "内缩生成线", "内缩值", "尖角处理", "内部", "外部",
                "剪口过滤", "修改线颜色", "缩放比例", "曲线精度", "连接容差", "曲线光滑",
                "导入时修改线颜色", "自动调整角度", "所有线", "外部线", "文本", "内部线",
                "冲孔1", "冲孔2", "自动信息识别", "显示顺序方向", "选中内线", "选中外线",
                "清除选择", "做圆", "最小尺寸", "最大尺寸", "最小尺寸", "最大尺寸",
                "宽", "高", "调整大小", "颜色线",
            ],
            pane.FieldLabels);
        Assert.Equal("-8.00", pane.Value("内缩值"));
        Assert.Equal("圆形", pane.Value("尖角处理"));
        Assert.Equal("1.00", pane.Value("缩放比例"));
        Assert.Equal("0.01", pane.Value("曲线精度"));
        Assert.Equal("0.05", pane.Value("连接容差"));
        Assert.Equal("0.00", pane.Value("曲线光滑"));
        Assert.True(pane.IsChecked("外部"));
        Assert.True(pane.IsChecked("导入时修改线颜色"));
        Assert.True(pane.IsChecked("外部线"));
        Assert.True(pane.IsChecked("内部线"));
        Assert.True(pane.IsChecked("冲孔1"));
        Assert.True(pane.IsChecked("冲孔2"));
        Assert.False(pane.Editor("内缩值").IsEnabled);
        Assert.False(pane.Editor("缩放比例").IsEnabled);
    }

    [Fact]
    [Trait("TestId", "CAD-HOST-004")]
    public async Task Confirmed_m02_inspection_publishes_real_geometry_to_shared_cad_host()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "dxf-source");
        try
        {
            var workspace = new InMemoryWorkspaceSession();
            var cad = new CadHostState();
            var loops = new[] { Rectangle() };
            var coordinator = new ImportCoordinator(
                new ImportDxfUseCase(new StubDxfReader()),
                new StubStore(),
                new StubGeometryReader(loops),
                workspace,
                workspace,
                cadHost: cad);
            coordinator.CreateProject("测试");

            await coordinator.InspectAsync(path, CancellationToken.None);
            Assert.Empty(cad.Loops);

            var shellViewModel = new AppShellViewModel(
                [new TestModule("M02", 2), new TestModule("M03", 3)],
                workspace,
                workspace,
                cad);
            var shell = new AppShellView(shellViewModel);
            shell.TopCommands.CommandButtons.Single(button => button.Descriptor.Label == "CAD工具")
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            coordinator.ConfirmMillimetres();

            Assert.Equal(Path.GetFileName(path), cad.FileName);
            Assert.Same(loops[0], Assert.Single(cad.Loops));
            Assert.False(cad.IsDemoGeometry);
            Assert.Equal("M03", shellViewModel.CurrentModule?.Id);
            Assert.False(shell.WorkspaceContent.IsVisible);
            Assert.Same(cad.Loops, shell.CadWorkspace.Drawing.Loops);

            var originalCentroid = cad.Loops[0].Centroid;
            shell.CadWorkspace.Drawing.OnClick!(new Point2D(10, 10));
            shell.CadWorkspace.Drawing.OnDrag!(new Point2D(5, 0));
            cad.Workbench.Commit();
            Assert.Equal(originalCentroid.X + 5, cad.Loops[0].Centroid.X, 6);
            cad.Workbench.Undo();
            Assert.Equal(originalCentroid.X, cad.Loops[0].Centroid.X, 6);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Confirmed_import_is_loaded_into_the_single_shared_workbench_session()
    {
        var state = new CadHostState();

        state.LoadConfirmedImport("40.DXF", [Rectangle()]);

        Assert.Same(state.Workbench.CurrentLoops, state.Loops);
        Assert.Same(Assert.Single(state.Loops), Assert.Single(state.Workbench.CurrentLoops!));
    }

    [Fact]
    public void Workbench_changes_refresh_the_host_projection_and_clear_resets_it()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        var changes = 0;
        state.Changed += (_, _) => changes++;

        state.Workbench.SelectPiece(new Point2D(10, 10));
        state.Workbench.MoveSelected(new Point2D(5, 0));

        Assert.Equal(2, changes);
        Assert.Equal(WorkbenchState.Previewing, state.Workbench.State);
        Assert.Contains("预览", state.StatusMessage);

        state.Clear();

        Assert.Equal(3, changes);
        Assert.Empty(state.Loops);
        Assert.Null(state.Workbench.SelectedLoopId);
        Assert.Equal(WorkbenchState.Ready, state.Workbench.State);
        Assert.Equal("请选择 DXF 文件并确认毫米单位。", state.StatusMessage);
    }

    [Fact]
    public void Host_report_error_preserves_the_current_session_geometry()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        var before = state.Loops;

        state.ReportError("参数无效。");

        Assert.Same(before, state.Loops);
        Assert.Equal("参数无效。", state.StatusMessage);
    }

    [Fact]
    public void Fixed_host_uses_the_interactive_canvas_and_shared_workbench_callbacks()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        var host = new CadWorkspaceHost(state);

        Assert.IsType<CanvasView>(host.Drawing);
        Assert.Same(state.Loops, host.Drawing.Loops);
        Assert.True(host.DrawingToolButtons[3].IsEnabled);

        host.Drawing.OnClick!(new Point2D(10, 10));
        Assert.Equal("imported", state.Workbench.SelectedLoopId);
        Assert.Equal("imported", host.Drawing.SelectedLoopId);

        host.Drawing.OnDrag!(new Point2D(5, 0));
        Assert.Equal(WorkbenchState.Previewing, state.Workbench.State);
        Assert.True(state.Workbench.CanCommit);
    }

    [Fact]
    public void Fixed_host_blocks_selection_and_drag_mutations_while_a_preview_is_pending()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        var host = new CadWorkspaceHost(state);
        host.Drawing.OnClick!(new Point2D(10, 10));
        host.Drawing.OnDrag!(new Point2D(5, 0));
        var preview = state.Loops;
        var selected = state.Workbench.SelectedLoopId;

        host.Drawing.OnClick!(new Point2D(500, 500));
        host.Drawing.OnDrag!(new Point2D(5, 0));

        Assert.Same(preview, state.Loops);
        Assert.Equal(selected, state.Workbench.SelectedLoopId);
        Assert.Equal(WorkbenchState.Previewing, state.Workbench.State);
        Assert.False(host.DrawingToolButtons[3].IsEnabled);
    }

    [Fact]
    public void Property_pane_drives_preview_cancel_commit_undo_and_redo_on_the_shared_session()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("open.DXF", [OpenContour()]);
        var pane = new CadPropertyPane(state);

        pane.ActionButton("闭合轮廓").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(WorkbenchState.Previewing, state.Workbench.State);
        Assert.Equal(5, state.Loops[0].Curves.Count);
        Assert.True(pane.ActionButton("提交到 CAD 会话").IsEnabled);
        Assert.True(pane.ActionButton("取消预览").IsEnabled);
        Assert.False(pane.Editor("内缩值").IsEnabled);

        pane.ActionButton("取消预览").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(4, state.Loops[0].Curves.Count);
        Assert.True(pane.Editor("内缩值").IsEnabled);
        pane.ActionButton("闭合轮廓").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        pane.ActionButton("提交到 CAD 会话").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.True(pane.ActionButton("撤销").IsEnabled);
        pane.ActionButton("撤销").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(4, state.Loops[0].Curves.Count);
        Assert.True(pane.ActionButton("重做").IsEnabled);
        pane.ActionButton("重做").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(5, state.Loops[0].Curves.Count);
        state.Workbench.SelectPiece(new Point2D(10, 10));
        pane.ActionButton("旋转 +15°").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.False(pane.ActionButton("撤销").IsEnabled);
        Assert.False(pane.ActionButton("清除选择").IsEnabled);
    }

    [Fact]
    public void Property_pane_validates_offset_and_exposes_rotation_and_honest_todo_controls()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        state.Workbench.SelectPiece(new Point2D(10, 10));
        var pane = new CadPropertyPane(state);
        var original = state.Loops;
        Assert.True(pane.Editor("内缩值").IsEnabled);

        pane.Editor("内缩值").Text = "NaN";
        pane.ActionButton("内缩生成线").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Same(original, state.Loops);
        Assert.Equal(WorkbenchState.Ready, state.Workbench.State);
        Assert.Contains("非零有限数值", state.StatusMessage);

        pane.Editor("内缩值").Text = "2.5";
        pane.ActionButton("内缩生成线").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(WorkbenchState.Previewing, state.Workbench.State);
        pane.ActionButton("取消预览").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        pane.ActionButton("旋转 +15°").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(WorkbenchState.Previewing, state.Workbench.State);

        var unsupported = pane.ActionButton("自动组合");
        Assert.False(unsupported.IsEnabled);
        Assert.Contains(TodoBadge.StandardText, ToolTip.GetTip(unsupported)?.ToString());
    }

    private static Loop2D Rectangle() => new("imported", LoopRole.Outer,
    [
        new LineSegment2D(new(0, 0), new(100, 0)),
        new LineSegment2D(new(100, 0), new(100, 50)),
        new LineSegment2D(new(100, 50), new(0, 50)),
        new LineSegment2D(new(0, 50), new(0, 0)),
    ]);

    private static Loop2D OpenContour() => new("open", LoopRole.Outer,
    [
        new LineSegment2D(new(0.05, 0), new(100, 0)),
        new LineSegment2D(new(100, 0), new(100, 50)),
        new LineSegment2D(new(100, 50), new(0, 50)),
        new LineSegment2D(new(0, 50), new(0, 0)),
    ]);

    private sealed class StubDxfReader : IDxfReader
    {
        public Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken) =>
            Task.FromResult(new DxfImportResult([], [], [], UnitDecision.Unresolved, DxfDeclaredUnit.Millimetres, 4));
    }

    private sealed class StubGeometryReader(IReadOnlyList<Loop2D> loops) : IImportGeometryReader
    {
        public Task<IReadOnlyList<Loop2D>> ReadAsync(string path, CancellationToken cancellationToken) => Task.FromResult(loops);
    }

    private sealed class StubStore : IProjectStore
    {
        public Task SaveAsync(string path, ProjectDocument project, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<ProjectDocument> LoadAsync(string path, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class TestModule(string id, int order) : IDesktopModule
    {
        public DesktopModuleMetadata Metadata { get; } = new(id, id, "Test", order);
        public Func<Control> CreateView => () => new Border();
    }
}
