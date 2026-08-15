using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class PieceTests
{
    private static Loop2D Rect(string id, LoopRole role, double x0, double y0, double x1, double y1) =>
        new(id, role, [new Polyline2D([new(x0, y0), new(x1, y0), new(x1, y1), new(x0, y1), new(x0, y0)])]);

    private static LineSegment2D Seg(double x0, double y0, double x1, double y1) =>
        new(new(x0, y0), new(x1, y1));

    [Fact]
    public void Piece_rejects_null_outer()
    {
        Assert.Throws<ArgumentNullException>(() => new PieceGeometry(null!, [], []));
    }

    [Fact]
    public void Piece_rejects_hole_as_outer()
    {
        var hole = Rect("h", LoopRole.Hole, 0, 0, 5, 5);
        Assert.Throws<ArgumentException>(() => new PieceGeometry(hole, [], []));
    }

    [Fact]
    public void Piece_holds_outer_holes_and_lines()
    {
        var outer = Rect("o", LoopRole.Outer, 0, 0, 100, 50);
        var hole = Rect("h", LoopRole.Hole, 10, 10, 20, 20);
        var cut = new InternalLine("c", LineRole.Cut, [Seg(30, 10, 40, 10)]);
        var mark = new InternalLine("m", LineRole.Mark, [Seg(50, 10, 50, 12)]);

        var piece = new PieceGeometry(outer, [hole], [cut, mark]);

        Assert.Equal(outer, piece.Outer);
        Assert.Single(piece.Holes);
        Assert.Equal(2, piece.Lines.Count);
    }

    [Fact]
    public void InternalLine_rejects_outline_role()
    {
        Assert.Throws<ArgumentException>(() => new InternalLine("l", LineRole.Outline, [Seg(0, 0, 1, 0)]));
    }
}
