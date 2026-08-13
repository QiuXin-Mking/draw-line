using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Modules.CadCanvas;

public enum CadObjectCategory
{
    OuterContour,
    Hole,
    InternalLine,
}

public sealed record DemoObject(string Id, string Name, CadObjectCategory Category, string Layer, Loop2D Loop);

/// <summary>Small, deterministic geometry set owned by M03; it never represents persisted project data.</summary>
public static class DemoGeometryFactory
{
    public static IReadOnlyList<DemoObject> Create() =>
    [
        new("PIECE-A-OUTER", "鞋面 A / 外轮廓", CadObjectCategory.OuterContour, "CUT-OUTER", Rectangle("PIECE-A-OUTER", LoopRole.Outer, 0, 0, 112, 62)),
        new("PIECE-A-HOLE", "鞋面 A / 定位孔", CadObjectCategory.Hole, "CUT-HOLE", Circle("PIECE-A-HOLE", new Point2D(26, 31), 5)),
        new("PIECE-A-INNER", "鞋面 A / 车缝线", CadObjectCategory.InternalLine, "STITCH", InternalPath("PIECE-A-INNER", 17, 15, 91, 47)),
        new("PIECE-B-OUTER", "后跟片 B / 外轮廓", CadObjectCategory.OuterContour, "CUT-OUTER", Rectangle("PIECE-B-OUTER", LoopRole.Outer, 132, 7, 205, 55)),
        new("PIECE-B-HOLE", "后跟片 B / 工艺孔", CadObjectCategory.Hole, "CUT-HOLE", Circle("PIECE-B-HOLE", new Point2D(169, 31), 4)),
        new("PIECE-B-INNER", "后跟片 B / 方向线", CadObjectCategory.InternalLine, "MARK", InternalPath("PIECE-B-INNER", 146, 31, 192, 31)),
    ];

    private static Loop2D Rectangle(string id, LoopRole role, double left, double bottom, double right, double top) =>
        new(id, role,
        [
            new LineSegment2D(new Point2D(left, bottom), new Point2D(right, bottom)),
            new LineSegment2D(new Point2D(right, bottom), new Point2D(right, top)),
            new LineSegment2D(new Point2D(right, top), new Point2D(left, top)),
            new LineSegment2D(new Point2D(left, top), new Point2D(left, bottom)),
        ]);

    private static Loop2D Circle(string id, Point2D centre, double radius) =>
        new(id, LoopRole.Hole,
        [
            new CircularArc2D(centre, radius, 0, 180),
            new CircularArc2D(centre, radius, 180, 180),
        ]);

    private static Loop2D InternalPath(string id, double x1, double y1, double x2, double y2) =>
        new(id, LoopRole.Hole,
        [
            new LineSegment2D(new Point2D(x1, y1), new Point2D(x2, y2)),
            new LineSegment2D(new Point2D(x2, y2), new Point2D(x1, y1)),
        ]);
}
