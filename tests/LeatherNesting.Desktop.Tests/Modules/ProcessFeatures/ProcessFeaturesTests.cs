using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.ProcessFeatures;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.ProcessFeatures;

public sealed class ProcessFeaturesTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T05")]
    public void Module_declares_M05_metadata()
    {
        var module = new ProcessFeaturesModule();

        Assert.Equal("M05", module.Metadata.Id);
        Assert.Equal("工艺特征", module.Metadata.Title);
        Assert.Equal("CAD 工作台", module.Metadata.Group);
        Assert.Equal(5, module.Metadata.Order);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T05")]
    public void Demo_model_keeps_feature_and_grading_rule_records_separate()
    {
        var viewModel = new ProcessFeaturesViewModel();

        Assert.Contains(viewModel.Features, feature => feature.Kind == "内线");
        Assert.Contains(viewModel.Features, feature => feature.Kind == "冲孔");
        Assert.Contains(viewModel.Features, feature => feature.Kind == "剪口");
        Assert.Contains(viewModel.Features, feature => feature.Kind == "文本");
        Assert.Contains(viewModel.Features, feature => feature.Kind == "Mark");
        Assert.NotEmpty(viewModel.GradingRules);
        Assert.NotSame(viewModel.Features, viewModel.GradingRules);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T05")]
    public void Existing_notch_validation_result_is_available_for_display()
    {
        var viewModel = new ProcessFeaturesViewModel();

        Assert.True(viewModel.NotchValidation.IsValid);
        Assert.Empty(viewModel.NotchValidation.Errors);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T05")]
    public void Submission_actions_remain_todo_and_do_not_mutate_demo_data()
    {
        var viewModel = new ProcessFeaturesViewModel();
        var features = viewModel.Features;
        var rules = viewModel.GradingRules;

        viewModel.CreateFeature();
        viewModel.SaveFeature();
        viewModel.GenerateGrading();
        viewModel.MapTool();

        Assert.NotNull(viewModel.TodoMessage);
        Assert.Contains("TODO", viewModel.TodoMessage);
        Assert.Equal(TodoBadge.StandardText, viewModel.TodoMessage!.Split('：')[1]);
        Assert.Same(features, viewModel.Features);
        Assert.Same(rules, viewModel.GradingRules);
    }
}
