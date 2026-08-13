using LeatherNesting.Application;
using LeatherNesting.Desktop.Composition;
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

    [Fact]
    public async Task Workbench_is_blocked_until_the_current_source_has_a_confirmed_unit()
    {
        var path = Path.GetTempFileName();
        var otherPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "dxf-source");
        try
        {
            var workspace = new InMemoryWorkspaceSession();
            var factory = new StubWorkbenchFactory();
            var coordinator = new ImportCoordinator(
                new ImportDxfUseCase(new StubReader(CreateResult())),
                new StubProjectStore(),
                new StubGeometryReader(),
                workspace,
                workspace,
                factory);
            coordinator.CreateProject("凉鞋");

            await coordinator.InspectAsync(path, CancellationToken.None);
            Assert.False(coordinator.CanEnterWorkbench(path));
            await Assert.ThrowsAsync<InvalidOperationException>(() => coordinator.CreateWorkbenchAsync(path, CancellationToken.None));

            coordinator.ConfirmMillimetres();
            Assert.True(coordinator.CanEnterWorkbench(path));
            Assert.False(coordinator.CanEnterWorkbench(otherPath));
            await coordinator.CreateWorkbenchAsync(path, CancellationToken.None);
            Assert.Equal(1, factory.Calls);
        }
        finally
        {
            File.Delete(path);
            File.Delete(otherPath);
        }
    }

    [Fact]
    public async Task Saving_persists_and_publishes_the_same_project_as_saved()
    {
        var workspace = new InMemoryWorkspaceSession();
        var store = new StubProjectStore();
        var coordinator = new ImportCoordinator(
            new ImportDxfUseCase(new StubReader(CreateResult())),
            store,
            new StubGeometryReader(),
            workspace,
            workspace);
        coordinator.CreateProject("凉鞋");

        await coordinator.SaveAsync("project.lnproj", CancellationToken.None);

        Assert.Equal("project.lnproj", store.SavedPath);
        Assert.False(coordinator.State.Project!.IsDirty);
        Assert.Equal("已保存", workspace.Snapshot.CurrentProject?.Status);
    }

    [Fact]
    public void Desktop_composition_discovers_the_local_import_module_once()
    {
        var workspace = new InMemoryWorkspaceSession();
        var module = Assert.Single(DesktopComposition.CreateModules(workspace, workspace), item => item.Metadata.Id == "M02");

        Assert.IsType<ImportModule>(module);
        Assert.Equal("M02", module.Metadata.Id);
        Assert.Equal(2, module.Metadata.Order);
    }

    private static DxfImportResult CreateResult() =>
        new([], [], [new ImportDiagnostic("DXF-UNIT-REVIEW", "Blocking", "确认单位")], UnitDecision.Unresolved, DxfDeclaredUnit.Millimetres, 4);

    private sealed class StubReader(DxfImportResult result) : IDxfReader
    {
        public Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class StubProjectStore : IProjectStore
    {
        public string? SavedPath { get; private set; }

        public Task SaveAsync(string path, ProjectDocument project, CancellationToken cancellationToken)
        {
            SavedPath = path;
            return Task.CompletedTask;
        }

        public Task<ProjectDocument> LoadAsync(string path, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StubGeometryReader : IImportGeometryReader
    {
        public Task<IReadOnlyList<LeatherNesting.Geometry.Loop2D>> ReadAsync(string path, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<LeatherNesting.Geometry.Loop2D>>([]);
    }

    private sealed class StubWorkbenchFactory : IImportWorkbenchFactory
    {
        public int Calls { get; private set; }

        public Task<Avalonia.Controls.Control> CreateAsync(string path, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<Avalonia.Controls.Control>(new Avalonia.Controls.Border());
        }
    }
}
