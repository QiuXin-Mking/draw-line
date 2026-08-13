using Avalonia.Controls;
using LeatherNesting.Desktop.Demo;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Modules.Projects;
using LeatherNesting.Desktop.Shell;
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
        Assert.Contains("TODO", viewModel.TodoMessage);
        viewModel.Duplicate();
        Assert.Contains("TODO", viewModel.TodoMessage);
        viewModel.Approve();
        Assert.Contains("TODO", viewModel.TodoMessage);
        viewModel.Restore();
        Assert.Contains("TODO", viewModel.TodoMessage);
        viewModel.EditOrder();

        Assert.Contains("TODO", viewModel.TodoMessage);
        Assert.Same(before, viewModel.Scenario);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void ViewModel_consumes_the_injected_projects_provider()
    {
        var provider = new TestProjectsProvider();

        var viewModel = new ProjectsViewModel(provider);

        Assert.Equal("注入项目", viewModel.Scenario.ProjectName);
        Assert.Same(provider.VersionHistory, viewModel.Scenario.VersionHistory);
        Assert.Same(provider.ChangeHistory, viewModel.Scenario.ChangeHistory);
        Assert.Same(provider.ExportHistory, viewModel.Scenario.ExportHistory);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void Selecting_a_version_exposes_its_read_only_difference_summary()
    {
        var viewModel = new ProjectsViewModel();
        var selected = viewModel.Scenario.VersionHistory[0];

        viewModel.SelectVersion(selected);

        Assert.Equal($"{selected.Version}（{selected.Date}）：{selected.Summary}", viewModel.SelectedVersionDetail);
        Assert.Same(selected, viewModel.Scenario.VersionHistory[0]);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void Status_trace_covers_all_product_states_including_expiry()
    {
        var trace = new ProjectsViewModel().StatusTrace;

        Assert.Contains("已完成 / 未完全满足", trace);
        Assert.Contains("校验失败 / 校验通过", trace);
        Assert.Contains("输入变更 → 已过期", trace);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T01")]
    public void Projects_module_is_discoverable_with_stable_M01_metadata_and_view_factory()
    {
        var discovered = DesktopModuleDiscovery.Discover(typeof(ProjectsModule).Assembly);
        IDesktopModule module = Assert.Single(discovered, item => item.Metadata.Id == "M01");

        Assert.Equal("项目与订单", module.Metadata.Title);
        Assert.Equal("项目", module.Metadata.Group);
        Assert.Equal(1, module.Metadata.Order);
        Assert.IsType<ProjectsView>(module.CreateView());
    }

    private sealed class TestProjectsProvider : IProjectsDemoProvider
    {
        public DemoProjectSummary Summary { get; } = new(
            "注入项目", "PRJ-INJECTED", "ORD-INJECTED", "客户", "款号", "2026-08-31",
            "普通", "tester", "草稿", "备注", "材料", "2.0", "mm", 1, 1, 50);

        public IReadOnlyList<VersionEntry> VersionHistory { get; } =
            Array.AsReadOnly(new[] { new VersionEntry("2.0", "2026-08-13", "差异") });

        public IReadOnlyList<HistoryEntry> ChangeHistory { get; } =
            Array.AsReadOnly(new[] { new HistoryEntry("2026-08-13", "变更") });

        public IReadOnlyList<HistoryEntry> ExportHistory { get; } =
            Array.AsReadOnly(new[] { new HistoryEntry("2026-08-13", "导出") });
    }
}
