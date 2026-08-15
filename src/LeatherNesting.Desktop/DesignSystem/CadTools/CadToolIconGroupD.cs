using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

namespace LeatherNesting.Desktop.DesignSystem.CadTools;

/// <summary>Original vector artwork for CAD toolbar commands 14–20.</summary>
public static class CadToolIconGroupD
{
    private const double StrokeWidth = 1.25;

    public static bool TryCreate(CadToolIconKey key, out Control? icon)
    {
        icon = key switch
        {
            CadToolIconKey.SharpCornerContour => Icon(
                Polyline(new Point(2, 15), new Point(7, 5), new Point(10, 10), new Point(16, 3)),
                Circle(5.75, 3.75, 2.5), Circle(8.75, 8.75, 2.5)),
            CadToolIconKey.CloseContour => Icon(
                Polyline(new Point(3, 5), new Point(14, 3), new Point(16, 13), new Point(5, 15), new Point(3, 5)),
                Line(1.5, 3, 5, 3), Line(1.5, 3, 1.5, 6.5)),
            CadToolIconKey.RoundContour => Icon(
                Polyline(new Point(2, 15), new Point(2, 8), new Point(3, 5), new Point(5, 3), new Point(8, 2), new Point(16, 2)),
                Line(2, 8, 8, 2), Circle(6.75, 0.75, 2.5)),
            CadToolIconKey.SmoothCurve => Icon(
                Polyline(new Point(2, 14), new Point(3, 9), new Point(5, 5), new Point(7, 5),
                    new Point(9, 8), new Point(11, 12), new Point(13, 12), new Point(15, 8), new Point(16, 4)),
                Line(2, 5, 16, 13)),
            CadToolIconKey.UvCurveDirection => Icon(
                Line(2, 3, 2, 9), Line(2, 9, 6, 9), Line(6, 9, 6, 3),
                Line(10, 3, 13, 9), Line(13, 9, 16, 3),
                Polyline(new Point(3, 14), new Point(6, 12.75), new Point(9, 14), new Point(12, 15.25), new Point(15, 14)),
                Line(13.5, 12.5, 15.5, 14), Line(15.5, 14, 13.5, 15.5)),
            CadToolIconKey.SharpenCorner => Icon(
                Polyline(new Point(2, 14), new Point(5, 13), new Point(7, 10), new Point(9, 5),
                    new Point(11, 10), new Point(13, 13), new Point(16, 14)),
                Line(9, 2, 9, 6), Line(7, 4, 9, 2), Line(11, 4, 9, 2)),
            CadToolIconKey.EraseSegment => Icon(
                Line(2, 13, 7, 8), Line(12, 3, 16, 7), Line(3, 15, 15, 15),
                Polyline(new Point(6, 6), new Point(10, 2), new Point(15, 7), new Point(11, 11), new Point(6, 6))),
            _ => null,
        };

        return icon is not null;
    }

    private static Viewbox Icon(params Shape[] shapes)
    {
        var canvas = new Canvas { Width = 18, Height = 18 };
        foreach (var shape in shapes)
            canvas.Children.Add(shape);

        return new Viewbox
        {
            Width = 18,
            Height = 18,
            Stretch = Stretch.Uniform,
            Child = canvas,
        };
    }

    private static Line Line(double x1, double y1, double x2, double y2) => new()
    {
        StartPoint = new Point(x1, y1),
        EndPoint = new Point(x2, y2),
        Stroke = AppTheme.ToolbarIconTeal,
        StrokeThickness = StrokeWidth,
        StrokeLineCap = PenLineCap.Round,
    };

    private static Ellipse Circle(double left, double top, double diameter)
    {
        var circle = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Fill = Brushes.Transparent,
            Stroke = AppTheme.ToolbarIconTeal,
            StrokeThickness = StrokeWidth,
        };
        Canvas.SetLeft(circle, left);
        Canvas.SetTop(circle, top);
        return circle;
    }

    private static Polyline Polyline(params Point[] points)
    {
        var polyline = new Polyline
        {
            Stroke = AppTheme.ToolbarIconTeal,
            StrokeThickness = StrokeWidth,
            StrokeLineCap = PenLineCap.Round,
        };
        foreach (var point in points)
            polyline.Points.Add(point);
        return polyline;
    }
}
