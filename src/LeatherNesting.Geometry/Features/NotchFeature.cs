namespace LeatherNesting.Geometry.Features;

/// <summary>Shape of a notch cutout.</summary>
public enum NotchShape { V, Square, U, HalfCircle, Mark }

/// <summary>Which side of the material the notch is cut on.</summary>
public enum MaterialSide { Inside, Outside }

/// <summary>Output mode: Cut (physical cut) or Mark (tool mark only).</summary>
public enum NotchOutputMode { Cut, Mark }

/// <summary>A notch feature anchored to a contour at a specific arc-length position.</summary>
public sealed record NotchFeature
{
    public string ContourId { get; }
    public double AnchorArcLength { get; }
    public NotchShape Shape { get; }
    public double Width { get; }
    public double Depth { get; }
    public MaterialSide MaterialSide { get; }
    public NotchOutputMode OutputMode { get; }
    public string LayerOrTool { get; }

    public NotchFeature(
        string contourId,
        double anchorArcLength,
        NotchShape shape,
        double width,
        double depth,
        MaterialSide materialSide,
        NotchOutputMode outputMode = NotchOutputMode.Cut,
        string layerOrTool = "CUT")
    {
        GeometryConstants.RejectNonFinite(anchorArcLength, nameof(anchorArcLength));
        GeometryConstants.RejectNonFinite(width, nameof(width));
        GeometryConstants.RejectNonFinite(depth, nameof(depth));

        if (string.IsNullOrWhiteSpace(contourId))
            throw new ArgumentException("剪口必须有轮廓标识符。", nameof(contourId));
        if (anchorArcLength < 0)
            throw new ArgumentOutOfRangeException(nameof(anchorArcLength), anchorArcLength, "弧长位置不能为负。");
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "剪口宽度必须为正。");
        if (depth <= 0)
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "剪口深度必须为正。");

        ContourId = contourId;
        AnchorArcLength = anchorArcLength;
        Shape = shape;
        Width = width;
        Depth = depth;
        MaterialSide = materialSide;
        OutputMode = outputMode;
        LayerOrTool = layerOrTool;
    }

    /// <summary>Generates the notch geometry as a polyline relative to the contour at the anchor point.</summary>
    public Polyline2D GenerateGeometry(Point2D anchorPoint, Point2D tangentDirection, Point2D normalDirection)
    {
        // Normal points toward material side
        var normal = MaterialSide == MaterialSide.Inside
            ? normalDirection
            : new Point2D(-normalDirection.X, -normalDirection.Y);

        return Shape switch
        {
            NotchShape.V => GenerateVNotch(anchorPoint, tangentDirection, normal),
            NotchShape.Square => GenerateSquareNotch(anchorPoint, tangentDirection, normal),
            NotchShape.U => GenerateUNotch(anchorPoint, tangentDirection, normal),
            NotchShape.HalfCircle => GenerateHalfCircleNotch(anchorPoint, tangentDirection, normal),
            NotchShape.Mark => GenerateMark(anchorPoint, tangentDirection),
            _ => throw new NotSupportedException($"不支持的剪口形状：{Shape}")
        };
    }

    private Polyline2D GenerateVNotch(Point2D anchor, Point2D tangent, Point2D normal)
    {
        var halfWidth = Width / 2;
        var left = new Point2D(anchor.X - halfWidth * tangent.X, anchor.Y - halfWidth * tangent.Y);
        var right = new Point2D(anchor.X + halfWidth * tangent.X, anchor.Y + halfWidth * tangent.Y);
        var tip = new Point2D(anchor.X + Depth * normal.X, anchor.Y + Depth * normal.Y);
        return new Polyline2D([left, tip, right]);
    }

    private Polyline2D GenerateSquareNotch(Point2D anchor, Point2D tangent, Point2D normal)
    {
        var halfWidth = Width / 2;
        var left = new Point2D(anchor.X - halfWidth * tangent.X, anchor.Y - halfWidth * tangent.Y);
        var right = new Point2D(anchor.X + halfWidth * tangent.X, anchor.Y + halfWidth * tangent.Y);
        var innerLeft = new Point2D(left.X + Depth * normal.X, left.Y + Depth * normal.Y);
        var innerRight = new Point2D(right.X + Depth * normal.X, right.Y + Depth * normal.Y);
        return new Polyline2D([left, innerLeft, innerRight, right]);
    }

    private Polyline2D GenerateUNotch(Point2D anchor, Point2D tangent, Point2D normal)
    {
        var halfWidth = Width / 2;
        var left = new Point2D(anchor.X - halfWidth * tangent.X, anchor.Y - halfWidth * tangent.Y);
        var right = new Point2D(anchor.X + halfWidth * tangent.X, anchor.Y + halfWidth * tangent.Y);
        var bottom = new Point2D(anchor.X + Depth * normal.X, anchor.Y + Depth * normal.Y);
        var radius = halfWidth;
        var points = new List<Point2D> { left };
        var steps = 8;
        for (var i = 1; i <= steps; i++)
        {
            var angle = Math.PI * (1 - (double)i / steps);
            var cx = bottom.X + radius * Math.Cos(angle) * tangent.X;
            var cy = bottom.Y + radius * Math.Sin(angle) * normal.Y;
            points.Add(new Point2D(cx, cy));
        }
        points.Add(right);
        return new Polyline2D(points);
    }

    private Polyline2D GenerateHalfCircleNotch(Point2D anchor, Point2D tangent, Point2D normal)
    {
        var radius = Width / 2;
        var points = new List<Point2D>();
        var steps = 16;
        for (var i = 0; i <= steps; i++)
        {
            var angle = Math.PI * (1 - (double)i / steps);
            var px = anchor.X + radius * Math.Cos(angle) * tangent.X + radius * Math.Sin(angle) * normal.X;
            var py = anchor.Y + radius * Math.Cos(angle) * tangent.Y + radius * Math.Sin(angle) * normal.Y;
            points.Add(new Point2D(px, py));
        }
        return new Polyline2D(points);
    }

    private static Polyline2D GenerateMark(Point2D anchor, Point2D tangent)
    {
        var halfWidth = 0.5; // small mark line
        return new Polyline2D([
            new Point2D(anchor.X - halfWidth * tangent.X, anchor.Y - halfWidth * tangent.Y),
            new Point2D(anchor.X + halfWidth * tangent.X, anchor.Y + halfWidth * tangent.Y)
        ]);
    }
}