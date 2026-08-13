namespace LeatherNesting.Desktop.Demo;

/// <summary>A version entry in the read-only version timeline.</summary>
public sealed record VersionEntry(string Version, string Date, string Summary);

/// <summary>A dated history entry (change or export).</summary>
public sealed record HistoryEntry(string Date, string Description);

/// <summary>Read-only sample data shared across the 12 demo modules, so the numbers stay consistent.</summary>
public sealed record DemoScenario(
    string ProjectName,
    string ProjectNumber,
    string OrderNumber,
    string Customer,
    string StyleNumber,
    string Deadline,
    string Priority,
    string Creator,
    string Status,
    string Notes,
    string Material,
    string Version,
    string Unit,
    int PieceCount,
    int MaterialCount,
    double UtilisationPercent,
    IReadOnlyList<VersionEntry> VersionHistory,
    IReadOnlyList<HistoryEntry> ChangeHistory,
    IReadOnlyList<HistoryEntry> ExportHistory)
{
    public const string DemoMarker = "DEMO";
}
