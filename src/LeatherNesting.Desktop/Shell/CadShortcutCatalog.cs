using Avalonia.Input;

namespace LeatherNesting.Desktop.Shell;

/// <summary>CAD 画布快捷键命令，对应参照软件快捷键表（02-功能整理.md §8.3）。</summary>
public enum CadShortcutCommand
{
    Cancel,
    ManualNest,
    AreaArrayNest,
    AreaBlendNest,
    Undo,
    Redo,
    Cut,
    Copy,
    Paste,
    SelectAll,
    InvertSelection,
    Delete,
    Mirror,
    Group,
    Ungroup,
    ExportToOrder,
    GroupPieces,
    RotateLeft,
    RotateRight,
    Rotate90,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
}

/// <summary>One keyboard shortcut → CAD command mapping.</summary>
public sealed record CadShortcutBinding(
    Key Key,
    KeyModifiers Modifiers,
    CadShortcutCommand Command,
    string Label);

/// <summary>单一事实源：参照软件 §8.3 快捷键全表。</summary>
public static class CadShortcutCatalog
{
    private const KeyModifiers Ctrl = KeyModifiers.Control;
    private const KeyModifiers Shift = KeyModifiers.Shift;

    public static IReadOnlyList<CadShortcutBinding> Bindings { get; } = Array.AsReadOnly(
        new[]
        {
            new CadShortcutBinding(Key.Escape, KeyModifiers.None, CadShortcutCommand.Cancel, "取消"),
            new CadShortcutBinding(Key.F5, KeyModifiers.None, CadShortcutCommand.ManualNest, "手动排版"),
            new CadShortcutBinding(Key.F7, KeyModifiers.None, CadShortcutCommand.AreaArrayNest, "区域阵列排版"),
            new CadShortcutBinding(Key.F8, KeyModifiers.None, CadShortcutCommand.AreaBlendNest, "区域混合排版"),

            new CadShortcutBinding(Key.Z, Ctrl, CadShortcutCommand.Undo, "撤销"),
            new CadShortcutBinding(Key.Y, Ctrl, CadShortcutCommand.Redo, "返回"),
            new CadShortcutBinding(Key.X, Ctrl, CadShortcutCommand.Cut, "剪切"),
            new CadShortcutBinding(Key.C, Ctrl, CadShortcutCommand.Copy, "复制"),
            new CadShortcutBinding(Key.V, Ctrl, CadShortcutCommand.Paste, "粘贴"),

            new CadShortcutBinding(Key.A, Ctrl, CadShortcutCommand.SelectAll, "全选"),
            new CadShortcutBinding(Key.A, Shift, CadShortcutCommand.InvertSelection, "反选"),
            new CadShortcutBinding(Key.Delete, KeyModifiers.None, CadShortcutCommand.Delete, "删除"),

            new CadShortcutBinding(Key.M, Ctrl, CadShortcutCommand.Mirror, "镜像"),
            new CadShortcutBinding(Key.G, Ctrl, CadShortcutCommand.Group, "组合模块"),
            new CadShortcutBinding(Key.G, Shift, CadShortcutCommand.Ungroup, "取消组合"),
            new CadShortcutBinding(Key.T, Ctrl, CadShortcutCommand.ExportToOrder, "导到订单"),
            new CadShortcutBinding(Key.G, Ctrl | Shift, CadShortcutCommand.GroupPieces, "组合裁片"),

            new CadShortcutBinding(Key.A, KeyModifiers.None, CadShortcutCommand.RotateLeft, "向左旋转"),
            new CadShortcutBinding(Key.D, KeyModifiers.None, CadShortcutCommand.RotateRight, "向右旋转"),
            new CadShortcutBinding(Key.Space, KeyModifiers.None, CadShortcutCommand.Rotate90, "旋转90°"),

            new CadShortcutBinding(Key.Up, KeyModifiers.None, CadShortcutCommand.MoveUp, "向上移动"),
            new CadShortcutBinding(Key.Down, KeyModifiers.None, CadShortcutCommand.MoveDown, "向下移动"),
            new CadShortcutBinding(Key.Left, KeyModifiers.None, CadShortcutCommand.MoveLeft, "向左移动"),
            new CadShortcutBinding(Key.Right, KeyModifiers.None, CadShortcutCommand.MoveRight, "向右移动"),
        });

    /// <summary>在绑定表中查找与按键事件匹配的项；未命中返回 null。</summary>
    public static CadShortcutBinding? Find(Key key, KeyModifiers modifiers) =>
        Bindings.FirstOrDefault(binding =>
            binding.Key == key && Normalize(binding.Modifiers) == Normalize(modifiers));

    private static KeyModifiers Normalize(KeyModifiers value)
    {
        // 方向键/A/D/空格等无修饰符键：忽略 Meta（macOS Cmd 差异）以避免误判。
        return value & ~KeyModifiers.Meta;
    }
}
