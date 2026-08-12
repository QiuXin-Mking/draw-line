using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Topology;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

/// <summary>Property-based tests for geometry invariants.</summary>
public sealed class GeometryPropertyTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-PRP-001")]
    public void Loop_winding_outer_is_ccw()
    {
        // CCW rectangle
        var loop = new Loop2D("outer", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(0, 10)),
            new LineSegment2D(new(0, 10), new(10, 10)),
            new LineSegment2D(new(10, 10), new(10, 0)),
            new LineSegment2D(new(10, 0), new(0, 0)),
        ]);

        var normalized = loop.NormalizeWinding();
        Assert.False(normalized.IsClockwise);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-PRP-002")]
    public void Loop_winding_hole_is_cw()
    {
        // CCW rectangle, but role=Hole → should normalize to CW
        var loop = new Loop2D("hole", LoopRole.Hole, [
            new LineSegment2D(new(0, 0), new(0, 10)),
            new LineSegment2D(new(0, 10), new(10, 10)),
            new LineSegment2D(new(10, 10), new(10, 0)),
            new LineSegment2D(new(10, 0), new(0, 0)),
        ]);

        var normalized = loop.NormalizeWinding();
        Assert.True(normalized.IsClockwise);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-PRP-003")]
    public void Reverse_twice_restores_original()
    {
        var original = new Loop2D("loop", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]);

        var reversed = original.Reverse();
        var restored = reversed.Reverse();

        Assert.Equal(original.Area, restored.Area, 6);
        Assert.Equal(original.Curves.Count, restored.Curves.Count);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-PRP-004")]
    public void Containment_tree_detects_outer_and_hole()
    {
        var outer = new Loop2D("outer", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 100)),
            new LineSegment2D(new(100, 100), new(0, 100)),
            new LineSegment2D(new(0, 100), new(0, 0)),
        ]);

        var hole = new Loop2D("hole", LoopRole.Hole, [
            new LineSegment2D(new(30, 30), new(30, 70)),
            new LineSegment2D(new(30, 70), new(70, 70)),
            new LineSegment2D(new(70, 70), new(70, 30)),
            new LineSegment2D(new(70, 30), new(30, 30)),
        ]);

        var tree = new ContainmentTree();
        var result = tree.Build([outer, hole]);

        Assert.Single(result.OuterLoops);
        Assert.Single(result.Holes);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-PRP-005")]
    public void Endpoint_index_finds_gaps()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };
        var index = new EndpointIndex(tolerance);

        index.Add(new LineSegment2D(new(0, 0), new(10, 0)), "a");
        index.Add(new LineSegment2D(new(10, 0), new(10, 10)), "b");
        index.Add(new LineSegment2D(new(10, 10), new(0.06, 10)), "c"); // gap 0.06 to (0,10)
        index.Add(new LineSegment2D(new(0, 10), new(0, 0)), "d");     // left edge closes at (0,10)

        var gaps = index.FindGaps();
        Assert.True(gaps.Count > 0, "Should find at least one gap");
    }
}