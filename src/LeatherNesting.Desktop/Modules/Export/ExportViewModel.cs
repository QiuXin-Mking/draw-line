using System.Text;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Export;

/// <summary>
/// Deterministic M11 presentation state. It deliberately has no writer, process launcher,
/// directory opener, device transport, or platform adapter dependency.
/// </summary>
public sealed class ExportViewModel
{
    private ExportDemoScenario _scenario = ExportDemoData.Scenarios[0];
    private readonly List<ExportOutputOption> _outputOptions = [.. ExportDemoData.OutputOptions];

    public ExportViewModel()
    {
        Settings = new ExportSettings(
            "DEMO/PROJ-2026-0813/v7/{时间戳}/",
            "{项目号}_{版本}_{材料}_{文件角色}",
            "毫米 (mm)",
            "材料左下角",
            "保持排样角度",
            "0.05 mm",
            "裁片编号 / 尺码 / 左右");
    }

    public IReadOnlyList<ExportDemoScenario> Scenarios => ExportDemoData.Scenarios;

    public ExportDemoScenario Scenario => _scenario;

    public IReadOnlyList<ExportOutputOption> OutputOptions => _outputOptions;

    public IReadOnlyList<ExportLayerMapping> LayerMappings => ExportDemoData.LayerMappings;

    public ExportSettings Settings { get; private set; }

    public string? ActionMessage { get; private set; }

    public bool CanRequestProductionExport => _scenario.CanExport;

    public bool HasExecutedExternalAction => false;

    public string ProductionExportStatus => CanRequestProductionExport
        ? "生产交接预览可用 · DEMO；实际文件写入仍为 TODO"
        : $"生产导出已禁用 · {_scenario.BlockingValidationCount} 个阻断校验必须先处理";

    public void SelectScenario(string scenarioId)
    {
        _scenario = Scenarios.Single(candidate => StringComparer.Ordinal.Equals(candidate.Id, scenarioId));
        ActionMessage = null;
    }

    public void UpdateSettings(
        string directory,
        string namingTemplate,
        string unit,
        string origin,
        string rotation)
    {
        Settings = Settings with
        {
            Directory = string.IsNullOrWhiteSpace(directory) ? Settings.Directory : directory.Trim(),
            NamingTemplate = string.IsNullOrWhiteSpace(namingTemplate) ? Settings.NamingTemplate : namingTemplate.Trim(),
            Unit = unit,
            Origin = origin,
            Rotation = rotation,
        };
        ActionMessage = "设置已更新到内存 DEMO；未创建目录、未写入配置或生产文件。";
    }

    public void SetOutputSelected(string outputId, bool isSelected)
    {
        var index = _outputOptions.FindIndex(option => StringComparer.Ordinal.Equals(option.Id, outputId));
        if (index < 0)
            throw new ArgumentException($"Unknown export output '{outputId}'.", nameof(outputId));

        _outputOptions[index] = _outputOptions[index] with { IsSelected = isSelected };
        ActionMessage = "输出选择仅更新内存 DEMO；未创建或删除任何文件。";
    }

    public bool RequestProductionExport()
    {
        if (!CanRequestProductionExport)
        {
            ActionMessage = $"禁止生产导出：仍有 {_scenario.BlockingValidationCount} 个阻断校验，请先返回 M10 校验处理。";
            return false;
        }

        ActionMessage = $"DEMO 交接包已生成预览；实际 DXF、报告、图片与 PDF 均未写入。{TodoBadge.StandardText}";
        return true;
    }

    public bool InvokeTodo(ExportTodoAction action)
    {
        var label = action switch
        {
            ExportTodoAction.WriteFiles => "实际文件写入与原子提交",
            ExportTodoAction.OpenOutputDirectory => "打开输出目录",
            ExportTodoAction.LaunchExternalProgram => "启动外部 CAD/PDF 程序",
            ExportTodoAction.ExportPlt => "PLT 适配与格式验证",
            ExportTodoAction.ExportDwg => "DWG 适配、许可与格式验证",
            ExportTodoAction.SendToDevice => "向生产设备发送",
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        ActionMessage = $"{label}：{TodoBadge.StandardText}，未执行任何外部路径。";
        return false;
    }

    public string BuildManifestPreview()
    {
        var outputs = string.Join(", ", OutputOptions.Where(option => option.IsSelected).Select(option => $"{option.Label}:{option.Role}"));
        var mappings = string.Join(", ", LayerMappings.Select(mapping => $"{mapping.Semantic}->{mapping.DxfLayer}/{mapping.LineType}"));
        var status = CanRequestProductionExport ? "PREVIEW_READY" : "BLOCKED";

        return new StringBuilder()
            .AppendLine("DEMO · manifest 预览 · 不可作为生产交接凭证")
            .AppendLine("schemaVersion: demo-export-manifest/v1")
            .AppendLine("projectNumber: PROJ-2026-0813")
            .AppendLine("projectVersion: v7-DEMO")
            .AppendLine("inputFingerprint: DEMO-SHA256-INPUT-NOT-COMPUTED")
            .AppendLine("outputHash: TODO · 实际文件生成后计算")
            .AppendLine($"validationStatus: {status}; blockingCount={_scenario.BlockingValidationCount}")
            .AppendLine($"directory: {Settings.Directory}")
            .AppendLine($"namingTemplate: {Settings.NamingTemplate}")
            .AppendLine($"unit: {Settings.Unit}; origin: {Settings.Origin}; rotation: {Settings.Rotation}")
            .AppendLine($"curveTolerance: {Settings.CurveTolerance}; labels: {Settings.LabelContent}")
            .AppendLine($"outputs: {outputs}")
            .AppendLine($"layerMappings: {mappings}")
            .Append("delivery: TODO · 未写盘、未启动外部程序、未发送设备")
            .ToString();
    }
}
