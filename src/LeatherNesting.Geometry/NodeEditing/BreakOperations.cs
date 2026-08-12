namespace LeatherNesting.Geometry.NodeEditing;

/// <summary>Break/cut operations on contours: single-point break and two-point segment removal.</summary>
public sealed class BreakOperations
{
    private readonly ToleranceProfile _tolerance;

    public BreakOperations(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Breaks a closed loop at a single point, opening it at that location.</summary>
    public BreakResult BreakAtPoint(Loop2D loop, Point2D breakPoint)
    {
        var nodeOps = new NodeOperations(_tolerance);
        var (curveIndex, t) = FindNearestPointOnLoop(loop, breakPoint);

        if (curveIndex < 0)
            return new BreakResult(loop, null, false, ["未找到断点位置。"]);

        var curve = loop.Curves[curveIndex];
        var actualPoint = curve.PointAt(t);

        // Rotate the curve order so the break point becomes the start/end
        var rotated = RotateCurves(loop, curveIndex, t);
        // Open the loop: remove the closing segment
        var openCurves = rotated.Take(rotated.Count - 1).ToList();

        if (openCurves.Count == 0)
            return new BreakResult(loop, null, false, ["断点操作后没有剩余曲线。"]);

        return new BreakResult(loop, openCurves, true, []);
    }

    /// <summary>Removes a segment between two points on the loop, creating two open sub-loops.</summary>
    public BreakResult RemoveSegment(Loop2D loop, Point2D pointA, Point2D pointB)
    {
        var (idxA, tA) = FindNearestPointOnLoop(loop, pointA);
        var (idxB, tB) = FindNearestPointOnLoop(loop, pointB);

        if (idxA < 0 || idxB < 0)
            return new BreakResult(loop, null, false, ["未找到断点位置。"]);

        // Ensure idxA < idxB by rotating
        if (idxA > idxB || (idxA == idxB && tA > tB))
        {
            (idxA, tA, idxB, tB) = (idxB, tB, idxA, tA);
        }

        var allCurves = loop.Curves.ToList();
        var remaining = new List<Curve2D>();

        // Keep curves before idxA
        for (var i = 0; i < idxA; i++)
            remaining.Add(allCurves[i]);

        // Keep the first part of curve at idxA
        if (tA > 0.001)
        {
            var (before, _) = SplitCurve(allCurves[idxA], tA);
            remaining.Add(before);
        }

        // Keep the second part of curve at idxB
        if (tB < 0.999)
        {
            var (_, after) = SplitCurve(allCurves[idxB], tB);
            remaining.Add(after);
        }

        // Keep curves after idxB
        for (var i = idxB + 1; i < allCurves.Count; i++)
            remaining.Add(allCurves[i]);

        if (remaining.Count == 0)
            return new BreakResult(loop, null, false, ["去段后没有剩余曲线。"]);

        return new BreakResult(loop, remaining, true, []);
    }

    private static (int CurveIndex, double T) FindNearestPointOnLoop(Loop2D loop, Point2D point)
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

    private static (double, double) ProjectToCurve(Curve2D curve, Point2D p)
    {
        if (curve is LineSegment2D l)
        {
            var dx = l.End.X - l.Start.X;
            var dy = l.End.Y - l.Start.Y;
            var len2 = dx * dx + dy * dy;
            if (len2 < 1e-12) return (0, l.Start.DistanceTo(p));
            var t = Math.Clamp(((p.X - l.Start.X) * dx + (p.Y - l.Start.Y) * dy) / len2, 0, 1);
            return (t, l.PointAt(t).DistanceTo(p));
        }
        if (curve is Polyline2D poly)
        {
            var bestT = 0.0; var bestDist = double.MaxValue;
            for (var i = 0; i < poly.Points.Count - 1; i++)
            {
                var seg = new LineSegment2D(poly.Points[i], poly.Points[i + 1]);
                var (segT, d) = ProjectToCurve(seg, p);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestT = (i + segT) / (poly.Points.Count - 1);
                }
            }
            return (bestT, bestDist);
        }
        if (curve is CircularArc2D arc)
        {
            var toPoint = p - arc.Centre;
            var angle = Math.Atan2(toPoint.Y, toPoint.X) * 180 / Math.PI;
            var sa = arc.StartAngleDegrees;
            var ea = sa + arc.SweepAngleDegrees;
            var clamped = Math.Clamp(angle, Math.Min(sa, ea), Math.Max(sa, ea));
            var t = (clamped - sa) / arc.SweepAngleDegrees;
            return (t, arc.PointAt(t).DistanceTo(p));
        }
        return (0, double.MaxValue);
    }

    private static (Curve2D, Curve2D) SplitCurve(Curve2D curve, double t)
    {
        return curve switch
        {
            LineSegment2D l => (
                new LineSegment2D(l.Start, l.PointAt(t)),
                new LineSegment2D(l.PointAt(t), l.End)),
            CircularArc2D a => (
                new CircularArc2D(a.Centre, a.Radius, a.StartAngleDegrees, a.SweepAngleDegrees * t),
                new CircularArc2D(a.Centre, a.Radius, a.StartAngleDegrees + a.SweepAngleDegrees * t, a.SweepAngleDegrees * (1 - t))),
            _ => (curve, curve)
        };
    }

    private static IReadOnlyList<Curve2D> RotateCurves(Loop2D loop, int curveIndex, double t)
    {
        var all = loop.Curves.ToList();
        if (t > 0.001)
        {
            var (_, after) = SplitCurve(all[curveIndex], t);
            all[curveIndex] = after;
        }
        return all.Skip(curveIndex).Concat(all.Take(curveIndex)).ToList();
    }
}

public sealed record BreakResult(Loop2D Original, IReadOnlyList<Curve2D>? RemainingCurves, bool Success, IReadOnlyList<string> Issues);