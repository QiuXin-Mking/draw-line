# Implement: 快捷键真实绑定

## Order

1. **契约**：新增 `src/LeatherNesting.Desktop/Shell/CadShortcutCatalog.cs`：`CadShortcutCommand` 枚举 + `CadShortcutBinding(Key, KeyModifiers, Command, Label)` + 静态 `Bindings`（§8.3 全表）。
2. **路由**：新增 `src/LeatherNesting.Desktop/Shell/CadShortcutRouter.cs`：构造接收 `Action<CadShortcutCommand, string>`（命令 + 标签），`HandleKeyDown(KeyEventArgs)` 匹配并分发，返回是否处理。
3. **接入**：`CadWorkspaceHost.cs`：`Drawing.Focusable = true`；构造时创建 router（回调把命令映射到 `AppShellViewModel` 路径或 `TryExecute`）；`Drawing.KeyDown += (s,e) => _router.HandleKeyDown(e)`。
4. **测试**：新增 `tests/LeatherNesting.Desktop.Tests/Shell/CadShortcutRouterTests.cs`。

## Command dispatch mapping（MVP）

- `Undo/Redo` → `AppShellViewModel.ActivateContextCommand` 对应 ShellMenuCommand（或工作台接线位）。
- `Cancel` → `CadToolCommandKey.Cancel`（`TryExecute` 或工作台 `Cancel`）。
- 其余（Cut/Copy/Paste/SelectAll/InvertSelection/Delete/Mirror/Group/Ungroup/ExportToOrder/GroupPieces/Rotate*/Move*/ManualNest/Area*Nest）→ 路由 M03 + TODO 占位（`ShowTodo`/`ReportUnsupported`），诚实不伪造。
- MVP 简化：`CadShortcutRouter` 只做「键→命令」匹配与 `e.Handled`；实际命令分发由注入回调决定，回调内部对齐现有菜单/工具占位逻辑。

## Test coverage（CadShortcutRouterTests.cs）

- `KEY-001` 契约：`CadShortcutCatalog.Bindings` 覆盖 §8.3 全部 22+ 项（含键+修饰符），无重复键绑定。
- `KEY-002` 命中：构造 `CadShortcutRouter` + 记录回调，`HandleKeyDown(Ctrl+Z)` 触发 `Undo` 且返回 true；Esc 触发 `Cancel`。
- `KEY-003` 未命中：无关键（如 F1）返回 false 且不触发回调。
- `KEY-004` 修饰符区分：`Ctrl+A` 命中 `SelectAll`，裸 `A` 命中 `RotateLeft`（不混淆）。
- `KEY-005` 接入：`new CadWorkspaceHost(state)` 后 `Drawing.Focusable == true`，`KeyDown` 事件订阅存在。

## Validation commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadShortcutRouter"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归，重点 RUL/AXIS/CAD-HOST/FRAME）

## Risky files / rollback points

- `CadWorkspaceHost.cs`：新增 router 字段 + KeyDown 转发；若与既有坐标提示/右键菜单事件冲突，回滚 = 移除转发但保留 catalog/router。
- `CanvasView.cs`：仅 `Focusable=true`，最小改动。
- 空格/方向键绑定的冲突面：A/D/空格/方向键仅在 CAD 画布聚焦生效，不拦截左键拖拽/滚轮。

## Follow-up checks before task.py start

- [ ] `prd.md` 已过收敛检查，无未决阻断问题。
- [ ] 用户已批准本实现计划。
- [ ] 验证命令全绿后再 `task.py start`。
