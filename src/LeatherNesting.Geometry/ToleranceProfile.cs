namespace LeatherNesting.Geometry;

/// <summary>Unified tolerance profile for all geometry operations. No magic numbers outside this file.</summary>
public sealed record ToleranceProfile
{
    /// <summary>Tolerance for snapping near-coincident endpoints during import, in mm.</summary>
    public double ImportSnapToleranceMm { get; init; } = 0.01;

    /// <summary>General topology tolerance (coincidence, gap detection), in mm.</summary>
    public double TopologyToleranceMm { get; init; } = 0.05;

    /// <summary>Chord tolerance for flattening curves to polylines, in mm.</summary>
    public double FlattenChordToleranceMm { get; init; } = 0.01;

    /// <summary>Collision / overlap detection tolerance, in mm.</summary>
    public double CollisionToleranceMm { get; init; } = 0.001;

    /// <summary>Round-trip tolerance for export → re-import verification, in mm.</summary>
    public double ExportRoundTripToleranceMm { get; init; } = 0.05;

    public static ToleranceProfile Default => new();

    public ToleranceProfile()
    {
        Validate();
    }

    private void Validate()
    {
        RejectNonPositive(ImportSnapToleranceMm, nameof(ImportSnapToleranceMm));
        RejectNonPositive(TopologyToleranceMm, nameof(TopologyToleranceMm));
        RejectNonPositive(FlattenChordToleranceMm, nameof(FlattenChordToleranceMm));
        RejectNonPositive(CollisionToleranceMm, nameof(CollisionToleranceMm));
        RejectNonPositive(ExportRoundTripToleranceMm, nameof(ExportRoundTripToleranceMm));
    }

    private static void RejectNonPositive(double value, string paramName)
    {
        if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, value, "公差值必须为正有限数。");
    }
}