using System.Globalization;
using System.Text;
using LeatherNesting.Application;
using LeatherNesting.Geometry;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>ASCII DXF writer for nesting results: LEATHER / PIECES / ANNOTATION layers
/// with closed polylines and TEXT annotations, matching the reference demo output.</summary>
public sealed class AsciiNestingDxfWriter : INestingDxfWriter
{
    private const string LeatherLayer = "LEATHER";
    private const string PiecesLayer = "PIECES";
    private const string AnnotationLayer = "ANNOTATION";

    public async Task WriteAsync(string path, NestingDxfDocument document, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("0");
        sb.AppendLine("SECTION");
        sb.AppendLine("2");
        sb.AppendLine("ENTITIES");

        WritePolyline(sb, document.Material, LeatherLayer);

        var labelHeight = Math.Max(8.0, Math.Min(MaterialWidth(document.Material), MaterialHeight(document.Material)) / 80.0);

        foreach (var piece in document.Pieces)
        {
            WritePolyline(sb, piece.PlacedLoop, PiecesLayer);
            var (cx, cy) = Centroid(piece.PlacedLoop);
            WriteText(sb, $"{piece.PieceId} {piece.RotationDegrees:g}°", cx, cy, labelHeight, AnnotationLayer);
        }

        var (_, _, _, maxY) = BoundsOf(document.Material);
        WriteText(sb, document.Title, 0, maxY + labelHeight * 1.5, labelHeight, AnnotationLayer);

        sb.AppendLine("0");
        sb.AppendLine("ENDSEC");
        sb.AppendLine("0");
        sb.AppendLine("EOF");
        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }

    private static void WritePolyline(StringBuilder sb, Loop2D loop, string layer)
    {
        var points = Flatten(loop);
        if (points.Count < 3)
            return;
        sb.AppendLine("0");
        sb.AppendLine("LWPOLYLINE");
        sb.AppendLine("8");
        sb.AppendLine(layer);
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

    private static void WriteText(StringBuilder sb, string text, double x, double y, double height, string layer)
    {
        sb.AppendLine("0");
        sb.AppendLine("TEXT");
        sb.AppendLine("8");
        sb.AppendLine(layer);
        sb.AppendLine("10");
        sb.AppendLine(x.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("20");
        sb.AppendLine(y.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("40");
        sb.AppendLine(height.ToString("R", CultureInfo.InvariantCulture));
        sb.AppendLine("1");
        sb.AppendLine(text);
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

    private static (double X, double Y) Centroid(Loop2D loop)
    {
        var points = Flatten(loop);
        if (points.Count == 0)
            return (0, 0);
        return (points.Average(p => p.X), points.Average(p => p.Y));
    }

    private static double MaterialWidth(Loop2D material) => BoundsOf(material) is var (minX, _, maxX, _) ? maxX - minX : 0;

    private static double MaterialHeight(Loop2D material) => BoundsOf(material) is var (_, minY, _, maxY) ? maxY - minY : 0;

    private static (double MinX, double MinY, double MaxX, double MaxY) BoundsOf(Loop2D loop)
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        foreach (var curve in loop.Curves)
        {
            var (cMinX, cMinY, cMaxX, cMaxY) = curve.Bounds;
            if (cMinX < minX) minX = cMinX;
            if (cMinY < minY) minY = cMinY;
            if (cMaxX > maxX) maxX = cMaxX;
            if (cMaxY > maxY) maxY = cMaxY;
        }
        return (minX, minY, maxX, maxY);
    }
}
