using Clipper2Lib;

namespace LeatherNesting.Geometry.Nesting;

/// <summary>Computes No-Fit Polygons (NFP) via Clipper2 Minkowski sums.
/// NFP(a, b) is the region where placing b's reference point would make b overlap a.</summary>
public sealed class NfpCalculator
{
    private readonly long _scale;
    private readonly ToleranceProfile _tolerance;

    public NfpCalculator(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
        _scale = GeometryConstants.IntegerScale;
    }

    /// <summary>Computes the NFP of <paramref name="b"/> relative to <paramref name="a"/>.
    /// Returned polygons are in absolute coordinates: placing b's reference point
    /// (its bounding-box bottom-left) inside any returned polygon means b overlaps a.</summary>
    public IReadOnlyList<Loop2D> Nfp(Loop2D a, Loop2D b)
    {
        var pathA = ClipperPathAdapter.ToPath64(a, _scale, _tolerance);
        if (pathA.Count < 3)
            return [];

        // Shift b so its reference point (bbox bottom-left) sits at the origin,
        // then NFP = a ⊕ (-b) = MinkowskiDiff(a, b-shifted).
        var (minX, minY, _, _) = BoundsOf(b);
        var shifted = new Transform2D(-minX, -minY, 0, false).Apply(b);
        var pathB = ClipperPathAdapter.ToPath64(shifted, _scale, _tolerance);
        if (pathB.Count < 3)
            return [];

        var nfpPaths = Clipper.MinkowskiDiff(pathA, pathB, true);
        return nfpPaths
            .Select((p, i) => PathToLoop(p, $"{a.StableId}-nfp-{b.StableId}-{i}"))
            .Where(loop => loop is not null)
            .Cast<Loop2D>()
            .ToList();
    }

    private Loop2D? PathToLoop(Path64 path, string id)
    {
        var points = path.Select(p => ClipperPathAdapter.ToPoint2D(p, _scale)).ToList();
        if (points.Count > 1 && points[0].DistanceTo(points[^1]) > _tolerance.TopologyToleranceMm)
            points.Add(points[0]);
        if (points.Count < 4)
            return null;
        return new Loop2D(id, LoopRole.Outer, [new Polyline2D(points)]);
    }

    private static (double MinX, double MinY, double MaxX, double MaxY) BoundsOf(Loop2D loop)
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        foreach (var curve in loop.Curves)
        {
            var (cMinX, cMinY, cMaxX, cMaxY) = curve.Bounds;
            if (cMinX < minX) minX = cMinX;
            if (cMinY < minY) minY = cMinY;
            if (cMaxX > maxX) maxX = cMaxX;
            if (cMaxY > maxY) maxY = cMaxY;
        }
        return (minX, minY, maxX, maxY);
    }
}
