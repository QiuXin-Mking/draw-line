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
    public CadOriginAxes(CanvasView source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Source.ViewChanged += (_, _) => InvalidateVisual();
    }

    public CanvasView Source { get; }

    /// <summary>Shared semantic brush used for the axes and labels.</summary>
    public IBrush AxisBrush => AppTheme.MaterialBoundary;

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

        // X axis: horizontal from the origin toward the right edge, arrow at the end.
        var xEnd = new Point(Bounds.Width, origin.Y);
        context.DrawLine(pen, origin, xEnd);
        DrawArrowX(context, pen, xEnd);
        DrawText(context, "+X", xEnd.X - 20, origin.Y - 16);

        // Y axis: vertical from the origin toward the top edge, arrow at the end.
        var yEnd = new Point(origin.X, 0);
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
