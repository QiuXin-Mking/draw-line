using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Nesting;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class NestingTests
{
    private static Loop2D Rect(string id, double w, double h) => new(id, LoopRole.Outer, [
        new Polyline2D([
            new(0, 0), new(w, 0), new(w, h), new(0, h), new(0, 0),
        ]),
    ]);

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-001")]
    public void Overlap_detected_for_intersecting_rectangles()
    {
        var a = Rect("a", 10, 10);
        var b = new Loop2D("b", LoopRole.Outer, [
            new Polyline2D([
                new(5, 0), new(15, 0), new(15, 10), new(5, 10), new(5, 0),
            ]),
        ]);
        var detector = new ClipperCollisionDetector();
        Assert.True(detector.Overlaps(a, b));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-002")]
    public void Edge_contact_is_not_overlap()
    {
        var a = Rect("a", 10, 10);
        var b = new Loop2D("b", LoopRole.Outer, [
            new Polyline2D([
                new(10, 0), new(20, 0), new(20, 10), new(10, 10), new(10, 0),
            ]),
        ]);
        var detector = new ClipperCollisionDetector();
        Assert.False(detector.Overlaps(a, b));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-003")]
    public void Placement_requires_gap_from_material_and_placed()
    {
        var detector = new ClipperCollisionDetector();
        var material = Rect("mat", 100, 100);
        var placed = new Loop2D("p", LoopRole.Outer, [
            new Polyline2D([
                new(5, 5), new(15, 5), new(15, 15), new(5, 15), new(5, 5),
            ]),
        ]);

        // x=17 → 2mm clear of placed (right edge 15), gap=5 violated.
        var tooClose = new Loop2D("c", LoopRole.Outer, [
            new Polyline2D([
                new(17, 5), new(27, 5), new(27, 15), new(17, 15), new(17, 5),
            ]),
        ]);
        Assert.False(detector.IsPlacementValid(tooClose, [placed], material, 5.0));

        // x=21 → 6mm clear, valid (and ≥5mm from all material edges).
        var ok = new Loop2D("c2", LoopRole.Outer, [
            new Polyline2D([
                new(21, 5), new(31, 5), new(31, 15), new(21, 15), new(21, 5),
            ]),
        ]);
        Assert.True(detector.IsPlacementValid(ok, [placed], material, 5.0));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-004")]
    public void Out_of_bounds_is_rejected()
    {
        var detector = new ClipperCollisionDetector();
        var material = Rect("mat", 20, 20);
        var candidate = new Loop2D("c", LoopRole.Outer, [
            new Polyline2D([
                new(15, 0), new(25, 0), new(25, 10), new(15, 10), new(15, 0),
            ]),
        ]);
        Assert.False(detector.IsPlacementValid(candidate, [], material, 0));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-005")]
    public void Rotates_to_90_when_0_does_not_fit()
    {
        var engine = new NestEngine();
        var piece = Rect("piece", 100, 50);   // 100 wide × 50 tall
        var material = Rect("mat", 60, 120);  // only fits rotated 90° (50 × 100)
        var result = engine.Nest(new NestRequest([piece], material, 0, [0, 90]));

        var placement = Assert.Single(result.Placements);
        Assert.Empty(result.Unplaced);
        Assert.Equal(90, placement.Transform.RotationDegrees, 6);
        Assert.Equal(5000, placement.PlacedLoop.Area, 6);
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-006")]
    public void Oversized_piece_goes_unplaced()
    {
        var engine = new NestEngine();
        var piece = Rect("big", 50, 50);
        var material = Rect("mat", 30, 30);
        var result = engine.Nest(new NestRequest([piece], material, 0, [0, 90]));

        Assert.Empty(result.Placements);
        Assert.Single(result.Unplaced, "big");
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-007")]
    public void Deterministic_same_input_same_output()
    {
        var engine = new NestEngine();
        var pieces = new[] { Rect("a", 10, 10), Rect("b", 10, 10), Rect("c", 10, 10) };
        var material = Rect("mat", 40, 40);
        var request = new NestRequest(pieces, material, 1, [0, 90]);

        var r1 = engine.Nest(request);
        var r2 = engine.Nest(request);

        Assert.Equal(r1.Placements.Count, r2.Placements.Count);
        for (var i = 0; i < r1.Placements.Count; i++)
        {
            Assert.Equal(r1.Placements[i].Transform, r2.Placements[i].Transform);
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-008")]
    public void Empty_input_returns_empty_result()
    {
        var engine = new NestEngine();
        var result = engine.Nest(new NestRequest([], Rect("mat", 10, 10), 0, [0, 90]));

        Assert.Empty(result.Placements);
        Assert.Empty(result.Unplaced);
        Assert.Equal(0, result.Utilization, 6);
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-009")]
    public void Invalid_request_throws()
    {
        var engine = new NestEngine();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            engine.Nest(new NestRequest([Rect("a", 10, 10)], Rect("mat", 100, 100), -1, [0, 90])));
        Assert.Throws<ArgumentException>(() =>
            engine.Nest(new NestRequest([Rect("a", 10, 10)], Rect("mat", 100, 100), 0, [])));
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-NEST-010")]
    public void Placed_pieces_do_not_overlap_and_respect_gap()
    {
        var engine = new NestEngine();
        var pieces = new[] { Rect("a", 10, 10), Rect("b", 10, 10), Rect("c", 10, 10), Rect("d", 10, 10) };
        var material = Rect("mat", 50, 50);
        var result = engine.Nest(new NestRequest(pieces, material, 2, [0, 90]));

        Assert.Equal(4, result.Placements.Count);

        var detector = new ClipperCollisionDetector();
        var placedLoops = result.Placements.Select(p => p.PlacedLoop).ToList();
        for (var i = 0; i < placedLoops.Count; i++)
        {
            var others = placedLoops.Where((_, j) => j != i).ToList();
            Assert.True(detector.IsPlacementValid(placedLoops[i], others, material, 2));
        }
    }
}
