namespace LeatherNesting.Desktop.Modules.Export;

public sealed record ExportOutputOption(string Id, string Label, string Role, bool IsSelected);

public sealed record ExportLayerMapping(string Semantic, string DxfLayer, string LineType);

public sealed record ExportSettings(
    string Directory,
    string NamingTemplate,
    string Unit,
    string Origin,
    string Rotation,
    string CurveTolerance,
    string LabelContent);

public sealed record ExportDemoScenario(
    string Id,
    string Name,
    string Description,
    int BlockingValidationCount)
{
    public bool CanExport => BlockingValidationCount == 0;

    public override string ToString() => Name;
}

public enum ExportTodoAction
{
    WriteFiles,
    OpenOutputDirectory,
    LaunchExternalProgram,
    ExportPlt,
    ExportDwg,
    SendToDevice,
}

public static class ExportDemoData
{
    public static IReadOnlyList<ExportDemoScenario> Scenarios { get; } =
    [
        new("blocked", "阻断方案", "包含两个未解决的生产阻断项，用于验证出口门禁。", 2),
        new("exportable", "可导出方案", "无阻断项；仅允许预览交接包，不生成真实文件。", 0),
    ];

    public static IReadOnlyList<ExportOutputOption> OutputOptions { get; } =
    [
        new("dxf", "DXF", "生产几何与工艺图层", true),
        new("json", "JSON", "结构化生产报告", true),
        new("csv", "CSV", "裁片与实例清单", true),
        new("png", "PNG", "排样缩略预览", true),
        new("pdf", "PDF", "交接说明预览", true),
    ];

    public static IReadOnlyList<ExportLayerMapping> LayerMappings { get; } =
    [
        new("外轮廓", "CUT_OUTER", "CONTINUOUS"),
        new("孔", "CUT_HOLE", "CONTINUOUS"),
        new("内部线", "SEW_INTERNAL", "DASHED"),
        new("冲孔/标记", "MARK_PUNCH", "CENTER"),
        new("裁片标识", "TEXT_PIECE", "CONTINUOUS"),
        new("材料边界", "MATERIAL_BOUNDARY", "PHANTOM"),
    ];
}
