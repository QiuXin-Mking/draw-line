using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.Views;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class CadOriginAxesTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "AXIS-001")]
    public void Origin_pixel_derives_from_the_view_origin_and_scale()
    {
        var canvas = new CanvasView();
        var axes = new CadOriginAxes(canvas);

        // Default view: offset=(0,0), scale=10 → origin projects to pixel (0,0).
        Assert.Equal(new Point(0, 0), axes.OriginPixel);

        // A known non-default view: offset=(50,80), scale=5.
        SetView(canvas, offset: new Point(50, 80), scale: 5);
        Assert.Equal(new Point(50, 80), axes.OriginPixel);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "AXIS-002")]
    public void Origin_pixel_tracks_view_changes()
    {
        var canvas = new CanvasView();
        var axes = new CadOriginAxes(canvas);
        SetView(canvas, offset: new Point(50, 80), scale: 5);
        Assert.Equal(new Point(50, 80), axes.OriginPixel);

        SetView(canvas, offset: new Point(-30, 120), scale: 2.5);
        Assert.Equal(new Point(-30, 120), axes.OriginPixel);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "AXIS-003")]
    public void Host_uses_self_drawn_axes_with_the_material_boundary_brush_not_a_static_text_block()
    {
        var host = new CadWorkspaceHost(new CadHostState());

        Assert.IsType<CadOriginAxes>(host.Axes);
        Assert.Same(AppTheme.MaterialBoundary, host.Axes.AxisBrush);
        Assert.False(host.Axes.IsHitTestVisible);

        var canvas = Assert.IsType<Grid>(host.Canvas.Child);
        Assert.DoesNotContain(canvas.Children.OfType<TextBlock>(), text => text.Text?.Contains("+X") == true);
        Assert.Contains(host.Axes, canvas.Children.Cast<object>());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "AXIS-004")]
    public void Axes_hide_when_the_origin_leaves_the_visible_area()
    {
        var canvas = new CanvasView();
        var axes = new CadOriginAxes(canvas) { Width = 200, Height = 200 };
        axes.Measure(new Size(200, 200));
        axes.Arrange(new Rect(0, 0, 200, 200));

        SetView(canvas, offset: new Point(10, 10), scale: 5);
        Assert.True(axes.IsOriginVisible);

        // Push the origin far outside the 200x200 control.
        SetView(canvas, offset: new Point(500, -400), scale: 5);
        Assert.False(axes.IsOriginVisible);
    }

    private static void SetView(CanvasView canvas, Point offset, double scale)
    {
        var scaleField = typeof(CanvasView).GetField("_scale", BindingFlags.Instance | BindingFlags.NonPublic);
        var offsetField = typeof(CanvasView).GetField("_offset", BindingFlags.Instance | BindingFlags.NonPublic);
        scaleField!.SetValue(canvas, scale);
        offsetField!.SetValue(canvas, offset);
    }
}
