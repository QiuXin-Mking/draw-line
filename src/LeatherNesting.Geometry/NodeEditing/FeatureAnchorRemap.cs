namespace LeatherNesting.Geometry.NodeEditing;

/// <summary>Remaps feature anchors after contour editing using arc-length or local geometry.
/// Features that cannot be uniquely remapped become Orphaned.</summary>
public sealed class FeatureAnchorRemap
{
    private readonly ToleranceProfile _tolerance;

    public FeatureAnchorRemap(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Remaps a feature anchor from the original loop to the modified loop.</summary>
    public AnchorRemapResult Remap(Loop2D original, Loop2D modified, double originalArcLength)
    {
        if (original.Length <= 0)
            return new AnchorRemapResult(originalArcLength, null, AnchorRemapStatus.Orphaned, "原轮廓长度为 0。");

        var ratio = originalArcLength / original.Length;
        var clamped = Math.Clamp(ratio, 0, 1);

        // Try arc-length remapping first
        var targetPoint = modified.PointAt(clamped);
        var (nearest, distance) = FindNearestPointOnLoop(modified, targetPoint);

        if (distance < _tolerance.TopologyToleranceMm)
        {
            var newArcLength = clamped * modified.Length;
            return new AnchorRemapResult(originalArcLength, newArcLength, AnchorRemapStatus.Remapped, "");
        }

        return new AnchorRemapResult(originalArcLength, null, AnchorRemapStatus.Orphaned,
            $"锚点无法唯一重映射：最近点距离 {distance:F3}mm 超过公差。");
    }

    /// <summary>Batch remaps anchors for all features on a loop.</summary>
    public IReadOnlyList<AnchorRemapResult> RemapAll(
        Loop2D original,
        Loop2D modified,
        IReadOnlyList<double> originalArcLengths)
    {
        return originalArcLengths.Select(al => Remap(original, modified, al)).ToList();
    }

    private static (double T, double Dist) FindNearestPointOnLoop(Loop2D loop, Point2D point)
    {
        var bestDist = double.MaxValue;
        var bestT = 0.0;
        var totalLength = loop.Length;
        var accum = 0.0;

        foreach (var curve in loop.Curves)
        {
            var (t, dist) = ProjectToCurve(curve, point);
            var globalT = (accum + t * curve.Length) / totalLength;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestT = globalT;
            }
            accum += curve.Length;
        }
        return (bestT, bestDist);
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
        return (0, curve.StartPoint.DistanceTo(p));
    }
}

public enum AnchorRemapStatus { Remapped, Orphaned }

public sealed record AnchorRemapResult(
    double OriginalArcLength,
    double? NewArcLength,
    AnchorRemapStatus Status,
    string Message);