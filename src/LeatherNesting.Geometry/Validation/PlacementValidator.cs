namespace LeatherNesting.Geometry.Validation;

/// <summary>Problem type detected during placement validation.</summary>
public enum ValidationProblemType { Overlap, OutOfBounds, GapViolation, AngleViolation, MirrorViolation, QuantityMismatch, OpenContour, OrphanedFeature }

/// <summary>A single validation problem with location and severity.</summary>
public sealed record ValidationProblem(
    ValidationProblemType Type,
    string Severity,
    string Message,
    string? EntityId = null);

/// <summary>Shared geometry validator for placement checking.
/// One rule source, two run modes: preview-lite and authoritative.</summary>
public sealed class PlacementValidator
{
    private readonly ToleranceProfile _tolerance;

    public PlacementValidator(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Validates a set of loops for cuttable state.</summary>
    public PlacementValidationResult Validate(IReadOnlyList<Loop2D> loops)
    {
        var problems = new List<ValidationProblem>();

        foreach (var loop in loops)
        {
            ValidateClosed(loop, problems);
            ValidateSelfIntersection(loop, problems);
        }

        ValidateOverlaps(loops, problems);

        return new PlacementValidationResult(problems);
    }

    /// <summary>Lightweight preview check — skips expensive overlap detection.</summary>
    public PlacementValidationResult PreviewValidate(IReadOnlyList<Loop2D> loops)
    {
        var problems = new List<ValidationProblem>();

        foreach (var loop in loops)
        {
            ValidateClosed(loop, problems);
            ValidateSelfIntersection(loop, problems);
        }

        return new PlacementValidationResult(problems);
    }

    private void ValidateClosed(Loop2D loop, List<ValidationProblem> problems)
    {
        // Check that the last curve's end connects to the first curve's start
        var first = loop.Curves[0].StartPoint;
        var last = loop.Curves[^1].EndPoint;
        if (first.DistanceTo(last) > _tolerance.TopologyToleranceMm)
            problems.Add(new(ValidationProblemType.OpenContour, "Blocking", $"轮廓 {loop.StableId} 未闭合。", loop.StableId));
    }

    private void ValidateSelfIntersection(Loop2D loop, List<ValidationProblem> problems)
    {
        var segments = FlattenToSegments(loop);
        for (var i = 0; i < segments.Count; i++)
        for (var j = i + 1; j < segments.Count; j++)
        {
            if (j == i + 1 || (i == 0 && j == segments.Count - 1)) continue;
            if (LineSegmentsIntersectInterior(segments[i], segments[j]))
            {
                problems.Add(new(ValidationProblemType.Overlap, "Blocking", $"轮廓 {loop.StableId} 自交。", loop.StableId));
                return;
            }
        }
    }

    private void ValidateOverlaps(IReadOnlyList<Loop2D> loops, List<ValidationProblem> problems)
    {
        for (var i = 0; i < loops.Count; i++)
        for (var j = i + 1; j < loops.Count; j++)
        {
            if (LoopsOverlap(loops[i], loops[j]))
                problems.Add(new(ValidationProblemType.Overlap, "Blocking", $"轮廓 {loops[i].StableId} 与 {loops[j].StableId} 重叠。"));
        }
    }

    private static bool LoopsOverlap(Loop2D a, Loop2D b)
    {
        // Simple bounding box check
        var (aMinX, aMinY, aMaxX, aMaxY) = GetBounds(a);
        var (bMinX, bMinY, bMaxX, bMaxY) = GetBounds(b);
        return aMinX < bMaxX && aMaxX > bMinX && aMinY < bMaxY && aMaxY > bMinY;
    }

    private static (double, double, double, double) GetBounds(Loop2D loop)
    {
        var minX = double.MaxValue; var minY = double.MaxValue;
        var maxX = double.MinValue; var maxY = double.MinValue;
        foreach (var curve in loop.Curves)
        {
            var (cMinX, cMinY, cMaxX, cMaxY) = curve.Bounds;
            if (cMinX < minX) minX = cMinX; if (cMinY < minY) minY = cMinY;
            if (cMaxX > maxX) maxX = cMaxX; if (cMaxY > maxY) maxY = cMaxY;
        }
        return (minX, minY, maxX, maxY);
    }

    private static List<LineSegment2D> FlattenToSegments(Loop2D loop)
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
}

public sealed record PlacementValidationResult(IReadOnlyList<ValidationProblem> Problems)
{
    public bool IsValid => Problems.All(p => p.Severity != "Blocking");
    public bool HasWarnings => Problems.Any(p => p.Severity == "Warning");
    public IReadOnlyList<ValidationProblem> Blocking => Problems.Where(p => p.Severity == "Blocking").ToList();
}