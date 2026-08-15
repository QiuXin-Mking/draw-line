using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

namespace LeatherNesting.Desktop.DesignSystem.CadTools;

/// <summary>Original compact vector artwork for CAD toolbar commands 21 through 27.</summary>
public static class CadToolIconGroupE
{
    private const double IconSize = 18;
    private const double StrokeThickness = 1.25;

    public static bool TryCreate(CadToolIconKey key, out Control? icon)
    {
        icon = key switch
        {
            CadToolIconKey.RegionOrdering => Icon(
                Box(2.5, 6.5, 9, 8),
                Box(6.5, 3.5, 9, 8),
                Line(3, 4.5, 5.5, 2),
                Line(5.5, 2, 8, 4.5)),
            CadToolIconKey.Transform => Icon(
                Box(4, 4, 10, 10),
                Line(4, 4, 1.75, 1.75),
                Line(1.75, 1.75, 1.75, 4.5),
                Line(1.75, 1.75, 4.5, 1.75),
                Line(14, 14, 16.25, 16.25),
                Line(16.25, 16.25, 16.25, 13.5),
                Line(16.25, 16.25, 13.5, 16.25)),
            CadToolIconKey.Undo => Icon(
                Line(15.5, 13.5, 15, 10),
                Line(15, 10, 12.5, 7),
                Line(12.5, 7, 9, 5.75),
                Line(9, 5.75, 5, 6),
                Line(5, 6, 8, 3),
                Line(5, 6, 8, 9)),
            CadToolIconKey.Redo => Icon(
                Line(2.5, 13.5, 3, 10),
                Line(3, 10, 5.5, 7),
                Line(5.5, 7, 9, 5.75),
                Line(9, 5.75, 13, 6),
                Line(13, 6, 10, 3),
                Line(13, 6, 10, 9)),
            CadToolIconKey.Cancel => Icon(
                Circle(2.25, 2.25, 13.5),
                Line(5.5, 5.5, 12.5, 12.5),
                Line(12.5, 5.5, 5.5, 12.5)),
            CadToolIconKey.Delete => Icon(
                Box(4.25, 5.5, 9.5, 10),
                Line(3, 5.5, 15, 5.5),
                Line(6.5, 3, 11.5, 3),
                Line(7, 8, 7, 13),
                Line(11, 8, 11, 13)),
            CadToolIconKey.Settings => Icon(
                Line(2, 4.5, 16, 4.5),
                Circle(5, 2.5, 4),
                Line(2, 9, 16, 9),
                Circle(10.5, 7, 4),
                Line(2, 13.5, 16, 13.5),
                Circle(7, 11.5, 4)),
            _ => null,
        };

        return icon is not null;
    }

    private static Viewbox Icon(params Shape[] shapes)
    {
        var canvas = new Canvas { Width = IconSize, Height = IconSize };
        foreach (var shape in shapes)
            canvas.Children.Add(shape);

        return new Viewbox
        {
            Width = IconSize,
            Height = IconSize,
            Stretch = Stretch.Uniform,
            Child = canvas,
        };
    }

    private static Line Line(double x1, double y1, double x2, double y2) => new()
    {
        StartPoint = new Point(x1, y1),
        EndPoint = new Point(x2, y2),
        Stroke = AppTheme.PrimaryText,
        StrokeThickness = StrokeThickness,
        StrokeLineCap = PenLineCap.Round,
    };

    private static Rectangle Box(double left, double top, double width, double height)
    {
        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = AppTheme.PrimaryText,
            StrokeThickness = StrokeThickness,
            Fill = Brushes.Transparent,
        };
        Canvas.SetLeft(rectangle, left);
        Canvas.SetTop(rectangle, top);
        return rectangle;
    }

    private static Ellipse Circle(double left, double top, double diameter)
    {
        var ellipse = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Stroke = AppTheme.PrimaryText,
            StrokeThickness = StrokeThickness,
            Fill = Brushes.Transparent,
        };
        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        return ellipse;
    }
}
