namespace LeatherNesting.Geometry;

/// <summary>Line role for nesting output, mapped to DXF color 62: Outline→0, Cut→3, Mark→5.</summary>
public enum LineRole
{
    Outline,
    Cut,
    Mark,
}

/// <summary>An internal line inside a piece: an open cut line (Cut) or a closed notch mark (Mark).</summary>
public sealed record InternalLine
{
    public string Id { get; }
    public LineRole Role { get; }
    public IReadOnlyList<Curve2D> Curves { get; }

    public InternalLine(string id, LineRole role, IReadOnlyList<Curve2D> curves)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("内部线必须有稳定标识符。", nameof(id));
        if (role == LineRole.Outline)
            throw new ArgumentException("内部线角色不能是 Outline（外轮廓由 PieceGeometry.Outer 表达）。", nameof(role));
        ArgumentNullException.ThrowIfNull(curves);
        if (curves.Count == 0)
            throw new ArgumentException("内部线至少需要一条曲线。", nameof(curves));
        Id = id;
        Role = role;
        Curves = curves;
    }
}
