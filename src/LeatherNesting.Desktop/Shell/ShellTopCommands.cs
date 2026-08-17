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
            new ShellMenuCommand("新建排版", "M01", false, Launch: ShellCommandLaunch.NewBoardSettings),
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

    /// <summary>「设置」下拉菜单：范围缩放/窗口/参数类设置；「语言」为带子菜单的选择项（中文/英文）。</summary>
    public static IReadOnlyList<ShellMenuEntry> SettingsMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("范围缩放", "M03", true),
            new ShellMenuCommand("订单窗口", "M01", false, NavigateToModule: false, Launch: ShellCommandLaunch.ToggleOrderWindow),
            new ShellMenuCommand("设置窗口", "M12", true),
            new ShellMenuCommand("排样设置", "M08", true),
            new ShellMenuCommand("发送设置", "M11", true),
            new ShellMenuCommand("导入参数", "M02", true),
            new ShellMenuCommand("导出参数", "M11", true),
            new ShellMenuSubmenu("语言", new ShellMenuEntry[]
            {
                new ShellMenuCommand("中文", "M03", true, NavigateToModule: false),
                new ShellMenuCommand("英文", "M03", true, NavigateToModule: false),
            }),
        });

    /// <summary>「帮助」下拉菜单：系统/授权/关于，均为占位，路由兜底到 M12（管理）。</summary>
    public static IReadOnlyList<ShellMenuEntry> HelpMenu { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("系统配置", "M12", true),
            new ShellMenuCommand("软件授权", "M12", true),
            new ShellMenuCommand("关于...", "M12", true),
        });

    /// <summary>返回指定顶级菜单的内容；未实现的菜单返回 null（渲染为占位）。</summary>
    public static IReadOnlyList<ShellMenuEntry>? EntriesFor(string label) =>
        StringComparer.Ordinal.Equals(label, "文件") ? FileMenu :
        StringComparer.Ordinal.Equals(label, "编辑") ? EditMenu :
        StringComparer.Ordinal.Equals(label, "操作") ? OperationMenu :
        StringComparer.Ordinal.Equals(label, "绘制") ? DrawMenu :
        StringComparer.Ordinal.Equals(label, "数据库") ? DatabaseMenu :
        StringComparer.Ordinal.Equals(label, "工具") ? ToolsMenu :
        StringComparer.Ordinal.Equals(label, "设置") ? SettingsMenu :
        StringComparer.Ordinal.Equals(label, "帮助") ? HelpMenu :
        null;
}

/// <summary>G 区 CAD 画布右键菜单契约，21 项按参照软件 AXTNester 实测顺序排列（见 02-功能整理.md §8.1）。
/// 标签照抄原文（含全角括号与「返回/组合模块/组合裁片」术语）；「删除分界」「粘贴」置灰。
/// 快捷键仅作标签提示，本层不实现全局按键绑定。</summary>
public static class ShellContextMenu
{
    public static IReadOnlyList<ShellMenuEntry> Entries { get; } = Array.AsReadOnly(
        new ShellMenuEntry[]
        {
            new ShellMenuCommand("手动排版（F5）", "M03", true),
            new ShellMenuCommand("添加分界", "M03", true),
            new ShellMenuCommand("删除分界", "M03", true, IsEnabled: false),
            new ShellMenuCommand("撤销（Ctrl+Z）", "M03", true),
            new ShellMenuCommand("返回（Ctrl+Y）", "M03", true),
            new ShellMenuCommand("取消（Esc）", "M03", true),
            new ShellMenuCommand("移动", "M03", true),
            new ShellMenuCommand("旋转", "M03", true),
            new ShellMenuCommand("剪切（Ctrl+X）", "M03", true),
            new ShellMenuCommand("复制（Ctrl+C）", "M03", true),
            new ShellMenuCommand("粘贴（Ctrl+V）", "M03", true, IsEnabled: false),
            new ShellMenuCommand("全选（Ctrl+A）", "M03", true),
            new ShellMenuCommand("反选（Shift+A）", "M03", true),
            new ShellMenuCommand("删除（Del）", "M03", true),
            new ShellMenuCommand("删除外部", "M03", true),
            new ShellMenuCommand("清空全部", "M03", true),
            new ShellMenuCommand("镜像（Ctrl+M）", "M03", true),
            new ShellMenuCommand("组合模块（Ctrl+G）", "M03", true),
            new ShellMenuCommand("取消组合（Shift+G）", "M03", true),
            new ShellMenuCommand("导到订单（Ctrl+T）", "M03", true),
            new ShellMenuCommand("组合裁片（Ctrl+Shift+G）", "M03", true),
        });
}

/// <summary>顶级菜单条目：命令、分隔线或子菜单。</summary>
public abstract record ShellMenuEntry;

/// <summary>shell 命令的落点：导航到模块，或打开独立对话框。</summary>
public enum ShellCommandLaunch
{
    Module,
    NewBoardSettings,
    ToggleOrderWindow,
}

public sealed record ShellMenuCommand(
    string Label,
    string TargetModuleId,
    bool IsPlaceholderAction,
    bool IsEnabled = true,
    bool NavigateToModule = true,
    ShellCommandLaunch Launch = ShellCommandLaunch.Module) : ShellMenuEntry;

public sealed record ShellMenuSeparator : ShellMenuEntry;

public sealed record ShellMenuSubmenu(
    string Label,
    IReadOnlyList<ShellMenuEntry> Children) : ShellMenuEntry;

public sealed record ShellToolbarCommand(
    string Label,
    ToolbarIconKey Icon,
    string TargetModuleId,
    bool IsPlaceholderAction,
    ShellCommandLaunch Launch = ShellCommandLaunch.Module);

/// <summary>Single source of truth for the icon toolbar order and shell navigation routes.</summary>
public static class ShellToolbar
{
    public static IReadOnlyList<ShellToolbarCommand> Commands { get; } = Array.AsReadOnly(
        new[]
        {
            new ShellToolbarCommand("新建排版", ToolbarIconKey.NewLayout, "M01", false, Launch: ShellCommandLaunch.NewBoardSettings),
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
