using System.Globalization;
using LeatherNesting.Geometry;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>Reads closed LWPOLYLINE entities back into Loop2D geometry (Stage 2 round-trip).</summary>
public sealed class AsciiDxfGeometryReader
{
    public async Task<IReadOnlyList<Loop2D>> ReadAsync(string path, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        if (lines.Length % 2 != 0) return [];

        var groups = Enumerable.Range(0, lines.Length / 2)
            .Select(i => new Group(lines[i * 2].Trim(), lines[i * 2 + 1].Trim()))
            .ToList();

        var loops = new List<Loop2D>();
        for (var i = 0; i < groups.Count; i++)
        {
            if (groups[i].Code != "0" || !groups[i].Value.Equals("LWPOLYLINE", StringComparison.OrdinalIgnoreCase)) continue;

            var end = FindEntityEnd(groups, i + 1);
            var fields = groups.Skip(i + 1).Take(end - i - 1).ToList();
            var flags = ParseInt(fields.FirstOrDefault(g => g.Code == "70")?.Value);
            var points = new List<Point2D>();
            for (var j = 0; j + 1 < fields.Count; j++)
            {
                if (fields[j].Code == "10" && fields[j + 1].Code == "20")
                {
                    points.Add(new Point2D(ParseDouble(fields[j].Value), ParseDouble(fields[j + 1].Value)));
                    j++;
                }
            }

            if ((flags & 1) == 1 && points.Count >= 3)
                loops.Add(new Loop2D($"loop-{loops.Count + 1}", LoopRole.Outer, [new Polyline2D(points)]));

            i = end - 1;
        }

        return loops;
    }

    private static int FindEntityEnd(IReadOnlyList<Group> groups, int start)
    {
        for (var index = start; index < groups.Count; index++)
            if (groups[index].Code == "0") return index;
        return groups.Count;
    }

    private static int ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static double ParseDouble(string value) =>
        double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    private sealed record Group(string Code, string Value);
}
