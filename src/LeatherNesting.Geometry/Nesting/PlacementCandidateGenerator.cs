namespace LeatherNesting.Geometry.Nesting;

/// <summary>Generates candidate placement transforms for a piece at a given rotation.
/// Anchors the piece's bounding-box corner to the material origin and to each placed-piece vertex,
/// returning candidates ordered bottom-left first.</summary>
public sealed class PlacementCandidateGenerator
{
    /// <summary>Enumerates candidate transforms for a piece rotated by <paramref name="rotationDegrees"/>,
    /// ordered by (translateY, translateX) ascending so the caller picks the bottom-left-most legal spot.</summary>
    public IReadOnlyList<Transform2D> Candidates(
        Loop2D piece,
        double rotationDegrees,
        IReadOnlyList<Loop2D> placed,
        double gapMm)
    {
        var rotated = new Transform2D(0, 0, rotationDegrees, false).Apply(piece);
        var (minX, minY, _, _) = BoundsOf(rotated);

        var anchors = new List<Point2D> { new(gapMm, gapMm) };
        foreach (var p in placed)
        {
            foreach (var v in VerticesOf(p))
            {
                anchors.Add(new(v.X + gapMm, v.Y));
                anchors.Add(new(v.X, v.Y + gapMm));
            }
        }

        var transforms = new List<Transform2D>(anchors.Count);
        foreach (var anchor in anchors)
        {
            var tx = anchor.X - minX;
            var ty = anchor.Y - minY;
            transforms.Add(new Transform2D(tx, ty, rotationDegrees, false));
        }

        return transforms
            .OrderBy(t => t.TranslateY)
            .ThenBy(t => t.TranslateX)
            .ToList();
    }

    private static IEnumerable<Point2D> VerticesOf(Loop2D loop)
    {
        foreach (var c in loop.Curves)
        {
            switch (c)
            {
                case LineSegment2D line:
                    yield return line.Start;
                    yield return line.End;
                    break;
                case Polyline2D polyline:
                    foreach (var p in polyline.Points)
                        yield return p;
                    break;
                case CircularArc2D arc:
                    yield return arc.StartPoint;
                    yield return arc.EndPoint;
                    break;
            }
        }
    }

    private static (double MinX, double MinY, double MaxX, double MaxY) BoundsOf(Loop2D loop)
    {
        var pts = VerticesOf(loop).ToList();
        if (pts.Count == 0)
            return (0, 0, 0, 0);
        return (
            pts.Min(p => p.X),
            pts.Min(p => p.Y),
            pts.Max(p => p.X),
            pts.Max(p => p.Y));
    }
}
