using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Offset;
using LeatherNesting.Infrastructure.Dxf;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class DxfRoundTripTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-RT-001")]
    public async Task Rectangle_round_trip_preserves_area()
    {
        var loop = new Loop2D("rect", LoopRole.Outer, [
            new Polyline2D([
                new(0, 0), new(100, 0), new(100, 50), new(0, 50), new(0, 0),
            ]),
        ]);

        var writer = new AsciiDxfWriter();
        var reader = new AsciiDxfGeometryReader();
        var path = Path.Combine(Path.GetTempPath(), $"roundtrip-{Guid.NewGuid():N}.dxf");

        try
        {
            await writer.WriteAsync(path, [loop], CancellationToken.None);
            var restored = await reader.ReadAsync(path, CancellationToken.None);

            Assert.Single(restored);
            Assert.Equal(5000.0, restored[0].Area, 1);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-RT-001")]
    public async Task Golden_rectangle_reads_correctly()
    {
        var reader = new AsciiDxfGeometryReader();
        var loops = await reader.ReadAsync(RepoFixture.Path("fixtures", "golden", "cad-repair", "rectangle.dxf"), CancellationToken.None);

        Assert.Single(loops);
        Assert.Equal(5000.0, loops[0].Area, 1);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-RT-001")]
    public async Task Offset_then_round_trip_preserves_area()
    {
        var loop = new Loop2D("rect", LoopRole.Outer, [
            new Polyline2D([
                new(0, 0), new(100, 0), new(100, 50), new(0, 50), new(0, 0),
            ]),
        ]);

        var offset = new OffsetAdapter().Offset([loop], 1.0, OffsetDirection.Inside);
        Assert.Single(offset.OffsetLoops);
        var beforeArea = offset.OffsetLoops[0].Area;

        var writer = new AsciiDxfWriter();
        var reader = new AsciiDxfGeometryReader();
        var path = Path.Combine(Path.GetTempPath(), $"roundtrip-{Guid.NewGuid():N}.dxf");

        try
        {
            await writer.WriteAsync(path, offset.OffsetLoops, CancellationToken.None);
            var restored = await reader.ReadAsync(path, CancellationToken.None);

            Assert.Single(restored);
            // Round-trip preserves the offset geometry's area (within tolerance).
            Assert.Equal(beforeArea, restored[0].Area, 1);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
