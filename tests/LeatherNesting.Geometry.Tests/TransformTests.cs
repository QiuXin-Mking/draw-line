using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class TransformTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-TRANSFORM")]
    public void Point_translate_rotate_mirror()
    {
        var translated = new Transform2D(10, 20, 0, false).Apply(new Point2D(0, 0));
        Assert.Equal(10, translated.X, 6);
        Assert.Equal(20, translated.Y, 6);

        var rotated = new Transform2D(0, 0, 90, false).Apply(new Point2D(1, 0));
        Assert.Equal(0, rotated.X, 6);
        Assert.Equal(1, rotated.Y, 6);

        var mirrored = new Transform2D(0, 0, 0, true).Apply(new Point2D(5, 3));
        Assert.Equal(-5, mirrored.X, 6);
        Assert.Equal(3, mirrored.Y, 6);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-TRANSFORM")]
    public void Loop_translate_preserves_area()
    {
        var loop = new Loop2D("rect", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var moved = new Transform2D(10, 20, 0, false).Apply(loop);

        Assert.Equal(loop.Area, moved.Area, 6);
        Assert.Equal(new Point2D(10, 20), moved.Curves[0].StartPoint);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-TRANSFORM")]
    public void Arc_rotation_adjusts_start_angle()
    {
        // Semicircle: centre (0,0) radius 10, 0°→180°.
        var loop = new Loop2D("semi", LoopRole.Outer, [
            new CircularArc2D(new(0, 0), 10, 0, 180),
            new LineSegment2D(new(-10, 0), new(10, 0)),
        ]);

        var rotated = new Transform2D(0, 0, 90, false).Apply(loop);
        var arc = Assert.IsType<CircularArc2D>(rotated.Curves[0]);

        Assert.Equal(90, arc.StartAngleDegrees, 6);
        Assert.Equal(180, arc.SweepAngleDegrees, 6);
    }
}
