using Clipper2Lib;

namespace LeatherNesting.Geometry;

/// <summary>Shared conversion between <see cref="Loop2D"/> and Clipper2 integer paths.
/// One scale-aware bridge reused by offset and nesting, so mm↔integer rounding stays consistent.</summary>
public static class ClipperPathAdapter
{
    /// <summary>Converts a loop to a Clipper2 Path64 using the given scale, flattening arcs by chord tolerance.</summary>
    public static Path64 ToPath64(Loop2D loop, long scale, ToleranceProfile tolerance)
    {
        var points = new List<Point2D>();
        foreach (var curve in loop.Curves)
        {
            switch (curve)
            {
                case LineSegment2D line:
                    points.Add(line.Start);
                    points.Add(line.End);
                    break;
                case Polyline2D polyline:
                    points.AddRange(polyline.Points);
                    break;
                case CircularArc2D arc:
                    points.AddRange(FlattenArc(arc, tolerance));
                    break;
            }
        }

        // Deduplicate consecutive identical points.
        var deduped = new List<Point2D>();
        foreach (var p in points)
        {
            if (deduped.Count == 0 || deduped[^1].DistanceTo(p) > tolerance.TopologyToleranceMm)
                deduped.Add(p);
        }

        if (deduped.Count < 3)
            return new Path64();

        return new Path64(deduped.Select(p => ToPoint64(p, scale)));
    }

    public static Point2D ToPoint2D(Point64 p, long scale) => new((double)p.X / scale, (double)p.Y / scale);

    public static Point64 ToPoint64(Point2D p, long scale) =>
        new((long)Math.Round(p.X * scale), (long)Math.Round(p.Y * scale));

    private static IReadOnlyList<Point2D> FlattenArc(CircularArc2D arc, ToleranceProfile tolerance)
    {
        var chordLen = arc.StartPoint.DistanceTo(arc.EndPoint);
        var n = (int)Math.Max(2, Math.Ceiling(chordLen / tolerance.FlattenChordToleranceMm));
        var pts = new List<Point2D>(n + 1);
        for (var i = 0; i <= n; i++)
            pts.Add(arc.PointAt((double)i / n));
        return pts;
    }
}
