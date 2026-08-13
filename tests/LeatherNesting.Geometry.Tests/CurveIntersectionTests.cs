using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Intersection;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class CurveIntersectionTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-INTERSECT")]
    public void LineArc_finds_arc_intersection_only()
    {
        // Upper semicircle of radius 5, from (5,0) to (-5,0) through the top.
        var arc = new CircularArc2D(new(0, 0), 5, 0, 180);
        // Vertical line through the centre crosses the circle at (0,5) and (0,-5);
        // only (0,5) is on the upper semicircle.
        var line = new LineSegment2D(new(0, -10), new(0, 10));

        var points = CurveIntersection.LineArc(line, arc);

        Assert.Single(points);
        Assert.Equal(0, points[0].X, 6);
        Assert.Equal(5, points[0].Y, 6);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-GEO-INTERSECT")]
    public void ArcArc_finds_two_circle_intersections()
    {
        // Two full circles of radius 5, centres (0,0) and (5,0), intersect at x=2.5.
        var a = new CircularArc2D(new(0, 0), 5, 0, 360);
        var b = new CircularArc2D(new(5, 0), 5, 0, 360);

        var points = CurveIntersection.ArcArc(a, b);

        Assert.Equal(2, points.Count);
        Assert.All(points, p => Assert.Equal(2.5, p.X, 6));
    }
}
