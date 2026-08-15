using LeatherNesting.Application.Domain;
using LeatherNesting.Domain;
using LeatherNesting.Geometry;

namespace LeatherNesting.Application;

/// <summary>Triggers a crash-recovery snapshot every N operations (throttled), without blocking the caller.</summary>
public sealed class OperationSnapshotCoordinator
{
    private readonly ISnapshotStore _store;
    private readonly string _projectPath;
    private readonly Func<IReadOnlyList<Loop2D>> _loops;
    private readonly Func<ProjectDocument> _document;
    private readonly SnapshotThrottle _throttle;

    public OperationSnapshotCoordinator(
        ISnapshotStore store,
        string projectPath,
        Func<IReadOnlyList<Loop2D>> loops,
        Func<ProjectDocument> document,
        int threshold = 10)
    {
        _store = store;
        _projectPath = projectPath;
        _loops = loops;
        _document = document;
        _throttle = new SnapshotThrottle(threshold);
    }

    /// <summary>Records one committed operation; flushes a snapshot when the threshold is reached.</summary>
    public void RecordOperation()
    {
        if (!_throttle.ShouldFlush())
            return;
        var project = NestingProjectFactory.FromLoops(_loops(), _document());
        _ = FlushAsync(project);
    }

    private async Task FlushAsync(NestingProject project)
    {
        try
        {
            await _store.SaveSnapshotAsync(_projectPath, project, CancellationToken.None);
        }
        catch
        {
            // A failed snapshot must not crash the caller (fire-and-forget).
        }
    }
}
