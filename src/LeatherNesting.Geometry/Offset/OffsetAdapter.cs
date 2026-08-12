using Clipper2Lib;

namespace LeatherNesting.Geometry.Offset;

/// <summary>Offsets contours using Clipper2 behind an adapter boundary.
/// Inputs are checked for simplicity/closure; self-intersecting input is blocked.</summary>
public sealed class OffsetAdapter
{
    private readonly ToleranceProfile _tolerance;
    private readonly double _scale;

    public OffsetAdapter(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
        _scale = GeometryConstants.IntegerScale;
    }

    /// <summary>Offsets a set of loops (outer + holes) as a whole, preserving containment.</summary>
    public OffsetResult Offset(
        IReadOnlyList<Loop2D> loops,
        double offsetDistanceMm,
        OffsetDirection direction,
        OffsetJoinStyle joinStyle = OffsetJoinStyle.Miter)
    {
        if (double.IsNaN(offsetDistanceMm) || double.IsInfinity(offsetDistanceMm))
            return EmptyResult(loops, offsetDistanceMm, direction, ["offset 距离不得为 NaN 或 Infinity。"]);

        var absDistance = Math.Abs(offsetDistanceMm);
        if (absDistance < _tolerance.TopologyToleranceMm)
            return new OffsetResult(loops, loops, offsetDistanceMm, direction, false, [], []);

        // Validate inputs: all loops must be simple and closed
        var diagnostics = ValidateInputs(loops);
        if (diagnostics.Count > 0)
            return EmptyResult(loops, offsetDistanceMm, direction, diagnostics);

        // Determine Clipper2 sign: positive = outward, negative = inward
        // Outer loops offset outward grows; holes offset outward shrinks
        var topoWarnings = new List<string>();
        var resultLoops = new List<Loop2D>();
        var topologyChanged = false;

        try
        {
            foreach (var loop in loops)
            {
                // Normalize winding so the sign below is independent of the input curve order.
                var oriented = loop.NormalizeWinding();
                var sign = direction switch
                {
                    OffsetDirection.Inside => oriented.Role == LoopRole.Outer ? -1.0 : 1.0,
                    OffsetDirection.Outside => oriented.Role == LoopRole.Outer ? 1.0 : -1.0,
                    _ => 1.0
                };
                var clipperDelta = sign * absDistance * _scale;
                var scaledDelta = (long)Math.Round(clipperDelta);

                var path = LoopToPath64(oriented);
                var co = new ClipperOffset();
                co.AddPath(path, ToClipperJoin(joinStyle), loop.Role == LoopRole.Outer ? EndType.Polygon : EndType.Polygon);

                var solution = new Paths64();
                co.Execute(scaledDelta, solution);

                if (solution.Count == 0)
                {
                    topologyChanged = true;
                    topoWarnings.Add($"轮廓 {loop.StableId} offset 后消失（1→0）。");
                    continue;
                }

                if (solution.Count > 1)
                {
                    topologyChanged = true;
                    topoWarnings.Add($"轮廓 {loop.StableId} offset 后分裂为 {solution.Count} 个独立环（1→{solution.Count}）。");
                }

                for (var i = 0; i < solution.Count; i++)
                {
                    var points = Path64ToPoints(solution[i]);
                    var polyline = new Polyline2D(points);
                    resultLoops.Add(new Loop2D(
                        $"{loop.StableId}-offset-{i + 1}",
                        loop.Role,
                        [polyline]));
                }
            }
        }
        catch (Exception ex)
        {
            diagnostics.Add($"offset 操作失败：{ex.Message}");
            return EmptyResult(loops, offsetDistanceMm, direction, diagnostics);
        }

        return new OffsetResult(
            resultLoops,
            loops,
            offsetDistanceMm,
            direction,
            topologyChanged,
            topoWarnings,
            diagnostics);
    }

