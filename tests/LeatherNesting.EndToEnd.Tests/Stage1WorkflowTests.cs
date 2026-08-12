using LeatherNesting.Application;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Domain;
using LeatherNesting.Infrastructure.Dxf;
using LeatherNesting.Infrastructure.Projects;
using Xunit;

namespace LeatherNesting.EndToEnd.Tests;

public sealed class Stage1WorkflowTests
{
    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-E2E-001")]
    public async Task Real_dxf_can_be_checked_confirmed_saved_and_reopened()
    {
        var projectPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.lnproj");
        try
        {
            var workflow = new ProjectWorkflowViewModel(new ImportDxfUseCase(new AsciiDxfReader()));
            workflow.CreateProject("凉鞋");

            await workflow.InspectAsync(RepoFixture.Path("凉鞋.dxf"), CancellationToken.None);
            workflow.ConfirmMillimetres();
            await new ZipProjectStore().SaveAsync(projectPath, workflow.Project!, CancellationToken.None);

            var reopened = await new ZipProjectStore().LoadAsync(projectPath, CancellationToken.None);
            Assert.Equal("凉鞋", reopened.Name);
            Assert.Single(reopened.Imports);
            Assert.Equal(UnitDecision.ConfirmedMillimetres, reopened.Imports[0].UnitDecision);
            Assert.NotEmpty(reopened.Imports[0].SourceSha256);
        }
        finally
        {
            if (File.Exists(projectPath)) File.Delete(projectPath);
            if (File.Exists(projectPath + ".bak")) File.Delete(projectPath + ".bak");
        }
    }

    private static class RepoFixture
    {
        public static string Path(params string[] parts) => System.IO.Path.GetFullPath(System.IO.Path.Combine(["..", "..", "..", "..", "..", .. parts]));
    }
}
