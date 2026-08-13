using LeatherNesting.Desktop.Demo;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Demo;

public sealed class DemoScenarioFactoryTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "F03-001")]
    public void Providers_are_stable_and_return_the_same_read_only_values()
    {
        var first = DemoScenarioFactory.Projects;
        var second = DemoScenarioFactory.Projects;

        Assert.Same(first, second);
        Assert.Same(first.Summary, DemoScenarioFactory.Summary.Summary);
        Assert.Equal("凉鞋排样演示", first.Summary.ProjectName);
        Assert.Equal("1.0.0", first.Summary.Version);
        Assert.Equal(9, first.Summary.PieceCount);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "F03-002")]
    public void Module_records_cannot_be_mutated_through_their_read_only_collections()
    {
        var provider = DemoScenarioFactory.Projects;

        Assert.Throws<NotSupportedException>(() =>
            ((IList<VersionEntry>)provider.VersionHistory).Add(new("2.0.0", "2026-08-14", "页面写入")));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<HistoryEntry>)provider.ChangeHistory).Add(new("2026-08-14", "页面写入")));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<HistoryEntry>)provider.ExportHistory).Add(new("2026-08-14", "页面写入")));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "F03-003")]
    public void Legacy_scenario_facade_matches_the_shared_summary_and_projects_records()
    {
        var scenario = DemoScenarioFactory.Default;
        var provider = DemoScenarioFactory.Projects;

        Assert.Equal(provider.Summary.ProjectName, scenario.ProjectName);
        Assert.Equal(provider.Summary.OrderNumber, scenario.OrderNumber);
        Assert.Equal(provider.Summary.Material, scenario.Material);
        Assert.Equal(provider.Summary.Version, scenario.Version);
        Assert.Same(provider.VersionHistory, scenario.VersionHistory);
        Assert.Same(provider.ChangeHistory, scenario.ChangeHistory);
        Assert.Same(provider.ExportHistory, scenario.ExportHistory);
    }
}
