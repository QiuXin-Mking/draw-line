using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class ArcAreaTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-ARC")]
    public void Semicircle_area_is_exact()
    {
        // Upper semicircle of radius 10, closed by its diameter.
        var loop = new Loop2D("semi", LoopRole.Outer, [
            new CircularArc2D(new(0, 0), 10, 0, 180),
            new LineSegment2D(new(-10, 0), new(10, 0)),
        ]);

        // Area = π·r²/2 = 50π ≈ 157.08 (chord approximation would give 0).
        Assert.Equal(Math.PI * 50, loop.Area, 3);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-ARC")]
    public void Full_circle_area_is_exact()
    {
        // A full circle as a single 360° arc.
        var loop = new Loop2D("circle", LoopRole.Outer, [
            new CircularArc2D(new(0, 0), 5, 0, 360),
        ]);

        Assert.Equal(Math.PI * 25, loop.Area, 3);
    }
}
