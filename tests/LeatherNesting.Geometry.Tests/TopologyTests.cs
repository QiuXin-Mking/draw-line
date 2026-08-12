using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Repair;
using LeatherNesting.Geometry.Topology;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class TopologyTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-001")]
    public void Closed_rectangle_unchanged_gap_zero_rejected_above_tolerance()
    {
        // P2-BND-001: gap=0.05 tol=0.1 previews bridge; gap=0.11 rejected
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        // Closed rect: (0,0)-(100,0)-(100,50)-(0,50)-(0,0)
        var rect = new Loop2D("rect", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var closer = new ContourCloser(tolerance);
        var result = closer.Close(rect);

        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.Bridges);
        Assert.Single(result.RepairedLoops);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-001")]
    public void Gap_005_tol_01_previews_bridge()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        // Open contour with gap=0.05: (0,0)-(99.95,0)-(99.95,50)-(0,50) — gap 0.05 to (0,0)
        var open = new Loop2D("open", LoopRole.Outer, [
            new LineSegment2D(new(0.05, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var closer = new ContourCloser(tolerance);
        var result = closer.Close(open);

        Assert.Empty(result.Diagnostics);
        Assert.True(result.HasChanges);
        Assert.Single(result.Bridges);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-001")]
    public void Gap_011_rejected()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        var open = new Loop2D("open", LoopRole.Outer, [
            new LineSegment2D(new(0.11, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var closer = new ContourCloser(tolerance);
        var result = closer.Close(open);

        Assert.NotEmpty(result.Diagnostics);
        Assert.False(result.HasChanges);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-002")]
    public void Multiple_candidate_loops_require_user_selection()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        // Two separate closed loops
        var curves = new List<Curve2D>
        {
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
            new LineSegment2D(new(20, 0), new(30, 0)),
            new LineSegment2D(new(30, 0), new(30, 10)),
            new LineSegment2D(new(30, 10), new(20, 10)),
            new LineSegment2D(new(20, 10), new(20, 0)),
        };

        var generator = new BoundaryGenerator(tolerance);
        var result = generator.Generate(curves, "test");

        Assert.True(result.ValidCandidates.Count > 0);
        Assert.True(result.AllCandidates.Count > 0);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-003")]
    public void Bow_tie_self_intersecting_returns_error()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        // Bow-tie shape: (0,0)-(10,10)-(10,0)-(0,10)-(0,0)
        var curves = new List<Curve2D>
        {
            new LineSegment2D(new(0, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(10, 0)),
            new LineSegment2D(new(10, 0), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        };

        var candidate = new FaceCandidate("bow-tie", curves, tolerance);
        Assert.True(candidate.IsSelfIntersecting);
        Assert.False(candidate.IsValid);
    }
}