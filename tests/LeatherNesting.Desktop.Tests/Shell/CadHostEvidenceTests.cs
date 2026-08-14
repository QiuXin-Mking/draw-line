using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using LeatherNesting.Application;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Modules.Import;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Shell;
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
            ["范围缩放", "绘制多段线", "绘制矩形"],
            host.DrawingToolButtons.Take(3).Select(ToolTip.GetTip));
        Assert.All(host.DrawingToolButtons, button => Assert.NotNull(ToolTip.GetTip(button)));
        Assert.Equal(24, host.DrawingToolButtons[0].Width);
        Assert.Equal(24, host.DrawingToolButtons[0].Height);
        Assert.Same(AppTheme.CadCanvasBackground, host.Canvas.Background);
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
    public void Unsupported_drawing_tools_report_todo_without_mutating_geometry()
    {
        var state = new CadHostState();
        state.LoadConfirmedImport("40.DXF", [Rectangle()]);
        var before = state.Loops.ToArray();
        var host = new CadWorkspaceHost(state);

        host.DrawingToolButtons.Single(button => Equals(ToolTip.GetTip(button), "绘制矩形"))
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(before, state.Loops);
        Assert.Contains(TodoBadge.StandardText, state.StatusMessage);
        Assert.Contains("绘制矩形", state.StatusMessage);
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
    }

    [Fact]
    [Trait("TestId", "CAD-HOST-004")]
    public async Task Confirmed_m02_inspection_publishes_real_geometry_to_shared_cad_host()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "dxf-source");
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

            coordinator.ConfirmMillimetres();

            Assert.Equal(Path.GetFileName(path), cad.FileName);
            Assert.Same(loops[0], Assert.Single(cad.Loops));
            Assert.False(cad.IsDemoGeometry);
        }
        finally { File.Delete(path); }
    }

    private static Loop2D Rectangle() => new("imported", LoopRole.Outer,
    [
        new LineSegment2D(new(0, 0), new(100, 0)),
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
