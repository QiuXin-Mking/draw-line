using LeatherNesting.Application.Domain;
using LeatherNesting.Domain;
using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Nesting;
using LeatherNesting.Infrastructure.Projects;
using PieceEntity = LeatherNesting.Application.Domain.Piece;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class ProjectSnapshotStoreTests
{
    private static Loop2D Rect(string id) => new(id, LoopRole.Outer, [
        new Polyline2D([new(0, 0), new(10, 0), new(10, 5), new(0, 5), new(0, 0)]),
    ]);

    private static NestingProject Project() => new(
        ProjectDocument.CreateNew("snap"),
        [new PieceEntity("p1", "鞋面", "L", Rect("p1"))],
        [new Material("m1", "牛皮", Rect("m1"))],
        [new NestResult([new NestPlacement("p1", new Transform2D(5, 5, 90, false), Rect("p1"))], [], 0.5)]);

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-SNAP-001")]
    public async Task Snapshot_round_trip_preserves_content()
    {
        var store = new ProjectSnapshotStore();
        var projectPath = Path.GetTempFileName();
        try
        {
            await store.SaveSnapshotAsync(projectPath, Project(), CancellationToken.None);

            var loaded = await store.LoadSnapshotAsync(projectPath, CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal("snap", loaded.Document.Name);
            Assert.Equal("鞋面", Assert.Single(loaded.Pieces).Name);
            Assert.Equal(0.5, Assert.Single(loaded.NestingResults).Utilization, 6);
        }
        finally
        {
            File.Delete(projectPath);
            File.Delete(ProjectSnapshotStore.SnapshotPath(projectPath));
        }
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-SNAP-002")]
    public async Task No_snapshot_returns_null_and_clear_removes_it()
    {
        var store = new ProjectSnapshotStore();
        var projectPath = Path.GetTempFileName();
        try
        {
            Assert.Null(await store.LoadSnapshotAsync(projectPath, CancellationToken.None));

            await store.SaveSnapshotAsync(projectPath, Project(), CancellationToken.None);
            Assert.NotNull(await store.LoadSnapshotAsync(projectPath, CancellationToken.None));

            store.ClearSnapshot(projectPath);
            Assert.Null(await store.LoadSnapshotAsync(projectPath, CancellationToken.None));
        }
        finally
        {
            File.Delete(projectPath);
            File.Delete(ProjectSnapshotStore.SnapshotPath(projectPath));
        }
    }
}
