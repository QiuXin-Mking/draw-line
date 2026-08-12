namespace LeatherNesting.Geometry.Repair;

using LeatherNesting.Geometry.Topology;

/// <summary>Detects and repairs gaps between disconnected curve segments.</summary>
public sealed class GapRepair
{
    private readonly ToleranceProfile _tolerance;

    public GapRepair(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Attempts to connect disconnected curves into closed loops by bridging gaps.</summary>
    public RepairResult Repair(IReadOnlyList<Curve2D> curves, string sourceId)
    {
        var index = new EndpointIndex(_tolerance);
        index.AddRange(curves, sourceId);
        var gaps = index.FindGaps();

        if (gaps.Count == 0)
            return new RepairResult([], [], [], ["未发现可修复的间隙。"]);

        var bridges = new List<BridgeSegment>();
        var diagnostics = new List<string>();
        var warnings = new List<string>();

        foreach (var (a, b, distance) in gaps)
        {
            var bridge = new LineSegment2D(a, b);
            var source = distance < _tolerance.TopologyToleranceMm / 2 ? BridgeSource.Extend : BridgeSource.Add;
            bridges.Add(new BridgeSegment(bridge, source, $"桥接间隙 {distance:F3}mm"));
            if (distance > _tolerance.TopologyToleranceMm / 2)
                warnings.Add($"间隙 {distance:F3}mm 超过公差一半，桥接段标记为 {source}。");
        }

        // Build a merged set of curves including bridges
        var allCurves = curves.Concat(bridges.Select(b => b.Segment)).ToList();
        var mergedIndex = new EndpointIndex(_tolerance);
        mergedIndex.AddRange(allCurves, sourceId);

        var graph = new PlanarGraph(_tolerance);
        graph.Build(mergedIndex);
        var loops = graph.ExtractClosedLoops();

        var resultLoops = loops.Select((edges, i) =>
        {
            var curveList = edges.Select(eid => graph.Edges[eid].Curve).ToList();
            return new Loop2D($"repaired-{sourceId}-{i + 1}", LoopRole.Outer, curveList);
        }).ToList();

        if (resultLoops.Count == 0)
            diagnostics.Add("修复后未找到闭合环。");

        return new RepairResult(resultLoops, bridges, diagnostics, warnings);
    }

    /// <summary>Joins curves that share endpoints, optionally trimming or extending to meet.</summary>
    public RepairResult Join(IReadOnlyList<Curve2D> curves, string sourceId)
    {
        var index = new EndpointIndex(_tolerance);
        index.AddRange(curves, sourceId);
        var gaps = index.FindGaps();
        var bridges = new List<BridgeSegment>();
        var diagnostics = new List<string>();
        var warnings = new List<string>();

        // Only join very close gaps (trim/extend threshold)
        var joinTolerance = _tolerance.TopologyToleranceMm / 2;
        var joinable = gaps.Where(g => g.Distance <= joinTolerance).ToList();
        var nonJoinable = gaps.Where(g => g.Distance > joinTolerance).ToList();

        foreach (var (a, b, distance) in joinable)
        {
            var source = distance < _tolerance.TopologyToleranceMm / 4 ? BridgeSource.Trim : BridgeSource.Extend;
            bridges.Add(new BridgeSegment(new LineSegment2D(a, b), source, $"连接端点 {source} {distance:F3}mm"));
        }

        foreach (var (_, _, distance) in nonJoinable)
            warnings.Add($"间隙 {distance:F3}mm 超过连接公差，需要手动检查。");

        if (nonJoinable.Count > 0)
            diagnostics.Add($"有 {nonJoinable.Count} 个间隙超出连接公差。");

        var allCurves = curves.Concat(bridges.Select(b => b.Segment)).ToList();
        var mergedIndex = new EndpointIndex(_tolerance);
        mergedIndex.AddRange(allCurves, sourceId);

        var graph = new PlanarGraph(_tolerance);
        graph.Build(mergedIndex);
        var loops = graph.ExtractClosedLoops();

        var resultLoops = loops.Select((edges, i) =>
        {
            var curveList = edges.Select(eid => graph.Edges[eid].Curve).ToList();
            return new Loop2D($"joined-{sourceId}-{i + 1}", LoopRole.Outer, curveList);
        }).ToList();

        return new RepairResult(resultLoops, bridges, diagnostics, warnings);
    }
}