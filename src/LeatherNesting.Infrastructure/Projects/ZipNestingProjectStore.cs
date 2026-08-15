using System.IO.Compression;
using System.Text.Json;
using LeatherNesting.Application;
using LeatherNesting.Application.Domain;

namespace LeatherNesting.Infrastructure.Projects;

/// <summary>Persists a <see cref="NestingProject"/> (metadata + pieces + materials + nesting results)
/// to a zip archive, mirroring the crash-safe write pattern of <see cref="ZipProjectStore"/>.</summary>
public sealed class ZipNestingProjectStore : INestingProjectStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public async Task SaveAsync(string path, NestingProject project, CancellationToken cancellationToken)
    {
        var temporary = path + ".tmp";
        var recovery = path + ".bak";
        try
        {
            if (File.Exists(path)) File.Copy(path, recovery, overwrite: true);
            await using (var file = File.Create(temporary))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: false))
            await using (var stream = zip.CreateEntry("manifest.json", CompressionLevel.Optimal).Open())
                await JsonSerializer.SerializeAsync(stream, project, JsonOptions, cancellationToken);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public async Task<NestingProject> LoadAsync(string path, CancellationToken cancellationToken)
    {
        await using var file = File.OpenRead(path);
        using var zip = new ZipArchive(file, ZipArchiveMode.Read);
        var entry = zip.GetEntry("manifest.json") ?? throw new InvalidDataException("项目缺少 manifest.json。");
        await using var stream = entry.Open();
        var project = await JsonSerializer.DeserializeAsync<NestingProject>(stream, JsonOptions, cancellationToken);
        if (project is null) throw new InvalidDataException("项目 manifest 无效。");

        // Tolerate legacy manifests that omit the newer collections.
        return project with
        {
            Pieces = project.Pieces ?? [],
            Materials = project.Materials ?? [],
            NestingResults = project.NestingResults ?? [],
        };
    }
}
