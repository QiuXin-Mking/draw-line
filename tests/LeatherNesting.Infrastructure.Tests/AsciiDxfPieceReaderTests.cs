using System.Globalization;
using System.Text;
using LeatherNesting.Geometry;
using LeatherNesting.Infrastructure.Dxf;
using Xunit;

namespace LeatherNesting.Infrastructure.Tests;

public sealed class AsciiDxfPieceReaderTests
{
    private static string Polyline(int color, bool closed, params double[] xy)
    {
        var sb = new StringBuilder();
        sb.AppendLine("0").AppendLine("LWPOLYLINE");
        sb.AppendLine("8").AppendLine("0");
        sb.AppendLine("62").AppendLine(color.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine("70").AppendLine(closed ? "1" : "0");
        sb.AppendLine("90").AppendLine((xy.Length / 2).ToString(CultureInfo.InvariantCulture));
        for (var i = 0; i < xy.Length; i += 2)
        {
            sb.AppendLine("10").AppendLine(xy[i].ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("20").AppendLine(xy[i + 1].ToString(CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }

    private static async Task<string> WriteDxfAsync(string body)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "0\nSECTION\n2\nENTITIES\n" + body + "0\nENDSEC\n0\nEOF\n");
        return path;
    }

    [Fact]
    [Trait("Stage", "4")]
    [Trait("TestId", "P4-DXFIN-001")]
    public async Task ReadPiecesAsync_classifies_roles_by_color()
    {
        var body =
            Polyline(0, true, 0, 0, 100, 0, 100, 50, 0, 50) +    // 外轮廓
            Polyline(3, true, 10, 10, 20, 10, 20, 20, 10, 20) +  // 内孔
            Polyline(3, false, 30, 10, 40, 10) +                 // 开放切割线
            Polyline(5, true, 50, 10, 52, 10, 52, 12, 50, 12);   // 标记线

        var path = await WriteDxfAsync(body);
        try
        {
            var pieces = await new AsciiDxfGeometryReader().ReadPiecesAsync(path, CancellationToken.None);

            var piece = Assert.Single(pieces);
            Assert.Single(piece.Holes);
            Assert.Equal(2, piece.Lines.Count);
            Assert.Contains(piece.Lines, l => l.Role == LineRole.Cut);
            Assert.Contains(piece.Lines, l => l.Role == LineRole.Mark);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
