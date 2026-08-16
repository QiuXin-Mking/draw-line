using System.Globalization;

namespace LeatherNesting.Desktop.Modules.BoardSettings;

/// <summary>版型设置弹窗的可编辑表单模型。校验失败不写入任何共享状态（PRD R5）。</summary>
public sealed class BoardSettingsViewModel
{
    public static IReadOnlyList<string> DirectionOptions { get; } = ["横向", "纵向"];

    public static IReadOnlyList<string> RemnantPolicyOptions { get; } = ["补齐", "丢弃"];

    public string Name { get; set; } = BoardSettingsConfig.Default.Name;

    public string Direction { get; set; } = "纵向";

    public string WidthText { get; set; } = BoardSettingsConfig.Default.WidthMm.ToString("0.00", CultureInfo.InvariantCulture);

    public string LengthText { get; set; } = BoardSettingsConfig.Default.LengthMm.ToString("0.00", CultureInfo.InvariantCulture);

    public string LayersText { get; set; } = BoardSettingsConfig.Default.Layers.ToString(CultureInfo.InvariantCulture);

    public string RemnantPolicy { get; set; } = BoardSettingsConfig.Default.RemnantPolicy;

    public string EdgeText { get; set; } = BoardSettingsConfig.Default.EdgeMm.ToString("0.00", CultureInfo.InvariantCulture);

    public string SpacingText { get; set; } = BoardSettingsConfig.Default.SpacingMm.ToString("0.00", CultureInfo.InvariantCulture);

    public string? WidthError { get; private set; }
    public string? LengthError { get; private set; }
    public string? LayersError { get; private set; }
    public string? EdgeError { get; private set; }
    public string? SpacingError { get; private set; }

    public bool HasErrors =>
        WidthError is not null || LengthError is not null || LayersError is not null ||
        EdgeError is not null || SpacingError is not null;

    public BoardSettingsConfig? ConfirmedConfig { get; private set; }

    /// <summary>校验全部字段：通过则生成配置并返回 true；任一非法返回 false 且仅在对应字段旁显示错误。</summary>
    public bool TryConfirm()
    {
        ClearErrors();

        // 材料层数仅接受阿拉伯数字（正整数）。
        if (!int.TryParse(LayersText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var layers) || layers <= 0)
            LayersError = "层数必须是大于 0 的整数（阿拉伯数字）。";

        var width = ParseNonNegative(WidthText, "宽度", out var widthError);
        WidthError = widthError;
        var length = ParseNonNegative(LengthText, "长度", out var lengthError);
        LengthError = lengthError;
        var edge = ParseNonNegative(EdgeText, "边缘", out var edgeError);
        EdgeError = edgeError;
        var spacing = ParseNonNegative(SpacingText, "间距", out var spacingError);
        SpacingError = spacingError;

        if (HasErrors)
            return false;

        ConfirmedConfig = new BoardSettingsConfig(
            Name ?? string.Empty,
            StringComparer.Ordinal.Equals(Direction, "横向") ? BoardDirection.Horizontal : BoardDirection.Vertical,
            width,
            length,
            layers,
            RemnantPolicy ?? BoardSettingsConfig.Default.RemnantPolicy,
            edge,
            spacing);
        return true;
    }

    /// <summary>取消：清空待确认配置，不改变任何已确认状态。</summary>
    public void Cancel() => ConfirmedConfig = null;

    private void ClearErrors() =>
        (WidthError, LengthError, LayersError, EdgeError, SpacingError) = (null, null, null, null, null);

    private static double ParseNonNegative(string? value, string label, out string? error)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) || parsed < 0)
        {
            error = $"{label}必须是大于等于 0 的数值。";
            return 0;
        }

        error = null;
        return parsed;
    }
}
