using Clipper2Lib;

namespace LeatherNesting.Geometry.Nesting;

/// <summary>Precise overlap / gap / boundary checks backed by Clipper2 boolean operations.
/// Edge-to-edge contact is allowed; any area intersection, gap shortfall, or out-of-bounds is rejected.</summary>
public sealed class ClipperCollisionDetector
{
    private readonly long _scale;
    private readonly ToleranceProfile _tolerance;

    public ClipperCollisionDetector(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
        _scale = GeometryConstants.IntegerScale;
    }

    /// <summary>True when the candidate contour is fully inside the material, clear of all placed pieces,
    /// and keeps at least <paramref name="gapMm"/> from both material edges and placed pieces.</summary>
    public bool IsPlacementValid(
        Loop2D candidate,
        IReadOnlyList<Loop2D> placed,
        Loop2D material,
        double gapMm)
    {
        var candidatePath = ClipperPathAdapter.ToPath64(candidate, _scale, _tolerance);
        if (candidatePath.Count < 3)
            return false;

        var materialPath = ClipperPathAdapter.ToPath64(material, _scale, _tolerance);
        if (materialPath.Count < 3)
            return false;

        // Inflate candidate by gap once; the inflated shape must stay inside the material
        // and stay clear of every placed piece. This enforces both edge gaps simultaneously.
        var inflated = gapMm > 0 ? Inflate(candidatePath, gapMm) : candidatePath;

        if (!InsideMaterial(inflated, materialPath))
            return false;

        foreach (var p in placed)
        {
            var placedPath = ClipperPathAdapter.ToPath64(p, _scale, _tolerance);
            if (placedPath.Count < 3)
                continue;
            if (PathsOverlap(inflated, placedPath))
                return false;
        }

        return true;
    }

    /// <summary>True when the two loops have any positive-area intersection.</summary>
    public bool Overlaps(Loop2D a, Loop2D b) => PathsOverlap(
        ClipperPathAdapter.ToPath64(a, _scale, _tolerance),
        ClipperPathAdapter.ToPath64(b, _scale, _tolerance));

    private static bool InsideMaterial(Path64 candidate, Path64 material)
    {
        var diff = Clipper.Difference(new Paths64 { candidate }, new Paths64 { material }, FillRule.NonZero);
        return diff.Count == 0;
    }

    private static bool PathsOverlap(Path64 a, Path64 b)
    {
        if (a.Count < 3 || b.Count < 3)
            return false;
        var result = Clipper.Intersect(new Paths64 { a }, new Paths64 { b }, FillRule.NonZero);
        return result.Count > 0;
    }

    private Path64 Inflate(Path64 path, double deltaMm)
    {
        // ClipperOffset grows outward only for positively-wound (CCW) paths; respect the input sign.
        var sign = Clipper.Area(path) >= 0 ? 1 : -1;
        var delta = (long)Math.Round(sign * deltaMm * _scale);
        var co = new ClipperOffset();
        co.AddPath(path, JoinType.Miter, EndType.Polygon);
        var solution = new Paths64();
        co.Execute(delta, solution);
        return solution.Count > 0 ? solution[0] : path;
    }
}
