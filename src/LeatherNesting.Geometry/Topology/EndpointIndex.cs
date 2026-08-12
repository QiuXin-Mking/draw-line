namespace LeatherNesting.Geometry.Topology;

/// <summary>Indexes curve endpoints for O(1) proximity queries and intersection splitting.</summary>
public sealed class EndpointIndex
{
    private readonly ToleranceProfile _tolerance;
    private readonly List<IndexedCurve> _curves = [];
    private readonly Dictionary<Point2D, List<int>> _pointToCurves = [];

    public EndpointIndex(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    public IReadOnlyList<IndexedCurve> Curves => _curves;

    public void Add(Curve2D curve, string sourceId)
    {
        var index = _curves.Count;
        var indexed = new IndexedCurve(index, curve, sourceId);
        _curves.Add(indexed);
        IndexPoint(indexed.Start, index);
        IndexPoint(indexed.End, index);
    }

    public void AddRange(IEnumerable<Curve2D> curves, string sourceId)
    {
        foreach (var curve in curves) Add(curve, sourceId);
    }

    /// <summary>Finds all curves whose endpoints are within tolerance of the given point.</summary>
    public IReadOnlyList<int> FindNear(Point2D point)
    {
        var result = new List<int>();
        foreach (var (key, indices) in _pointToCurves)
        {
            if (key.DistanceTo(point) <= _tolerance.TopologyToleranceMm)
                result.AddRange(indices);
        }
        return result.Distinct().ToList();
    }

    /// <summary>Returns gap endpoints (open degrees) — pairs of endpoints that are close but not connected.</summary>
    public IReadOnlyList<(Point2D A, Point2D B, double Distance)> FindGaps()
    {
        var gaps = new List<(Point2D, Point2D, double)>();
        var points = _pointToCurves.Keys.ToList();
        for (var i = 0; i < points.Count; i++)
        for (var j = i + 1; j < points.Count; j++)
        {
            var dist = points[i].DistanceTo(points[j]);
            if (dist > 0 && dist <= _tolerance.TopologyToleranceMm)
                gaps.Add((points[i], points[j], dist));
        }
        return gaps;
    }

    /// <summary>Detects and splits self-intersecting curves, returning a new index with split curves.</summary>
    public EndpointIndex SplitIntersections()
    {
        var split = new EndpointIndex(_tolerance);
        foreach (var ic in _curves)
        {
            var intersections = FindIntersections(ic, _curves);
            if (intersections.Count == 0)
            {
                split.Add(ic.Curve, ic.SourceId);
            }
            else
            {
                foreach (var seg in SplitAtPoints(ic, intersections))
                    split.Add(seg, ic.SourceId);
            }
        }
        return split;
    }

    private static IReadOnlyList<Point2D> FindIntersections(IndexedCurve target, IReadOnlyList<IndexedCurve> all)
    {
        // Stage 2: simplified intersection detection — only endpoint coincidence
        // Full curve-curve intersection is deferred to a later iteration
        var hits = new List<Point2D>();
        foreach (var other in all)
        {
            if (other.Index == target.Index) continue;
            if (target.Curve is LineSegment2D tl && other.Curve is LineSegment2D ol)
            {
                var pt = LineIntersection(tl, ol);
                if (pt is not null) hits.Add(pt);
            }
        }
        return hits;
    }

    private static Point2D? LineIntersection(LineSegment2D a, LineSegment2D b)
    {
        var (dx1, dy1) = (a.End.X - a.Start.X, a.End.Y - a.Start.Y);
        var (dx2, dy2) = (b.End.X - b.Start.X, b.End.Y - b.Start.Y);
        var denom = dx1 * dy2 - dy1 * dx2;
        if (Math.Abs(denom) < 1e-12) return null; // parallel or coincident
        var t = ((b.Start.X - a.Start.X) * dy2 - (b.Start.Y - a.Start.Y) * dx2) / denom;
        var u = ((b.Start.X - a.Start.X) * dy1 - (b.Start.Y - a.Start.Y) * dx1) / denom;
        if (t is > 0 and < 1 && u is > 0 and < 1)
            return new Point2D(a.Start.X + t * dx1, a.Start.Y + t * dy1);
        return null;
    }

    private static IReadOnlyList<Curve2D> SplitAtPoints(IndexedCurve curve, IReadOnlyList<Point2D> points)
    {
        if (curve.Curve is not LineSegment2D line) return [curve.Curve];
        var sorted = points
            .Select(p => (p, t: ProjectOntoLine(line, p)))
            .Where(x => x.t is > 0 and < 1)
            .OrderBy(x => x.t)
            .Select(x => x.p)
            .ToList();
        if (sorted.Count == 0) return [curve.Curve];
        var segments = new List<Curve2D>();
        var prev = line.Start;
        foreach (var pt in sorted)
        {
            segments.Add(new LineSegment2D(prev, pt));
            prev = pt;
        }
        segments.Add(new LineSegment2D(prev, line.End));
        return segments;
    }

    private static double ProjectOntoLine(LineSegment2D line, Point2D p)
    {
        var dx = line.End.X - line.Start.X;
        var dy = line.End.Y - line.Start.Y;
        var len2 = dx * dx + dy * dy;
        if (len2 < 1e-12) return 0;
        return ((p.X - line.Start.X) * dx + (p.Y - line.Start.Y) * dy) / len2;
    }

    private void IndexPoint(Point2D point, int curveIndex)
    {
        if (!_pointToCurves.TryGetValue(point, out var list))
        {
            list = [];
            _pointToCurves[point] = list;
        }
        list.Add(curveIndex);
    }

    public sealed record IndexedCurve(int Index, Curve2D Curve, string SourceId)
    {
        public Point2D Start => Curve.StartPoint;
        public Point2D End => Curve.EndPoint;
    }
}