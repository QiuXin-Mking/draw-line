using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Views;

/// <summary>Self-drawn X/Y axes anchored to the model origin (0,0).
/// The axes translate with the canvas view and hide when the origin leaves the
/// visible area. Arrows and +X/+Y labels use the shared material-boundary brush.
/// The origin, axis endpoints and labels are clamped into the control bounds so the
/// indicator never overlaps surrounding toolbars or the coordinate readout.</summary>
public sealed class CadOriginAxes : Control
{
    /// <summary>Short axis length in model millimetres (X and Y).</summary>
    public const double AxisLengthMm = 10;

    /// <summary>Minimum inset from the control edge so labels and arrows stay inside.</summary>
    public const double EdgeMargin = 24;

    public CadOriginAxes(CanvasView source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Source.ViewChanged += (_, _) => InvalidateVisual();
    }

    public CanvasView Source { get; }

    /// <summary>Shared semantic brush used for the axes and labels.</summary>
    public IBrush AxisBrush => AppTheme.MaterialBoundary;

    /// <summary>Axis endpoint pixel offset along the positive direction, in model mm.</summary>
    public double AxisLengthPx => AxisLengthMm * Source.ViewScale;

    /// <summary>Pixel position of the model origin (0,0) within this control.</summary>
    public Point OriginPixel => new(
        -Source.ViewOriginModel.X * Source.ViewScale,
        Source.ViewOriginModel.Y * Source.ViewScale);

    /// <summary>True when the model origin projects inside this control's visible area.</summary>
    public bool IsOriginVisible =>
        OriginPixel.X >= 0 && OriginPixel.Y >= 0 &&
        OriginPixel.X <= Bounds.Width && OriginPixel.Y <= Bounds.Height;

    /// <summary>Origin pixel pushed inside the control with an edge margin, so the
    /// whole indicator (axes + labels) stays visible and never overlaps the toolbar.</summary>
    public Point ClampedOrigin => new(
        Clamp(OriginPixel.X, EdgeMargin, Math.Max(EdgeMargin, Bounds.Width - EdgeMargin)),
        Clamp(OriginPixel.Y, EdgeMargin, Math.Max(EdgeMargin, Bounds.Height - EdgeMargin)));

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || !IsOriginVisible)
            return;

        var origin = ClampedOrigin;
        var pen = new Pen(AxisBrush, 1.5);
        var length = AxisLengthPx;

        // X axis: short horizontal segment from the origin toward +X, clamped to the width.
        var xEnd = new Point(Math.Min(origin.X + length, Bounds.Width), origin.Y);
        context.DrawLine(pen, origin, xEnd);
        DrawArrowX(context, pen, xEnd);
        DrawLabelClamped(context, BuildText("+X"), xEnd.X - 18, origin.Y - 20);

        // Y axis: short vertical segment from the origin toward +Y, clamped to the height.
        var yEnd = new Point(origin.X, Math.Max(origin.Y - length, 0));
        context.DrawLine(pen, origin, yEnd);
        DrawArrowY(context, pen, yEnd);
        DrawLabelClamped(context, BuildText("+Y"), origin.X + 8, yEnd.Y + 4);
    }

    private static void DrawArrowX(DrawingContext context, Pen pen, Point tip)
    {
        context.DrawLine(pen, tip, new Point(tip.X - 8, tip.Y - 5));
        context.DrawLine(pen, tip, new Point(tip.X - 8, tip.Y + 5));
    }

    private static void DrawArrowY(DrawingContext context, Pen pen, Point tip)
    {
        context.DrawLine(pen, tip, new Point(tip.X - 5, tip.Y + 8));
        context.DrawLine(pen, tip, new Point(tip.X + 5, tip.Y + 8));
    }

    private void DrawLabelClamped(DrawingContext context, FormattedText text, double x, double y)
    {
        var cx = Clamp(x, 0, Math.Max(0, Bounds.Width - text.Width));
        var cy = Clamp(y, 0, Math.Max(0, Bounds.Height - text.Height));
        context.DrawText(text, new Point(cx, cy));
    }

    private static FormattedText BuildText(string text) => new(
        text,
        System.Globalization.CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        Typeface.Default,
        10,
        AppTheme.MaterialBoundary);

    private static double Clamp(double value, double min, double max) => Math.Clamp(value, min, max);
}
