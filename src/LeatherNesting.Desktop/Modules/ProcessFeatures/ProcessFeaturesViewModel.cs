using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Geometry.Features;

namespace LeatherNesting.Desktop.Modules.ProcessFeatures;

/// <summary>M05 read-only demo state. Commands are explicit placeholders until feature persistence and tool integration exist.</summary>
public sealed class ProcessFeaturesViewModel
{
    public IReadOnlyList<ProcessFeatureRecord> Features => ProcessFeatureDemoData.Features;

    public IReadOnlyList<GradingRuleRecord> GradingRules => ProcessFeatureDemoData.GradingRules;

    public NotchValidationResult NotchValidation => ProcessFeatureDemoData.NotchValidation;

    public string? TodoMessage { get; private set; }

    public void CreateFeature() => SetTodo("创建特征");

    public void SaveFeature() => SetTodo("保存特征");

    public void GenerateGrading() => SetTodo("码齿生成");

    public void MapTool() => SetTodo("刀具映射");

    private void SetTodo(string action) => TodoMessage = $"{action}：{TodoBadge.StandardText}";
}
