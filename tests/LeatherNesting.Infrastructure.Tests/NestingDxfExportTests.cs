using LeatherNesting.Application;
using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Nesting;
using LeatherNesting.Infrastructure.Dxf;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class NestingDxfExportTests
{
    private static Loop2D Rect(string id, double w, double h, double ox = 0, double oy = 0) => new(id, LoopRole.Outer, [
        new Polyline2D([
            new(ox, oy), new(ox + w, oy), new(ox + w, oy + h), new(ox, oy + h), new(ox, oy),
        ]),
    ]);

    private static Loop2D Hole(string id, double x0, double y0, double x1, double y1) => new(id, LoopRole.Hole, [
        new Polyline2D([
            new(x0, y0), new(x1, y0), new(x1, y1), new(x0, y1), new(x0, y0),
        ]),
    ]);

    private static (double MinX, double MinY, double MaxX, double MaxY) BoundsOf(Loop2D loop)
    {
        var pts = loop.Curves.SelectMany(c => c switch
        {
            Polyline2D p => p.Points,
            LineSegment2D l => new[] { l.Start, l.End },
            _ => Array.Empty<Point2D>()
        }).ToList();
        return (pts.Min(p => p.X), pts.Min(p => p.Y), pts.Max(p => p.X), pts.Max(p => p.Y));
    }

    private static HashSet<int> ReadColors(string path)
    {
        var lines = File.ReadAllLines(path).Select(l => l.Trim()).ToList();
        var colors = new HashSet<int>();
        for (var i = 0; i + 1 < lines.Count; i++)
            if (lines[i] == "62" && int.TryParse(lines[i + 1], out var color))
                colors.Add(color);
        return colors;
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-DXFOUT-001")]
    public async Task Empty_nesting_exports_material_and_title_only()
    {
        var material = Rect("mat", 100, 50);
        var result = new NestResult([], [], 0);
        var useCase = new ExportNestingDxfUseCase(new AsciiNestingDxfWriter());

        var path = Path.GetTempFileName();
        try
        {
            await useCase.ExportAsync(path, result, material, 5, new Dictionary<string, PieceGeometry>(), CancellationToken.None);

            var import = await new AsciiDxfReader().ReadAsync(path, CancellationToken.None);
            Assert.Equal(2, import.Entities.Count); // 1 LWPOLYLINE + 1 TEXT
            Assert.Contains(import.Entities, e => e.Kind == DxfEntityKind.LwPolyline && e.Layer == "LEATHER");
            Assert.Contains(import.Entities, e => e.Kind == DxfEntityKind.Text && e.Layer == "ANNOTATION");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-DXFOUT-002")]
    public async Task Nesting_exports_pieces_and_annotations_with_layers()
    {
        var engine = new NestEngine();
        var pieces = new[] { Rect("p1", 20, 20), Rect("p2", 15, 30) };
        var material = Rect("mat", 100, 100);
        var result = engine.Nest(new NestRequest(pieces, material, 2, [0, 90]));
        var useCase = new ExportNestingDxfUseCase(new AsciiNestingDxfWriter());

        var path = Path.GetTempFileName();
        try
        {
            await useCase.ExportAsync(path, result, material, 2, new Dictionary<string, PieceGeometry>(), CancellationToken.None);

            var import = await new AsciiDxfReader().ReadAsync(path, CancellationToken.None);
            // 1 leather + 2 pieces + (2 piece labels + 1 title) = 6
            Assert.Equal(6, import.Entities.Count);
            Assert.Equal(1, import.Entities.Count(e => e.Layer == "LEATHER"));
            Assert.Equal(2, import.Entities.Count(e => e.Layer == "PIECES"));
            Assert.Equal(3, import.Entities.Count(e => e.Layer == "ANNOTATION"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-DXFOUT-003")]
    public async Task Round_trip_piece_contours_match_placements()
    {
        var engine = new NestEngine();
        var pieces = new[] { Rect("p1", 20, 20), Rect("p2", 15, 30) };
        var material = Rect("mat", 100, 100);
        var result = engine.Nest(new NestRequest(pieces, material, 2, [0, 90]));
        var useCase = new ExportNestingDxfUseCase(new AsciiNestingDxfWriter());

        var path = Path.GetTempFileName();
        try
        {
            await useCase.ExportAsync(path, result, material, 2, new Dictionary<string, PieceGeometry>(), CancellationToken.None);

            var loops = await new AsciiDxfGeometryReader().ReadAsync(path, CancellationToken.None);
            Assert.Equal(1 + result.Placements.Count, loops.Count); // 1 leather + N pieces

            for (var i = 0; i < result.Placements.Count; i++)
            {
                var expected = result.Placements[i].PlacedLoop;
                var actual = loops[i + 1];
                Assert.Equal(expected.Area, actual.Area, 3);
                var (eMinX, eMinY, eMaxX, eMaxY) = BoundsOf(expected);
                var (aMinX, aMinY, aMaxX, aMaxY) = BoundsOf(actual);
                Assert.Equal(eMinX, aMinX, 6);
                Assert.Equal(eMinY, aMinY, 6);
                Assert.Equal(eMaxX, aMaxX, 6);
                Assert.Equal(eMaxY, aMaxY, 6);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    [Trait("Stage", "4")]
    [Trait("TestId", "P4-DXFOUT-004")]
    public async Task Export_writes_color_coded_line_roles()
    {
        var outer = Rect("p1", 40, 20);
        var hole = Hole("p1-hole", 10, 5, 15, 10);
        var cut = new InternalLine("p1-cut", LineRole.Cut, [new LineSegment2D(new(20, 5), new(30, 5))]);
        var mark = new InternalLine("p1-mark", LineRole.Mark, [new LineSegment2D(new(30, 10), new(32, 10))]);
        var piece = new PieceGeometry(outer, [hole], [cut, mark]);

        var material = Rect("mat", 100, 100);
        var engine = new NestEngine();
        var result = engine.Nest(new NestRequest([outer], material, 2, [0]));
        var pieces = new Dictionary<string, PieceGeometry> { [outer.StableId] = piece };
        var useCase = new ExportNestingDxfUseCase(new AsciiNestingDxfWriter());

        var path = Path.GetTempFileName();
        try
        {
            await useCase.ExportAsync(path, result, material, 2, pieces, CancellationToken.None);

            var colors = ReadColors(path);
            Assert.Contains(0, colors); // 外轮廓
            Assert.Contains(3, colors); // 切割线 / 内孔
            Assert.Contains(5, colors); // 标记线
        }
        finally
        {
            File.Delete(path);
        }
    }
}
