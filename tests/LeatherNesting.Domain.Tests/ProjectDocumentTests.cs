using LeatherNesting.Domain;
using Xunit;

namespace LeatherNesting.Domain.Tests;

public sealed class ProjectDocumentTests
{
    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-PRJ-003")]
    public void Create_new_project_sets_schema_and_clean_revision()
    {
        var project = ProjectDocument.CreateNew("Stage one");

        Assert.Equal(ProjectDocument.CurrentSchemaVersion, project.SchemaVersion);
        Assert.Equal(0, project.Revision);
        Assert.False(project.IsDirty);
        Assert.NotEqual(Guid.Empty, project.Id);
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-003")]
    public void Commit_import_records_unit_decision_and_marks_project_dirty()
    {
        var project = ProjectDocument.CreateNew("Stage one");
        var report = ImportReport.Create("fixture.dxf", "abc", UnitDecision.ConfirmedMillimetres, []);

        var updated = project.CommitImport(report);

        Assert.True(updated.IsDirty);
        Assert.Equal(1, updated.Revision);
        Assert.Equal(UnitDecision.ConfirmedMillimetres, updated.Imports.Single().UnitDecision);
    }
}
