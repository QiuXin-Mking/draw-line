using System.Text.Json.Serialization;

namespace LeatherNesting.Geometry;

/// <summary>2D affine transform: translation, rotation (degrees CCW), uniform mirror.</summary>
public sealed record Transform2D
{
    public double TranslateX { get; init; }
    public double TranslateY { get; init; }
    public double RotationDegrees { get; init; }
    public bool Mirror { get; init; }

    public static Transform2D Identity => new(0, 0, 0, false);

    [JsonConstructor]
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

    /// <summary>Creates a transform that rotates by <paramref name="degrees"/> about the given centre point.</summary>
    public static Transform2D RotateAbout(Point2D centre, double degrees)
    {
        var rotatedCentre = new Transform2D(0, 0, degrees, false).Apply(centre);
        return new Transform2D(centre.X - rotatedCentre.X, centre.Y - rotatedCentre.Y, degrees, false);
    }

    /// <summary>Applies the transform to a point. Order: mirror (about Y) → rotate → translate.</summary>
    public Point2D Apply(Point2D point)
    {
        var x = point.X;
        var y = point.Y;
        if (Mirror) x = -x;

        var radians = RotationDegrees * Math.PI / 180;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var rotatedX = x * cos - y * sin;
        var rotatedY = x * sin + y * cos;
        return new Point2D(rotatedX + TranslateX, rotatedY + TranslateY);
    }

    /// <summary>Applies the transform to every curve of a loop, preserving its identity and role.</summary>
    public Loop2D Apply(Loop2D loop) =>
        new(loop.StableId, loop.Role, loop.Curves.Select(Apply).ToList());

    /// <summary>Applies the transform to an internal line, preserving its identity and role.</summary>
    public InternalLine Apply(InternalLine line) =>
        new(line.Id, line.Role, line.Curves.Select(Apply).ToList());

    /// <summary>Applies the transform to a single curve.</summary>
    public Curve2D Apply(Curve2D curve) => curve switch
    {
        LineSegment2D line => new LineSegment2D(Apply(line.Start), Apply(line.End)),
        Polyline2D polyline => new Polyline2D(polyline.Points.Select(Apply).ToList()),
        CircularArc2D arc => ApplyArc(arc),
        _ => throw new NotSupportedException($"不支持的曲线类型：{curve.GetType().Name}")
    };

    private CircularArc2D ApplyArc(CircularArc2D arc)
    {
        var start = arc.StartAngleDegrees;
        var sweep = arc.SweepAngleDegrees;
        if (Mirror)
        {
            // Mirror about Y maps angle θ → 180° − θ and flips sweep direction.
            start = 180 - start;
            sweep = -sweep;
        }
        start += RotationDegrees;
        return new CircularArc2D(Apply(arc.Centre), arc.Radius, start, sweep);
    }
}