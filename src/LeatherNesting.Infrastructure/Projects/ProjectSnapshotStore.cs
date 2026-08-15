using System.Text.Json;
using LeatherNesting.Application;
using LeatherNesting.Application.Domain;

namespace LeatherNesting.Infrastructure.Projects;

/// <summary>Persists a crash-recovery snapshot of a <see cref="NestingProject"/> to a sibling
/// <c>.autosave</c> file, reusing the zip serialization of <see cref="ZipNestingProjectStore"/>.</summary>
public sealed class ProjectSnapshotStore : ISnapshotStore
{
    private readonly ZipNestingProjectStore _store = new();

    public static string SnapshotPath(string projectPath) => projectPath + ".autosave";

    public Task SaveSnapshotAsync(string projectPath, NestingProject project, CancellationToken cancellationToken) =>
        _store.SaveAsync(SnapshotPath(projectPath), project, cancellationToken);

    public async Task<NestingProject?> LoadSnapshotAsync(string projectPath, CancellationToken cancellationToken)
    {
        var path = SnapshotPath(projectPath);
        if (!File.Exists(path))
            return null;

        try
        {
            return await _store.LoadAsync(path, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or JsonException)
        {
            // A corrupt snapshot is treated as absent, not as a crash.
            return null;
        }
    }

    public void ClearSnapshot(string projectPath)
    {
        var path = SnapshotPath(projectPath);
        if (File.Exists(path))
            File.Delete(path);
    }
}
