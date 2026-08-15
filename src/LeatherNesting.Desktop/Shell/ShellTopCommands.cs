using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Stable top-level menu contract matching the desktop reference information architecture.</summary>
public static class ShellTopMenu
{
    public const string PlaceholderText = "待补充";

    public static IReadOnlyList<string> Labels { get; } = Array.AsReadOnly(
        new[] { "文件", "编辑", "操作", "绘制", "数据库", "工具", "设置", "帮助" });

    /// <summary>「文件」下拉菜单命令，与图标工具栏共用同一套 shell 导航（模块 + 占位提示）。</summary>
    public static IReadOnlyList<ShellMenuEntry> FileMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("新建排版", "M01", true),
            new ShellMenuCommand("导入排版进度(axn)", "M02", true),
            new ShellMenuCommand("导出排版进度(axn)", "M11", true),
            new ShellMenuCommand("恢复数据", "M12", true),
            new ShellMenuCommand("导出统计报表", "M11", true),
        });

    /// <summary>「编辑」下拉菜单：动作均为占位，快捷键写在标签内以对齐参照软件（Esc/Del 保留原名）。</summary>
    public static IReadOnlyList<ShellMenuEntry> EditMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("撤销(Ctrl+Z)", "M03", true),
            new ShellMenuCommand("回撤(Ctrl+Y)", "M03", true),
            new ShellMenuSeparator(),
            new ShellMenuCommand("剪切(Ctrl+X)", "M03", true),
            new ShellMenuCommand("复制(Ctrl+C)", "M03", true),
            new ShellMenuCommand("粘贴(Ctrl+V)", "M03", true),
            new ShellMenuSeparator(),
            new ShellMenuCommand("全选(Ctrl+A)", "M03", true),
            new ShellMenuCommand("反选(Shift+A)", "M03", true),
            new ShellMenuCommand("按类型选择", "M03", true),
            new ShellMenuCommand("取消选择(Esc)", "M03", true),
            new ShellMenuSeparator(),
            new ShellMenuCommand("删除(Del)", "M03", true),
            new ShellMenuCommand("删除外部", "M03", true),
            new ShellMenuCommand("清空全部", "M03", true),
            new ShellMenuCommand("镜像(Ctrl+M)", "M03", true),
            new ShellMenuCommand("组合(Ctrl+G)", "M03", true),
            new ShellMenuCommand("取消组合(Shift+G)", "M03", true),
            new ShellMenuSeparator(),
            new ShellMenuCommand("导到订单(Ctrl+T)", "M03", true),
        });

    /// <summary>「操作」下拉菜单：与参考软件 A 区「操作」菜单一致；「测试项」置灰不可用。</summary>
    public static IReadOnlyList<ShellMenuEntry> OperationMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("开始排版", "M08", true),
            new ShellMenuCommand("停止排版", "M08", true),
            new ShellMenuCommand("取消排版", "M08", true),
            new ShellMenuCommand("等宽长条", "M05", true),
            new ShellMenuCommand("更新皮料", "M07", true),
            new ShellMenuCommand("发送切割", "M11", true),
            new ShellMenuCommand("测试项", "M08", true, IsEnabled: false),
        });

    /// <summary>「绘制」下拉菜单：CAD 画布作图命令，路由到 M03。</summary>
    public static IReadOnlyList<ShellMenuEntry> DrawMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("绘制孔", "M03", true),
            new ShellMenuCommand("绘制线", "M03", true),
        });

    /// <summary>「数据库」下拉菜单：订单管理路由到 M01（项目与订单）。</summary>
    public static IReadOnlyList<ShellMenuEntry> DatabaseMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("订单管理", "M01", true),
        });

    /// <summary>「工具」下拉菜单：CAD 工具路由到 M02，统计报表路由到 M11（导出），其余为占位。</summary>
    public static IReadOnlyList<ShellMenuEntry> ToolsMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("CAD工具", "M02", true),
            new ShellMenuCommand("实时投影", "M03", true),
            new ShellMenuCommand("实时看板", "M03", true),
            new ShellMenuCommand("统计报表", "M11", true),
            new ShellMenuCommand("串口工具", "M03", true),
        });

    /// <summary>返回指定顶级菜单的内容；未实现的菜单返回 null（渲染为占位）。</summary>
    public static IReadOnlyList<ShellMenuEntry>? EntriesFor(string label) =>
        StringComparer.Ordinal.Equals(label, "文件") ? FileMenu :
        StringComparer.Ordinal.Equals(label, "编辑") ? EditMenu :
        StringComparer.Ordinal.Equals(label, "操作") ? OperationMenu :
        StringComparer.Ordinal.Equals(label, "绘制") ? DrawMenu :
        StringComparer.Ordinal.Equals(label, "数据库") ? DatabaseMenu :
        StringComparer.Ordinal.Equals(label, "工具") ? ToolsMenu :
        null;
}

/// <summary>顶级菜单条目：命令或分隔线。</summary>
public abstract record ShellMenuEntry;

public sealed record ShellMenuCommand(
    string Label,
    string TargetModuleId,
    bool IsPlaceholderAction,
    bool IsEnabled = true) : ShellMenuEntry;

public sealed record ShellMenuSeparator : ShellMenuEntry;

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
