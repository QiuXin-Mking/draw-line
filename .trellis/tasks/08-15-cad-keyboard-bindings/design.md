# Design: 快捷键真实绑定

## Architecture

```
CadShortcutCatalog（静态契约：§8.3 全表 → CadShortcutBinding { Keys, Command, Label }）
        │ 引用
        ▼
CadShortcutRouter（构造函数持有命令分发回调 Action<CadShortcutCommand>）
        │ HandleKeyDown(KeyEventArgs) → bool（已处理/未处理）
        ▼
CadWorkspaceHost（创建 router；CanvasView.Focusable=true；Drawing.KeyDown += router.HandleKeyDown）
        ▼ 分发
AppShellViewModel.ActivateContextCommand / CadToolbarController.TryExecute / CadHostState.ReportUnsupported
```

## Files

| 文件 | 变更 |
| --- | --- |
| `src/LeatherNesting.Desktop/Shell/CadShortcutCatalog.cs` | 新增静态契约：`IReadOnlyList<CadShortcutBinding>`，覆盖 §8.3 全表 |
| `src/LeatherNesting.Desktop/Shell/CadShortcutRouter.cs` | 新增：`HandleKeyDown(KeyEventArgs) → bool`，匹配 Keys 后调用分发回调 |
| `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs` | `Drawing.Focusable=true`；`Drawing.KeyDown += (s,e) => _router.HandleKeyDown(e)` |
| `tests/LeatherNesting.Desktop.Tests/Shell/CadShortcutRouterTests.cs` | 新测试 |

## Contract: CadShortcutBinding

- `record CadShortcutBinding(Key Key, KeyModifiers Modifiers, CadShortcutCommand Command, string Label)`
- `enum CadShortcutCommand` 覆盖 §8.3：`ManualNest(F5), AreaArrayNest(F7), AreaBlendNest(F8), Undo, Redo, Cut, Copy, Paste, SelectAll, InvertSelection, Delete, Mirror, Group, Ungroup, ExportToOrder, GroupPieces, RotateLeft, RotateRight, Rotate90, MoveUp, MoveDown, MoveLeft, MoveRight, Cancel(Esc)`。
- 映射表（§8.3 原文）：
  - Esc→Cancel；F5→ManualNest；F7→AreaArrayNest；F8→AreaBlendNest
  - Ctrl+Z→Undo；Ctrl+Y→Redo；Ctrl+X→Cut；Ctrl+C→Copy；Ctrl+V→Paste
  - Ctrl+A→SelectAll；Shift+A→InvertSelection；Del→Delete
  - Ctrl+M→Mirror；Ctrl+G→Group；Shift+G→Ungroup；Ctrl+T→ExportToOrder；Ctrl+Shift+G→GroupPieces
  - A→RotateLeft；D→RotateRight；Space→Rotate90；↑/↓/←/→→MoveUp/Down/Left/Right

## Router behavior

- `HandleKeyDown(KeyEventArgs e)`：解析 `e.Key` + `e.KeyModifiers`（用 `KeyGestures` 语义或手动匹配），在 catalog 中找绑定；命中 → 调 `_execute(command, label)` 返回 true（`e.Handled=true`）；未命中 → 返回 false（不吞键）。
- 分发回调由 `CadWorkspaceHost` 构造时注入：`command => _viewModel.ActivateContextCommand(...)` 或 `TryExecute`。MVP：统一走「已实现命令用工作台/接线位，其余 TODO」（复用 `AppShellViewModel` 现有路径）。
- 空格/方向键在 CAD 画布聚焦时绑定旋转/移动；CanvasView 无文本输入，无冲突。

## Compatibility & Rollback

- `CanvasView` 仅加 `Focusable=true` + 事件转发，不改渲染/交互。
- `CadWorkspaceHost` 新增字段，构造时创建 router；测试构造 `new CadWorkspaceHost(state)` 保持兼容（router 可选或默认）。
- 回滚 = 撤销新增 2 文件 + `CadWorkspaceHost` 改动。
- 无数据/持久化影响。

## Verification commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadShortcutRouter"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归）
