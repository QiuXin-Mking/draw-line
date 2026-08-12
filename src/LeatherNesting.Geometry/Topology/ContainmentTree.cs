namespace LeatherNesting.Geometry.Topology;

/// <summary>Builds the outer/hole containment tree from a set of closed loops.</summary>
public sealed class ContainmentTree
{
    private readonly ToleranceProfile _tolerance;

    public ContainmentTree(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Classifies loops into outer/hole and builds the containment hierarchy.</summary>
    public ContainmentResult Build(IReadOnlyList<Loop2D> loops)
    {
        if (loops.Count == 0)
            return new ContainmentResult([], [], []);

        // Compute bounding boxes and assign roles
        var nodes = loops.Select((loop, i) => new LoopNode(i, loop, ComputeCentroid(loop))).ToList();

        // Outer loops: those not contained by any other loop
        // Hole loops: contained by an outer loop
        var outers = new List<LoopNode>();
        var holes = new List<(LoopNode Hole, int OuterIndex)>();

        foreach (var node in nodes)
        {
            bool isContained = false;
            foreach (var other in nodes)
            {
                if (other.Index == node.Index) continue;
                if (ContainsPoint(other.Loop, node.Centroid))
                {
                    isContained = true;
                    holes.Add((node, other.Index));
                    break;
                }
            }
            if (!isContained) outers.Add(node);
        }

        var diagnostics = new List<string>();
        if (outers.Count == 0 && nodes.Count > 0)
            diagnostics.Add("所有轮廓彼此嵌套，无法确定外环。");

        return new ContainmentResult(
            outers.Select(n => n.Loop).ToList(),
            holes.Select(h => h.Hole.Loop).ToList(),
            diagnostics);
    }

    private static Point2D ComputeCentroid(Loop2D loop)
    {
        var points = loop.Curves.SelectMany(c => c is LineSegment2D l
            ? [l.Start, l.End]
            : c is Polyline2D p ? p.Points
            : c is CircularArc2D a ? [a.StartPoint, a.EndPoint]
            : [c.StartPoint]).ToList();
        if (points.Count == 0) return Point2D.Origin;
        return new(points.Average(p => p.X), points.Average(p => p.Y));
    }

    private bool ContainsPoint(Loop2D container, Point2D point)
    {
        // Ray casting: count intersections with a horizontal ray to the right
        var intersections = 0;
        foreach (var curve in container.Curves)
        {
            var segments = FlattenToSegments(curve);
            foreach (var (start, end) in segments)
            {
                if (RayIntersectsSegment(point, start, end))
                    intersections++;
            }
        }
        return intersections % 2 == 1;
    }

    private static IReadOnlyList<(Point2D Start, Point2D End)> FlattenToSegments(Curve2D curve)
    {
        return curve switch
        {
            LineSegment2D l => [(l.Start, l.End)],
            Polyline2D p => Enumerable.Range(0, p.Points.Count - 1).Select(i => (p.Points[i], p.Points[i + 1])).ToList(),
            CircularArc2D a => [(a.StartPoint, a.EndPoint)],
            _ => [],
        };
    }

    private static bool RayIntersectsSegment(Point2D point, Point2D a, Point2D b)
    {
        if (point.Y < Math.Min(a.Y, b.Y) || point.Y >= Math.Max(a.Y, b.Y)) return false;
        if (a.Y == b.Y) return false;
        var xIntersect = a.X + (point.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y);
        return xIntersect > point.X;
    }

    private sealed record LoopNode(int Index, Loop2D Loop, Point2D Centroid);

    public sealed record ContainmentResult(
        IReadOnlyList<Loop2D> OuterLoops,
        IReadOnlyList<Loop2D> Holes,
        IReadOnlyList<string> Diagnostics);
}