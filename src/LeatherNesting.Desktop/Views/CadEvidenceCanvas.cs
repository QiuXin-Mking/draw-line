using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Views;

/// <summary>Black evidence-aligned read-only projection for confirmed imported geometry.</summary>
public sealed class CadEvidenceCanvas : Control
{
    private static readonly IPen OuterPen = new Pen(AppTheme.GeometryOuterContour, 1);
    private static readonly IPen InnerPen = new Pen(AppTheme.GeometryInternalLine, 1);
    private IReadOnlyList<Loop2D> _loops = [];

    public void SetData(IReadOnlyList<Loop2D> loops)
    {
        _loops = loops ?? [];
        InvalidateVisual();
    }

    public void Refit()
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(AppTheme.CanvasBlack, new Rect(Bounds.Size));
        if (_loops.Count == 0 || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var points = _loops.SelectMany(loop => loop.Curves).SelectMany(Sample).ToArray();
        if (points.Length == 0) return;
        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxY = points.Max(point => point.Y);
        var scale = Math.Min(Bounds.Width / Math.Max(1, maxX - minX), Bounds.Height / Math.Max(1, maxY - minY)) * 0.72;
        var offsetX = (Bounds.Width - (maxX - minX) * scale) / 2 - minX * scale;
        var offsetY = (Bounds.Height - (maxY - minY) * scale) / 2 + maxY * scale;

        foreach (var loop in _loops)
        {
            var pen = loop.Role == LoopRole.Outer ? OuterPen : InnerPen;
            foreach (var curve in loop.Curves)
            {
                var sampled = Sample(curve).ToArray();
                for (var index = 1; index < sampled.Length; index++)
                    context.DrawLine(pen, Pixel(sampled[index - 1]), Pixel(sampled[index]));
            }
        }
        Point Pixel(Point2D point) => new(offsetX + point.X * scale, offsetY - point.Y * scale);
    }

    private static IEnumerable<Point2D> Sample(Curve2D curve)
    {
        const int steps = 24;
        yield return curve.StartPoint;
        if (curve is CircularArc2D arc)
            for (var index = 1; index < steps; index++) yield return arc.PointAt((double)index / steps);
        else if (curve is Polyline2D polyline)
            foreach (var point in polyline.Points.Skip(1).SkipLast(1)) yield return point;
        yield return curve.EndPoint;
    }
}
