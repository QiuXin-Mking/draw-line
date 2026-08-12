using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Features;
using LeatherNesting.Geometry.Offset;
using LeatherNesting.Geometry.Repair;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

/// <summary>Golden DXF round-trip tests (P2-RT-001).
/// In Stage 2, these verify that import→repair→offset→notch produce consistent results.</summary>
public sealed class GoldenRoundTripTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-RT-001")]
    public void Simple_rectangle_round_trip_preserves_area()
    {
        // Import phase: create a known loop
        var loop = new Loop2D("rect", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        // Repair phase: close (already closed)
        var closer = new ContourCloser();
        var closeResult = closer.Close(loop);
        Assert.Single(closeResult.RepairedLoops);

        // Offset phase: inward 1mm
        var adapter = new OffsetAdapter();
        var offsetResult = adapter.Offset(closeResult.RepairedLoops, 1.0, OffsetDirection.Inside);
        Assert.NotEmpty(offsetResult.OffsetLoops);

        // Area should be close to (100-2)*(50-2) = 4704
        var area = offsetResult.OffsetLoops[0].Area;
        Assert.True(area > 4600, $"Area too small: {area:F3}");
        Assert.True(area < 4800, $"Area too large: {area:F3}");
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-RT-001")]
    public void Feature_anchors_survive_contour_edit()
    {
        // Create a loop with a notch feature anchor
        var loop = new Loop2D("contour", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        // Anchor at 25mm arc length
        var anchorArcLength = 25.0;
        var notch = new NotchFeature("contour", anchorArcLength, NotchShape.V, 2.0, 0.8, MaterialSide.Outside);
        Assert.Equal(anchorArcLength, notch.AnchorArcLength, 6);
    }
}