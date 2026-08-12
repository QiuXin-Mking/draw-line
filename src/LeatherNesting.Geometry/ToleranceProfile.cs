namespace LeatherNesting.Geometry;

/// <summary>Unified tolerance profile for all geometry operations. No magic numbers outside this file.</summary>
public sealed record ToleranceProfile
{
    private double _importSnapToleranceMm = 0.01;
    private double _topologyToleranceMm = 0.05;
    private double _flattenChordToleranceMm = 0.01;
    private double _collisionToleranceMm = 0.001;
    private double _exportRoundTripToleranceMm = 0.05;

    /// <summary>Tolerance for snapping near-coincident endpoints during import, in mm.</summary>
    public double ImportSnapToleranceMm
    {
        get => _importSnapToleranceMm;
        init => _importSnapToleranceMm = RejectNonPositive(value, nameof(ImportSnapToleranceMm));
    }

    /// <summary>General topology tolerance (coincidence, gap detection), in mm.</summary>
    public double TopologyToleranceMm
    {
        get => _topologyToleranceMm;
        init => _topologyToleranceMm = RejectNonPositive(value, nameof(TopologyToleranceMm));
    }

    /// <summary>Chord tolerance for flattening curves to polylines, in mm.</summary>
    public double FlattenChordToleranceMm
    {
        get => _flattenChordToleranceMm;
        init => _flattenChordToleranceMm = RejectNonPositive(value, nameof(FlattenChordToleranceMm));
    }

    /// <summary>Collision / overlap detection tolerance, in mm.</summary>
    public double CollisionToleranceMm
    {
        get => _collisionToleranceMm;
        init => _collisionToleranceMm = RejectNonPositive(value, nameof(CollisionToleranceMm));
    }

    /// <summary>Round-trip tolerance for export → re-import verification, in mm.</summary>
    public double ExportRoundTripToleranceMm
    {
        get => _exportRoundTripToleranceMm;
        init => _exportRoundTripToleranceMm = RejectNonPositive(value, nameof(ExportRoundTripToleranceMm));
    }

    public static ToleranceProfile Default => new();

    private static double RejectNonPositive(double value, string paramName)
    {
        if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, value, "公差值必须为正有限数。");
        return value;
    }
}
