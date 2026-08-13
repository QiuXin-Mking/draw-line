namespace LeatherNesting.Geometry.Nesting;

/// <summary>Greedy bottom-left-fill nest engine. Deterministic: same input always yields the same output.</summary>
public sealed class NestEngine
{
    private readonly ClipperCollisionDetector _detector;
    private readonly PlacementCandidateGenerator _candidates;

    public NestEngine(ToleranceProfile? tolerance = null)
    {
        _detector = new ClipperCollisionDetector(tolerance);
        _candidates = new PlacementCandidateGenerator();
    }

    public NestResult Nest(NestRequest request)
    {
        if (request.Pieces.Count == 0)
            return new NestResult([], [], 0);
        if (request.GapMm < 0)
            throw new ArgumentOutOfRangeException(nameof(request), "间隙不得为负数。");
        if (request.AllowedRotationsDegrees.Count == 0)
            throw new ArgumentException("至少需要一个允许旋转角。", nameof(request));

        // Deterministic order: largest area first, stable id as tie-break.
        var ordered = request.Pieces
            .OrderByDescending(p => p.Area)
            .ThenBy(p => p.StableId)
            .ToList();

        var placements = new List<NestPlacement>();
        var placedLoops = new List<Loop2D>();
        var unplaced = new List<string>();

        foreach (var piece in ordered)
        {
            var placement = FindPlacement(piece, request, placedLoops);
            if (placement is null)
            {
                unplaced.Add(piece.StableId);
                continue;
            }

            placements.Add(placement);
            placedLoops.Add(placement.PlacedLoop);
        }

        var materialArea = request.Material.Area;
        var utilization = materialArea > 0
            ? placements.Sum(p => p.PlacedLoop.Area) / materialArea
            : 0;

        return new NestResult(placements, unplaced, utilization);
    }

    private NestPlacement? FindPlacement(Loop2D piece, NestRequest request, IReadOnlyList<Loop2D> placedLoops)
    {
        foreach (var rotation in request.AllowedRotationsDegrees.OrderBy(r => r))
        {
            foreach (var transform in _candidates.Candidates(piece, rotation, placedLoops, request.GapMm))
            {
                var placedLoop = transform.Apply(piece);
                if (_detector.IsPlacementValid(placedLoop, placedLoops, request.Material, request.GapMm))
                    return new NestPlacement(piece.StableId, transform, placedLoop);
            }
        }

        return null;
    }
}
