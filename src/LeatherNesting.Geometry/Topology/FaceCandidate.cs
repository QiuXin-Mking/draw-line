namespace LeatherNesting.Geometry.Topology;

/// <summary>A candidate face (closed loop) extracted from the planar graph, requiring user selection.</summary>
public sealed record FaceCandidate
{
    public string CandidateId { get; }
    public IReadOnlyList<Curve2D> Curves { get; }
    public double Area { get; }
    public bool IsSelfIntersecting { get; }
    public bool IsValid { get; }
    public IReadOnlyList<string> Issues { get; }

    public FaceCandidate(
        string candidateId,
        IReadOnlyList<Curve2D> curves,
        ToleranceProfile? tolerance = null)
    {
        var tol = tolerance ?? ToleranceProfile.Default;
        CandidateId = candidateId;
        Curves = curves;
        Area = ComputeArea(curves);
        var issues = new List<string>();
        IsSelfIntersecting = CheckSelfIntersection(curves, tol, issues);
        IsValid = !IsSelfIntersecting;
        Issues = issues;
    }

    /// <summary>Generates all closed face candidates from the planar graph.</summary>
    public static IReadOnlyList<FaceCandidate> FromGraph(PlanarGraph graph)
    {
        var loops = graph.ExtractClosedLoops();
        return loops.Select((loop, i) =>
        {
            var curves = loop.Select(eid => graph.Edges[eid].Curve).ToList();
            return new FaceCandidate($"face-{i + 1}", curves);
        }).ToList();
    }

    private static double ComputeArea(IReadOnlyList<Curve2D> curves)
    {
        var sum = 0.0;
        foreach (var curve in curves)
        {
            if (curve is LineSegment2D l)
                sum += l.Start.X * l.End.Y - l.End.X * l.Start.Y;
            else if (curve is Polyline2D p)
                for (var i = 0; i < p.Points.Count - 1; i++)
                    sum += p.Points[i].X * p.Points[i + 1].Y - p.Points[i + 1].X * p.Points[i].Y;
            else if (curve is CircularArc2D a)
            {
                var s = a.StartPoint; var e = a.EndPoint;
                sum += s.X * e.Y - e.X * s.Y;
            }
        }
        return Math.Abs(sum) / 2.0;
    }

    private static bool CheckSelfIntersection(IReadOnlyList<Curve2D> curves, ToleranceProfile tolerance, List<string> issues)
    {
        // Simplified: check for bow-tie and self-intersecting patterns
        // Full implementation in a later iteration with curve-curve intersection
        if (curves.Count < 3)
        {
            issues.Add("候选面少于 3 条边。");
            return false;
        }

        var lineSegments = curves.SelectMany(c => c is LineSegment2D l
            ? [l] : c is Polyline2D p
            ? Enumerable.Range(0, p.Points.Count - 1).Select(i => new LineSegment2D(p.Points[i], p.Points[i + 1]))
            : []).ToList();

        for (var i = 0; i < lineSegments.Count; i++)
        for (var j = i + 1; j < lineSegments.Count; j++)
        {
            if (j == i + 1 || (i == 0 && j == lineSegments.Count - 1)) continue;
            if (LineSegmentsIntersectInterior(lineSegments[i], lineSegments[j]))
            {
                issues.Add("检测到自交（bow-tie 或类似模式）。");
                return true;
            }
        }
        return false;
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