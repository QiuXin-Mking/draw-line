using Avalonia.Controls;
using LeatherNesting.Application;
using LeatherNesting.Desktop.Workspace;
using LeatherNesting.Domain;

namespace LeatherNesting.Desktop.Modules.Import;

/// <summary>Coordinates inspection, confirmation, persistence, and the workspace's single current project.</summary>
public sealed class ImportCoordinator : IImportCoordinator
{
    private readonly ImportDxfUseCase importDxf;
    private readonly IProjectStore projectStore;
    private readonly IImportGeometryReader geometryReader;
    private readonly IWorkspaceSession workspace;
    private readonly IWorkspaceCommands workspaceCommands;
    private readonly IImportWorkbenchFactory? workbenchFactory;
    private ImportDxfPreparation? _preparation;

    public ImportCoordinator(
        ImportDxfUseCase importDxf,
        IProjectStore projectStore,
        IImportGeometryReader geometryReader,
        IWorkspaceSession workspace,
        IWorkspaceCommands workspaceCommands,
        IImportWorkbenchFactory? workbenchFactory = null)
    {
        this.importDxf = importDxf ?? throw new ArgumentNullException(nameof(importDxf));
        this.projectStore = projectStore ?? throw new ArgumentNullException(nameof(projectStore));
        this.geometryReader = geometryReader ?? throw new ArgumentNullException(nameof(geometryReader));
        this.workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        this.workspaceCommands = workspaceCommands ?? throw new ArgumentNullException(nameof(workspaceCommands));
        this.workbenchFactory = workbenchFactory;
        ImportModule.BindCoordinator(this);
    }

    public ImportWorkflowState State { get; private set; } = ImportWorkflowState.Empty;

    public WorkspaceSnapshot Workspace => workspace.Snapshot;

    public void CreateProject(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _preparation = null;
        State = new ImportWorkflowState(ProjectDocument.CreateNew(name.Trim()), null);
        PublishCurrentProject();
    }

    public async Task InspectAsync(string path, CancellationToken cancellationToken)
    {
        if (State.Project is null) throw new InvalidOperationException("请先新建项目。");
        _preparation = await importDxf.InspectAsync(path, cancellationToken);
        State = State with { Inspection = _preparation.Result };
    }

    public void ConfirmMillimetres()
    {
        if (State.Project is null || _preparation is null) throw new InvalidOperationException("没有可确认的 DXF 导入。");
        var project = _preparation.CommitTo(State.Project, UnitDecision.ConfirmedMillimetres);
        _preparation = null;
        State = new ImportWorkflowState(project, null);
        PublishCurrentProject();
    }

    public void CancelImport()
    {
        _preparation = null;
        State = State with { Inspection = null };
    }

    public async Task SaveAsync(string path, CancellationToken cancellationToken)
    {
        if (State.Project is null) throw new InvalidOperationException("没有可保存的项目。");
        await projectStore.SaveAsync(path, State.Project, cancellationToken);
        State = State with { Project = State.Project.MarkSaved() };
        PublishCurrentProject();
    }

    public bool CanEnterWorkbench(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !State.RequiresUnitConfirmation &&
        State.Project?.Imports.Any(import =>
            import.UnitDecision == UnitDecision.ConfirmedMillimetres &&
            PathsEqual(import.SourcePath, path)) == true;

    public Task<Control> CreateWorkbenchAsync(string path, CancellationToken cancellationToken)
    {
        if (!CanEnterWorkbench(path))
            throw new InvalidOperationException("必须先检查当前 DXF 并确认毫米单位，才能进入工艺工作台。");
        if (workbenchFactory is not null) return workbenchFactory.CreateAsync(path, cancellationToken);
        return CreateDefaultWorkbenchAsync(path, cancellationToken);
    }

    private async Task<Control> CreateDefaultWorkbenchAsync(string path, CancellationToken cancellationToken)
    {
        var loops = await geometryReader.ReadAsync(path, cancellationToken);
        if (loops.Count == 0) throw new InvalidOperationException("DXF 中没有可编辑的闭合轮廓。");
        throw new NotSupportedException("工艺工作台尚未由 Desktop composition 接入。");
    }

    private void PublishCurrentProject()
    {
        var project = State.Project!;
        workspaceCommands.SetCurrentProject(new WorkspaceProjectSummary(
            project.Id.ToString("N"),
            project.Name,
            Status: project.IsDirty ? "未保存" : "已保存"));
    }

    private static bool PathsEqual(string left, string right) =>
        StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(left), Path.GetFullPath(right));
}
