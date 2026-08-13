namespace LeatherNesting.Desktop.Demo;

/// <summary>Read-only sample data shared across the 12 demo modules, so the numbers stay consistent.</summary>
public sealed record DemoScenario(
    string ProjectName,
    string OrderNumber,
    string Material,
    string Version,
    string Unit,
    int PieceCount,
    int MaterialCount,
    double UtilisationPercent)
{
    public const string DemoMarker = "DEMO";
}
