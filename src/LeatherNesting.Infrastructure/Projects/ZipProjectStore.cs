using System.IO.Compression;
using System.Text.Json;
using LeatherNesting.Application;
using LeatherNesting.Domain;

namespace LeatherNesting.Infrastructure.Projects;

public sealed class ZipProjectStore : IProjectStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public async Task SaveAsync(string path, ProjectDocument project, CancellationToken cancellationToken)
    {
        var temporary = path + ".tmp";
        var recovery = path + ".bak";
        try
        {
            // Keep a separately readable complete copy before replacing an existing archive.
            // The active project is never opened for writing, so a failed write leaves it intact.
            if (File.Exists(path)) File.Copy(path, recovery, overwrite: true);
            await using (var file = File.Create(temporary))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: false))
            await using (var stream = zip.CreateEntry("manifest.json", CompressionLevel.Optimal).Open())
                await JsonSerializer.SerializeAsync(stream, project.MarkSaved(), JsonOptions, cancellationToken);
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public async Task<ProjectDocument> LoadAsync(string path, CancellationToken cancellationToken)
    {
        await using var file = File.OpenRead(path);
        using var zip = new ZipArchive(file, ZipArchiveMode.Read);
        var entry = zip.GetEntry("manifest.json") ?? throw new InvalidDataException("项目缺少 manifest.json。");
        await using var stream = entry.Open();
        var project = await JsonSerializer.DeserializeAsync<ProjectDocument>(stream, JsonOptions, cancellationToken);
        return project ?? throw new InvalidDataException("项目 manifest 无效。");
    }
}
