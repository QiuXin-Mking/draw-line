namespace LeatherNesting.Desktop.Modules.Validation;

public enum ValidationSeverity
{
    Blocking,
    Warning,
    Information,
}

public sealed record ValidationIssue(
    string Id,
    ValidationSeverity Severity,
    string ObjectId,
    string ObjectName,
    string RuleId,
    string RuleName,
    string Message,
    string Suggestion);

public sealed record ValidationRule(
    string Id,
    string Name,
    string Scope,
    string Description,
    string ProductionImpact);

public sealed record ValidationDemoScenario(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<ValidationIssue> Issues)
{
    public int BlockingCount => Issues.Count(issue => issue.Severity == ValidationSeverity.Blocking);

    public int WarningCount => Issues.Count(issue => issue.Severity == ValidationSeverity.Warning);

    public int InformationCount => Issues.Count(issue => issue.Severity == ValidationSeverity.Information);

    public bool CanApprove => BlockingCount == 0;

    public bool CanExportForProduction => BlockingCount == 0;

    public override string ToString() => Name;
}

public static class ValidationDemoData
{
    public static IReadOnlyList<ValidationRule> Rules { get; } =
    [
        new("VAL-GEO-001", "轮廓必须闭合", "裁片轮廓", "生产轮廓不得包含大于 0.10 mm 的开口。", "阻断审批与生产导出"),
        new("VAL-NEST-004", "裁片不得重叠", "排样实例", "实例安全边界不得与另一实例相交。", "阻断审批与生产导出"),
        new("VAL-MAT-002", "材料边距建议", "材料边界", "裁片与材料边界建议保留至少 8 mm。", "产生警告，需人工复核"),
        new("VAL-PROC-007", "剪口映射完整", "工艺特征", "剪口应具有明确的刀具或生产映射。", "产生提示，进入交接清单"),
    ];

    public static IReadOnlyList<ValidationDemoScenario> Scenarios { get; } =
    [
        new(
            "with-errors",
            "含错误方案",
            "演示阻断、警告和提示并存时的出口控制。",
            [
                new("ISS-001", ValidationSeverity.Blocking, "PIECE-VAMP-39-L", "鞋面 39 左", "VAL-GEO-001", "轮廓必须闭合", "外轮廓存在 0.42 mm 开口。", "返回几何修复页，闭合端点后重新校验。"),
                new("ISS-002", ValidationSeverity.Blocking, "INSTANCE-QTR-39-R-08", "后帮 39 右 · 实例 08", "VAL-NEST-004", "裁片不得重叠", "与实例 07 的安全边界重叠 3.8 mm²。", "在结果复核页移开实例并重新执行碰撞校验。"),
                new("ISS-003", ValidationSeverity.Warning, "INSTANCE-TONGUE-39-03", "鞋舌 39 · 实例 03", "VAL-MAT-002", "材料边距建议", "距材料上边缘仅 5 mm。", "将实例向下移动至少 3 mm，或由负责人确认风险。"),
                new("ISS-004", ValidationSeverity.Information, "NOTCH-VAMP-39-L-02", "鞋面 39 左 · 剪口 02", "VAL-PROC-007", "剪口映射完整", "尚未选择生产刀具映射。", "在工艺特征页补充刀具映射后更新交接清单。"),
            ]),
        new(
            "valid",
            "有效方案",
            "演示无阻断项、仅保留复核提示时的审批准备状态。",
            [
                new("ISS-101", ValidationSeverity.Warning, "INSTANCE-LINING-39-06", "里料 39 · 实例 06", "VAL-MAT-002", "材料边距建议", "距材料右边缘为 7.5 mm。", "人工确认边缘质量；生产前建议调整至 8 mm。"),
                new("ISS-102", ValidationSeverity.Information, "NOTCH-QTR-39-R-01", "后帮 39 右 · 剪口 01", "VAL-PROC-007", "剪口映射完整", "刀具映射来自演示规则快照。", "生产交接时核对设备侧刀具编号。"),
            ]),
    ];
}
