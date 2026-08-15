using System.Globalization;
using LeatherNesting.Geometry;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>Reads closed LWPOLYLINE and ARC entities back into Loop2D geometry,
/// preserving bulge arcs as CircularArc2D (Stage 2 round-trip).</summary>
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
            if (groups[i].Code != "0") continue;
            var type = groups[i].Value.ToUpperInvariant();

            if (type == "LWPOLYLINE")
            {
                var loop = ReadLwPolyline(groups, i + 1, loops.Count);
                if (loop is not null) loops.Add(loop);
            }
            else if (type == "ARC")
            {
                var loop = ReadArc(groups, i + 1, loops.Count);
                if (loop is not null) loops.Add(loop);
            }
        }

        return loops;
    }

    private static Loop2D? ReadLwPolyline(IReadOnlyList<Group> groups, int start, int index)
    {
        var end = FindEntityEnd(groups, start);
        var fields = groups.Skip(start).Take(end - start).ToList();
        var flags = ParseInt(fields.FirstOrDefault(g => g.Code == "70")?.Value);
        if ((flags & 1) != 1) return null; // only closed polylines form piece outlines

        var vertices = new List<(Point2D Point, double Bulge)>();
        for (var j = 0; j + 1 < fields.Count; j++)
        {
            if (fields[j].Code != "10" || fields[j + 1].Code != "20") continue;
            var x = ParseDouble(fields[j].Value);
            var y = ParseDouble(fields[j + 1].Value);
            var bulge = 0.0;
            if (j + 2 < fields.Count && fields[j + 2].Code == "42")
                bulge = ParseDouble(fields[j + 2].Value);
            vertices.Add((new Point2D(x, y), bulge));
            j++; // consume the 20
        }

        if (vertices.Count < 3) return null;

        var curves = new List<Curve2D>(vertices.Count);
        for (var i = 0; i < vertices.Count; i++)
        {
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];
            curves.Add(current.Bulge == 0
                ? new LineSegment2D(current.Point, next.Point)
                : BulgeToArc(current.Point, next.Point, current.Bulge));
        }

        return new Loop2D($"loop-{index + 1}", LoopRole.Outer, curves);
    }

    private static Loop2D? ReadArc(IReadOnlyList<Group> groups, int start, int index)
    {
        var end = FindEntityEnd(groups, start);
        var fields = groups.Skip(start).Take(end - start).ToList();

        var centre = ReadPoint(fields);
        var radius = ParseDouble(fields.FirstOrDefault(g => g.Code == "40")?.Value ?? "0");
        var startAngle = ParseDouble(fields.FirstOrDefault(g => g.Code == "50")?.Value ?? "0");
        var endAngle = ParseDouble(fields.FirstOrDefault(g => g.Code == "51")?.Value ?? "0");

        if (radius <= 0) return null;
        var arc = new CircularArc2D(centre, radius, startAngle, endAngle - startAngle);
        return new Loop2D($"loop-{index + 1}", LoopRole.Outer, [arc]);
    }

    private static Point2D ReadPoint(IReadOnlyList<Group> fields)
    {
        for (var i = 0; i + 1 < fields.Count; i++)
        {
            if (fields[i].Code == "10" && fields[i + 1].Code == "20")
                return new Point2D(ParseDouble(fields[i].Value), ParseDouble(fields[i + 1].Value));
        }
        return Point2D.Origin;
    }

    private static CircularArc2D BulgeToArc(Point2D p1, Point2D p2, double bulge)
    {
        var chord = p1.DistanceTo(p2);
        var radius = chord * (1 + bulge * bulge) / (4 * Math.Abs(bulge));
        var mid = new Point2D((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        var dx = (p2.X - p1.X) / chord;
        var dy = (p2.Y - p1.Y) / chord;
        // Perpendicular unit vector (CCW from chord direction).
        var centre = new Point2D(
            mid.X - dy * chord * (1 - bulge * bulge) / (4 * bulge),
            mid.Y + dx * chord * (1 - bulge * bulge) / (4 * bulge));
        var startAngle = Math.Atan2(p1.Y - centre.Y, p1.X - centre.X) * 180 / Math.PI;
        var sweep = 4 * Math.Atan(bulge) * 180 / Math.PI;
        return new CircularArc2D(centre, radius, startAngle, sweep);
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
