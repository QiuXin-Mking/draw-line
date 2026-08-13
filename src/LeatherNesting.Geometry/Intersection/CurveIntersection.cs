namespace LeatherNesting.Geometry.Intersection;

/// <summary>Curve-curve intersection for line-arc and arc-arc (line-line is trivial and handled elsewhere).</summary>
public static class CurveIntersection
{
    /// <summary>Returns the intersection points of a line segment and a circular arc.</summary>
    public static IReadOnlyList<Point2D> LineArc(LineSegment2D line, CircularArc2D arc)
    {
        var dx = line.End.X - line.Start.X;
        var dy = line.End.Y - line.Start.Y;
        var fx = line.Start.X - arc.Centre.X;
        var fy = line.Start.Y - arc.Centre.Y;
        var a = dx * dx + dy * dy;
        if (a < 1e-12) return [];
        var b = 2 * (fx * dx + fy * dy);
        var c = fx * fx + fy * fy - arc.Radius * arc.Radius;
        var disc = b * b - 4 * a * c;
        if (disc < 0) return [];

        var sqrt = Math.Sqrt(disc);
        var result = new List<Point2D>();
        foreach (var t in new[] { (-b - sqrt) / (2 * a), (-b + sqrt) / (2 * a) })
        {
            if (t is < 0 or > 1) continue;
            var point = line.PointAt(t);
            if (arc.ContainsPoint(point)) result.Add(point);
        }
        return result;
    }

    /// <summary>Returns the intersection points of two circular arcs.</summary>
    public static IReadOnlyList<Point2D> ArcArc(CircularArc2D a, CircularArc2D b)
    {
        var dx = b.Centre.X - a.Centre.X;
        var dy = b.Centre.Y - a.Centre.Y;
        var d = Math.Sqrt(dx * dx + dy * dy);
        if (d < 1e-12) return []; // concentric
        if (d > a.Radius + b.Radius || d < Math.Abs(a.Radius - b.Radius)) return []; // no intersection

        var along = (a.Radius * a.Radius - b.Radius * b.Radius + d * d) / (2 * d);
        var h2 = a.Radius * a.Radius - along * along;
        if (h2 < 0) return []; // tangent
        var h = Math.Sqrt(h2);
        var midX = a.Centre.X + along * dx / d;
        var midY = a.Centre.Y + along * dy / d;

        var result = new List<Point2D>();
        foreach (var (ox, oy) in new[] { (h * dy / d, -h * dx / d), (-h * dy / d, h * dx / d) })
        {
            var point = new Point2D(midX + ox, midY + oy);
            if (a.ContainsPoint(point) && b.ContainsPoint(point)) result.Add(point);
        }
        return result;
    }

    /// <summary>Returns true if two curves intersect in their interior (excluding shared endpoints).</summary>
    public static bool CurvesIntersect(Curve2D a, Curve2D b)
    {
        foreach (var pa in Flatten(a))
        foreach (var pb in Flatten(b))
        {
            if (pa is LineSegment2D la && pb is LineSegment2D lb && SegmentsIntersectInterior(la, lb)) return true;
            if (pa is LineSegment2D ll && pb is CircularArc2D aa && LineArc(ll, aa).Count > 0) return true;
            if (pa is CircularArc2D cc && pb is LineSegment2D lll && LineArc(lll, cc).Count > 0) return true;
            if (pa is CircularArc2D c1 && pb is CircularArc2D c2 && ArcArc(c1, c2).Count > 0) return true;
        }
        return false;
    }

    private static IReadOnlyList<Curve2D> Flatten(Curve2D curve) => curve switch
    {
        Polyline2D p => Enumerable.Range(0, p.Points.Count - 1)
            .Select(i => (Curve2D)new LineSegment2D(p.Points[i], p.Points[i + 1])).ToList(),
        _ => [curve],
    };

    private static bool SegmentsIntersectInterior(LineSegment2D a, LineSegment2D b)
    {
        var (dx1, dy1) = (a.End.X - a.Start.X, a.End.Y - a.Start.Y);
        var (dx2, dy2) = (b.End.X - b.Start.X, b.End.Y - b.Start.Y);
        var denom = dx1 * dy2 - dy1 * dx2;
        if (Math.Abs(denom) < 1e-12) return false;
        var t = ((b.Start.X - a.Start.X) * dy2 - (b.Start.Y - a.Start.Y) * dx2) / denom;
        var u = ((b.Start.X - a.Start.X) * dy1 - (b.Start.Y - a.Start.Y) * dx1) / denom;
        return t is > 0.001 and < 0.999 && u is > 0.001 and < 0.999;
    }
}
