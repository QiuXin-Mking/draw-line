using LeatherNesting.Desktop.Tests.DesignSystem.CadTools;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

public sealed class AvaloniaUiIsolationTests
{
    [Fact]
    public void Cad_tool_icon_visual_tests_belong_to_the_non_parallel_Avalonia_UI_collection()
    {
        Type[] iconVisualTestClasses =
        [
            typeof(CadToolIconFactoryTests),
            typeof(CadToolIconGroupsABTests),
            typeof(CadToolIconGroupsCDTests),
            typeof(CadToolIconGroupETests),
        ];

        Assert.All(iconVisualTestClasses, testClass =>
        {
            var collection = Assert.Single(
                testClass.CustomAttributes,
                attribute => attribute.AttributeType == typeof(CollectionAttribute));
            var collectionName = Assert.Single(collection.ConstructorArguments).Value;
            Assert.Equal("Avalonia UI", Assert.IsType<string>(collectionName));
        });
    }
}
