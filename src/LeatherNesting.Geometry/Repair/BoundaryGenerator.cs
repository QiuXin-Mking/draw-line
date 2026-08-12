namespace LeatherNesting.Geometry.Repair;

using LeatherNesting.Geometry.Topology;

/// <summary>Generates candidate boundaries from disconnected curve segments.
/// Never uses convex hull in place of the real boundary. Multiple candidates require user selection.</summary>
public sealed class BoundaryGenerator
{
    private readonly ToleranceProfile _tolerance;

    public BoundaryGenerator(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Generates all valid closed loop candidates from the curve graph.</summary>
    public BoundaryGenerationResult Generate(IReadOnlyList<Curve2D> curves, string sourceId)
    {
        if (curves.Count == 0)
            return new BoundaryGenerationResult([], [], ["没有曲线可供生成边界。"]);

        var index = new EndpointIndex(_tolerance);
        index.AddRange(curves, sourceId);
        var split = index.SplitIntersections();

        var graph = new PlanarGraph(_tolerance);
        graph.Build(split);
        var candidates = FaceCandidate.FromGraph(graph);

        var diagnostics = new List<string>();
        var warnings = new List<string>();

        if (candidates.Count == 0)
        {
            diagnostics.Add("无法从给定曲线中找到闭合轮廓。");
            return new BoundaryGenerationResult([], [], diagnostics);
        }

        if (candidates.Count > 1)
            warnings.Add($"发现 {candidates.Count} 个候选环，请选择目标边界。不自动选择最大面积。");

        // Filter out T-junction candidates (edges that don't form a proper loop)
        var validCandidates = candidates.Where(c => c.IsValid).ToList();

        if (validCandidates.Count == 0)
            diagnostics.Add("所有候选环均无效（自交或其他问题）。");

        return new BoundaryGenerationResult(validCandidates, candidates, diagnostics.Concat(warnings).ToList());
    }

    /// <summary>Generates boundary from a specific candidate selected by the user.</summary>
    public Loop2D? GenerateFromCandidate(FaceCandidate candidate, string stableId, LoopRole role = LoopRole.Outer)
    {
        if (!candidate.IsValid) return null;
        return new Loop2D(stableId, role, candidate.Curves);
    }

    public sealed record BoundaryGenerationResult(
        IReadOnlyList<FaceCandidate> ValidCandidates,
        IReadOnlyList<FaceCandidate> AllCandidates,
        IReadOnlyList<string> Diagnostics);
}