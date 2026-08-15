using System.Globalization;
using System.Text;
using LeatherNesting.Geometry;
using LeatherNesting.Infrastructure.Dxf;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class DxfArcTopologyTests
{
    private static async Task<IReadOnlyList<Loop2D>> ReadAsync(string dxf)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, dxf);
            return await new AsciiDxfGeometryReader().ReadAsync(path, CancellationToken.None);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string LwPolyline(params (double X, double Y, double Bulge)[] vertices)
    {
        var sb = new StringBuilder();
        sb.AppendLine("0").AppendLine("LWPOLYLINE");
        sb.AppendLine("8").AppendLine("0");
        sb.AppendLine("70").AppendLine("1");
        sb.AppendLine("90").AppendLine(vertices.Length.ToString(CultureInfo.InvariantCulture));
        foreach (var v in vertices)
        {
            sb.AppendLine("10").AppendLine(v.X.ToString("R", CultureInfo.InvariantCulture));
            sb.AppendLine("20").AppendLine(v.Y.ToString("R", CultureInfo.InvariantCulture));
            sb.AppendLine("42").AppendLine(v.Bulge.ToString("R", CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }

    private static string ArcEntity(double cx, double cy, double r, double start, double end)
    {
        var sb = new StringBuilder();
        sb.AppendLine("0").AppendLine("ARC");
        sb.AppendLine("8").AppendLine("0");
        sb.AppendLine("10").AppendLine(cx.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("20").AppendLine(cy.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("40").AppendLine(r.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("50").AppendLine(start.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("51").AppendLine(end.ToString("R", CultureInfo.InvariantCulture));
        return sb.ToString();
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-ARC-001")]
    public async Task Bulge_polyline_reads_arc_as_circular_arc()
    {
        // A(0,0) → B(10,0) line, B(10,0) → C(20,0) half-circle (bulge=1), C → A line.
        var dxf = LwPolyline((0, 0, 0), (10, 0, 1), (20, 0, 0));
        var loops = await ReadAsync(dxf);

        var loop = Assert.Single(loops);
        Assert.Equal(3, loop.Curves.Count);
        Assert.IsType<LineSegment2D>(loop.Curves[0]);

        var arc = Assert.IsType<CircularArc2D>(loop.Curves[1]);
        Assert.Equal(5, arc.Radius, 6);
        Assert.Equal(15, arc.Centre.X, 6);
        Assert.Equal(0, arc.Centre.Y, 6);
        Assert.Equal(180, arc.StartAngleDegrees, 6);
        Assert.Equal(180, arc.SweepAngleDegrees, 6);
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-ARC-002")]
    public async Task Arc_entity_reads_as_circular_arc()
    {
        var dxf = ArcEntity(cx: 10, cy: 5, r: 5, start: 270, end: 360);
        var loops = await ReadAsync(dxf);

        var loop = Assert.Single(loops);
        var arc = Assert.IsType<CircularArc2D>(Assert.Single(loop.Curves));
        Assert.Equal(5, arc.Radius, 6);
        Assert.Equal(10, arc.Centre.X, 6);
        Assert.Equal(5, arc.Centre.Y, 6);
        Assert.Equal(270, arc.StartAngleDegrees, 6);
        Assert.Equal(90, arc.SweepAngleDegrees, 6);
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-ARC-003")]
    public async Task Straight_polyline_area_unchanged()
    {
        // 100×50 rectangle, all bulges 0.
        var dxf = LwPolyline((0, 0, 0), (100, 0, 0), (100, 50, 0), (0, 50, 0));
        var loops = await ReadAsync(dxf);

        var loop = Assert.Single(loops);
        Assert.Equal(5000, loop.Area, 3);
    }
}
