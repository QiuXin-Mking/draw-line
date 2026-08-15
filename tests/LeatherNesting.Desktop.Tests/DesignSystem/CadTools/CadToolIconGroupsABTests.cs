using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LeatherNesting.Desktop.DesignSystem.CadTools;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.DesignSystem.CadTools;

public sealed class CadToolIconGroupsABTests
{
    public static TheoryData<CadToolIconKey> GroupAKeys => new()
    {
        CadToolIconKey.ExportToOrder,
        CadToolIconKey.Select,
        CadToolIconKey.Refit,
    };

    public static TheoryData<CadToolIconKey> GroupBKeys => new()
    {
        CadToolIconKey.DrawPolyline,
        CadToolIconKey.DrawRectangle,
        CadToolIconKey.DrawCircle,
        CadToolIconKey.DrawLine,
        CadToolIconKey.TextAnnotation,
        CadToolIconKey.Dimension,
        CadToolIconKey.EditNodeOrFillet,
    };

    [Theory]
    [MemberData(nameof(GroupAKeys))]
    [Trait("TestId", "AC-CAD-T05")]
    public void Group_a_creates_a_compact_vector_for_each_owned_key(CadToolIconKey key)
    {
        Assert.True(CadToolIconGroupA.TryCreate(key, out var icon));

        AssertCompactVector(icon);
    }

    [Theory]
    [MemberData(nameof(GroupBKeys))]
    [Trait("TestId", "AC-CAD-T05")]
    public void Group_b_creates_a_compact_vector_for_each_owned_key(CadToolIconKey key)
    {
        Assert.True(CadToolIconGroupB.TryCreate(key, out var icon));

        AssertCompactVector(icon);
    }

    [Fact]
    public void Groups_reject_keys_owned_by_other_groups()
    {
        Assert.False(CadToolIconGroupA.TryCreate(CadToolIconKey.DrawPolyline, out var groupAIcon));
        Assert.Null(groupAIcon);
        Assert.False(CadToolIconGroupB.TryCreate(CadToolIconKey.Refit, out var groupBIcon));
        Assert.Null(groupBIcon);
    }

    private static void AssertCompactVector(Control? icon)
    {
        var viewbox = Assert.IsType<Viewbox>(icon);
        Assert.Equal(18, viewbox.Width);
        Assert.Equal(18, viewbox.Height);

        var canvas = Assert.IsType<Canvas>(viewbox.Child);
        Assert.Equal(18, canvas.Width);
        Assert.Equal(18, canvas.Height);
        Assert.NotEmpty(canvas.Children);
        Assert.All(canvas.Children, child =>
        {
            var shape = Assert.IsAssignableFrom<Shape>(child);
            Assert.InRange(shape.StrokeThickness, 1, 1.5);
        });
    }
}
