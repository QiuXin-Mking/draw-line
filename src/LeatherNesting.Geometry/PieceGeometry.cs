namespace LeatherNesting.Geometry;

/// <summary>Pure geometry of a nesting piece: outer contour plus inner holes and internal lines, placed as one unit.
/// Identity is the outer loop's <see cref="Loop2D.StableId"/>.</summary>
public sealed record PieceGeometry
{
    public Loop2D Outer { get; }
    public IReadOnlyList<Loop2D> Holes { get; }
    public IReadOnlyList<InternalLine> Lines { get; }

    public PieceGeometry(Loop2D outer, IReadOnlyList<Loop2D> holes, IReadOnlyList<InternalLine> lines)
    {
        ArgumentNullException.ThrowIfNull(outer);
        if (outer.Role != LoopRole.Outer)
            throw new ArgumentException("部件外轮廓必须是 Outer 角色。", nameof(outer));
        Outer = outer;
        Holes = holes ?? Array.Empty<Loop2D>();
        Lines = lines ?? Array.Empty<InternalLine>();
    }
}
