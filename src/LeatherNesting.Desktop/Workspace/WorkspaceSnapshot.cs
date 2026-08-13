namespace LeatherNesting.Desktop.Workspace;

/// <summary>
/// Immutable state shared by desktop modules. It deliberately contains no Avalonia controls or views.
/// </summary>
public sealed record WorkspaceSnapshot(
    WorkspaceProjectSummary? CurrentProject,
    string? SelectedObjectId,
    string? ActiveModuleId,
    string? DemoHint,
    string? TodoHint)
{
    public static WorkspaceSnapshot Empty { get; } = new(null, null, null, null, null);
}
