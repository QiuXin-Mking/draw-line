using System.Text.Json.Serialization;

namespace LeatherNesting.Geometry;

/// <summary>Base type for 2D curve segments.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineSegment2D), "line")]
[JsonDerivedType(typeof(CircularArc2D), "arc")]
[JsonDerivedType(typeof(Polyline2D), "polyline")]
public abstract record Curve2D
{
    public abstract Point2D StartPoint { get; }
    public abstract Point2D EndPoint { get; }
    public abstract double Length { get; }
    public abstract Point2D PointAt(double t);
    /// <summary>Returns the bounding box as (minX, minY, maxX, maxY).</summary>
    public abstract (double MinX, double MinY, double MaxX, double MaxY) Bounds { get; }
}

/// <summary>Straight line segment between two points.</summary>
public sealed record LineSegment2D(Point2D Start, Point2D End) : Curve2D
{
    public override Point2D StartPoint => Start;
    public override Point2D EndPoint => End;
    public override double Length => Start.DistanceTo(End);
    public override Point2D PointAt(double t) => new(Start.X + (End.X - Start.X) * t, Start.Y + (End.Y - Start.Y) * t);
    public override (double MinX, double MinY, double MaxX, double MaxY) Bounds =>
        (Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y), Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));
}

/// <summary>Circular arc defined by centre, radius, start angle, and sweep angle (CCW positive).</summary>
public sealed record CircularArc2D : Curve2D
{
    public Point2D Centre { get; init; }
    public double Radius { get; init; }
    public double StartAngleDegrees { get; init; }
    public double SweepAngleDegrees { get; init; }

    public CircularArc2D(Point2D centre, double radius, double startAngleDegrees, double sweepAngleDegrees)
    {
        GeometryConstants.RejectNonFinite(radius, nameof(radius));
        GeometryConstants.RejectNonFinite(startAngleDegrees, nameof(startAngleDegrees));
        GeometryConstants.RejectNonFinite(sweepAngleDegrees, nameof(sweepAngleDegrees));
        if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, "圆弧半径必须为正。");
        Centre = centre;
        Radius = radius;
        StartAngleDegrees = startAngleDegrees;
        SweepAngleDegrees = sweepAngleDegrees;
    }

    public override Point2D StartPoint => AnglePoint(StartAngleDegrees);
    public override Point2D EndPoint => AnglePoint(StartAngleDegrees + SweepAngleDegrees);
    public override double Length => Math.Abs(SweepAngleDegrees * Math.PI / 180 * Radius);
    public override Point2D PointAt(double t) => AnglePoint(StartAngleDegrees + SweepAngleDegrees * t);
    public override (double MinX, double MinY, double MaxX, double MaxY) Bounds
    {
        get
        {
            var start = StartPoint;
            var end = EndPoint;
            return (Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
        }
    }

    private Point2D AnglePoint(double degrees)
    {
        var rad = degrees * Math.PI / 180;
        return new(Centre.X + Radius * Math.Cos(rad), Centre.Y + Radius * Math.Sin(rad));
    }

    /// <summary>Returns true if the point lies on the arc (within tolerance), for intersection tests.</summary>
    public bool ContainsPoint(Point2D point, double tolerance = 1e-6)
    {
        if (Math.Abs(point.DistanceTo(Centre) - Radius) > tolerance)
            return false;

        var angle = Math.Atan2(point.Y - Centre.Y, point.X - Centre.X) * 180 / Math.PI;
        var delta = (angle - StartAngleDegrees) % 360;
        if (delta < 0) delta += 360;
        return SweepAngleDegrees >= 0
            ? delta <= SweepAngleDegrees + tolerance
            : delta >= 360 + SweepAngleDegrees - tolerance;
    }
}

/// <summary>Polyline composed of a sequence of connected points.</summary>
public sealed record Polyline2D : Curve2D
{
    public IReadOnlyList<Point2D> Points { get; }
    public override Point2D StartPoint => Points[0];
    public override Point2D EndPoint => Points[^1];
    public override double Length { get; }
    public override (double MinX, double MinY, double MaxX, double MaxY) Bounds { get; }

    public Polyline2D(IReadOnlyList<Point2D> points)
    {
        if (points.Count < 2)
            throw new ArgumentException("折线至少需要两个点。", nameof(points));
        Points = points;
        Length = ComputeLength();
        Bounds = ComputeBounds();
    }

    public override Point2D PointAt(double t)
    {
        var clamped = Math.Clamp(t, 0, 1);
        var totalSegments = Points.Count - 1;
        if (totalSegments == 0) return Points[0];
        var segmentIndex = (int)(clamped * totalSegments);
        if (segmentIndex >= totalSegments) segmentIndex = totalSegments - 1;
        var segT = clamped * totalSegments - segmentIndex;
        return new(
            Points[segmentIndex].X + (Points[segmentIndex + 1].X - Points[segmentIndex].X) * segT,
            Points[segmentIndex].Y + (Points[segmentIndex + 1].Y - Points[segmentIndex].Y) * segT);
    }

    private double ComputeLength()
    {
        var total = 0.0;
        for (var i = 0; i < Points.Count - 1; i++)
            total += Points[i].DistanceTo(Points[i + 1]);
        return total;
    }

    private (double MinX, double MinY, double MaxX, double MaxY) ComputeBounds()
    {
        var minX = double.MaxValue; var minY = double.MaxValue;
        var maxX = double.MinValue; var maxY = double.MinValue;
        foreach (var p in Points)
        {
            if (p.X < minX) minX = p.X; if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X; if (p.Y > maxY) maxY = p.Y;
        }
        return (minX, minY, maxX, maxY);
    }
}