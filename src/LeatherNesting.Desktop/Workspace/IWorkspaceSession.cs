namespace LeatherNesting.Desktop.Workspace;

/// <summary>Read-only subscription point for the current desktop workspace state.</summary>
public interface IWorkspaceSession
{
    WorkspaceSnapshot Snapshot { get; }

    /// <summary>Raised after a command has replaced <see cref="Snapshot"/> with a new value.</summary>
    event EventHandler<WorkspaceSnapshot>? SnapshotChanged;
}
