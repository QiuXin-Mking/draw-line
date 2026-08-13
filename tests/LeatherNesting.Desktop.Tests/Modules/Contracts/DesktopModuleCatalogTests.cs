using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Contracts;

public sealed class DesktopModuleCatalogTests
{
    [Fact]
    public void CreateValidated_rejects_duplicate_module_ids()
    {
        var modules = new IDesktopModule[]
        {
            new TestModule("M01", order: 10),
            new TestModule("M01", order: 20)
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => DesktopModuleCatalog.CreateValidated(modules));

        Assert.Contains("M01", exception.Message);
    }

    [Fact]
    public void CreateValidated_orders_by_order_without_reordering_equal_values()
    {
        var firstWithSameOrder = new TestModule("M02", order: 20);
        var modules = new IDesktopModule[]
        {
            firstWithSameOrder,
            new TestModule("M01", order: 10),
            new TestModule("M03", order: 20)
        };

        var catalog = DesktopModuleCatalog.CreateValidated(modules);

        Assert.Equal(new[] { "M01", "M02", "M03" }, catalog.Select(module => module.Metadata.Id));
        Assert.Same(firstWithSameOrder, catalog[1]);
    }

    [Fact]
    public void Metadata_is_immutable()
    {
        var metadata = new DesktopModuleMetadata("M01", "项目与订单", "项目", 10);

        Assert.All(
            typeof(DesktopModuleMetadata).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.Equal("M01", metadata.Id);
        Assert.Equal("项目与订单", metadata.Title);
        Assert.Equal("项目", metadata.Group);
        Assert.Equal(10, metadata.Order);
    }

    private sealed class TestModule(string id, int order) : IDesktopModule
    {
        public DesktopModuleMetadata Metadata { get; } = new(id, $"Title {id}", "Test", order);

        public Func<Control> CreateView { get; } = static () => new UserControl();
    }
}
