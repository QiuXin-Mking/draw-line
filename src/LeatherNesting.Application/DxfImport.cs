using System.Security.Cryptography;
using LeatherNesting.Domain;

namespace LeatherNesting.Application;

public enum DxfEntityKind { LwPolyline, Polyline, Line, Arc, Text, Other }

public enum DxfDeclaredUnit { Unknown, Unitless, Inches, Feet, Miles, Millimetres, Centimetres, Metres }

public sealed record DxfEntity(string Id, DxfEntityKind Kind, string Layer, bool IsClosed, int VertexCount);

public sealed record DxfImportResult(
    IReadOnlyList<DxfEntity> Entities,
    IReadOnlyList<DxfEntity> ClosedPieceCandidates,
    IReadOnlyList<ImportDiagnostic> Diagnostics,
    UnitDecision UnitDecision,
    DxfDeclaredUnit DeclaredUnit,
    int? DeclaredUnitCode);

public interface IDxfReader
{
    Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken);
}

/// <summary>Application boundary that preserves inspection before a project mutation is allowed.</summary>
public sealed class ImportDxfUseCase(IDxfReader reader)
{
    public async Task<ImportDxfPreparation> InspectAsync(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var result = await reader.ReadAsync(path, cancellationToken);
        try
        {
            var content = await File.ReadAllBytesAsync(path, cancellationToken);
            var sourceSha256 = Convert.ToHexString(SHA256.HashData(content));
            return new ImportDxfPreparation(path, sourceSha256, result);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            var diagnostics = result.Diagnostics.Append(new ImportDiagnostic(
                "DXF-SOURCE-FINGERPRINT-FAILED",
                "Blocking",
                $"无法读取 DXF 原文件以生成 SHA-256 指纹：{exception.Message}",
                path)).ToList();
            return new ImportDxfPreparation(path, null, result with { Diagnostics = diagnostics });
        }
    }
}

public sealed record ImportDxfPreparation(string SourcePath, string? SourceSha256, DxfImportResult Result)
{
    public ProjectDocument CommitTo(ProjectDocument project, UnitDecision confirmedUnit)
    {
        if (confirmedUnit != UnitDecision.ConfirmedMillimetres)
            throw new InvalidOperationException("导入必须明确确认毫米单位后才能写入项目。");
        if (string.IsNullOrWhiteSpace(SourceSha256))
            throw new InvalidOperationException("无法记录原始 DXF 指纹，不能提交导入。");
        return project.CommitImport(ImportReport.Create(SourcePath, SourceSha256, confirmedUnit, Result.Diagnostics));
    }
}
