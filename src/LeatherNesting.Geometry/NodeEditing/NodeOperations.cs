namespace LeatherNesting.Geometry.NodeEditing;

/// <summary>Node display, insert, move, and delete operations on loop contours.</summary>
public sealed class NodeOperations
{
    private readonly ToleranceProfile _tolerance;

    public NodeOperations(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Extracts all distinct node points from a loop.</summary>
    public IReadOnlyList<Point2D> GetNodes(Loop2D loop)
    {
        var nodes = new List<Point2D>();
        foreach (var curve in loop.Curves)
        {
            if (curve is LineSegment2D l)
            {
                nodes.Add(l.Start);
                nodes.Add(l.End);
            }
            else if (curve is Polyline2D p)
            {
                nodes.AddRange(p.Points);
            }
            else if (curve is CircularArc2D a)
            {
                nodes.Add(a.StartPoint);
                nodes.Add(a.EndPoint);
            }
        }
        return DeduplicateConsecutive(nodes);
    }

    /// <summary>Inserts a node at the nearest point on the loop, splitting the containing curve.</summary>
    public NodeEditResult InsertNode(Loop2D loop, Point2D insertionPoint)
    {
        var (curveIndex, t) = FindNearestPoint(loop, insertionPoint);
        if (curveIndex < 0)
            return new NodeEditResult(loop, insertionPoint, false, ["未找到插入点。"]);

        var curve = loop.Curves[curveIndex];
        var splitPoint = curve.PointAt(t);

        if (splitPoint.DistanceTo(curve.StartPoint) < _tolerance.TopologyToleranceMm ||
            splitPoint.DistanceTo(curve.EndPoint) < _tolerance.TopologyToleranceMm)
            return new NodeEditResult(loop, insertionPoint, false, ["插入点与现有节点过近。"]);

        var (before, after) = SplitCurve(curve, t);
        var newCurves = loop.Curves.ToList();
        newCurves.RemoveAt(curveIndex);
        newCurves.Insert(curveIndex, after);
        newCurves.Insert(curveIndex, before);

        var newLoop = new Loop2D(loop.StableId, loop.Role, newCurves);
        return new NodeEditResult(newLoop, splitPoint, true, []);
    }

    /// <summary>Moves a node to a new position, validating the result.</summary>
    public NodeEditResult MoveNode(Loop2D loop, int nodeIndex, Point2D newPosition)
    {
        var nodes = GetNodes(loop).ToList();
        if (nodeIndex < 0 || nodeIndex >= nodes.Count)
            return new NodeEditResult(loop, newPosition, false, ["节点索引越界。"]);

        // Flatten all curves to points and move the target
        var allPoints = FlattenPoints(loop).ToList();
        var targetIndex = FindPointIndex(allPoints, nodes[nodeIndex]);
        if (targetIndex < 0)
            return new NodeEditResult(loop, newPosition, false, ["未找到目标节点。"]);

        allPoints[targetIndex] = newPosition;

        // Rebuild curves
        var newLoop = RebuildLoop(loop, allPoints);
        if (newLoop is null)
            return new NodeEditResult(loop, newPosition, false, ["移动节点导致自交或无效轮廓。"]);

        return new NodeEditResult(newLoop, newPosition, true, []);
    }

    /// <summary>Deletes a node from the loop. Blocked if fewer than 3 unique points remain.</summary>
    public NodeEditResult DeleteNode(Loop2D loop, int nodeIndex)
    {
        var nodes = GetNodes(loop).ToList();
        if (nodes.Count <= 3)
            return new NodeEditResult(loop, Point2D.Origin, false, ["删除节点将导致少于 3 个点，无法保持闭合轮廓。"]);

        if (nodeIndex < 0 || nodeIndex >= nodes.Count)
            return new NodeEditResult(loop, Point2D.Origin, false, ["节点索引越界。"]);

        var allPoints = FlattenPoints(loop).ToList();
        var targetIndex = FindPointIndex(allPoints, nodes[nodeIndex]);
        if (targetIndex < 0)
            return new NodeEditResult(loop, Point2D.Origin, false, ["未找到目标节点。"]);

        allPoints.RemoveAt(targetIndex);

        var newLoop = RebuildLoop(loop, allPoints);
        if (newLoop is null)
            return new NodeEditResult(loop, Point2D.Origin, false, ["删除节点导致自交或无效轮廓。"]);

        return new NodeEditResult(newLoop, nodes[nodeIndex], true, []);
    }

    private (Curve2D Before, Curve2D After) SplitCurve(Curve2D curve, double t)
    {
        return curve switch
        {
            LineSegment2D l => (
                new LineSegment2D(l.Start, l.PointAt(t)),
                new LineSegment2D(l.PointAt(t), l.End)),
            Polyline2D p => SplitPolyline(p, t),
            CircularArc2D a => (
                new CircularArc2D(a.Centre, a.Radius, a.StartAngleDegrees, a.SweepAngleDegrees * t),
                new CircularArc2D(a.Centre, a.Radius, a.StartAngleDegrees + a.SweepAngleDegrees * t, a.SweepAngleDegrees * (1 - t))),
            _ => throw new NotSupportedException($"不支持的曲线类型：{curve.GetType().Name}")
        };
    }

    private static (Curve2D, Curve2D) SplitPolyline(Polyline2D poly, double t)
    {
        var targetLen = poly.Length * t;
        var accum = 0.0;
        for (var i = 0; i < poly.Points.Count - 1; i++)
        {
            var segLen = poly.Points[i].DistanceTo(poly.Points[i + 1]);
            if (accum + segLen >= targetLen)
            {
                var segT = (targetLen - accum) / segLen;
                var splitPoint = new Point2D(
                    poly.Points[i].X + (poly.Points[i + 1].X - poly.Points[i].X) * segT,
                    poly.Points[i].Y + (poly.Points[i + 1].Y - poly.Points[i].Y) * segT);
                var before = new Polyline2D([.. poly.Points.Take(i + 1), splitPoint]);
                var after = new Polyline2D([splitPoint, .. poly.Points.Skip(i + 1)]);
                return (before, after);
            }
            accum += segLen;
        }
        return (poly, poly);
    }

    private (int CurveIndex, double T) FindNearestPoint(Loop2D loop, Point2D point)
    {
        var bestDist = double.MaxValue;
        var bestCurve = -1;
        var bestT = 0.0;
        for (var i = 0; i < loop.Curves.Count; i++)
        {
            var (t, dist) = ProjectToCurve(loop.Curves[i], point);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestCurve = i;
                bestT = t;
            }
        }
        return (bestCurve, bestT);
    }

