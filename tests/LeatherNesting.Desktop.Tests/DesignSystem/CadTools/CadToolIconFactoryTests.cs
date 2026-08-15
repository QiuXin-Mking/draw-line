using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
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
            Assert.Equal(Stretch.Uniform, viewbox.Stretch);

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

            var firstShapes = Assert.IsType<Canvas>(first.Child).Children;
            var secondShapes = Assert.IsType<Canvas>(second.Child).Children;
            Assert.Equal(firstShapes.Count, secondShapes.Count);
            Assert.All(firstShapes.Zip(secondShapes), pair => Assert.NotSame(pair.First, pair.Second));
        }
    }

    [Fact]
    [Trait("Stage", "S2")]
    public void Each_catalog_key_is_owned_by_exactly_its_declared_icon_group()
    {
        foreach (var tool in CadToolCatalog.All)
        {
            var results = new[]
            {
                CreateFromGroupA(tool.IconKey),
                CreateFromGroupB(tool.IconKey),
                CreateFromGroupC(tool.IconKey),
                CreateFromGroupD(tool.IconKey),
                CreateFromGroupE(tool.IconKey),
            };

            Assert.Equal(1, results.Count(result => result.Created));
            Assert.True(results[(int)tool.Group].Created);
            Assert.NotNull(results[(int)tool.Group].Icon);
            Assert.All(results.Where((_, index) => index != (int)tool.Group), result =>
                Assert.Null(result.Icon));
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

    private static (bool Created, Control? Icon) CreateFromGroupA(CadToolIconKey key)
    {
        var created = CadToolIconGroupA.TryCreate(key, out var icon);
        return (created, icon);
    }

    private static (bool Created, Control? Icon) CreateFromGroupB(CadToolIconKey key)
    {
        var created = CadToolIconGroupB.TryCreate(key, out var icon);
        return (created, icon);
    }

    private static (bool Created, Control? Icon) CreateFromGroupC(CadToolIconKey key)
    {
        var created = CadToolIconGroupC.TryCreate(key, out var icon);
        return (created, icon);
    }

    private static (bool Created, Control? Icon) CreateFromGroupD(CadToolIconKey key)
    {
        var created = CadToolIconGroupD.TryCreate(key, out var icon);
        return (created, icon);
    }

    private static (bool Created, Control? Icon) CreateFromGroupE(CadToolIconKey key)
    {
        var created = CadToolIconGroupE.TryCreate(key, out var icon);
        return (created, icon);
    }
}
