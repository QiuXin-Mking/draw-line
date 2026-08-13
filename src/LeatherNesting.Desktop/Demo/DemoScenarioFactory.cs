namespace LeatherNesting.Desktop.Demo;

/// <summary>Provides a single shared demo scenario instance.</summary>
public static class DemoScenarioFactory
{
    public static DemoScenario Default { get; } = new(
        ProjectName: "凉鞋排样演示",
        OrderNumber: "ORD-2026-0801",
        Material: "真皮 · 2000×1000 mm",
        Version: "1.0.0",
        Unit: "mm",
        PieceCount: 9,
        MaterialCount: 3,
        UtilisationPercent: 43.2);
}
