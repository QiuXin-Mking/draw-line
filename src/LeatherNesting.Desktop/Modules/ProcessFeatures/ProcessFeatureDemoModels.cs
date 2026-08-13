using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Features;

namespace LeatherNesting.Desktop.Modules.ProcessFeatures;

/// <summary>Read-only process-feature row displayed by the M05 demo.</summary>
public sealed record ProcessFeatureRecord(string Kind, string Name, string Detail, string Tool);

/// <summary>Read-only grading-library rule displayed independently from process features.</summary>
public sealed record GradingRuleRecord(decimal Size, int SquareCount, int HalfCircleCount, int PointCount, int HalfSizeCount);

/// <summary>Immutable M05 demonstration data. Process features and grading rules intentionally have separate record types.</summary>
public static class ProcessFeatureDemoData
{
    public static IReadOnlyList<ProcessFeatureRecord> Features { get; } = Array.AsReadOnly([
        new ProcessFeatureRecord("内线", "缝线定位", "距边 8.0 mm", "MARK"),
        new ProcessFeatureRecord("冲孔", "定位孔", "直径 3.0 mm", "PUNCH-03"),
        new ProcessFeatureRecord("剪口", "侧缝 V 剪口", "宽 2.0 mm · 深 0.8 mm · 外侧", "CUT"),
        new ProcessFeatureRecord("文本", "裁片标识", "FRONT / 31", "MARK"),
        new ProcessFeatureRecord("Mark", "对位标记", "长度 1.0 mm", "MARK"),
    ]);

    public static IReadOnlyList<GradingRuleRecord> GradingRules { get; } = Array.AsReadOnly([
        new GradingRuleRecord(0.0m, 0, 0, 1, 0),
        new GradingRuleRecord(0.5m, 0, 0, 1, 0),
        new GradingRuleRecord(1.0m, 0, 0, 1, 0),
        new GradingRuleRecord(3.0m, 0, 0, 0, 0),
        new GradingRuleRecord(6.5m, 0, 0, 1, 0),
    ]);

    public static NotchValidationResult NotchValidation { get; } = ValidateDemoNotch();

    private static NotchValidationResult ValidateDemoNotch()
    {
        var contour = new Loop2D("m05-demo-contour", LoopRole.Outer,
        [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);
        var notch = new NotchFeature("m05-demo-contour", 20.0, NotchShape.V, 2.0, 0.8, MaterialSide.Outside);

        return new NotchValidator().Validate(notch, contour, []);
    }
}
