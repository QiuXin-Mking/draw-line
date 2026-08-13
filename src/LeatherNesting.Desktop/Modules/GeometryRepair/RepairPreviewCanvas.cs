using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Modules.GeometryRepair;

/// <summary>Read-only before/after overlay for the module-owned repair preview.</summary>
public sealed class RepairPreviewCanvas : Control
{
    private static readonly IPen BeforePen = new Pen(new SolidColorBrush(Color.FromArgb(0x80, 0x8C, 0x98, 0xA5)), 3);
    private static readonly IPen KeptPen = new Pen(new SolidColorBrush(Color.FromRgb(0x55, 0xAA, 0xF5)), 2);
    private static readonly IPen AddedPen = new Pen(new SolidColorBrush(Color.FromRgb(0x42, 0xC7, 0x7A)), 3);
    private IReadOnlyList<Loop2D> _before = [];
    private IReadOnlyList<Loop2D> _after = [];

    public void SetData(IReadOnlyList<Loop2D> before, IReadOnlyList<Loop2D> after)
    {
        _before = before;
        _after = after;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(0x1E, 0x24, 0x2B)), new Rect(Bounds.Size));
        var allSegments = _before.SelectMany(Flatten).Concat(_after.SelectMany(Flatten)).ToArray();
        if (allSegments.Length == 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var minX = allSegments.Min(segment => Math.Min(segment.Start.X, segment.End.X));
        var minY = allSegments.Min(segment => Math.Min(segment.Start.Y, segment.End.Y));
        var maxX = allSegments.Max(segment => Math.Max(segment.Start.X, segment.End.X));
        var maxY = allSegments.Max(segment => Math.Max(segment.Start.Y, segment.End.Y));
        var scale = Math.Min((Bounds.Width - 36) / Math.Max(1, maxX - minX), (Bounds.Height - 36) / Math.Max(1, maxY - minY));
        var centreX = (minX + maxX) / 2;
        var centreY = (minY + maxY) / 2;
        Point ToPixel(Point2D point) => new(Bounds.Width / 2 + (point.X - centreX) * scale, Bounds.Height / 2 - (point.Y - centreY) * scale);

        foreach (var segment in _before.SelectMany(Flatten))
            context.DrawLine(BeforePen, ToPixel(segment.Start), ToPixel(segment.End));

        var beforeKeys = _before.SelectMany(Flatten).Select(SegmentKey).ToHashSet();
        foreach (var segment in _after.SelectMany(Flatten))
            context.DrawLine(beforeKeys.Contains(SegmentKey(segment)) ? KeptPen : AddedPen, ToPixel(segment.Start), ToPixel(segment.End));
    }

    private static string SegmentKey(LineSegment2D line) => $"{line.Start.X:F4},{line.Start.Y:F4}>{line.End.X:F4},{line.End.Y:F4}";

    private static IEnumerable<LineSegment2D> Flatten(Loop2D loop)
    {
        foreach (var curve in loop.Curves)
        {
            switch (curve)
            {
                case LineSegment2D line:
                    yield return line;
                    break;
                case Polyline2D polyline:
                    for (var index = 0; index < polyline.Points.Count - 1; index++)
                        yield return new LineSegment2D(polyline.Points[index], polyline.Points[index + 1]);
                    break;
                case CircularArc2D arc:
                    var previous = arc.StartPoint;
                    for (var index = 1; index <= 24; index++)
                    {
                        var current = arc.PointAt(index / 24d);
                        yield return new LineSegment2D(previous, current);
                        previous = current;
                    }
                    break;
            }
        }
    }
}
