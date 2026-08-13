using LeatherNesting.Desktop.Demo;
using LeatherNesting.Desktop.Modules.Projects;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Projects;

public sealed class ProjectsTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void DemoScenario_has_all_order_fields()
    {
        var scenario = DemoScenarioFactory.Default;
        Assert.False(string.IsNullOrWhiteSpace(scenario.ProjectName));
        Assert.False(string.IsNullOrWhiteSpace(scenario.ProjectNumber));
        Assert.False(string.IsNullOrWhiteSpace(scenario.Customer));
        Assert.False(string.IsNullOrWhiteSpace(scenario.StyleNumber));
        Assert.False(string.IsNullOrWhiteSpace(scenario.Deadline));
        Assert.False(string.IsNullOrWhiteSpace(scenario.Priority));
        Assert.False(string.IsNullOrWhiteSpace(scenario.Creator));
        Assert.False(string.IsNullOrWhiteSpace(scenario.Status));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void DemoScenario_has_non_empty_histories()
    {
        var scenario = DemoScenarioFactory.Default;
        Assert.NotEmpty(scenario.VersionHistory);
        Assert.NotEmpty(scenario.ChangeHistory);
        Assert.NotEmpty(scenario.ExportHistory);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void Todo_actions_do_not_mutate_scenario()
    {
        var viewModel = new ProjectsViewModel();
        var before = viewModel.Scenario;

        viewModel.NewProject();
        viewModel.Approve();
        viewModel.EditOrder();

        Assert.Contains("TODO", viewModel.TodoMessage);
        Assert.Same(before, viewModel.Scenario);
    }
}
