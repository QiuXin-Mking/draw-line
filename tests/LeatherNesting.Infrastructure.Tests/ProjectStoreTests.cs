using LeatherNesting.Domain;
using LeatherNesting.Infrastructure.Projects;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class ProjectStoreTests
{
    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-PRJ-001")]
    public async Task Save_then_load_preserves_import_traceability()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.lnproj");
        try
        {
            var project = ProjectDocument.CreateNew("Persist").CommitImport(ImportReport.Create("source.dxf", "hash", UnitDecision.ConfirmedMillimetres, []));
            var store = new ZipProjectStore();

            await store.SaveAsync(path, project, CancellationToken.None);
            var loaded = await store.LoadAsync(path, CancellationToken.None);

            Assert.Equal(project.Id, loaded.Id);
            Assert.Equal("hash", loaded.Imports.Single().SourceSha256);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-PRJ-002")]
    public async Task Save_to_missing_directory_leaves_no_temporary_project_file()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"), "project.lnproj");
        var store = new ZipProjectStore();

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => store.SaveAsync(path, ProjectDocument.CreateNew("Persist"), CancellationToken.None));

        Assert.False(File.Exists(path));
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-PRJ-002")]
    public async Task Saving_an_existing_project_keeps_the_previous_complete_version_as_recovery_copy()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.lnproj");
        var recoveryPath = path + ".bak";
        var store = new ZipProjectStore();
        var original = ProjectDocument.CreateNew("First");
        var replacement = original.CommitImport(ImportReport.Create("source.dxf", "hash", UnitDecision.ConfirmedMillimetres, []));
        try
        {
            await store.SaveAsync(path, original, CancellationToken.None);
            await store.SaveAsync(path, replacement, CancellationToken.None);

            var recovered = await store.LoadAsync(recoveryPath, CancellationToken.None);

            Assert.Equal(original.Id, recovered.Id);
            Assert.Equal(0, recovered.Revision);
            Assert.Empty(recovered.Imports);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(recoveryPath)) File.Delete(recoveryPath);
        }
    }
}
