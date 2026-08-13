namespace LeatherNesting.Desktop.Demo;

/// <summary>Exposes read-only demo providers and the legacy scenario facade.</summary>
public static class DemoScenarioFactory
{
    /// <summary>Module-scoped sample records for the Projects UI.</summary>
    public static IProjectsDemoProvider Projects { get; } = new ProjectsDemoProvider();

    /// <summary>Shared shell-facing projection of the current demo project.</summary>
    public static IDemoProjectSummaryProvider Summary { get; } = Projects;

    /// <summary>
    /// Compatibility facade for existing pages. New modules should consume their narrow provider instead.
    /// </summary>
    public static DemoScenario Default { get; } = DemoScenario.From(Projects);

    private sealed class ProjectsDemoProvider : IProjectsDemoProvider
    {
        public DemoProjectSummary Summary { get; } = new(
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
            UtilisationPercent: 43.2);

        public IReadOnlyList<VersionEntry> VersionHistory { get; } = Array.AsReadOnly(new VersionEntry[]
        {
            new("1.0.0", "2026-08-13", "初始导入凉鞋 DXF，确认毫米单位。"),
            new("0.9.0", "2026-08-11", "新建项目骨架，尚未导入。"),
        });

        public IReadOnlyList<HistoryEntry> ChangeHistory { get; } = Array.AsReadOnly(new HistoryEntry[]
        {
            new("2026-08-13 09:00", "确认毫米并提交导入。"),
            new("2026-08-13 08:30", "导入凉鞋 DXF（9 个闭合裁片）。"),
        });

        public IReadOnlyList<HistoryEntry> ExportHistory { get; } = Array.AsReadOnly(new HistoryEntry[]
        {
            new("2026-08-12", "demo-export-v1.dxf"),
            new("2026-08-10", "demo-export-v0.dxf"),
        });
    }
}
