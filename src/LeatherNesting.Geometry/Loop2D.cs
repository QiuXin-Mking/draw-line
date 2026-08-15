using System.Text.Json.Serialization;

namespace LeatherNesting.Geometry;

/// <summary>Role of a loop in a piece: outer boundary or hole.</summary>
public enum LoopRole { Outer, Hole }

/// <summary>A closed contour composed of a sequence of curves, with normalized winding.</summary>
public sealed record Loop2D
{
    public string StableId { get; }
    public LoopRole Role { get; }
    public IReadOnlyList<Curve2D> Curves { get; }
    public bool IsClockwise { get; }
    public double Area { get; }

    /// <summary>Total perimeter length of the loop, in mm.</summary>
    public double Length => Curves.Sum(c => c.Length);

    /// <summary>Returns true if the point is inside the loop (ray-casting point-in-polygon).</summary>
    public bool ContainsPoint(Point2D point)
    {
        var intersections = 0;
        foreach (var curve in Curves)
        foreach (var (start, end) in FlattenToSegments(curve))
        {
            if (point.Y < Math.Min(start.Y, end.Y) || point.Y >= Math.Max(start.Y, end.Y)) continue;
            if (start.Y == end.Y) continue;
            var xIntersect = start.X + (point.Y - start.Y) * (end.X - start.X) / (end.Y - start.Y);
            if (xIntersect > point.X) intersections++;
        }
        return intersections % 2 == 1;
    }

    private static IReadOnlyList<(Point2D Start, Point2D End)> FlattenToSegments(Curve2D curve) => curve switch
    {
        LineSegment2D l => [(l.Start, l.End)],
        Polyline2D p => Enumerable.Range(0, p.Points.Count - 1).Select(i => (p.Points[i], p.Points[i + 1])).ToList(),
        CircularArc2D a => [(a.StartPoint, a.EndPoint)],
        _ => []
    };

    /// <summary>Bounding box of the loop, in mm.</summary>
    public (double MinX, double MinY, double MaxX, double MaxY) Bounds
    {
        get
        {
            var minX = double.MaxValue; var minY = double.MaxValue;
            var maxX = double.MinValue; var maxY = double.MinValue;
            foreach (var curve in Curves)
            {
                var (cMinX, cMinY, cMaxX, cMaxY) = curve.Bounds;
                minX = Math.Min(minX, cMinX); minY = Math.Min(minY, cMinY);
                maxX = Math.Max(maxX, cMaxX); maxY = Math.Max(maxY, cMaxY);
            }
            return (minX, minY, maxX, maxY);
        }
    }

    /// <summary>Centre of the loop's bounding box, used as the default rotation pivot.</summary>
    public Point2D Centroid
    {
        get
        {
            var (minX, minY, maxX, maxY) = Bounds;
            return new Point2D((minX + maxX) / 2, (minY + maxY) / 2);
        }
    }

    [JsonConstructor]
    public Loop2D(string stableId, LoopRole role, IReadOnlyList<Curve2D> curves)
    {
        if (string.IsNullOrWhiteSpace(stableId))
            throw new ArgumentException("轮廓必须有稳定标识符。", nameof(stableId));
        if (curves.Count < 1)
            throw new ArgumentException("轮廓至少需要一条曲线。", nameof(curves));
        StableId = stableId;
        Role = role;
        Curves = curves;
        IsClockwise = ComputeWinding();
        Area = ComputeSignedArea();
    }

    /// <summary>Returns a new Loop2D with winding normalized (outer = CCW, hole = CW).</summary>
    public Loop2D NormalizeWinding()
    {
        var clockwise = ComputeWinding();
        var expectedClockwise = Role == LoopRole.Outer ? false : true;
        return clockwise == expectedClockwise ? this : Reverse();
    }

