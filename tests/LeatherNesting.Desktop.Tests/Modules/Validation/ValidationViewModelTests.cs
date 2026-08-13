using LeatherNesting.Desktop.Modules.Validation;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Validation;

public sealed class ValidationViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T10")]
    public void Error_scenario_summarizes_all_severities_and_blocks_exits()
    {
        var viewModel = new ValidationViewModel();

        Assert.True(viewModel.Scenario.BlockingCount > 0);
        Assert.True(viewModel.Scenario.WarningCount > 0);
        Assert.True(viewModel.Scenario.InformationCount > 0);
        Assert.False(viewModel.Scenario.CanApprove);
        Assert.False(viewModel.Scenario.CanExportForProduction);
        Assert.Contains("阻断", viewModel.ApprovalStatus);
        Assert.Contains("禁用", viewModel.ProductionExportStatus);
        Assert.False(viewModel.RequestApproval());
        Assert.False(viewModel.RequestProductionExport());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T10")]
    public void Every_issue_identifies_object_rule_and_recommendation()
    {
        var viewModel = new ValidationViewModel();

        Assert.All(viewModel.Scenarios.SelectMany(scenario => scenario.Issues), issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.ObjectId));
            Assert.False(string.IsNullOrWhiteSpace(issue.ObjectName));
            Assert.False(string.IsNullOrWhiteSpace(issue.RuleId));
            Assert.False(string.IsNullOrWhiteSpace(issue.RuleName));
            Assert.False(string.IsNullOrWhiteSpace(issue.Suggestion));
        });
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T10")]
    public void Valid_scenario_allows_demo_actions_but_marks_them_todo()
    {
        var viewModel = new ValidationViewModel();

        viewModel.SelectScenario("valid");

        Assert.Equal(0, viewModel.Scenario.BlockingCount);
        Assert.True(viewModel.Scenario.CanApprove);
        Assert.True(viewModel.Scenario.CanExportForProduction);
        Assert.True(viewModel.RequestApproval());
        Assert.Contains("TODO", viewModel.ActionMessage);
        Assert.True(viewModel.RequestProductionExport());
        Assert.Contains("TODO", viewModel.ActionMessage);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T10")]
    public void Locate_is_an_explicit_navigation_placeholder()
    {
        var viewModel = new ValidationViewModel();
        var issue = viewModel.Issues[0];

        viewModel.Locate(issue);

        Assert.Contains(issue.ObjectId, viewModel.ActionMessage);
        Assert.Contains("TODO", viewModel.ActionMessage);
        Assert.Contains("M09", viewModel.ActionMessage);
    }

    [Theory]
    [InlineData("with-errors")]
    [InlineData("valid")]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T10")]
    public void Quality_report_is_prominently_demo_and_never_claims_pdf_delivery(string scenarioId)
    {
        var viewModel = new ValidationViewModel();
        viewModel.SelectScenario(scenarioId);

        var report = viewModel.BuildReportPreview();

        Assert.StartsWith("DEMO", report);
        Assert.Contains("不可作为生产放行凭证", report);
        Assert.Contains("PDF", report);
        Assert.Contains("TODO", report);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T10")]
    public void Module_declares_m10_metadata_and_is_discoverable()
    {
        var module = new ValidationModule();

        Assert.Equal("M10", module.Metadata.Id);
        Assert.Equal("校验", module.Metadata.Title);
        Assert.Equal(10, module.Metadata.Order);
        Assert.IsType<ValidationView>(module.CreateView());

        var discovered = DesktopModuleDiscovery.Discover(typeof(ValidationModule).Assembly);
        Assert.Contains(discovered, item => item is ValidationModule && item.Metadata.Id == "M10");
    }
}
