using Avalonia.Controls;
using Avalonia.Interactivity;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.CadCanvas.Toolbar;

[Collection("Avalonia UI")]
public sealed class CadToolbarControllerTests
{
    [Fact]
    [Trait("Stage", "S5")]
    [Trait("TestId", "AC-CAD-T08")]
    public void Refit_button_invokes_the_real_handler_once_without_reporting_todo()
    {
        var host = LoadedHost();
        var statusBefore = host.StatusMessage;
        var refitCalls = 0;
        var controller = new CadToolbarController(host, () => refitCalls++);

        Click(controller, CadToolCommandKey.Refit);

        Assert.Equal(1, refitCalls);
        Assert.Equal(statusBefore, host.StatusMessage);
        Assert.DoesNotContain(TodoBadge.StandardText, host.StatusMessage);
    }

    [Fact]
    [Trait("Stage", "S5")]
    [Trait("TestId", "AC-CAD-T07")]
    public void Every_todo_command_reports_its_catalog_label_without_mutating_host_content()
    {
        var host = LoadedHost();
        var loopsBefore = host.Loops;
        var fileNameBefore = host.FileName;
        var controller = new CadToolbarController(host, () => { });
        controller.State.SetAvailability(hasUndo: true, hasRedo: true, hasSelection: true);

        var todoTools = CadToolCatalog.All
            .Where(tool => tool.ImplementationState == CadToolImplementationState.Todo)
            .ToArray();

        foreach (var tool in todoTools)
        {
            Assert.True(controller.TryExecute(tool.CommandKey));
            Assert.Contains(tool.Label, host.StatusMessage);
            Assert.Contains(TodoBadge.StandardText, host.StatusMessage);
            Assert.Same(loopsBefore, host.Loops);
            Assert.Equal(fileNameBefore, host.FileName);
        }

        Assert.Equal(24, todoTools.Length);
    }

    [Fact]
    [Trait("Stage", "S5")]
    [Trait("TestId", "AC-CAD-T09")]
    public void Select_cancel_and_escape_change_tool_state_without_deleting_geometry()
    {
        var host = LoadedHost();
        var loopsBefore = host.Loops;
        var controller = new CadToolbarController(host, () => { });
        controller.State.SetAvailability(hasUndo: false, hasRedo: false, hasSelection: true);

        Assert.True(controller.TryExecute(CadToolCommandKey.DrawRectangle));
        controller.State.SetPendingStep(true);
        Assert.True(controller.TryExecute(CadToolCommandKey.Cancel));
        Assert.Equal(CadToolCommandKey.DrawRectangle, controller.State.ActiveTool);
        Assert.False(controller.State.HasPendingStep);

        Assert.True(controller.HandleEscape());
        Assert.Equal(CadToolCommandKey.Select, controller.State.ActiveTool);
        Assert.True(controller.State.HasSelection);
        Assert.Same(loopsBefore, host.Loops);
        Assert.NotEmpty(host.Loops);
    }

    [Theory]
    [InlineData(CadToolCommandKey.Undo)]
    [InlineData(CadToolCommandKey.Redo)]
    [InlineData(CadToolCommandKey.Delete)]
    public void Disabled_commands_do_not_invoke_handlers_or_publish_todo(CadToolCommandKey command)
    {
        var host = LoadedHost();
        var statusBefore = host.StatusMessage;
        var loopsBefore = host.Loops;
        var refitCalls = 0;
        var controller = new CadToolbarController(host, () => refitCalls++);

        Assert.False(controller.TryExecute(command));

        Assert.Equal(0, refitCalls);
        Assert.Equal(statusBefore, host.StatusMessage);
        Assert.Same(loopsBefore, host.Loops);
    }

    [Fact]
    public void State_changes_are_projected_to_view_visibility_active_and_enabled_status()
    {
        var controller = new CadToolbarController(new CadHostState(), () => { });

        Assert.Equal(27, controller.View.VisibleDefinitions.Count);
        Assert.Equal(CadToolCommandKey.Select, controller.View.ActiveKey);
        Assert.False(Button(controller, CadToolCommandKey.Undo).IsEnabled);
        Assert.False(Button(controller, CadToolCommandKey.Redo).IsEnabled);
        Assert.False(Button(controller, CadToolCommandKey.Delete).IsEnabled);

        controller.State.SetAvailability(hasUndo: true, hasRedo: false, hasSelection: true);
        controller.State.TryExecute(CadToolCommandKey.DrawCircle);
        controller.State.SetMode(CadToolbarMode.NestingReview);

        Assert.Equal(
            [
                CadToolCommandKey.Select,
                CadToolCommandKey.Undo,
                CadToolCommandKey.Redo,
                CadToolCommandKey.Cancel,
                CadToolCommandKey.Delete,
                CadToolCommandKey.Settings,
            ],
            controller.View.VisibleDefinitions.Select(tool => tool.CommandKey));
        Assert.Equal(CadToolCommandKey.Select, controller.View.ActiveKey);
        Assert.True(Button(controller, CadToolCommandKey.Undo).IsEnabled);
        Assert.False(Button(controller, CadToolCommandKey.Redo).IsEnabled);
        Assert.True(Button(controller, CadToolCommandKey.Delete).IsEnabled);
        Assert.False(Button(controller, CadToolCommandKey.Cancel).IsEnabled);
    }

    [Fact]
    public void View_clicks_route_by_command_key_to_the_controller()
    {
        var host = LoadedHost();
        var controller = new CadToolbarController(host, () => { });

        Click(controller, CadToolCommandKey.DrawSpline);

        Assert.Equal(CadToolCommandKey.DrawSpline, controller.State.ActiveTool);
        Assert.Contains("绘制自由曲线/样条", host.StatusMessage);
        Assert.Contains(TodoBadge.StandardText, host.StatusMessage);
    }

    private static CadHostState LoadedHost()
    {
        var host = new CadHostState();
        host.LoadConfirmedImport("sample.dxf", [DemoGeometryFactory.Create()[0].Loop]);
        return host;
    }

    private static void Click(CadToolbarController controller, CadToolCommandKey command) =>
        Button(controller, command).RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

    private static Button Button(CadToolbarController controller, CadToolCommandKey command)
    {
        var order = CadToolCatalog.All.Single(tool => tool.CommandKey == command).Order;
        return controller.View.Buttons[order - 1];
    }
}
