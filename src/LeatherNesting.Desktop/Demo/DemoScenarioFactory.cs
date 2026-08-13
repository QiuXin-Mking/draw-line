namespace LeatherNesting.Desktop.Demo;

/// <summary>Provides a single shared demo scenario instance.</summary>
public static class DemoScenarioFactory
{
    public static DemoScenario Default { get; } = new(
        ProjectName: "凉鞋排样演示",
        ProjectNumber: "PRJ-2026-0801",
        OrderNumber: "ORD-2026-0801",
        Customer: "示例鞋材厂",
        StyleNumber: "凉鞋-01",
        Deadline: "2026-08-20",
        Priority: "高",
        Creator: "qx",
        Status: "草稿",
        Notes: "用于 UI 演示的示例项目。",
        Material: "真皮 · 2000×1000 mm",
        Version: "1.0.0",
        Unit: "mm",
        PieceCount: 9,
        MaterialCount: 3,
        UtilisationPercent: 43.2,
        VersionHistory:
        [
            new("1.0.0", "2026-08-13", "初始导入凉鞋 DXF，确认毫米单位。"),
            new("0.9.0", "2026-08-11", "新建项目骨架，尚未导入。"),
        ],
        ChangeHistory:
        [
            new("2026-08-13 09:00", "确认毫米并提交导入。"),
            new("2026-08-13 08:30", "导入凉鞋 DXF（9 个闭合裁片）。"),
        ],
        ExportHistory:
        [
            new("2026-08-12", "demo-export-v1.dxf"),
            new("2026-08-10", "demo-export-v0.dxf"),
        ]);
}