    private static (double T, double Dist) ProjectToCurve(Curve2D curve, Point2D point)
    {
        return curve switch
        {
            LineSegment2D l => ProjectToLine(l, point),
            Polyline2D p => ProjectToPolyline(p, point),
            CircularArc2D a => ProjectToArc(a, point),
            _ => (0, double.MaxValue)
        };
    }

    private static (double, double) ProjectToLine(LineSegment2D line, Point2D p)
    {
        var dx = line.End.X - line.Start.X;
        var dy = line.End.Y - line.Start.Y;
        var len2 = dx * dx + dy * dy;
        if (len2 < 1e-12) return (0, line.Start.DistanceTo(p));
        var t = Math.Clamp(((p.X - line.Start.X) * dx + (p.Y - line.Start.Y) * dy) / len2, 0, 1);
        var proj = line.PointAt(t);
        return (t, proj.DistanceTo(p));
    }

    private static (double, double) ProjectToPolyline(Polyline2D poly, Point2D p)
    {
        var bestT = 0.0;
        var bestDist = double.MaxValue;
        for (var i = 0; i < poly.Points.Count - 1; i++)
        {
            var seg = new LineSegment2D(poly.Points[i], poly.Points[i + 1]);
            var (segT, dist) = ProjectToLine(seg, p);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestT = (i + segT) / (poly.Points.Count - 1);
            }
        }
        return (bestT, bestDist);
    }

    private static (double, double) ProjectToArc(CircularArc2D arc, Point2D p)
    {
        var toPoint = p - arc.Centre;
        var angle = Math.Atan2(toPoint.Y, toPoint.X) * 180 / Math.PI;
        var startAngle = arc.StartAngleDegrees;
        var endAngle = startAngle + arc.SweepAngleDegrees;
        var clamped = Math.Clamp(angle, Math.Min(startAngle, endAngle), Math.Max(startAngle, endAngle));
        var t = (clamped - startAngle) / arc.SweepAngleDegrees;
        var proj = arc.PointAt(t);
        return (t, proj.DistanceTo(p));
    }

    private static List<Point2D> FlattenPoints(Loop2D loop)
    {
        return loop.Curves.SelectMany(c => c switch
        {
            LineSegment2D l => new[] { l.Start, l.End },
            Polyline2D p => p.Points,
            CircularArc2D a => new[] { a.StartPoint, a.EndPoint },
            _ => Array.Empty<Point2D>()
        }).ToList();
    }

    private static int FindPointIndex(IReadOnlyList<Point2D> points, Point2D target)
    {
        for (var i = 0; i < points.Count; i++)
        {
            if (points[i].DistanceTo(target) < 1e-9) return i;
        }
        return -1;
    }

    private static Loop2D? RebuildLoop(Loop2D original, IReadOnlyList<Point2D> points)
    {
        // Simple rebuild: create polyline segments. For arcs, this loses curvature.
        // A more sophisticated rebuild would preserve arc metadata.
        if (points.Count < 3) return null;

        // Check self-intersection
        for (var i = 0; i < points.Count - 1; i++)
        for (var j = i + 1; j < points.Count - 1; j++)
        {
            if (j == i + 1 || (i == 0 && j == points.Count - 2)) continue;
            var a = new LineSegment2D(points[i], points[i + 1]);
            var b = new LineSegment2D(points[j], points[j + 1]);
            var (dx1, dy1) = (a.End.X - a.Start.X, a.End.Y - a.Start.Y);
            var (dx2, dy2) = (b.End.X - b.Start.X, b.End.Y - b.Start.Y);
            var denom = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(denom) < 1e-12) continue;
            var t = ((b.Start.X - a.Start.X) * dy2 - (b.Start.Y - a.Start.Y) * dx2) / denom;
            var u = ((b.Start.X - a.Start.X) * dy1 - (b.Start.Y - a.Start.Y) * dx1) / denom;
            if (t is > 0.001 and < 0.999 && u is > 0.001 and < 0.999) return null;
        }

        var segments = new List<Curve2D>();
        for (var i = 0; i < points.Count - 1; i++)
            segments.Add(new LineSegment2D(points[i], points[i + 1]));
        segments.Add(new LineSegment2D(points[^1], points[0]));

        return new Loop2D(original.StableId, original.Role, segments);
    }

    private static IReadOnlyList<Point2D> DeduplicateConsecutive(IReadOnlyList<Point2D> points)
    {
        var result = new List<Point2D>();
        foreach (var p in points)
        {
            if (result.Count == 0 || result[^1].DistanceTo(p) > 1e-9)
                result.Add(p);
        }
        return result;
    }
}

public sealed record NodeEditResult(Loop2D Loop, Point2D AffectedPoint, bool Success, IReadOnlyList<string> Issues);