    public Loop2D Reverse()
    {
        var reversed = new List<Curve2D>(Curves.Count);
        for (var i = Curves.Count - 1; i >= 0; i--)
        {
            var c = Curves[i];
            reversed.Add(c switch
            {
                LineSegment2D l => new LineSegment2D(l.End, l.Start),
                CircularArc2D a => new CircularArc2D(a.Centre, a.Radius, a.StartAngleDegrees + a.SweepAngleDegrees, -a.SweepAngleDegrees),
                Polyline2D p => new Polyline2D(p.Points.Reverse().ToList()),
                _ => throw new NotSupportedException($"不支持的曲线类型：{c.GetType().Name}")
            });
        }
        return new Loop2D(StableId, Role, reversed);
    }

    /// <summary>Returns the point at fraction <paramref name="t"/> (0..1) of the loop perimeter length.</summary>
    public Point2D PointAt(double t)
    {
        var clamped = Math.Clamp(t, 0, 1);
        var total = Length;
        if (total <= 0)
            return Curves[0].StartPoint;

        var target = clamped * total;
        var accumulated = 0.0;
        foreach (var curve in Curves)
        {
            var curveLength = curve.Length;
            if (curveLength <= 0)
                continue;

            if (accumulated + curveLength >= target)
            {
                var localT = (target - accumulated) / curveLength;
                return curve.PointAt(localT);
            }
            accumulated += curveLength;
        }
        return Curves[^1].EndPoint;
    }

    private bool ComputeWinding()
    {
        // Trapezoid shoelace variant: positive sum = clockwise, negative = counter-clockwise.
        var sum = 0.0;
        foreach (var curve in Curves)
        {
            if (curve is LineSegment2D line)
                sum += (line.End.X - line.Start.X) * (line.End.Y + line.Start.Y);
            else if (curve is Polyline2D poly)
                for (var i = 0; i < poly.Points.Count - 1; i++)
                    sum += (poly.Points[i + 1].X - poly.Points[i].X) * (poly.Points[i + 1].Y + poly.Points[i].Y);
            else if (curve is CircularArc2D arc)
            {
                var start = arc.StartPoint; var end = arc.EndPoint;
                sum += (end.X - start.X) * (end.Y + start.Y);
            }
        }
        // Ring closing edge (trapezoid form): completes contours whose polyline omits
        // the closing vertex. Contributes 0 when the ring is already closed.
        sum += (Curves[0].StartPoint.X - Curves[^1].EndPoint.X) * (Curves[0].StartPoint.Y + Curves[^1].EndPoint.Y);
        return sum > 0; // positive = clockwise
    }

    private double ComputeSignedArea()
    {
        var sum = 0.0;
        foreach (var curve in Curves)
        {
            if (curve is LineSegment2D line)
                sum += (line.Start.X * line.End.Y - line.End.X * line.Start.Y);
            else if (curve is Polyline2D poly)
                for (var i = 0; i < poly.Points.Count - 1; i++)
                    sum += poly.Points[i].X * poly.Points[i + 1].Y - poly.Points[i + 1].X * poly.Points[i].Y;
            else if (curve is CircularArc2D arc)
            {
                // Exact arc-aware area via Green's theorem:
                // r²·sweep + r·(cx·(sinθ₂−sinθ₁) − cy·(cosθ₂−cosθ₁))
                var startAngle = arc.StartAngleDegrees * Math.PI / 180;
                var sweep = arc.SweepAngleDegrees * Math.PI / 180;
                var endAngle = startAngle + sweep;
                var cx = arc.Centre.X;
                var cy = arc.Centre.Y;
                var r = arc.Radius;
                sum += r * r * sweep
                     + r * cx * (Math.Sin(endAngle) - Math.Sin(startAngle))
                     - r * cy * (Math.Cos(endAngle) - Math.Cos(startAngle));
            }
        }
        // Ring closing edge: last curve end → first curve start. Completes contours whose
        // polyline omits the closing vertex; contributes 0 when the ring is already closed.
        sum += Curves[^1].EndPoint.X * Curves[0].StartPoint.Y
             - Curves[0].StartPoint.X * Curves[^1].EndPoint.Y;
        return Math.Abs(sum) / 2.0;
    }
}