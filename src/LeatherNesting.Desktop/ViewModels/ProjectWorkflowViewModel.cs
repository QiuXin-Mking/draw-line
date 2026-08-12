using LeatherNesting.Application;
using LeatherNesting.Domain;

namespace LeatherNesting.Desktop.ViewModels;

/// <summary>Coordinates the Stage 1 project/import workflow without putting business rules in the view.</summary>
public sealed class ProjectWorkflowViewModel(ImportDxfUseCase importDxf)
{
    private ImportDxfPreparation? preparation;

    public ImportWizardViewModel Wizard { get; } = new();
    public ProjectDocument? Project { get; private set; }
    public DxfImportResult? Inspection => preparation?.Result;
    public IReadOnlyList<ImportDiagnostic> Diagnostics => preparation?.Result.Diagnostics ?? [];
    public bool RequiresUnitConfirmation => preparation is not null;

    public void CreateProject(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Project = ProjectDocument.CreateNew(name.Trim());
        CancelImport();
    }

    public async Task InspectAsync(string path, CancellationToken cancellationToken)
    {
        if (Project is null) throw new InvalidOperationException("请先新建项目。");
        Wizard.Select(path);
        preparation = await importDxf.InspectAsync(path, cancellationToken);
    }

    public void ConfirmMillimetres()
    {
        if (Project is null || preparation is null) throw new InvalidOperationException("没有可确认的 DXF 导入。");
        Project = preparation.CommitTo(Project, UnitDecision.ConfirmedMillimetres);
        preparation = null;
        Wizard.Cancel();
    }

    public void CancelImport()
    {
        preparation = null;
        Wizard.Cancel();
    }

    public void MarkSaved()
    {
        if (Project is null) throw new InvalidOperationException("没有可保存的项目。");
        Project = Project.MarkSaved();
    }
}
