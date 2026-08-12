namespace LeatherNesting.Geometry.Offset;

/// <summary>Result of an offset operation with topology change tracking.</summary>
public sealed record OffsetResult(
    IReadOnlyList<Loop2D> OffsetLoops,
    IReadOnlyList<Loop2D> SourceLoops,
    double OffsetDistanceMm,
    OffsetDirection Direction,
    bool TopologyChanged,
    IReadOnlyList<string> TopologyWarnings,
    IReadOnlyList<string> Diagnostics)
{
    public bool IsEmpty => OffsetLoops.Count == 0;
    public bool RequiresConfirmation => TopologyChanged;
}

/// <summary>Direction of offset relative to material.</summary>
public enum OffsetDirection { Inside, Outside }

/// <summary>Join style for offset corners.</summary>
public enum OffsetJoinStyle { Miter, Square, Round }