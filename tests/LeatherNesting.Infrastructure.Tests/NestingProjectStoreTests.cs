using System.IO.Compression;
using LeatherNesting.Application.Domain;
using LeatherNesting.Domain;
using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Nesting;
using LeatherNesting.Infrastructure.Projects;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class NestingProjectStoreTests
{
    private static Loop2D Rect(string id) => new(id, LoopRole.Outer, [
        new Polyline2D([
            new(0, 0), new(10, 0), new(10, 5), new(0, 5), new(0, 0),
        ]),
    ]);

    /// <summary>A loop mixing all three curve kinds: line, circular arc, polyline.</summary>
    private static Loop2D MixedCurveLoop(string id) => new(id, LoopRole.Outer, [
        new LineSegment2D(new(0, 0), new(10, 0)),
        new CircularArc2D(new(10, 5), 5, 270, 180),
        new Polyline2D([new(10, 10), new(0, 10), new(0, 0)]),
    ]);

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-PERSIST-001")]
    public async Task NestingProject_round_trip_preserves_content()
    {
        var store = new ZipNestingProjectStore();
        var project = new NestingProject(
            ProjectDocument.CreateNew("test-project"),
            [new Piece("p1", "鞋面", "L", MixedCurveLoop("p1"))],
            [new Material("m1", "头层牛皮", Rect("m1"))],
            [new NestResult([new NestPlacement("p1", new Transform2D(5, 5, 90, false), Rect("p1"))], [], 0.5)]);

        var path = Path.GetTempFileName();
        try
        {
            await store.SaveAsync(path, project, CancellationToken.None);
            var loaded = await store.LoadAsync(path, CancellationToken.None);

            Assert.Equal("test-project", loaded.Document.Name);

            var piece = Assert.Single(loaded.Pieces);
            Assert.Equal("鞋面", piece.Name);
            Assert.Equal("L", piece.Size);
            Assert.Equal(project.Pieces[0].Outline.Area, piece.Outline.Area, 6);

            var material = Assert.Single(loaded.Materials);
            Assert.Equal("头层牛皮", material.Name);

            var nesting = Assert.Single(loaded.NestingResults);
            Assert.Equal(0.5, nesting.Utilization, 6);
            Assert.Equal(90, nesting.Placements[0].Transform.RotationDegrees, 6);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-PERSIST-002")]
    public async Task Polymorphic_curves_round_trip_with_correct_types()
    {
        var store = new ZipNestingProjectStore();
        var project = new NestingProject(
            ProjectDocument.CreateNew("curves"),
            [new Piece("p1", "x", "M", MixedCurveLoop("p1"))],
            [],
            []);

        var path = Path.GetTempFileName();
        try
        {
            await store.SaveAsync(path, project, CancellationToken.None);
            var loaded = await store.LoadAsync(path, CancellationToken.None);

            var curves = Assert.Single(loaded.Pieces).Outline.Curves;
            Assert.Equal(3, curves.Count);
            Assert.IsType<LineSegment2D>(curves[0]);
            Assert.IsType<CircularArc2D>(curves[1]);
            Assert.IsType<Polyline2D>(curves[2]);

            var arc = Assert.IsType<CircularArc2D>(curves[1]);
            Assert.Equal(5, arc.Radius, 6);
            Assert.Equal(270, arc.StartAngleDegrees, 6);
            Assert.Equal(180, arc.SweepAngleDegrees, 6);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-PERSIST-003")]
    public async Task Legacy_manifest_without_collections_loads_empty()
    {
        var store = new ZipNestingProjectStore();
        var path = Path.GetTempFileName();
        try
        {
            // Simulate a legacy manifest that only carries the document metadata.
            var json = """{"document":{"id":"00000000-0000-0000-0000-000000000000","name":"legacy","schemaVersion":1,"revision":0,"isDirty":false,"imports":[]}}""";
            await using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: false))
            await using (var stream = zip.CreateEntry("manifest.json", CompressionLevel.Optimal).Open())
            await using (var writer = new StreamWriter(stream))
                await writer.WriteAsync(json);

            var loaded = await store.LoadAsync(path, CancellationToken.None);

            Assert.Equal("legacy", loaded.Document.Name);
            Assert.Empty(loaded.Pieces);
            Assert.Empty(loaded.Materials);
            Assert.Empty(loaded.NestingResults);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
