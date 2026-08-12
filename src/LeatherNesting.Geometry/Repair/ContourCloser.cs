namespace LeatherNesting.Geometry.Repair;

/// <summary>Closes open contours by adding a bridge segment between start and end points.</summary>
public sealed class ContourCloser
{
    private readonly ToleranceProfile _tolerance;

    public ContourCloser(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Attempts to close a loop by bridging the gap between start and end points.</summary>
    public RepairResult Close(Loop2D loop, string? newId = null)
    {
        var diagnostics = new List<string>();
        var bridges = new List<BridgeSegment>();

        var curves = loop.Curves;
        var first = curves[0].StartPoint;
        var last = curves[^1].EndPoint;
        var gap = first.DistanceTo(last);

        // Already closed — nothing to do.
        if (gap < 1e-9)
            return new RepairResult([loop], [], [], []);

        // Gap within tolerance — bridge it.
        if (gap <= _tolerance.TopologyToleranceMm)
        {
            var bridge = new LineSegment2D(last, first);
            bridges.Add(new BridgeSegment(bridge, BridgeSource.Add, $"闭合间隙 {gap:F3}mm"));
            var closedCurves = curves.Append(bridge).ToList();
            var closedLoop = new Loop2D(newId ?? loop.StableId, loop.Role, closedCurves);
            return new RepairResult([closedLoop], bridges, [], []);
        }

        // Gap too large — reject.
        diagnostics.Add($"轮廓两端间距 {gap:F3}mm 超过修复公差，无法自动闭合。");
        return new RepairResult([loop], [], diagnostics, []);
    }
}