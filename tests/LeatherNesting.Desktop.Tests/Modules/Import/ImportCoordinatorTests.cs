using LeatherNesting.Application;
using LeatherNesting.Desktop.Modules.Import;
using LeatherNesting.Desktop.Workspace;
using LeatherNesting.Domain;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Import;

public sealed class ImportCoordinatorTests
{
    [Fact]
    public async Task Confirming_an_inspection_updates_the_same_current_workspace_project()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "dxf-source");
        try
        {
            var workspace = new InMemoryWorkspaceSession();
            var coordinator = new ImportCoordinator(
                new ImportDxfUseCase(new StubReader(CreateResult())),
                new StubProjectStore(),
                new StubGeometryReader(),
                workspace,
                workspace);

            coordinator.CreateProject("凉鞋");
            await coordinator.InspectAsync(path, CancellationToken.None);

            Assert.True(coordinator.State.RequiresUnitConfirmation);
            Assert.Equal(DxfDeclaredUnit.Millimetres, coordinator.State.Inspection?.DeclaredUnit);
            Assert.Empty(coordinator.State.Project!.Imports);

            coordinator.ConfirmMillimetres();

            var project = coordinator.State.Project!;
            Assert.Single(project.Imports);
            Assert.Equal(project.Id.ToString("N"), workspace.Snapshot.CurrentProject?.Id);
            Assert.Equal(project.Name, workspace.Snapshot.CurrentProject?.Name);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Cancelling_an_inspection_keeps_the_workspace_on_the_unmodified_project()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "dxf-source");
        try
        {
            var workspace = new InMemoryWorkspaceSession();
            var coordinator = new ImportCoordinator(
                new ImportDxfUseCase(new StubReader(CreateResult())),
                new StubProjectStore(),
                new StubGeometryReader(),
                workspace,
                workspace);
            coordinator.CreateProject("凉鞋");
            var expectedId = workspace.Snapshot.CurrentProject!.Id;

            await coordinator.InspectAsync(path, CancellationToken.None);
            coordinator.CancelImport();

            Assert.False(coordinator.State.RequiresUnitConfirmation);
            Assert.Empty(coordinator.State.Project!.Imports);
            Assert.Equal(expectedId, workspace.Snapshot.CurrentProject?.Id);
        }
        finally { File.Delete(path); }
    }

    private static DxfImportResult CreateResult() =>
        new([], [], [new ImportDiagnostic("DXF-UNIT-REVIEW", "Blocking", "确认单位")], UnitDecision.Unresolved, DxfDeclaredUnit.Millimetres, 4);

    private sealed class StubReader(DxfImportResult result) : IDxfReader
    {
        public Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class StubProjectStore : IProjectStore
    {
        public Task SaveAsync(string path, ProjectDocument project, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ProjectDocument> LoadAsync(string path, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StubGeometryReader : IImportGeometryReader
    {
        public Task<IReadOnlyList<LeatherNesting.Geometry.Loop2D>> ReadAsync(string path, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<LeatherNesting.Geometry.Loop2D>>([]);
    }
}
