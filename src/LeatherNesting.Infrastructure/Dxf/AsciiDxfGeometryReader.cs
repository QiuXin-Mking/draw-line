using System.Globalization;
using LeatherNesting.Geometry;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>Reads DXF entities back into geometry: closed LWPOLYLINE/ARC → Loop2D (round-trip),
/// or color-coded closed/open polylines → PieceGeometry (nesting input).</summary>
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

    /// <summary>Reads a DXF into pieces: color 62 classifies line roles (0 outline / 3 cut / 5 mark),
    /// closed polylines become outer/hole loops, open polylines become internal cut/mark lines.</summary>
    public async Task<IReadOnlyList<PieceGeometry>> ReadPiecesAsync(string path, CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        if (lines.Length % 2 != 0) return [];

        var groups = Enumerable.Range(0, lines.Length / 2)
            .Select(i => new Group(lines[i * 2].Trim(), lines[i * 2 + 1].Trim()))
            .ToList();

        var outers = new List<Loop2D>();
        var holes = new List<Loop2D>();
        var cutLines = new List<InternalLine>();
        var markLines = new List<InternalLine>();
        var index = 0;

        for (var i = 0; i < groups.Count; i++)
        {
            if (groups[i].Code != "0" || !groups[i].Value.Equals("LWPOLYLINE", StringComparison.OrdinalIgnoreCase))
                continue;

            var raw = ReadPolylineRaw(groups, i + 1);
            if (raw is null) continue;
            var (curves, closed, color) = raw.Value;

            if (closed)
            {
                var loop = new Loop2D($"loop-{index}", LoopRole.Outer, curves);
                if (color == 3)
                    holes.Add(new Loop2D($"loop-{index}", LoopRole.Hole, curves));
                else if (color == 5)
                    markLines.Add(new InternalLine($"line-{index}", LineRole.Mark, curves));
                else
                    outers.Add(loop);
            }
            else
            {
                var role = color == 5 ? LineRole.Mark : LineRole.Cut;
                var line = new InternalLine($"line-{index}", role, curves);
                if (role == LineRole.Mark) markLines.Add(line); else cutLines.Add(line);
            }

            index++;
        }

        return GroupIntoPieces(outers, holes, cutLines, markLines);
    }

    private static IReadOnlyList<PieceGeometry> GroupIntoPieces(
        IReadOnlyList<Loop2D> outers,
        IReadOnlyList<Loop2D> holes,
        IReadOnlyList<InternalLine> cutLines,
        IReadOnlyList<InternalLine> markLines)
    {
        var holesByOuter = outers.ToDictionary(o => o.StableId, _ => new List<Loop2D>());
        var linesByOuter = outers.ToDictionary(o => o.StableId, _ => new List<InternalLine>());

        foreach (var hole in holes)
        {
            var outer = FindContaining(outers, Centroid(hole));
            if (outer is not null) holesByOuter[outer.StableId].Add(hole);
        }

        foreach (var line in cutLines.Concat(markLines))
        {
            var outer = FindContaining(outers, Centroid(line.Curves));
            if (outer is not null) linesByOuter[outer.StableId].Add(line);
        }

        return outers
            .Select(o => new PieceGeometry(o, holesByOuter[o.StableId], linesByOuter[o.StableId]))
            .ToList();
    }

    private static Loop2D? FindContaining(IReadOnlyList<Loop2D> outers, Point2D point) =>
        outers.Where(o => o.ContainsPoint(point)).OrderBy(o => o.Area).FirstOrDefault();

    private static Point2D Centroid(Loop2D loop) => Centroid(loop.Curves);

    private static Point2D Centroid(IReadOnlyList<Curve2D> curves)
    {
        var points = curves.SelectMany(c => c switch
        {
            LineSegment2D l => new[] { l.Start, l.End },
            Polyline2D p => p.Points,
            CircularArc2D a => new[] { a.StartPoint, a.EndPoint },
            _ => Array.Empty<Point2D>()
        }).ToList();
        if (points.Count == 0) return Point2D.Origin;
        return new Point2D(points.Average(p => p.X), points.Average(p => p.Y));
    }

    private static Loop2D? ReadLwPolyline(IReadOnlyList<Group> groups, int start, int index)
    {
        var raw = ReadPolylineRaw(groups, start);
        if (raw is null || !raw.Value.Closed || raw.Value.Curves.Count < 3)
            return null;
        return new Loop2D($"loop-{index + 1}", LoopRole.Outer, raw.Value.Curves.ToList());
    }

    private static (IReadOnlyList<Curve2D> Curves, bool Closed, int Color)? ReadPolylineRaw(IReadOnlyList<Group> groups, int start)
    {
        var end = FindEntityEnd(groups, start);
        var fields = groups.Skip(start).Take(end - start).ToList();
        var flags = ParseInt(fields.FirstOrDefault(g => g.Code == "70")?.Value);
        var color = ParseInt(fields.FirstOrDefault(g => g.Code == "62")?.Value);
        var closed = (flags & 1) == 1;

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
            j++;
        }

        if (vertices.Count < 2) return null;

        var curveCount = closed ? vertices.Count : vertices.Count - 1;
        var curves = new List<Curve2D>(curveCount);
        for (var i = 0; i < curveCount; i++)
        {
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];
            curves.Add(current.Bulge == 0
                ? new LineSegment2D(current.Point, next.Point)
                : BulgeToArc(current.Point, next.Point, current.Bulge));
        }

        return (curves, closed, color);
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
