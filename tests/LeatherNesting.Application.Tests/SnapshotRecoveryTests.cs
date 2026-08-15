using LeatherNesting.Application;
using LeatherNesting.Application.Domain;
using LeatherNesting.Domain;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Application.Tests;

public sealed class SnapshotRecoveryTests
{
    private static Loop2D Rect(string id) => new(id, LoopRole.Outer, [
        new Polyline2D([new(0, 0), new(10, 0), new(10, 5), new(0, 5), new(0, 0)]),
    ]);

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-SNAP-003")]
    public void Throttle_flushes_every_10_operations()
    {
        var throttle = new SnapshotThrottle(10);
        for (var i = 1; i <= 9; i++)
            Assert.False(throttle.ShouldFlush());
        Assert.True(throttle.ShouldFlush());   // 10th
        Assert.False(throttle.ShouldFlush());  // 11th resets count
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-SNAP-004")]
    public void Factory_builds_pieces_from_stable_id()
    {
        var doc = ProjectDocument.CreateNew("test");
        var project = NestingProjectFactory.FromLoops([Rect("loop-1"), Rect("loop-2")], doc);

        Assert.Equal(2, project.Pieces.Count);
        Assert.Equal("loop-1", project.Pieces[0].Id);
        Assert.Equal("loop-2", project.Pieces[1].Id);
        Assert.Empty(project.Pieces[0].Name);
        Assert.Empty(project.Pieces[0].Size);
    }

    [Fact]
    [Trait("Stage", "3")]
    [Trait("TestId", "P3-SNAP-005")]
    public void Coordinator_flushes_after_10_operations()
    {
        var store = new FakeSnapshotStore();
        var doc = ProjectDocument.CreateNew("test");
        var loops = new List<Loop2D> { Rect("loop-1") };
        var coordinator = new OperationSnapshotCoordinator(store, "proj.lnproj", () => loops, () => doc, threshold: 10);

        for (var i = 0; i < 9; i++)
            coordinator.RecordOperation();
        Assert.Equal(0, store.SaveCount); // fewer than 10 → no flush

        coordinator.RecordOperation();    // 10th → flush
        Assert.Equal(1, store.SaveCount);
        Assert.Single(store.LastProject!.Pieces);
    }

    private sealed class FakeSnapshotStore : ISnapshotStore
    {
        public int SaveCount { get; private set; }
        public NestingProject? LastProject { get; private set; }

        public Task SaveSnapshotAsync(string projectPath, NestingProject project, CancellationToken cancellationToken)
        {
            SaveCount++;
            LastProject = project;
            return Task.CompletedTask;
        }

        public Task<NestingProject?> LoadSnapshotAsync(string projectPath, CancellationToken cancellationToken) =>
            Task.FromResult<NestingProject?>(null);

        public void ClearSnapshot(string projectPath) { }
    }
}
