namespace LeatherNesting.Desktop.Workspace;

/// <summary>Small, view-independent description of the project open in the desktop workspace.</summary>
public sealed record WorkspaceProjectSummary(
    string Id,
    string Name,
    string? ProjectNumber = null,
    string? Status = null);
