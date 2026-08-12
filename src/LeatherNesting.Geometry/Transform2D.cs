namespace LeatherNesting.Geometry;

/// <summary>2D affine transform: translation, rotation (degrees CCW), uniform mirror.</summary>
public sealed record Transform2D
{
    public double TranslateX { get; init; }
    public double TranslateY { get; init; }
    public double RotationDegrees { get; init; }
    public bool Mirror { get; init; }

    public static Transform2D Identity => new(0, 0, 0, false);

    public Transform2D(double translateX, double translateY, double rotationDegrees, bool mirror)
    {
        GeometryConstants.RejectNonFinite(translateX, nameof(translateX));
        GeometryConstants.RejectNonFinite(translateY, nameof(translateY));
        GeometryConstants.RejectNonFinite(rotationDegrees, nameof(rotationDegrees));
        TranslateX = translateX;
        TranslateY = translateY;
        RotationDegrees = rotationDegrees;
        Mirror = mirror;
    }

    public bool IsIdentity => TranslateX == 0 && TranslateY == 0 && RotationDegrees == 0 && !Mirror;
}