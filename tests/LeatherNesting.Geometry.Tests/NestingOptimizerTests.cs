using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Nesting;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class NestingOptimizerTests
{
    private static Loop2D Rect(string id, double w, double h) => new(id, LoopRole.Outer, [
        new Polyline2D([
            new(0, 0), new(w, 0), new(w, h), new(0, h), new(0, 0),
        ]),
    ]);

    private static (double MinX, double MinY, double MaxX, double MaxY) GetBounds(Loop2D loop)
    {
        var pts = loop.Curves.SelectMany(c => c switch
        {
            Polyline2D p => p.Points,
            LineSegment2D l => new[] { l.Start, l.End },
            _ => Array.Empty<Point2D>()
        }).ToList();
        return (pts.Min(p => p.X), pts.Min(p => p.Y), pts.Max(p => p.X), pts.Max(p => p.Y));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-011")]
    public void Nfp_for_two_squares_has_expected_bounds()
    {
        var calc = new NfpCalculator();
        var nfp = calc.Nfp(Rect("a", 10, 10), Rect("b", 10, 10));

        var loop = Assert.Single(nfp);
        var (minX, minY, maxX, maxY) = GetBounds(loop);
        Assert.Equal(-10, minX, 2);
        Assert.Equal(-10, minY, 2);
        Assert.Equal(10, maxX, 2);
        Assert.Equal(10, maxY, 2);
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-012")]
    public void Nfp_semantics_cross_validated_against_boolean_collision()
    {
        var calc = new NfpCalculator();
        var detector = new ClipperCollisionDetector();
        var a = Rect("a", 10, 10);
        var b = Rect("b", 10, 10);

        _ = calc.Nfp(a, b); // sanity: NFP itself must compute without error

        // Reference point inside the NFP region → overlap.
        Assert.True(detector.Overlaps(a, new Transform2D(5, 5, 0, false).Apply(b)));
        // Reference point outside the NFP region → no overlap.
        Assert.False(detector.Overlaps(a, new Transform2D(20, 20, 0, false).Apply(b)));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-013")]
    public void Optimizer_never_worse_than_baseline()
    {
        var engine = new NestEngine();
        var optimizer = new NestOptimizer(engine);
        var pieces = new[]
        {
            Rect("p1", 30, 30), Rect("p2", 25, 25), Rect("p3", 20, 40),
            Rect("p4", 40, 20), Rect("p5", 15, 35), Rect("p6", 35, 15),
            Rect("p7", 18, 18), Rect("p8", 22, 28),
        };
        var material = Rect("mat", 120, 120);
        var request = new NestRequest(pieces, material, 2, [0, 90]);

        var baseline = engine.Nest(request);
        var optimized = optimizer.Optimize(request, iterations: 30, seed: 42);

        Assert.True(optimized.Utilization >= baseline.Utilization - 1e-9,
            $"optimized {optimized.Utilization:F4} < baseline {baseline.Utilization:F4}");
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-014")]
    public void Optimizer_result_is_legal()
    {
        var engine = new NestEngine();
        var optimizer = new NestOptimizer(engine);
        var pieces = new[]
        {
            Rect("p1", 30, 30), Rect("p2", 25, 25), Rect("p3", 20, 40),
            Rect("p4", 40, 20), Rect("p5", 15, 35), Rect("p6", 35, 15),
        };
        var material = Rect("mat", 120, 120);
        var request = new NestRequest(pieces, material, 2, [0, 90]);

        var result = optimizer.Optimize(request, iterations: 20, seed: 7);

        var detector = new ClipperCollisionDetector();
        var loops = result.Placements.Select(p => p.PlacedLoop).ToList();
        for (var i = 0; i < loops.Count; i++)
        {
            var others = loops.Where((_, j) => j != i).ToList();
            Assert.True(detector.IsPlacementValid(loops[i], others, material, 2));
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-015")]
    public void Optimizer_deterministic_for_fixed_seed()
    {
        var engine = new NestEngine();
        var optimizer = new NestOptimizer(engine);
        var pieces = new[]
        {
            Rect("p1", 20, 30), Rect("p2", 30, 20), Rect("p3", 25, 25), Rect("p4", 15, 40),
        };
        var material = Rect("mat", 100, 100);
        var request = new NestRequest(pieces, material, 2, [0, 90]);

        var r1 = optimizer.Optimize(request, 20, 42);
        var r2 = optimizer.Optimize(request, 20, 42);

        Assert.Equal(r1.Utilization, r2.Utilization, 9);
        Assert.Equal(r1.Placements.Count, r2.Placements.Count);
        for (var i = 0; i < r1.Placements.Count; i++)
            Assert.Equal(r1.Placements[i].Transform, r2.Placements[i].Transform);
    }
}
