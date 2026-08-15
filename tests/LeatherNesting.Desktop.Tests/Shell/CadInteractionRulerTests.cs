using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.Views;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class CadInteractionRulerTests
{
    private static readonly Loop2D Rectangle = new("imported", LoopRole.Outer,
    [
        new LineSegment2D(new(0, 0), new(100, 0)),
        new LineSegment2D(new(100, 0), new(100, 50)),
        new LineSegment2D(new(100, 50), new(0, 50)),
        new LineSegment2D(new(0, 50), new(0, 0)),
    ]);

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "RUL-001")]
    public void Canvas_view_exposes_view_state_and_notifies_when_fit_changes()
    {
        var canvas = new CanvasView();
        var changes = 0;
        canvas.ViewChanged += (_, _) => changes++;

        Assert.Equal(10, canvas.ViewScale);
        Assert.Equal(new Point2D(0, 0), canvas.ViewOriginModel);

        canvas.SetData([Rectangle], refit: true);

        Assert.Equal(1, changes);
        Assert.True(canvas.ViewScale > 0);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "RUL-002")]
    public void Wheel_zoom_changes_the_view_and_raises_view_changed()
    {
        var canvas = new CanvasView();
        canvas.SetData([Rectangle], refit: false);
        var scaleBefore = canvas.ViewScale;
        var changes = 0;
        canvas.ViewChanged += (_, _) => changes++;

        RaiseWheel(canvas, delta: 1);

        Assert.Equal(1, changes);
        Assert.Equal(scaleBefore * 1.1, canvas.ViewScale, precision: 6);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "RUL-003")]
    public void Shell_rulers_are_self_drawn_semantic_controls_not_static_text()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());

        Assert.IsType<CadRuler>(shell.VerticalRuler);
        Assert.IsType<CadRuler>(shell.HorizontalRuler);
        Assert.Equal(CadRulerOrientation.Vertical, shell.VerticalRuler.Orientation);
        Assert.Equal(CadRulerOrientation.Horizontal, shell.HorizontalRuler.Orientation);
        Assert.Same(AppTheme.RulerChrome, shell.VerticalRuler.Surface);
        Assert.Same(AppTheme.RulerTick, shell.VerticalRuler.TickBrush);
        Assert.Same(shell.CadWorkspace.Drawing, shell.VerticalRuler.Source);
        Assert.Same(shell.CadWorkspace.Drawing, shell.HorizontalRuler.Source);
        Assert.Equal(22, shell.VerticalRuler.Width);
        Assert.Equal(20, shell.HorizontalRuler.Height);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "RUL-004")]
    public void Ruler_tick_step_is_adaptive_across_zoom_levels()
    {
        var canvas = new CanvasView();
        var ruler = new CadRuler(canvas, CadRulerOrientation.Horizontal);

        canvas.SetData([Rectangle], refit: false);
        var coarse = ruler.TickStepMm;

        RaiseWheel(canvas, delta: 1);
        var finer = ruler.TickStepMm;

        // ModelToPixel must be consistent with the canvas view origin.
        Assert.Equal(
            (0 - canvas.ViewOriginModel.X) * canvas.ViewScale,
            ruler.ModelToPixel(0),
            precision: 6);
        Assert.True(finer <= coarse, "zooming in should refine the tick step or keep it equal");
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "RUL-005")]
    public void Coordinate_overlay_renders_model_mm_and_clears_via_the_update_seam()
    {
        var host = new CadWorkspaceHost(new CadHostState());
        var text = Assert.IsType<TextBlock>(FindCoordinateOverlay(host));

        Assert.Equal(string.Empty, host.CoordinateText);
        Assert.Equal(string.Empty, text.Text);

        host.UpdateCoordinates(new Point2D(5.5, -2.5));

        Assert.Equal("X 5.50 mm · Y -2.50 mm", host.CoordinateText);
        Assert.Same(AppTheme.CadCoordinateText, text.Foreground);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "RUL-006")]
    public void Pointer_exit_clears_the_coordinate_readout()
    {
        var host = new CadWorkspaceHost(new CadHostState());
        host.UpdateCoordinates(new Point2D(5.5, -2.5));
        Assert.NotEqual(string.Empty, host.CoordinateText);

        host.Drawing.RaiseEvent(CreatePointerExited(host.Drawing));

        Assert.Equal(string.Empty, host.CoordinateText);
    }

    private static void RaiseWheel(CanvasView canvas, double delta)
    {
        var method = typeof(CanvasView).GetMethod("OnPointerWheelChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        var pointer = new Avalonia.Input.Pointer(0, PointerType.Mouse, false);
        var eventArgs = new PointerWheelEventArgs(
            canvas,
            pointer,
            rootVisual: null!,
            new Point(10, 10),
            timestamp: 0,
            new PointerPointProperties(),
            KeyModifiers.None,
            new Vector(0, delta));
        method!.Invoke(canvas, [eventArgs]);
    }

    private static PointerEventArgs CreatePointerExited(Control target)
    {
        var pointer = new Avalonia.Input.Pointer(1, PointerType.Mouse, false);
        return new PointerEventArgs(
            Avalonia.Input.InputElement.PointerExitedEvent,
            target,
            pointer,
            rootVisual: null!,
            new Point(9999, 9999),
            0,
            new PointerPointProperties(),
            KeyModifiers.None);
    }

    private static TextBlock FindCoordinateOverlay(CadWorkspaceHost host)
    {
        var canvas = Assert.IsType<Grid>(host.Canvas.Child);
        return canvas.Children.OfType<TextBlock>()
            .Single(text => ReferenceEquals(text.Foreground, AppTheme.CadCoordinateText));
    }
}
