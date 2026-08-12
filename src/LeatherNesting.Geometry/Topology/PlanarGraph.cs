namespace LeatherNesting.Geometry.Topology;

/// <summary>Half-edge planar graph for topology analysis.</summary>
public sealed class PlanarGraph
{
    private readonly List<HalfEdge> _edges = [];
    private readonly List<Face> _faces = [];
    private readonly ToleranceProfile _tolerance;

    public PlanarGraph(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    public IReadOnlyList<HalfEdge> Edges => _edges;
    public IReadOnlyList<Face> Faces => _faces;

    /// <summary>Builds the graph from an endpoint index.</summary>
    public void Build(EndpointIndex index)
    {
        _edges.Clear();
        _faces.Clear();
        BuildFromCurves(index.Curves);
    }

    private void BuildFromCurves(IReadOnlyList<EndpointIndex.IndexedCurve> curves)
    {
        // Create half-edges
        foreach (var ic in curves)
        {
            var edge = new HalfEdge(
                Id: _edges.Count,
                Start: ic.Curve.StartPoint,
                End: ic.Curve.EndPoint,
                Curve: ic.Curve,
                SourceId: ic.SourceId);
            _edges.Add(edge);
        }

        // Link next/prev: group edges sharing endpoints
        LinkEdges();
    }

    private void LinkEdges()
    {
        // Simple bidirectional linking: edges sharing an endpoint are linked
        for (var i = 0; i < _edges.Count; i++)
        for (var j = i + 1; j < _edges.Count; j++)
        {
            if (AreCoincident(_edges[i].End, _edges[j].Start, _tolerance.TopologyToleranceMm))
            {
                _edges[i] = _edges[i] with { NextId = _edges[j].Id };
                _edges[j] = _edges[j] with { PrevId = _edges[i].Id };
            }
            if (AreCoincident(_edges[j].End, _edges[i].Start, _tolerance.TopologyToleranceMm))
            {
                _edges[j] = _edges[j] with { NextId = _edges[i].Id };
                _edges[i] = _edges[i] with { PrevId = _edges[j].Id };
            }
        }
    }

    /// <summary>Extracts closed loops by traversing half-edges.</summary>
    public IReadOnlyList<IReadOnlyList<int>> ExtractClosedLoops()
    {
        var loops = new List<IReadOnlyList<int>>();
        var visited = new HashSet<int>();
        for (var i = 0; i < _edges.Count; i++)
        {
            if (visited.Contains(i)) continue;
            var loop = TraverseLoop(i, visited);
            if (loop.Count >= 3) loops.Add(loop);
        }
        return loops;
    }

    private IReadOnlyList<int> TraverseLoop(int startId, HashSet<int> visited)
    {
        var loop = new List<int>();
        var current = startId;
        while (!visited.Contains(current))
        {
            visited.Add(current);
            loop.Add(current);
            var edge = _edges[current];
            var next = edge.NextId;
            if (next < 0 || next >= _edges.Count) break;
            current = next;
            if (current == startId) break;
        }
        return loop;
    }

    private static bool AreCoincident(Point2D a, Point2D b, double tolerance) => a.DistanceTo(b) <= tolerance;

    public sealed record HalfEdge(int Id, Point2D Start, Point2D End, Curve2D Curve, string SourceId, int NextId = -1, int PrevId = -1);

    public sealed record Face(int Id, IReadOnlyList<int> EdgeIds, bool IsOuter);
}