using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Views;

/// <summary>Self-drawn X/Y axes anchored to the model origin (0,0).
/// The axes translate with the canvas view and hide when the origin leaves the
/// visible area. Arrows and +X/+Y labels use the shared material-boundary brush.</summary>
public sealed class CadOriginAxes : Control
{
    /// <summary>Short axis length in model millimetres (X and Y).</summary>
    public const double AxisLengthMm = 10;

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

    public override void Render(DrawingContext context)
    {
        if (!IsOriginVisible || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var origin = OriginPixel;
        var pen = new Pen(AxisBrush, 1.5);
        var length = AxisLengthPx;

        // X axis: short horizontal segment from the origin toward +X, arrow at the end.
        var xEnd = new Point(origin.X + length, origin.Y);
        context.DrawLine(pen, origin, xEnd);
        DrawArrowX(context, pen, xEnd);
        DrawText(context, "+X", xEnd.X - 20, origin.Y - 16);

        // Y axis: short vertical segment from the origin toward +Y, arrow at the end.
        var yEnd = new Point(origin.X, origin.Y - length);
        context.DrawLine(pen, origin, yEnd);
        DrawArrowY(context, pen, yEnd);
        DrawText(context, "+Y", origin.X + 6, yEnd.Y + 2);
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

    private static void DrawText(DrawingContext context, string text, double x, double y)
    {
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            AppTheme.MaterialBoundary);
        context.DrawText(formatted, new Point(x, y));
    }
}
