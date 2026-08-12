using LeatherNesting.Domain;
using LeatherNesting.Application;
using LeatherNesting.Infrastructure.Dxf;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class AsciiDxfReaderTests
{
    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-001")]
    public async Task Sandal_fixture_has_nine_closed_polyline_candidates()
    {
        var result = await new AsciiDxfReader().ReadAsync(RepoFixture.Path("凉鞋.dxf"), CancellationToken.None);

        Assert.Equal(9, result.ClosedPieceCandidates.Count);
        Assert.Contains(result.Entities, entity => entity.Kind == DxfEntityKind.Text);
    }

    [Theory]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-002")]
    [InlineData("38.DXF")]
    [InlineData("39.DXF")]
    [InlineData("40.DXF")]
    [InlineData("41.DXF")]
    [InlineData("42.DXF")]
    [InlineData("43.DXF")]
    [InlineData("44.DXF")]
    [InlineData("45.DXF")]
    public async Task Legacy_fixture_reports_open_polyline_without_silent_closure(string fileName)
    {
        var result = await new AsciiDxfReader().ReadAsync(RepoFixture.Path("03-测试数据/划线", fileName), CancellationToken.None);

        Assert.Empty(result.ClosedPieceCandidates);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "DXF-OPEN-POLYLINE");
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-005")]
    public async Task Legacy_fixture_keeps_vertex_counts_for_each_open_polyline()
    {
        var result = await new AsciiDxfReader().ReadAsync(RepoFixture.Path("03-测试数据/划线", "38.DXF"), CancellationToken.None);

        var polylines = result.Entities.Where(entity => entity.Kind == DxfEntityKind.Polyline).ToList();

        Assert.Equal(81, polylines.Count);
        Assert.All(polylines, entity => Assert.True(entity.VertexCount >= 2));
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-006")]
    public async Task Header_unit_is_recorded_but_still_requires_business_confirmation()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "0\nSECTION\n2\nHEADER\n9\n$INSUNITS\n70\n4\n0\nENDSEC\n0\nSECTION\n2\nENTITIES\n0\nENDSEC\n0\nEOF\n");
        try
        {
            var result = await new AsciiDxfReader().ReadAsync(path, CancellationToken.None);

            Assert.Equal(DxfDeclaredUnit.Millimetres, result.DeclaredUnit);
            Assert.Equal(UnitDecision.Unresolved, result.UnitDecision);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-004")]
    public async Task Invalid_file_returns_a_diagnostic_instead_of_throwing()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "0\nNOT_A_SECTION\n");
        try
        {
            var result = await new AsciiDxfReader().ReadAsync(path, CancellationToken.None);
            Assert.Contains(result.Diagnostics, item => item.Code == "DXF-INVALID-HEADER");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-001")]
    public async Task Closed_legacy_polyline_is_a_piece_candidate()
    {
        var path = await WriteFixtureAsync("""
            0
            SECTION
            2
            ENTITIES
            0
            POLYLINE
            8
            CUT
            70
            1
            0
            VERTEX
            10
            0
            0
            VERTEX
            10
            10
            0
            VERTEX
            10
            10
            0
            SEQEND
            0
            ENDSEC
            0
            EOF
            """);
        try
        {
            var result = await new AsciiDxfReader().ReadAsync(path, CancellationToken.None);

            var candidate = Assert.Single(result.ClosedPieceCandidates);
            Assert.Equal(DxfEntityKind.Polyline, candidate.Kind);
            Assert.Equal(3, candidate.VertexCount);
            Assert.Equal("CUT", candidate.Layer);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    [Trait("Stage", "1")]
    [Trait("TestId", "P1-DXF-004")]
    public async Task Unterminated_legacy_polyline_is_reported_without_becoming_a_candidate()
    {
        var path = await WriteFixtureAsync("""
            0
            SECTION
            2
            ENTITIES
            0
            POLYLINE
            70
            1
            0
            VERTEX
            10
            0
            0
            ENDSEC
            0
            EOF
            """);
        try
        {
            var result = await new AsciiDxfReader().ReadAsync(path, CancellationToken.None);

            Assert.Empty(result.ClosedPieceCandidates);
            Assert.Contains(result.Diagnostics, item => item.Code == "DXF-POLYLINE-MISSING-SEQEND");
        }
        finally { File.Delete(path); }
    }

    private static async Task<string> WriteFixtureAsync(string contents)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, contents);
        return path;
    }
}

internal static class RepoFixture
{
    public static string Path(params string[] parts) => System.IO.Path.GetFullPath(System.IO.Path.Combine(["..", "..", "..", "..", "..", .. parts]));
}
