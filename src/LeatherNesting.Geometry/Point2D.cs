namespace LeatherNesting.Geometry;

/// <summary>2D point with finite coordinate validation.</summary>
public sealed record Point2D
{
    public double X { get; init; }
    public double Y { get; init; }

    public Point2D(double x, double y)
    {
        GeometryConstants.RejectNonFinite(x, nameof(x));
        GeometryConstants.RejectNonFinite(y, nameof(y));
        X = x;
        Y = y;
    }

    public static Point2D Origin => new(0, 0);

    public double DistanceTo(Point2D other) =>
        Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

    public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);
    public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);
    public static Point2D operator *(Point2D p, double scalar) => new(p.X * scalar, p.Y * scalar);
}