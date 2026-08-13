using Avalonia.Controls;
using LeatherNesting.Application;
using LeatherNesting.Desktop.Workspace;
using LeatherNesting.Domain;

namespace LeatherNesting.Desktop.Modules.Import;

/// <summary>UI-facing boundary for the import flow. Implementations own import state and workspace publication.</summary>
public interface IImportCoordinator
{
    ImportWorkflowState State { get; }

    WorkspaceSnapshot Workspace { get; }

    void CreateProject(string name);

    Task InspectAsync(string path, CancellationToken cancellationToken);

    void ConfirmMillimetres();

    void CancelImport();

    Task SaveAsync(string path, CancellationToken cancellationToken);

    bool CanEnterWorkbench(string path);

    Task<Control> CreateWorkbenchAsync(string path, CancellationToken cancellationToken);
}

/// <summary>A coherent projection of the project and the current, uncommitted DXF inspection.</summary>
public sealed record ImportWorkflowState(ProjectDocument? Project, DxfImportResult? Inspection)
{
    public static ImportWorkflowState Empty { get; } = new(null, null);

    public IReadOnlyList<ImportDiagnostic> Diagnostics => Inspection?.Diagnostics ?? [];

    public bool RequiresUnitConfirmation => Inspection is not null;

    public bool HasConfirmedImport => Project?.Imports.Any(import => import.UnitDecision == UnitDecision.ConfirmedMillimetres) == true;
}

/// <summary>Reads source geometry for the workbench without exposing infrastructure to the view.</summary>
public interface IImportGeometryReader
{
    Task<IReadOnlyList<LeatherNesting.Geometry.Loop2D>> ReadAsync(string path, CancellationToken cancellationToken);
}

/// <summary>Creates a workbench view for imported geometry outside the ImportView UI code.</summary>
public interface IImportWorkbenchFactory
{
    Task<Control> CreateAsync(string path, CancellationToken cancellationToken);
}
