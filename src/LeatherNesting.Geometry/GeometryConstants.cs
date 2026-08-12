namespace LeatherNesting.Geometry;

/// <summary>Project-wide geometry constants. All business-internal units are millimetres.</summary>
public static class GeometryConstants
{
    /// <summary>Scaling factor used to convert mm to Clipper2-compatible integer coordinates.</summary>
    /// <remarks>1e6 ≈ 1 nm precision; extreme values must be checked for overflow before scaling.</remarks>
    public const long IntegerScale = 1_000_000L;

    /// <summary>Maximum safe coordinate in mm before scaling would overflow a 64-bit integer.</summary>
    public const double MaxSafeMillimetreCoordinate = (double)(long.MaxValue / IntegerScale / 2);

    public static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    public static void RejectNonFinite(double value, string paramName)
    {
        if (!IsFinite(value))
            throw new ArgumentOutOfRangeException(paramName, value, "几何值不得为 NaN 或 Infinity。");
    }
}