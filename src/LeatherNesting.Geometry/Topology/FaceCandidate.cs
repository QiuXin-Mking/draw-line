using LeatherNesting.Geometry.Intersection;

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
                // Exact arc-aware area via Green's theorem.
                var startAngle = a.StartAngleDegrees * Math.PI / 180;
                var sweep = a.SweepAngleDegrees * Math.PI / 180;
                var endAngle = startAngle + sweep;
                var cx = a.Centre.X;
                var cy = a.Centre.Y;
                var r = a.Radius;
                sum += r * r * sweep
                     + r * cx * (Math.Sin(endAngle) - Math.Sin(startAngle))
                     - r * cy * (Math.Cos(endAngle) - Math.Cos(startAngle));
            }
        }
        return Math.Abs(sum) / 2.0;
    }

    private static bool CheckSelfIntersection(IReadOnlyList<Curve2D> curves, ToleranceProfile tolerance, List<string> issues)
    {
        if (curves.Count < 3)
        {
            issues.Add("候选面少于 3 条边。");
            return false;
        }

        for (var i = 0; i < curves.Count; i++)
        for (var j = i + 1; j < curves.Count; j++)
        {
            // Adjacent curves share an endpoint, which is not a self-intersection.
            if (j == i + 1 || (i == 0 && j == curves.Count - 1)) continue;
            if (CurveIntersection.CurvesIntersect(curves[i], curves[j]))
            {
                issues.Add("检测到自交（bow-tie 或类似模式）。");
                return true;
            }
        }
        return false;
    }
}