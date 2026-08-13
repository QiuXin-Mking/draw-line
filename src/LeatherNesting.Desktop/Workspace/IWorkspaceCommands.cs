namespace LeatherNesting.Desktop.Workspace;

/// <summary>Cross-module workspace intentions. Implementations own all state transitions.</summary>
public interface IWorkspaceCommands
{
    void SetCurrentProject(WorkspaceProjectSummary? project);

    void NavigateTo(string moduleId);

    void SelectObject(string? objectId);

    void OpenObject(string objectId, string? moduleId = null);

    void ShowDemoHint(string? message);

    void ShowTodo(string? message);
}