    private List<string> ValidateInputs(IReadOnlyList<Loop2D> loops)
    {
        var issues = new List<string>();
        foreach (var loop in loops)
        {
            if (loop.Curves.Count < 1)
                issues.Add($"轮廓 {loop.StableId} 没有曲线。");
            // Check for self-intersection
            var segments = FlattenLoopToSegments(loop);
            for (var i = 0; i < segments.Count; i++)
            for (var j = i + 1; j < segments.Count; j++)
            {
                if (j == i + 1 || (i == 0 && j == segments.Count - 1)) continue;
                if (LineSegmentsIntersectInterior(segments[i], segments[j]))
                {
                    issues.Add($"轮廓 {loop.StableId} 自交，无法 offset。");
                    break;
                }
            }
        }
        return issues;
    }

    private static List<LineSegment2D> FlattenLoopToSegments(Loop2D loop)
    {
        return loop.Curves.SelectMany(c => c switch
        {
            LineSegment2D l => [l],
            Polyline2D p => Enumerable.Range(0, p.Points.Count - 1)
                .Select(i => new LineSegment2D(p.Points[i], p.Points[i + 1])),
            CircularArc2D a => [new LineSegment2D(a.StartPoint, a.EndPoint)],
            _ => new List<LineSegment2D>()
        }).ToList();
    }

    private static bool LineSegmentsIntersectInterior(LineSegment2D a, LineSegment2D b)
    {
        var (dx1, dy1) = (a.End.X - a.Start.X, a.End.Y - a.Start.Y);
        var (dx2, dy2) = (b.End.X - b.Start.X, b.End.Y - b.Start.Y);
        var denom = dx1 * dy2 - dy1 * dx2;
        if (Math.Abs(denom) < 1e-12) return false;
        var t = ((b.Start.X - a.Start.X) * dy2 - (b.Start.Y - a.Start.Y) * dx2) / denom;
        var u = ((b.Start.X - a.Start.X) * dy1 - (b.Start.Y - a.Start.Y) * dx1) / denom;
        return t is > 0.001 and < 0.999 && u is > 0.001 and < 0.999;
    }

    private static OffsetResult EmptyResult(IReadOnlyList<Loop2D> loops, double distance, OffsetDirection direction, List<string> diagnostics) =>
        new([], loops, distance, direction, false, [], diagnostics);

    private Path64 LoopToPath64(Loop2D loop)
    {
        var points = loop.Curves.SelectMany(c => c switch
        {
            LineSegment2D l => new[] { l.Start, l.End },
            Polyline2D p => FlattenPolyline(p.Points),
            CircularArc2D a => FlattenArc(a),
            _ => Array.Empty<Point2D>()
        }).ToList();

        // Deduplicate consecutive identical points
        var deduped = new List<Point2D>();
        foreach (var p in points)
        {
            if (deduped.Count == 0 || deduped[^1].DistanceTo(p) > _tolerance.TopologyToleranceMm)
                deduped.Add(p);
        }

        if (deduped.Count < 3) return new Path64();
        return new Path64(deduped.Select(p => new Point64(
            (long)Math.Round(p.X * _scale),
            (long)Math.Round(p.Y * _scale))));
    }

    private static IReadOnlyList<Point2D> FlattenPolyline(IReadOnlyList<Point2D> points)
    {
        return points;
    }

    private IReadOnlyList<Point2D> FlattenArc(CircularArc2D arc)
    {
        var chordLen = arc.StartPoint.DistanceTo(arc.EndPoint);
        var n = (int)Math.Max(2, Math.Ceiling(chordLen / _tolerance.FlattenChordToleranceMm));
        var points = new List<Point2D>(n + 1);
        for (var i = 0; i <= n; i++)
            points.Add(arc.PointAt((double)i / n));
        return points;
    }

    private IReadOnlyList<Point2D> Path64ToPoints(Path64 path)
    {
        return path.Select(p => new Point2D((double)p.X / _scale, (double)p.Y / _scale)).ToList();
    }

    private static JoinType ToClipperJoin(OffsetJoinStyle style) => style switch
    {
        OffsetJoinStyle.Miter => JoinType.Miter,
        OffsetJoinStyle.Square => JoinType.Square,
        OffsetJoinStyle.Round => JoinType.Round,
        _ => JoinType.Miter
    };
}