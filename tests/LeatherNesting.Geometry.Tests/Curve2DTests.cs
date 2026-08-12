using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class Curve2DTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-CRV-001")]
    public void Line_segment_length_is_correct()
    {
        var line = new LineSegment2D(new(0, 0), new(3, 4));
        Assert.Equal(5, line.Length, 6);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-CRV-002")]
    public void Point2D_rejects_NaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Point2D(double.NaN, 0));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-CRV-003")]
    public void Point2D_rejects_Infinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Point2D(0, double.PositiveInfinity));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-CRV-004")]
    public void Polyline_requires_at_least_two_points()
    {
        Assert.Throws<ArgumentException>(() => new Polyline2D([new(0, 0)]));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-CRV-005")]
    public void Polyline_exists_with_two_points()
    {
        var poly = new Polyline2D([new(0, 0), new(10, 0)]);
        Assert.Equal(10, poly.Length, 6);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-CRV-006")]
    public void Circular_arc_requires_positive_radius()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CircularArc2D(Point2D.Origin, 0, 0, 90));
    }
}