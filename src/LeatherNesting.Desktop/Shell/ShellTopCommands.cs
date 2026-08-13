using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Stable top-level menu contract matching the desktop reference information architecture.</summary>
public static class ShellTopMenu
{
    public const string PlaceholderText = "TODO · 菜单内容待补充";

    public static IReadOnlyList<string> Labels { get; } = Array.AsReadOnly(
        new[] { "文件", "编辑", "操作", "绘制", "数据库", "工具", "设置", "帮助" });
}

public sealed record ShellToolbarCommand(
    string Label,
    ToolbarIconKey Icon,
    string TargetModuleId,
    bool IsPlaceholderAction);

/// <summary>Single source of truth for the icon toolbar order and shell navigation routes.</summary>
public static class ShellToolbar
{
    public static IReadOnlyList<ShellToolbarCommand> Commands { get; } = Array.AsReadOnly(
        new[]
        {
            new ShellToolbarCommand("新建排版", ToolbarIconKey.NewLayout, "M01", true),
            new ShellToolbarCommand("订单管理", ToolbarIconKey.OrderManagement, "M01", false),
            new ShellToolbarCommand("CAD工具", ToolbarIconKey.CadTools, "M02", false),
            new ShellToolbarCommand("开始排版", ToolbarIconKey.StartNesting, "M08", true),
            new ShellToolbarCommand("停止排版", ToolbarIconKey.StopNesting, "M08", true),
            new ShellToolbarCommand("取消排版", ToolbarIconKey.CancelNesting, "M08", true),
            new ShellToolbarCommand("范围缩放", ToolbarIconKey.ZoomExtent, "M03", false),
            new ShellToolbarCommand("设置窗口", ToolbarIconKey.SettingsWindow, "M12", false),
            new ShellToolbarCommand("等宽长条", ToolbarIconKey.EqualWidthStrip, "M05", true),
            new ShellToolbarCommand("发送切割", ToolbarIconKey.SendCut, "M11", true),
        });
}
