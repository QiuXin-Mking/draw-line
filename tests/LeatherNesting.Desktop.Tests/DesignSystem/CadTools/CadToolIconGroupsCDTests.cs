using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LeatherNesting.Desktop.DesignSystem.CadTools;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.DesignSystem.CadTools;

[Collection("Avalonia UI")]
public sealed class CadToolIconGroupsCDTests
{
    public static TheoryData<CadToolIconKey> GroupCKeys => new()
    {
        CadToolIconKey.HolePattern,
        CadToolIconKey.DrawSpline,
        CadToolIconKey.Notch,
    };

    public static TheoryData<CadToolIconKey> GroupDKeys => new()
    {
        CadToolIconKey.SharpCornerContour,
        CadToolIconKey.CloseContour,
        CadToolIconKey.RoundContour,
        CadToolIconKey.SmoothCurve,
        CadToolIconKey.UvCurveDirection,
        CadToolIconKey.SharpenCorner,
        CadToolIconKey.EraseSegment,
    };

    [Theory]
    [MemberData(nameof(GroupCKeys))]
    public void GroupC_creates_each_owned_key_as_compact_vector_artwork(CadToolIconKey key)
    {
        Assert.True(CadToolIconGroupC.TryCreate(key, out var icon));

        AssertCompactVectorIcon(icon);
    }

    [Theory]
    [MemberData(nameof(GroupDKeys))]
    public void GroupD_creates_each_owned_key_as_compact_vector_artwork(CadToolIconKey key)
    {
        Assert.True(CadToolIconGroupD.TryCreate(key, out var icon));

        AssertCompactVectorIcon(icon);
    }

    [Fact]
    public void Groups_decline_keys_owned_by_other_groups()
    {
        Assert.False(CadToolIconGroupC.TryCreate(CadToolIconKey.Select, out var groupCIcon));
        Assert.Null(groupCIcon);
        Assert.False(CadToolIconGroupD.TryCreate(CadToolIconKey.Settings, out var groupDIcon));
        Assert.Null(groupDIcon);
    }

    [Fact]
    public void Notch_uses_an_explicit_triangular_cut_in_the_contour()
    {
        Assert.True(CadToolIconGroupC.TryCreate(CadToolIconKey.Notch, out var icon));

        var contour = Assert.Single(GetCanvas(icon).Children.OfType<Polyline>());
        Assert.Contains(contour.Points, point => point.X == 9 && point.Y == 10);
        Assert.Contains(contour.Points, point => point.X == 12 && point.Y == 4);
        Assert.Contains(contour.Points, point => point.X == 15 && point.Y == 10);
    }

    [Fact]
    public void Uv_direction_uses_vector_strokes_instead_of_font_glyphs()
    {
        Assert.True(CadToolIconGroupD.TryCreate(CadToolIconKey.UvCurveDirection, out var icon));

        var canvas = GetCanvas(icon);
        Assert.DoesNotContain(canvas.Children, child => child is TextBlock or Image);
        Assert.True(canvas.Children.OfType<Line>().Count() >= 4);
    }

    private static void AssertCompactVectorIcon(Control? icon)
    {
        var viewbox = Assert.IsType<Viewbox>(icon);
        Assert.InRange(viewbox.Width, 16, 18);
        Assert.InRange(viewbox.Height, 16, 18);

        var canvas = GetCanvas(viewbox);
        Assert.Equal(18, canvas.Width);
        Assert.Equal(18, canvas.Height);
        Assert.NotEmpty(canvas.Children);
        Assert.All(canvas.Children, child => Assert.IsAssignableFrom<Shape>(child));
        Assert.DoesNotContain(canvas.Children, child => child is TextBlock or Image);

        var strokedShapes = canvas.Children.OfType<Shape>().Where(shape => shape.Stroke is not null).ToArray();
        Assert.NotEmpty(strokedShapes);
        Assert.All(strokedShapes, shape => Assert.InRange(shape.StrokeThickness, 1, 1.5));
    }

    private static Canvas GetCanvas(Control? icon) =>
        Assert.IsType<Canvas>(Assert.IsType<Viewbox>(icon).Child);
}
