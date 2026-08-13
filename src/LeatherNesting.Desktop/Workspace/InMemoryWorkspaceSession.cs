namespace LeatherNesting.Desktop.Workspace;

/// <summary>
/// UI-thread in-memory implementation of the workspace contract.
/// Each effective command publishes one replacement snapshot after the state is updated.
/// </summary>
public sealed class InMemoryWorkspaceSession : IWorkspaceSession, IWorkspaceCommands
{
    private WorkspaceSnapshot _snapshot;

    public InMemoryWorkspaceSession(WorkspaceSnapshot? initialSnapshot = null) =>
        _snapshot = initialSnapshot ?? WorkspaceSnapshot.Empty;

    public WorkspaceSnapshot Snapshot => _snapshot;

    public event EventHandler<WorkspaceSnapshot>? SnapshotChanged;

    public void SetCurrentProject(WorkspaceProjectSummary? project) =>
        Replace(_snapshot with { CurrentProject = project });

    public void NavigateTo(string moduleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        Replace(_snapshot with { ActiveModuleId = moduleId });
    }

    public void SelectObject(string? objectId) =>
        Replace(_snapshot with { SelectedObjectId = objectId });

    public void OpenObject(string objectId, string? moduleId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);
        if (moduleId is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        }

        Replace(_snapshot with
        {
            SelectedObjectId = objectId,
            ActiveModuleId = moduleId ?? _snapshot.ActiveModuleId,
        });
    }

    public void ShowDemoHint(string? message) =>
        Replace(_snapshot with { DemoHint = message });

    public void ShowTodo(string? message) =>
        Replace(_snapshot with { TodoHint = message });

    private void Replace(WorkspaceSnapshot next)
    {
        if (next == _snapshot)
        {
            return;
        }

        _snapshot = next;
        SnapshotChanged?.Invoke(this, next);
    }
}
