using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

namespace LeatherNesting.Desktop.DesignSystem.CadTools;

/// <summary>Creates the order, selection, and viewport icons owned by CAD tool group A.</summary>
public static class CadToolIconGroupA
{
    private const double IconSize = 18;
    private const double StrokeThickness = 1.25;

    public static bool TryCreate(CadToolIconKey key, out Control? icon)
    {
        icon = key switch
        {
            CadToolIconKey.ExportToOrder => Icon(
                Box(2.25, 1.75, 8.5, 14.5),
                Line(4.5, 5, 8.5, 5),
                Line(4.5, 8, 8.5, 8),
                Line(9.5, 12, 16, 12, AppTheme.ClassicFocus),
                Line(13, 9, 16, 12, AppTheme.ClassicFocus),
                Line(13, 15, 16, 12, AppTheme.ClassicFocus)),
            CadToolIconKey.Select => Icon(
                Line(3, 1.75, 3, 14),
                Line(3, 1.75, 13.25, 9),
                Line(13.25, 9, 8.25, 9.75),
                Line(8.25, 9.75, 11.5, 15.5),
                Line(11.5, 15.5, 9.25, 16.5),
                Line(9.25, 16.5, 6.25, 10.75),
                Line(6.25, 10.75, 3, 14)),
            CadToolIconKey.Refit => Icon(
                Line(1.75, 6.5, 1.75, 1.75), Line(1.75, 1.75, 6.5, 1.75),
                Line(11.5, 1.75, 16.25, 1.75), Line(16.25, 1.75, 16.25, 6.5),
                Line(16.25, 11.5, 16.25, 16.25), Line(16.25, 16.25, 11.5, 16.25),
                Line(6.5, 16.25, 1.75, 16.25), Line(1.75, 16.25, 1.75, 11.5),
                Box(5.25, 5.25, 7.5, 7.5, AppTheme.ClassicFocus)),
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

    private static Rectangle Box(
        double left,
        double top,
        double width,
        double height,
        IBrush? stroke = null)
    {
        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = stroke ?? AppTheme.PrimaryText,
            StrokeThickness = StrokeThickness,
            Fill = Brushes.Transparent,
        };
        Canvas.SetLeft(rectangle, left);
        Canvas.SetTop(rectangle, top);
        return rectangle;
    }
}
