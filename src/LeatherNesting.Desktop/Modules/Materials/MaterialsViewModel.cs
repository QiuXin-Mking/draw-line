using System.Globalization;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Materials;

public enum MaterialKind
{
    Sheet,
    Roll,
}

/// <summary>Editable demo-only material values. They are intentionally not connected to project persistence or nesting.</summary>
public sealed class MaterialsViewModel
{
    private readonly List<MaterialDemo> _materials =
    [
        new("MAT-01", "头层牛皮 · 黑色", MaterialKind.Sheet, 2000, 1000, 2, 12, 8, "横向", "86%", "2.00 m²", "DEMO · 片料面积"),
        new("MAT-02", "超纤革 · 米白", MaterialKind.Roll, 1370, null, 3, 10, 6, "纵向", "可用宽 1320 mm", "6.80 m", "DEMO · 卷料用长"),
        new("MAT-03", "里料 · 灰色", MaterialKind.Roll, 1200, null, 1, 8, 5, "双向", "可用宽 1160 mm", "3.10 m", "DEMO · 卷料用长"),
    ];

    public IReadOnlyList<MaterialDemo> Materials => _materials;

    public MaterialDemo Selected { get; private set; }

    public string? WidthError { get; private set; }
    public string? LengthError { get; private set; }
    public string? LayerError { get; private set; }
    public string TodoMessage { get; private set; } = TodoBadge.StandardText;

    public MaterialsViewModel() => Selected = _materials[0];

    public void Select(string materialId)
    {
        Selected = _materials.Single(material => material.Id == materialId);
        ClearErrors();
    }

    public bool UpdateSelected(string? width, string? length, string? layers)
    {
        ClearErrors();
        var parsedWidth = ParsePositive(width, "宽度", out var widthError);
        WidthError = widthError;

        double? parsedLength = null;
        if (Selected.Kind == MaterialKind.Sheet)
        {
            parsedLength = ParsePositive(length, "长度", out var lengthError);
            LengthError = lengthError;
        }

        var parsedLayers = ParsePositiveInteger(layers, "层数", out var layerError);
        LayerError = layerError;
        if (WidthError is not null || LengthError is not null || LayerError is not null)
            return false;

        var index = _materials.FindIndex(material => material.Id == Selected.Id);
        Selected = Selected with { WidthMm = parsedWidth, LengthMm = parsedLength, Layers = parsedLayers };
        _materials[index] = Selected;
        TodoMessage = $"材料参数仅更新在内存 DEMO：{TodoBadge.StandardText}";
        return true;
    }

    public string Summary => $"DEMO · {_materials.Count} 种材料 · 片料 {_materials.Count(x => x.Kind == MaterialKind.Sheet)} 张 · 卷料 {_materials.Count(x => x.Kind == MaterialKind.Roll)} 卷 · 面积/用长均为演示估算";

    private void ClearErrors() => (WidthError, LengthError, LayerError) = (null, null, null);

    private static double ParsePositive(string? value, string label, out string? error)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            error = $"{label}必须是大于 0 的数值。";
            return 0;
        }

        error = null;
        return parsed;
    }

    private static int ParsePositiveInteger(string? value, string label, out string? error)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            error = $"{label}必须是大于 0 的整数。";
            return 0;
        }

        error = null;
        return parsed;
    }
}

public sealed record MaterialDemo(
    string Id,
    string Name,
    MaterialKind Kind,
    double WidthMm,
    double? LengthMm,
    int Layers,
    double EdgeMm,
    double SpacingMm,
    string Direction,
    string UsableArea,
    string DemoEstimate,
    string EstimateLabel);
