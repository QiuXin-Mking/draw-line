using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LeatherNesting.Desktop.DesignSystem.CadTools;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;
using VectorPath = Avalonia.Controls.Shapes.Path;

namespace LeatherNesting.Desktop.Tests.DesignSystem.CadTools;

[Collection("Avalonia UI")]
public sealed class CadToolIconGroupETests
{
    private static readonly CadToolIconKey[] GroupEKeys =
    [
        CadToolIconKey.RegionOrdering,
        CadToolIconKey.Transform,
        CadToolIconKey.Undo,
        CadToolIconKey.Redo,
        CadToolIconKey.Cancel,
        CadToolIconKey.Delete,
        CadToolIconKey.Settings,
    ];

    [Fact]
    [Trait("Stage", "S2-C")]
    public void Every_group_e_key_creates_a_compact_vector_control()
    {
        foreach (var key in GroupEKeys)
        {
            Assert.True(CadToolIconGroupE.TryCreate(key, out var control));
            var icon = Assert.IsType<Viewbox>(control);
            Assert.Equal(18, icon.Width);
            Assert.Equal(18, icon.Height);

            var canvas = Assert.IsType<Canvas>(icon.Child);
            Assert.Equal(18, canvas.Width);
            Assert.Equal(18, canvas.Height);
            Assert.NotEmpty(canvas.Children);
            Assert.All(canvas.Children, child => Assert.IsAssignableFrom<Shape>(child));
            Assert.DoesNotContain(canvas.Children, child => child is TextBlock or Image);
            Assert.All(canvas.Children.OfType<Shape>(), shape =>
                Assert.InRange(shape.StrokeThickness, 1d, 1.5d));
        }
    }

    [Fact]
    [Trait("Stage", "S2-C")]
    public void Group_e_icons_have_distinct_vector_silhouettes()
    {
        var signatures = GroupEKeys.Select(key =>
        {
            Assert.True(CadToolIconGroupE.TryCreate(key, out var control));
            var canvas = Assert.IsType<Canvas>(Assert.IsType<Viewbox>(control).Child);
            return VectorSignature(canvas);
        });

        Assert.Equal(GroupEKeys.Length, signatures.Distinct().Count());
    }

    [Fact]
    [Trait("Stage", "S2-C")]
    public void Non_group_e_keys_are_declined_without_creating_a_control()
    {
        var otherGroupKeys = Enum.GetValues<CadToolIconKey>().Except(GroupEKeys);

        foreach (var key in otherGroupKeys)
        {
            Assert.False(CadToolIconGroupE.TryCreate(key, out var control));
            Assert.Null(control);
        }
    }

    private static string VectorSignature(Canvas canvas) => string.Join('|', canvas.Children.Select(child => child switch
    {
        VectorPath path => $"P:{path.Data}",
        Line line => $"L:{line.StartPoint}:{line.EndPoint}",
        Rectangle rectangle => $"R:{Canvas.GetLeft(rectangle)}:{Canvas.GetTop(rectangle)}:{rectangle.Width}:{rectangle.Height}",
        Ellipse ellipse => $"E:{Canvas.GetLeft(ellipse)}:{Canvas.GetTop(ellipse)}:{ellipse.Width}:{ellipse.Height}",
        _ => child.GetType().Name,
    }));
}
