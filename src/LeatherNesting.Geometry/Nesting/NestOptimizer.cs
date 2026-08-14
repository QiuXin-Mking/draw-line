namespace LeatherNesting.Geometry.Nesting;

/// <summary>Local-search optimizer: shuffles placement order and keeps the best-seen result.
/// Deterministic for a fixed seed; utilization is monotonically non-decreasing versus the BLF baseline.</summary>
public sealed class NestOptimizer
{
    private readonly NestEngine _engine;

    public NestOptimizer(NestEngine? engine = null)
    {
        _engine = engine ?? new NestEngine();
    }

    /// <summary>Runs <paramref name="iterations"/> order-shuffles seeded by <paramref name="seed"/>,
    /// returning the highest-utilization result (never worse than the BLF baseline).</summary>
    public NestResult Optimize(NestRequest request, int iterations = 50, int seed = 2026)
    {
        if (iterations < 0)
            throw new ArgumentOutOfRangeException(nameof(iterations), "迭代次数不得为负数。");

        var random = new Random(seed);
        var best = _engine.Nest(request);

        for (var i = 0; i < iterations; i++)
        {
            var shuffled = request.Pieces.OrderBy(_ => random.Next()).ToList();
            var candidate = _engine.NestInOrder(request, shuffled);
            if (candidate.Utilization > best.Utilization)
                best = candidate;
        }

        return best;
    }
}
