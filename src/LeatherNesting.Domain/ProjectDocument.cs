namespace LeatherNesting.Domain;

public enum UnitDecision { Unresolved, ConfirmedMillimetres }

public sealed record ImportDiagnostic(string Code, string Severity, string Message, string? EntityId = null);

public sealed record ImportReport(
    string SourcePath,
    string SourceSha256,
    UnitDecision UnitDecision,
    IReadOnlyList<ImportDiagnostic> Diagnostics)
{
    public static ImportReport Create(string sourcePath, string sourceSha256, UnitDecision unitDecision, IReadOnlyList<ImportDiagnostic> diagnostics) =>
        new(sourcePath, sourceSha256, unitDecision, diagnostics);
}

public sealed record ProjectDocument(
    Guid Id,
    string Name,
    int SchemaVersion,
    long Revision,
    bool IsDirty,
    IReadOnlyList<ImportReport> Imports)
{
    public const int CurrentSchemaVersion = 1;

    public static ProjectDocument CreateNew(string name) => new(Guid.NewGuid(), name, CurrentSchemaVersion, 0, false, []);

    public ProjectDocument CommitImport(ImportReport report) => this with
    {
        Revision = Revision + 1,
        IsDirty = true,
        Imports = [.. Imports, report],
    };

    public ProjectDocument MarkSaved() => this with { IsDirty = false };
}
