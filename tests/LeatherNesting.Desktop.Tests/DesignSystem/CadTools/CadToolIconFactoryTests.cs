using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LeatherNesting.Desktop.DesignSystem.CadTools;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.DesignSystem.CadTools;

[Collection("Avalonia UI")]
public sealed class CadToolIconFactoryTests
{
    [Fact]
    [Trait("Stage", "S2")]
    [Trait("TestId", "AC-CAD-T05")]
    public void Catalogs_27_icon_keys_are_all_rendered_as_18_by_18_vectors()
    {
        var catalogKeys = CadToolCatalog.All.Select(tool => tool.IconKey).ToArray();

        Assert.Equal(27, catalogKeys.Length);
        Assert.Equal(27, catalogKeys.Distinct().Count());
        Assert.Equal(Enum.GetValues<CadToolIconKey>().Order(), catalogKeys.Order());

        foreach (var key in catalogKeys)
        {
            var viewbox = Assert.IsType<Viewbox>(CadToolIconFactory.Create(key));
            Assert.Equal(18, viewbox.Width);
            Assert.Equal(18, viewbox.Height);

            var canvas = Assert.IsType<Canvas>(viewbox.Child);
            Assert.Equal(18, canvas.Width);
            Assert.Equal(18, canvas.Height);
            Assert.NotEmpty(canvas.Children);
            Assert.All(canvas.Children, child => Assert.IsAssignableFrom<Shape>(child));
            Assert.DoesNotContain(canvas.Children, child => child is Image or TextBlock);
        }
    }

    [Fact]
    [Trait("Stage", "S2")]
    public void Every_create_call_returns_a_fresh_visual_tree()
    {
        foreach (var key in Enum.GetValues<CadToolIconKey>())
        {
            var first = Assert.IsType<Viewbox>(CadToolIconFactory.Create(key));
            var second = Assert.IsType<Viewbox>(CadToolIconFactory.Create(key));

            Assert.NotSame(first, second);
            Assert.NotSame(first.Child, second.Child);
        }
    }

    [Fact]
    [Trait("Stage", "S2")]
    public void Unknown_icon_key_is_rejected_explicitly()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CadToolIconFactory.Create((CadToolIconKey)int.MaxValue));

        Assert.Equal("key", error.ParamName);
    }
}
