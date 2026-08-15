using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

namespace LeatherNesting.Desktop.DesignSystem.CadTools;

/// <summary>Creates the basic drawing and annotation icons owned by CAD tool group B.</summary>
public static class CadToolIconGroupB
{
    private const double IconSize = 18;
    private const double StrokeThickness = 1.25;

    public static bool TryCreate(CadToolIconKey key, out Control? icon)
    {
        icon = key switch
        {
            CadToolIconKey.DrawPolyline => Icon(
                Line(2, 14.5, 6.25, 5),
                Line(6.25, 5, 11.25, 11),
                Line(11.25, 11, 16, 3.5),
                Circle(1.25, 13.75, 1.5), Circle(5.5, 4.25, 1.5),
                Circle(10.5, 10.25, 1.5), Circle(15.25, 2.75, 1.5)),
            CadToolIconKey.DrawRectangle => Icon(
                Box(2.25, 3.25, 13.5, 11.5),
                Circle(1.5, 2.5, 1.5, AppTheme.ClassicFocus),
                Circle(15, 13.5, 1.5, AppTheme.ClassicFocus)),
            CadToolIconKey.DrawCircle => Icon(Circle(2.25, 2.25, 13.5)),
            CadToolIconKey.DrawLine => Icon(
                Line(2.5, 15.5, 15.5, 2.5),
                Circle(1.5, 14.5, 2, AppTheme.ClassicFocus),
                Circle(14.5, 1.5, 2, AppTheme.ClassicFocus)),
            CadToolIconKey.TextAnnotation => Icon(
                Line(3, 3, 15, 3),
                Line(9, 3, 9, 15),
                Line(6.5, 15, 11.5, 15)),
            CadToolIconKey.Dimension => Icon(
                Line(3, 3, 3, 15), Line(15, 3, 15, 15),
                Line(3, 9, 15, 9, AppTheme.ClassicFocus),
                Line(3, 9, 6, 6.75, AppTheme.ClassicFocus),
                Line(3, 9, 6, 11.25, AppTheme.ClassicFocus),
                Line(15, 9, 12, 6.75, AppTheme.ClassicFocus),
                Line(15, 9, 12, 11.25, AppTheme.ClassicFocus)),
            CadToolIconKey.EditNodeOrFillet => Icon(
                Line(2.25, 15.5, 2.25, 5.5),
                Line(2.25, 5.5, 7, 5.5),
                Line(7, 5.5, 10.5, 9),
                Line(10.5, 9, 15.75, 9),
                Circle(5.75, 4.25, 2.5, AppTheme.ClassicFocus),
                Line(13, 2.25, 13, 6.25, AppTheme.ClassicFocus),
                Line(11, 4.25, 15, 4.25, AppTheme.ClassicFocus)),
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

    private static Line Line(double x1, double y1, double x2, double y2, IBrush? stroke = null) => new()
    {
        StartPoint = new Point(x1, y1),
        EndPoint = new Point(x2, y2),
        Stroke = stroke ?? AppTheme.PrimaryText,
        StrokeThickness = StrokeThickness,
        StrokeLineCap = PenLineCap.Round,
        StrokeJoin = PenLineJoin.Round,
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

    private static Ellipse Circle(
        double left,
        double top,
        double diameter,
        IBrush? stroke = null)
    {
        var ellipse = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Stroke = stroke ?? AppTheme.PrimaryText,
            StrokeThickness = StrokeThickness,
            Fill = Brushes.Transparent,
        };
        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        return ellipse;
    }
}
