using System.Globalization;
using System.Text;
using LeatherNesting.Geometry;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>Minimal ASCII DXF writer for closed contours (LWPOLYLINE). Stage 2 round-trip only.</summary>
public sealed class AsciiDxfWriter : IDxfWriter
{
    public async Task WriteAsync(string path, IReadOnlyList<Loop2D> loops, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("0");
        sb.AppendLine("SECTION");
        sb.AppendLine("2");
        sb.AppendLine("ENTITIES");
        foreach (var loop in loops)
            WriteLoop(sb, loop);
        sb.AppendLine("0");
        sb.AppendLine("ENDSEC");
        sb.AppendLine("0");
        sb.AppendLine("EOF");
        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }

    private static void WriteLoop(StringBuilder sb, Loop2D loop)
    {
        var points = Flatten(loop);
        if (points.Count < 3) return;
        sb.AppendLine("0");
        sb.AppendLine("LWPOLYLINE");
        sb.AppendLine("8");
        sb.AppendLine("0");
        sb.AppendLine("70");
        sb.AppendLine("1"); // closed
        sb.AppendLine("90");
        sb.AppendLine(points.Count.ToString(CultureInfo.InvariantCulture));
        foreach (var p in points)
        {
            sb.AppendLine("10");
            sb.AppendLine(p.X.ToString("R", CultureInfo.InvariantCulture));
            sb.AppendLine("20");
            sb.AppendLine(p.Y.ToString("R", CultureInfo.InvariantCulture));
        }
    }

    private static List<Point2D> Flatten(Loop2D loop)
    {
        var points = loop.Curves.SelectMany(c => c switch
        {
            LineSegment2D l => new[] { l.Start, l.End },
            Polyline2D p => p.Points,
            CircularArc2D a => FlattenArc(a),
            _ => Array.Empty<Point2D>()
        }).ToList();

        var dedup = new List<Point2D>();
        foreach (var p in points)
            if (dedup.Count == 0 || dedup[^1].DistanceTo(p) > 1e-9)
                dedup.Add(p);
        if (dedup.Count > 1 && dedup[^1].DistanceTo(dedup[0]) <= 1e-9)
            dedup.RemoveAt(dedup.Count - 1);
        return dedup;
    }

    private static IReadOnlyList<Point2D> FlattenArc(CircularArc2D arc)
    {
        var chordLen = arc.StartPoint.DistanceTo(arc.EndPoint);
        var n = (int)Math.Max(2, Math.Ceiling(chordLen / 0.01));
        var points = new List<Point2D>(n + 1);
        for (var i = 0; i <= n; i++)
            points.Add(arc.PointAt((double)i / n));
        return points;
    }
}
