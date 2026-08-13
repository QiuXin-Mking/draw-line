namespace LeatherNesting.Desktop.Demo;

/// <summary>A version entry in the read-only version timeline.</summary>
public sealed record VersionEntry(string Version, string Date, string Summary);

/// <summary>A dated history entry (change or export).</summary>
public sealed record HistoryEntry(string Date, string Description);

/// <summary>
/// Read-only project fields shared by desktop modules and the shell.
/// It contains no view, shell, or infrastructure dependency.
/// </summary>
public sealed record DemoProjectSummary(
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
    double UtilisationPercent);

/// <summary>Provides the shared, view-independent project summary used in demo mode.</summary>
public interface IDemoProjectSummaryProvider
{
    DemoProjectSummary Summary { get; }
}

/// <summary>Provides the read-only sample records needed by the Projects module.</summary>
public interface IProjectsDemoProvider : IDemoProjectSummaryProvider
{
    IReadOnlyList<VersionEntry> VersionHistory { get; }
    IReadOnlyList<HistoryEntry> ChangeHistory { get; }
    IReadOnlyList<HistoryEntry> ExportHistory { get; }
}

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

    internal static DemoScenario From(IProjectsDemoProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var summary = provider.Summary;
        return new(
            summary.ProjectName,
            summary.ProjectNumber,
            summary.OrderNumber,
            summary.Customer,
            summary.StyleNumber,
            summary.Deadline,
            summary.Priority,
            summary.Creator,
            summary.Status,
            summary.Notes,
            summary.Material,
            summary.Version,
            summary.Unit,
            summary.PieceCount,
            summary.MaterialCount,
            summary.UtilisationPercent,
            provider.VersionHistory,
            provider.ChangeHistory,
            provider.ExportHistory);
    }
}
