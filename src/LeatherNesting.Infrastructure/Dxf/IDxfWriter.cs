using LeatherNesting.Geometry;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>Writes closed contours to ASCII DXF (Stage 2 round-trip).</summary>
public interface IDxfWriter
{
    Task WriteAsync(string path, IReadOnlyList<Loop2D> loops, CancellationToken cancellationToken);
}
