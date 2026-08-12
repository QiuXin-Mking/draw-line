using System.Globalization;
using LeatherNesting.Application;
using LeatherNesting.Domain;

namespace LeatherNesting.Infrastructure.Dxf;

/// <summary>Small, dependency-free Stage 1 reader. It inventories ASCII DXF entities; geometry repair remains Stage 2.</summary>
public sealed class AsciiDxfReader : IDxfReader
{
    public async Task<DxfImportResult> ReadAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            var groups = ToGroups(lines);
            if (!groups.Any(group => Is(group, "0", "SECTION")))
                return Invalid("DXF-INVALID-HEADER", "文件不含 DXF SECTION 头。", path);
            return Parse(groups);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or FormatException)
        {
            return Invalid("DXF-READ-FAILED", $"无法读取 DXF：{exception.Message}", path);
        }
    }

    private static DxfImportResult Parse(IReadOnlyList<Group> groups)
    {
        var entities = new List<DxfEntity>();
        var diagnostics = new List<ImportDiagnostic>();
        var inEntities = false;
        for (var index = 0; index < groups.Count; index++)
        {
            if (Is(groups[index], "2", "ENTITIES")) { inEntities = true; continue; }
            if (inEntities && Is(groups[index], "0", "ENDSEC")) break;
            if (!inEntities || groups[index].Code != "0") continue;
            var type = groups[index].Value.ToUpperInvariant();
            if (type is not ("LWPOLYLINE" or "POLYLINE" or "LINE" or "ARC" or "TEXT")) continue;
            var polylineEnd = type == "POLYLINE" ? FindPolylineEnd(groups, index + 1) : null;
            var end = polylineEnd?.End ?? FindEntityEnd(groups, index + 1);
            var fields = groups.Skip(index + 1).Take(end - index - 1).ToList();
            var layer = fields.FirstOrDefault(group => group.Code == "8")?.Value ?? "0";
            var flags = ParseInt(fields.FirstOrDefault(group => group.Code == "70")?.Value);
            var vertices = fields.Count(group => group.Code == "10");
            var kind = Enum.Parse<DxfEntityKind>(type switch { "LWPOLYLINE" => "LwPolyline", _ => char.ToUpperInvariant(type[0]) + type[1..].ToLowerInvariant() });
            var closed = (flags & 1) == 1;
            var entity = new DxfEntity($"entity-{entities.Count + 1}", kind, layer, closed, vertices);
            entities.Add(entity);
            AddPolylineDiagnostics(diagnostics, entity, polylineEnd);
            index = end - 1;
        }
        var (declaredUnitCode, declaredUnit) = ReadDeclaredUnit(groups);
        var unit = UnitDecision.Unresolved;
        diagnostics.Add(new("DXF-UNIT-REVIEW", "Blocking", "DXF 单位必须由用户确认后才可提交为毫米项目。"));
        var candidates = entities.Where(entity =>
            (entity.Kind is DxfEntityKind.LwPolyline or DxfEntityKind.Polyline) &&
            entity.IsClosed &&
            entity.VertexCount >= 3 &&
            !diagnostics.Any(diagnostic => diagnostic.EntityId == entity.Id && diagnostic.Severity == "Blocking"))
            .ToList();
        return new(entities, candidates, diagnostics, unit, declaredUnit, declaredUnitCode);
    }

    private static void AddPolylineDiagnostics(
        ICollection<ImportDiagnostic> diagnostics,
        DxfEntity entity,
        PolylineEnd? polylineEnd)
    {
        if (entity.Kind is not (DxfEntityKind.LwPolyline or DxfEntityKind.Polyline)) return;

        if (entity.Kind == DxfEntityKind.Polyline && polylineEnd is { IsTerminated: false })
            diagnostics.Add(new("DXF-POLYLINE-MISSING-SEQEND", "Blocking", "旧式 POLYLINE 缺少 SEQEND；已保留可见顶点清单，但不能作为裁片轮廓。", entity.Id));
        if (!entity.IsClosed)
            diagnostics.Add(new("DXF-OPEN-POLYLINE", "Blocking", $"{entity.Kind} 未闭合；请在阶段 2 的修复工作台预览并确认。", entity.Id));
        if (entity.IsClosed && entity.VertexCount < 3)
            diagnostics.Add(new("DXF-POLYLINE-TOO-FEW-VERTICES", "Blocking", "闭合多段线少于三个顶点，不能构成裁片轮廓。", entity.Id));
    }

    private static (int? Code, DxfDeclaredUnit Unit) ReadDeclaredUnit(IReadOnlyList<Group> groups)
    {
        for (var index = 0; index < groups.Count - 1; index++)
        {
            if (Is(groups[index], "9", "$INSUNITS") && groups[index + 1].Code == "70")
            {
                var code = ParseInt(groups[index + 1].Value);
                return (code, code switch
                {
                    0 => DxfDeclaredUnit.Unitless,
                    1 => DxfDeclaredUnit.Inches,
                    2 => DxfDeclaredUnit.Feet,
                    3 => DxfDeclaredUnit.Miles,
                    4 => DxfDeclaredUnit.Millimetres,
                    5 => DxfDeclaredUnit.Centimetres,
                    6 => DxfDeclaredUnit.Metres,
                    _ => DxfDeclaredUnit.Unknown,
                });
            }
        }
        return (null, DxfDeclaredUnit.Unknown);
    }

    private static int FindEntityEnd(IReadOnlyList<Group> groups, int start)
    {
        for (var index = start; index < groups.Count; index++) if (groups[index].Code == "0") return index;
        return groups.Count;
    }
    private static PolylineEnd FindPolylineEnd(IReadOnlyList<Group> groups, int start)
    {
        for (var index = start; index < groups.Count; index++)
        {
            if (Is(groups[index], "0", "SEQEND")) return new(index, true);
            if (Is(groups[index], "0", "ENDSEC") || Is(groups[index], "0", "EOF")) return new(index, false);
        }
        return new(groups.Count, false);
    }
    private static int ParseInt(string? value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
    private static bool Is(Group group, string code, string value) =>
        group.Code == code && group.Value.Equals(value, StringComparison.OrdinalIgnoreCase);
    private static IReadOnlyList<Group> ToGroups(IReadOnlyList<string> lines)
    {
        if (lines.Count % 2 != 0) throw new FormatException("DXF 组码行数必须为偶数。");
        return Enumerable.Range(0, lines.Count / 2).Select(index => new Group(lines[index * 2].Trim(), lines[index * 2 + 1].Trim())).ToList();
    }
    private static DxfImportResult Invalid(string code, string message, string path) => new([], [], [new(code, "Blocking", message, path)], UnitDecision.Unresolved, DxfDeclaredUnit.Unknown, null);
    private sealed record Group(string Code, string Value);
    private sealed record PolylineEnd(int End, bool IsTerminated);
}
