using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Features;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class NotchTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOT-001")]
    public void Valid_notch_passes_validation()
    {
        var contour = new Loop2D("contour", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var notch = new NotchFeature("contour", 20.0, NotchShape.V, 2.0, 0.8, MaterialSide.Outside);
        var validator = new NotchValidator();
        var result = validator.Validate(notch, contour, []);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOT-002")]
    public void Zero_width_notch_fails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NotchFeature("contour", 10.0, NotchShape.V, 0, 1.0, MaterialSide.Outside));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOT-002")]
    public void Negative_depth_notch_fails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NotchFeature("contour", 10.0, NotchShape.V, 2.0, -1.0, MaterialSide.Outside));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOT-002")]
    public void NaN_width_notch_fails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NotchFeature("contour", 10.0, NotchShape.V, double.NaN, 1.0, MaterialSide.Outside));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOT-002")]
    public void Overlapping_notches_fails()
    {
        var contour = new Loop2D("contour", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var existing = new NotchFeature("contour", 20.0, NotchShape.V, 2.0, 0.8, MaterialSide.Outside);
        // Place a second notch very close to the first
        var notch = new NotchFeature("contour", 20.5, NotchShape.V, 2.0, 0.8, MaterialSide.Outside);

        var validator = new NotchValidator();
        var result = validator.Validate(notch, contour, [existing]);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOT-001")]
    public void Notch_geometry_is_generated_correctly()
    {
        var notch = new NotchFeature("contour", 10.0, NotchShape.V, 2.0, 0.8, MaterialSide.Outside);

        var anchor = new Point2D(10, 0);
        var tangent = new Point2D(1, 0); // horizontal
        var normal = new Point2D(0, -1); // pointing down (outside)

        var geometry = notch.GenerateGeometry(anchor, tangent, normal);
        Assert.NotNull(geometry);
        Assert.True(geometry.Points.Count >= 2);
    }
}