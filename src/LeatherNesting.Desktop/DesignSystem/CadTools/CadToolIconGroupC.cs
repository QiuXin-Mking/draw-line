using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

namespace LeatherNesting.Desktop.DesignSystem.CadTools;

/// <summary>Original vector artwork for CAD toolbar commands 11–13.</summary>
public static class CadToolIconGroupC
{
    private const double StrokeWidth = 1.25;

    public static bool TryCreate(CadToolIconKey key, out Control? icon)
    {
        icon = key switch
        {
            CadToolIconKey.HolePattern => Icon(
                Circle(2.5, 2.5, 4), Circle(11.5, 2.5, 4),
                Circle(2.5, 11.5, 4), Circle(11.5, 11.5, 4),
                Line(9, 1.5, 9, 16.5), Line(1.5, 9, 16.5, 9)),
            CadToolIconKey.DrawSpline => Icon(
                Polyline(new Point(2, 13), new Point(3, 8), new Point(5, 5), new Point(7, 6),
                    new Point(9, 9), new Point(11, 13), new Point(13, 13), new Point(15, 9), new Point(16, 5)),
                Circle(0.75, 11.75, 2.5), Circle(7.75, 7.75, 2.5), Circle(14.75, 3.75, 2.5)),
            CadToolIconKey.Notch => Icon(
                Polyline(new Point(2, 10), new Point(9, 10), new Point(12, 4), new Point(15, 10), new Point(16, 10)),
                Line(2, 14, 16, 14)),
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
