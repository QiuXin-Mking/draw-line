# Implement: CAD 画布右键菜单（21 项）

## Order

1. **契约**：`src/LeatherNesting.Desktop/Shell/ShellTopCommands.cs` 新增静态 `ShellContextMenu`，`Entries` 含 21 个 `ShellMenuCommand`（见 `prd.md` 需求 1 的完整标签表）。
2. **路由接线位**：`src/LeatherNesting.Desktop/Shell/AppShellViewModel.cs` 新增 `ActivateContextCommand(ShellMenuCommand)`，委托 `ActivateMenuCommand` + XML 注释文档化未来工作台映射。
3. **挂载**：`src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs` 构造新增 `Action<ShellMenuCommand>? activateContext = null`；构建 `ContextMenu` 挂到 `Drawing.ContextMenu`，`MenuItem` 构建镜像 `TopCommandArea.CreateCommandItem`；空激活时用空安全委托。
4. **接线**：`src/LeatherNesting.Desktop/Shell/AppShellView.cs:53` 传 `_viewModel.ActivateContextCommand`。
5. **测试**：新增 `tests/LeatherNesting.Desktop.Tests/Shell/CadContextMenuTests.cs`。

## Test coverage（CadContextMenuTests.cs）

- `CTX-001` 契约：`ShellContextMenu.Entries` 21 项、标签与顺序与需求 1 一致、无分隔线。
- `CTX-002` 置灰：`删除分界`、`粘贴（Ctrl+V）` 两项 `IsEnabled == false`，其余 `true`。
- `CTX-003` 挂载：`new CadWorkspaceHost(state)` 后 `Drawing.ContextMenu` 非空、`Items` 21 个 `MenuItem`、顺序一致、置灰透传、`Header`/`Foreground` 正确。
- `CTX-004` 激活：用 `AppShellViewModel` + `InMemoryWorkspaceSession`（仿 `TopCommandAreaTests` TOP-004），点击某项 → 路由 M03 + `TodoHint` 含标签与 `TodoBadge.StandardText`。
- `CTX-005` 接线位存在性：`ActivateContextCommand` 对任一命令与 `ActivateMenuCommand` 行为一致（委托路径不抛异常）。

## Validation commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadContextMenu"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归，确认既有 `TopCommandArea`/`CadHostEvidence`/`CloneSurfaceColor` 测试仍绿）

## Risky files / rollback points

- `CadWorkspaceHost.cs`：新增参数默认 `null`，4 处既有测试构造路径不破坏；若 Avalonia 构建报错，回滚点 = 移除 `Drawing.ContextMenu` 赋值。
- `ShellTopCommands.cs`：仅追加静态契约，不改既有 `ShellTopMenu`/`ShellToolbar`。
- 无持久化/数据层改动。

## Follow-up checks before task.py start

- [ ] `prd.md` 已过收敛检查，无重复事实、无未决阻断问题。
- [ ] 用户已明确批准本实现计划。
- [ ] 全部验证命令通过后再 `task.py start`。
