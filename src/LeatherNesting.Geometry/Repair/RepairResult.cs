namespace LeatherNesting.Geometry.Repair;

/// <summary>Describes the provenance of a bridge segment added during repair.</summary>
public enum BridgeSource { Extend, Trim, Add }

/// <summary>A bridge segment that connects two endpoints during gap repair.</summary>
public sealed record BridgeSegment(
    Curve2D Segment,
    BridgeSource Source,
    string Description);

/// <summary>Result of a repair operation, with preview geometry and diagnostics.</summary>
public sealed record RepairResult(
    IReadOnlyList<Loop2D> RepairedLoops,
    IReadOnlyList<BridgeSegment> Bridges,
    IReadOnlyList<string> Diagnostics,
    IReadOnlyList<string> Warnings)
{
    public bool HasChanges => Bridges.Count > 0;
    public bool IsRepairable => Diagnostics.Count == 0;
}