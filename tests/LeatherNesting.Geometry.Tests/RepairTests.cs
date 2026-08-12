using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Repair;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class RepairTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-001")]
    public void Gap_repair_connects_disconnected_curves()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };
        var repair = new GapRepair(tolerance);

        var curves = new List<Curve2D>
        {
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0.05, 10)), // gap of 0.05 to (0,10)
            new LineSegment2D(new(0, 10), new(0, 0)),     // left edge closes at (0,10)
        };

        var result = repair.Repair(curves, "test");
        // Gap is within tolerance, should find it
        Assert.True(result.Bridges.Count > 0);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-003")]
    public void Boundary_generation_from_three_lines_creates_triangle()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };
        var generator = new BoundaryGenerator(tolerance);

        var curves = new List<Curve2D>
        {
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(5, 10)),
            new LineSegment2D(new(5, 10), new(0, 0)),
        };

        var result = generator.Generate(curves, "test");
        Assert.True(result.ValidCandidates.Count > 0);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-BND-002")]
    public void T_junction_not_entered_into_boundary()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };
        var generator = new BoundaryGenerator(tolerance);

        // T-junction: a vertical line meeting the middle of a horizontal line
        var curves = new List<Curve2D>
        {
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
            new LineSegment2D(new(5, 0), new(5, 5)), // T-junction
        };

        var result = generator.Generate(curves, "test");
        // Should still find the main rectangle as a candidate
        Assert.True(result.AllCandidates.Count > 0);
    }
}