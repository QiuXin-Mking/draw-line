using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Application;
using LeatherNesting.Domain;
using Xunit;

namespace LeatherNesting.Desktop.Tests;

public sealed class ImportWizardViewModelTests
{
    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-UI-001")]
    public void Cancel_discards_session_and_returns_to_select_file()
    {
        var viewModel = new ImportWizardViewModel();
        viewModel.Select("test.dxf");
        viewModel.Cancel();
        Assert.Equal(ImportWizardStep.SelectFile, viewModel.Step);
        Assert.Null(viewModel.SelectedPath);
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-UI-002")]
    public async Task Workflow_requires_millimetres_confirmation_before_project_is_changed()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "dxf-source");
        try
        {
            var result = new DxfImportResult([], [], [new ImportDiagnostic("DXF-UNIT-REVIEW", "Blocking", "确认单位")], UnitDecision.Unresolved, DxfDeclaredUnit.Millimetres, 4);
            var viewModel = new ProjectWorkflowViewModel(new ImportDxfUseCase(new StubReader(result)));
            viewModel.CreateProject("凉鞋");

            await viewModel.InspectAsync(path, CancellationToken.None);

            Assert.Equal(ImportWizardStep.UnitReview, viewModel.Wizard.Step);
            Assert.True(viewModel.RequiresUnitConfirmation);
            Assert.Equal(DxfDeclaredUnit.Millimetres, viewModel.Inspection?.DeclaredUnit);
            Assert.Empty(viewModel.Project!.Imports);

            viewModel.ConfirmMillimetres();

            Assert.Single(viewModel.Project.Imports);
            Assert.True(viewModel.Project.IsDirty);
            Assert.False(viewModel.RequiresUnitConfirmation);
        }
        finally { File.Delete(path); }
    }

    private sealed class StubReader(DxfImportResult result) : IDxfReader
    {
        public Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
