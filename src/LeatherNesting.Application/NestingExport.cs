using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Nesting;

namespace LeatherNesting.Application;

/// <summary>A placed piece in a nesting DXF export.</summary>
public sealed record NestingDxfPiece(string PieceId, double RotationDegrees, Loop2D PlacedLoop);

/// <summary>Structured output model for a nesting DXF export.</summary>
public sealed record NestingDxfDocument(Loop2D Material, IReadOnlyList<NestingDxfPiece> Pieces, string Title);

/// <summary>Writes a nesting result as DXF.</summary>
public interface INestingDxfWriter
{
    Task WriteAsync(string path, NestingDxfDocument document, CancellationToken cancellationToken);
}

/// <summary>Application boundary: assembles a <see cref="NestResult"/> into a DXF document and writes it.</summary>
public sealed class ExportNestingDxfUseCase(INestingDxfWriter writer)
{
    public async Task ExportAsync(
        string path,
        NestResult result,
        Loop2D material,
        double gapMm,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var pieces = result.Placements
            .Select(p => new NestingDxfPiece(p.PieceId, p.Transform.RotationDegrees, p.PlacedLoop))
            .ToList();

        var document = new NestingDxfDocument(material, pieces, BuildTitle(material, gapMm, result));
        await writer.WriteAsync(path, document, cancellationToken);
    }

    private static string BuildTitle(Loop2D material, double gapMm, NestResult result)
    {
        var (minX, minY, maxX, maxY) = BoundsOf(material);
        return $"Leather {maxX - minX:g} x {maxY - minY:g} mm | gap {gapMm:g} mm | " +
               $"placed {result.Placements.Count} | utilization {result.Utilization * 100:F2}%";
    }

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
