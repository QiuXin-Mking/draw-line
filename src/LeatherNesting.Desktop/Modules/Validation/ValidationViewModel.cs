using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Validation;

/// <summary>Local, explicitly non-persistent state for the M10 validation demonstration.</summary>
public sealed class ValidationViewModel
{
    private ValidationDemoScenario _scenario = ValidationDemoData.Scenarios[0];

    public IReadOnlyList<ValidationDemoScenario> Scenarios => ValidationDemoData.Scenarios;

    public IReadOnlyList<ValidationRule> Rules => ValidationDemoData.Rules;

    public ValidationDemoScenario Scenario => _scenario;

    public IReadOnlyList<ValidationIssue> Issues => _scenario.Issues;

    public string? ActionMessage { get; private set; }

    public string ApprovalStatus => _scenario.CanApprove
        ? "准备审批 · 无阻断项（DEMO）"
        : $"审批已阻断 · {_scenario.BlockingCount} 个阻断问题待处理";

    public string ProductionExportStatus => _scenario.CanExportForProduction
        ? "生产导出入口可演示（实际文件生成仍为 TODO）"
        : "生产导出已禁用 · 必须先清除全部阻断问题";

    public void SelectScenario(string scenarioId)
    {
        _scenario = Scenarios.Single(scenario => StringComparer.Ordinal.Equals(scenario.Id, scenarioId));
        ActionMessage = null;
    }

    public void Locate(ValidationIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        ActionMessage = $"定位 {issue.ObjectName}（{issue.ObjectId}）：{TodoBadge.StandardText}；未来通过工作区导航消息打开 M09 结果复核。";
    }

    public bool RequestApproval()
    {
        if (!_scenario.CanApprove)
        {
            ActionMessage = "审批被阻断：请先解决全部阻断问题。";
            return false;
        }

        ActionMessage = $"审批、豁免签名与状态持久化：{TodoBadge.StandardText}";
        return true;
    }

    public bool RequestProductionExport()
    {
        if (!_scenario.CanExportForProduction)
        {
            ActionMessage = "生产导出被阻断：请先解决全部阻断问题。";
            return false;
        }

        ActionMessage = $"生产 PDF / 文件导出：{TodoBadge.StandardText}";
        return true;
    }

    public string BuildReportPreview() =>
        $"DEMO · 质量报告预览（不可作为生产放行凭证）\n" +
        $"方案：{_scenario.Name}\n" +
        $"结论：{(_scenario.CanApprove ? "无阻断项，等待人工审批" : "不通过，禁止生产放行")}\n" +
        $"问题：阻断 {_scenario.BlockingCount} / 警告 {_scenario.WarningCount} / 提示 {_scenario.InformationCount}\n" +
        "校验基线：演示规则集 2026.08 · 报告编号 DEMO-QA-0813\n" +
        $"PDF 生成与签章：{TodoBadge.StandardText}";
}
