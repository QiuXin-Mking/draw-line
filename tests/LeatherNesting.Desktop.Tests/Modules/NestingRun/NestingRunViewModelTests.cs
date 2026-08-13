using LeatherNesting.Desktop.Modules.NestingRun;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.NestingRun;

public sealed class NestingRunViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Demo_run_follows_the_supported_improvement_and_completion_path()
    {
        var viewModel = new NestingRunViewModel();

        Assert.True(viewModel.Prepare());
        Assert.True(viewModel.Start());
        Assert.True(viewModel.ReportBetterDemoResult());
        Assert.True(viewModel.ResumeDemoRun());
        Assert.True(viewModel.Complete());

        Assert.Equal(NestingRunState.Completed, viewModel.State);
        Assert.Equal("完成", viewModel.StateLabel);
        Assert.True(viewModel.BestMetrics.UtilizationPercent > viewModel.RunStartMetrics.UtilizationPercent);
        Assert.All(viewModel.Timeline, entry => Assert.Contains("TODO · 模拟状态", entry.Detail));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Invalid_transition_is_rejected_without_changing_state()
    {
        var viewModel = new NestingRunViewModel();

        Assert.False(viewModel.Complete());

        Assert.Equal(NestingRunState.Ready, viewModel.State);
        Assert.Contains("不能", viewModel.Feedback);
        Assert.Contains("TODO · 模拟状态", viewModel.Feedback);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Stop_keeps_the_best_complete_demo_result()
    {
        var viewModel = RunningViewModel();
        Assert.True(viewModel.ReportBetterDemoResult());
        var bestBeforeStop = viewModel.BestMetrics;

        Assert.True(viewModel.Stop());

        Assert.Equal(NestingRunState.StoppedWithBest, viewModel.State);
        Assert.Equal(bestBeforeStop, viewModel.BestMetrics);
        Assert.Contains("保留当前最佳", viewModel.OutcomeSummary);
        Assert.Contains("不可用于生产", viewModel.OutcomeSummary);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Cancel_rolls_back_temporary_demo_improvements()
    {
        var viewModel = RunningViewModel();
        var runStart = viewModel.RunStartMetrics;
        Assert.True(viewModel.ReportBetterDemoResult());
        Assert.NotEqual(runStart, viewModel.BestMetrics);

        Assert.True(viewModel.Cancel());

        Assert.Equal(NestingRunState.Cancelled, viewModel.State);
        Assert.Equal(runStart, viewModel.BestMetrics);
        Assert.Contains("回滚", viewModel.OutcomeSummary);
        Assert.Contains("未写入方案", viewModel.OutcomeSummary);
    }

    [Theory]
    [InlineData("快速", 2, "0° / 180°", "优先级 → 面积", 42, false)]
    [InlineData("均衡", 5, "0° / 90° / 180° / 270°", "面积 → 优先级", 2026, true)]
    [InlineData("精细", 15, "0°--359°", "大件优先 → 小件填空", 731, true)]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Strategy_presets_expose_all_required_demo_settings(
        string preset,
        int minutes,
        string angles,
        string order,
        int seed,
        bool fillSmallPieces)
    {
        var viewModel = new NestingRunViewModel();

        Assert.True(viewModel.SelectPreset(preset));

        Assert.Equal(minutes, viewModel.Settings.TimeBudgetMinutes);
        Assert.Equal(angles, viewModel.Settings.AllowedAngles);
        Assert.Equal(order, viewModel.Settings.PlacementOrder);
        Assert.Equal(seed, viewModel.Settings.Seed);
        Assert.Equal(fillSmallPieces, viewModel.Settings.FillSmallPieces);
        Assert.Contains("TODO", viewModel.Settings.Disclaimer);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Ready_state_accepts_valid_in_memory_strategy_edits()
    {
        var viewModel = new NestingRunViewModel();

        Assert.True(viewModel.UpdateSettings("8", "0° / 180°", "优先级 → 面积", "99", true));

        Assert.Equal(8, viewModel.Settings.TimeBudgetMinutes);
        Assert.Equal(99, viewModel.Settings.Seed);
        Assert.True(viewModel.Settings.FillSmallPieces);
        Assert.Contains("TODO", viewModel.Feedback);
    }

    [Theory]
    [InlineData("0", "42")]
    [InlineData("-1", "42")]
    [InlineData("abc", "42")]
    [InlineData("5", "abc")]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Invalid_budget_or_seed_is_rejected_without_changing_settings(string budget, string seed)
    {
        var viewModel = new NestingRunViewModel();
        var original = viewModel.Settings;

        Assert.False(viewModel.UpdateSettings(budget, "0° / 180°", "优先级 → 面积", seed, true));

        Assert.Equal(original, viewModel.Settings);
        Assert.Contains("必须", viewModel.Feedback);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T08")]
    public void Module_definition_registers_M08_in_stable_navigation_position()
    {
        var module = new NestingRunModule();

        Assert.Equal("M08", module.Metadata.Id);
        Assert.Equal("排样运行", module.Metadata.Title);
        Assert.Equal("排样", module.Metadata.Group);
        Assert.Equal(8, module.Metadata.Order);
        Assert.IsType<NestingRunView>(module.CreateView());
    }

    private static NestingRunViewModel RunningViewModel()
    {
        var viewModel = new NestingRunViewModel();
        Assert.True(viewModel.Prepare());
        Assert.True(viewModel.Start());
        return viewModel;
    }
}
