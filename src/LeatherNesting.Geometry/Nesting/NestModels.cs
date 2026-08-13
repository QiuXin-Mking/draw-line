namespace LeatherNesting.Geometry.Nesting;

/// <summary>Inputs for a nest run: pieces to place, the material contour, gap, and allowed rotations.</summary>
public sealed record NestRequest(
    IReadOnlyList<Loop2D> Pieces,
    Loop2D Material,
    double GapMm,
    IReadOnlyList<double> AllowedRotationsDegrees);

/// <summary>A single placed piece: its transform and the resulting placed contour.</summary>
public sealed record NestPlacement(
    string PieceId,
    Transform2D Transform,
    Loop2D PlacedLoop);

/// <summary>Result of a nest run: placed pieces, unplaced piece ids, and area utilization (0..1).</summary>
public sealed record NestResult(
    IReadOnlyList<NestPlacement> Placements,
    IReadOnlyList<string> Unplaced,
    double Utilization);
