namespace LeatherNesting.Desktop.Modules.BoardSettings;

/// <summary>面料摆放方向：横向 / 纵向（切换面料宽长轴）。</summary>
public enum BoardDirection
{
    Horizontal,
    Vertical,
}

/// <summary>
/// 版型设置的已确认内存配置（DEMO）。确定时写入共享内存状态，不持久化、不启动排样。
/// 默认值按 2026-08-15 用户实测确认：1360.00 / 0.00（无限长卷料）/ 6 层 / 补齐 / 边缘 0.00 / 间距 2.00。
/// </summary>
public sealed record BoardSettingsConfig(
    string Name,
    BoardDirection Direction,
    double WidthMm,
    double LengthMm,
    int Layers,
    string RemnantPolicy,
    double EdgeMm,
    double SpacingMm)
{
    public static BoardSettingsConfig Default { get; } = new(
        Name: "",
        Direction: BoardDirection.Vertical,
        WidthMm: 1360.00,
        LengthMm: 0.00,
        Layers: 6,
        RemnantPolicy: "补齐",
        EdgeMm: 0.00,
        SpacingMm: 2.00);

    /// <summary>确定后的状态栏配置摘要。</summary>
    public string Summary =>
        $"版型「{(string.IsNullOrWhiteSpace(Name) ? "未命名" : Name)}」{DirectionText} {WidthMm:0.00}×{LengthMm:0.00}mm " +
        $"{Layers}层 余片{RemnantPolicy} 边缘{EdgeMm:0.00} 间距{SpacingMm:0.00}";

    private string DirectionText => Direction == BoardDirection.Horizontal ? "横向" : "纵向";
}

/// <summary>共享内存配置状态（PRD R2「共享的内存配置状态」）。弹窗确定时写入，取消/非法输入不得改变。</summary>
public sealed class BoardSettingsStore
{
    public static BoardSettingsStore Default { get; } = new();

    public BoardSettingsConfig Current { get; private set; } = BoardSettingsConfig.Default;

    public bool IsConfirmed { get; private set; }

    public event EventHandler<BoardSettingsConfig>? Confirmed;

    public void Confirm(BoardSettingsConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        Current = config;
        IsConfirmed = true;
        Confirmed?.Invoke(this, config);
    }

    public void Reset()
    {
        Current = BoardSettingsConfig.Default;
        IsConfirmed = false;
    }
}
