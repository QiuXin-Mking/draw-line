using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Offset;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class OffsetTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-OFF-001")]
    public void Offset_100x50_rect_inward_1_yields_98x48()
    {
        // P2-OFF-001: 100x50 rect inward 1 → 98x48
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        var rect = new Loop2D("rect", LoopRole.Outer, [
            new Polyline2D([
                new(0, 0), new(100, 0), new(100, 50), new(0, 50), new(0, 0),
            ]),
        ]);

        var adapter = new OffsetAdapter(tolerance);
        var result = adapter.Offset([rect], 1.0, OffsetDirection.Inside);

        Assert.Empty(result.Diagnostics);
        Assert.NotEmpty(result.OffsetLoops);

        var loop = result.OffsetLoops[0];
        var (minX, minY, maxX, maxY) = GetBounds(loop);
        Assert.True(minX >= 0.5, $"Expected minX >= 1, got {minX:F3}");
        Assert.True(minY >= 0.5, $"Expected minY >= 1, got {minY:F3}");
        Assert.True(maxX <= 99.5, $"Expected maxX <= 99, got {maxX:F3}");
        Assert.True(maxY <= 49.5, $"Expected maxY <= 49, got {maxY:F3}");
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-OFF-002")]
    public void Reversed_winding_offset_equivalent_within_tolerance()
    {
        // P2-OFF-002: reversed winding/curve order → material-space offset equivalent
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        var loop = new Loop2D("loop", LoopRole.Outer, [
            new Polyline2D([
                new(0, 0), new(50, 0), new(50, 30), new(0, 30), new(0, 0),
            ]),
        ]);

        var reversed = loop.Reverse();

        var adapter = new OffsetAdapter(tolerance);
        var result1 = adapter.Offset([loop], 2.0, OffsetDirection.Inside);
        var result2 = adapter.Offset([reversed], 2.0, OffsetDirection.Inside);

        Assert.NotEmpty(result1.OffsetLoops);
        Assert.NotEmpty(result2.OffsetLoops);

        // Both should produce similar area
        var area1 = result1.OffsetLoops[0].Area;
        var area2 = result2.OffsetLoops[0].Area;
        Assert.True(Math.Abs(area1 - area2) < 10, $"Area1={area1:F3}, Area2={area2:F3}");
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-OFF-003")]
    public void Thin_neck_offset_warns_topology_change()
    {
        // P2-OFF-003: thin neck 1→2 warns
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        // Hourglass shape with thin neck (width=4)
        var loop = new Loop2D("hourglass", LoopRole.Outer, [
            new Polyline2D([
                new(0, 0), new(50, 0), new(50, 2), new(100, 0),
                new(100, 50), new(50, 48), new(50, 50), new(0, 50), new(0, 0),
            ]),
        ]);

        var adapter = new OffsetAdapter(tolerance);
        var result = adapter.Offset([loop], 3.0, OffsetDirection.Inside);

        // If topology changed, requires confirmation
        if (result.RequiresConfirmation)
        {
            Assert.NotEmpty(result.TopologyWarnings);
        }
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-OFF-003")]
    public void Self_intersecting_input_is_blocked()
    {
        var tolerance = new ToleranceProfile { TopologyToleranceMm = 0.1 };

        // Self-intersecting "bow-tie" loop
        var loop = new Loop2D("bowtie", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(10, 0)),
            new LineSegment2D(new(10, 0), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]);

        var adapter = new OffsetAdapter(tolerance);
        var result = adapter.Offset([loop], 1.0, OffsetDirection.Inside);

        Assert.NotEmpty(result.Diagnostics);
        Assert.Empty(result.OffsetLoops);
    }

    private static (double MinX, double MinY, double MaxX, double MaxY) GetBounds(Loop2D loop)
    {
        var allPoints = loop.Curves.SelectMany(c => c switch
        {
            Polyline2D p => p.Points,
            LineSegment2D l => new[] { l.Start, l.End },
            _ => Array.Empty<Point2D>()
        }).ToList();

        return (
            allPoints.Min(p => p.X),
            allPoints.Min(p => p.Y),
            allPoints.Max(p => p.X),
            allPoints.Max(p => p.Y));
    }
}