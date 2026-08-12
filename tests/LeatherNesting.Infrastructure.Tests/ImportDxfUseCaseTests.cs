using LeatherNesting.Application;
using LeatherNesting.Domain;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class ImportDxfUseCaseTests
{
    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-007")]
    public async Task Import_stays_uncommitted_until_millimetres_are_confirmed()
    {
        var sourcePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(sourcePath, "stage-1-dxf-source");
        try
        {
            var result = new DxfImportResult([], [], [new ImportDiagnostic("DXF-UNIT-REVIEW", "Blocking", "确认单位")], UnitDecision.Unresolved, DxfDeclaredUnit.Millimetres, 4);
            var preparation = await new ImportDxfUseCase(new StubReader(result)).InspectAsync(sourcePath, CancellationToken.None);
            var beforeCommit = ProjectDocument.CreateNew("凉鞋");

            Assert.Empty(beforeCommit.Imports);
            Assert.Equal(UnitDecision.Unresolved, preparation.Result.UnitDecision);

            var committed = preparation.CommitTo(beforeCommit, UnitDecision.ConfirmedMillimetres);

            Assert.True(committed.IsDirty);
            Assert.Equal(1, committed.Revision);
            Assert.Single(committed.Imports);
            Assert.Equal(UnitDecision.ConfirmedMillimetres, committed.Imports[0].UnitDecision);
            Assert.NotEmpty(committed.Imports[0].SourceSha256);
        }
        finally { File.Delete(sourcePath); }
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-004")]
    public async Task Missing_source_after_inspection_returns_a_blocking_diagnostic_without_throwing()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.dxf");
        var result = new DxfImportResult([], [], [], UnitDecision.Unresolved, DxfDeclaredUnit.Unknown, null);

        var preparation = await new ImportDxfUseCase(new StubReader(result)).InspectAsync(sourcePath, CancellationToken.None);

        Assert.Null(preparation.SourceSha256);
        Assert.Contains(preparation.Result.Diagnostics, item => item.Code == "DXF-SOURCE-FINGERPRINT-FAILED" && item.Severity == "Blocking");
        Assert.Throws<InvalidOperationException>(() => preparation.CommitTo(ProjectDocument.CreateNew("凉鞋"), UnitDecision.ConfirmedMillimetres));
    }

    private sealed class StubReader(DxfImportResult result) : IDxfReader
    {
        public Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
