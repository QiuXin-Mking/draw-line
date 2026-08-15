# Design: CAD 画布右键菜单（21 项）

## Architecture

沿用顶部菜单「契约单一事实源 + View 构建 + ViewModel 路由」三层结构，新增一个 CAD 右键菜单契约与一条激活入口。

```
ShellContextMenu（静态契约，21 项）
        │ 引用于
        ▼
CadWorkspaceHost（构建 ContextMenu 挂到 Drawing.ContextMenu）
        │ 点击 → Action<ShellMenuCommand>
        ▼
AppShellViewModel.ActivateContextCommand（接线位，当前委托 ActivateMenuCommand：路由 M03 + ShowTodo）
```

## Files

| 文件 | 变更 |
| --- | --- |
| `src/LeatherNesting.Desktop/Shell/ShellTopCommands.cs` | 新增静态 `ShellContextMenu` 契约（21 个 `ShellMenuCommand`），复用既有 `ShellMenuCommand`/`ShellMenuEntry` 记录 |
| `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs` | 构造新增可选参 `Action<ShellMenuCommand>? activateContext = null`；构建 `ContextMenu` 挂到 `Drawing.ContextMenu`；`MenuItem` 构建镜像 `TopCommandArea.CreateCommandItem` |
| `src/LeatherNesting.Desktop/Shell/AppShellView.cs` | `CadWorkspace` 构造传 `_viewModel.ActivateContextCommand`（`AppShellView.cs:53`） |
| `src/LeatherNesting.Desktop/Shell/AppShellViewModel.cs` | 新增 `ActivateContextCommand(ShellMenuCommand)` 接线位方法（当前委托 `ActivateMenuCommand`） |
| `tests/LeatherNesting.Desktop.Tests/Shell/CadContextMenuTests.cs` | 新测试文件，`[Collection("Avalonia UI")]` |

## Contract: ShellContextMenu

- 静态类，`IReadOnlyList<ShellMenuEntry> Entries`，21 项按序、无分隔线（参照 §8.1 原样为扁平列表）。
- 全部 `TargetModuleId = "M03"`、`IsPlaceholderAction = true`、`NavigateToModule = true`。
- 置灰两项：`删除分界`、`粘贴（Ctrl+V）` → `IsEnabled = false`。
- 标签照抄用户文本（全角括号，与 §8.1 一致），术语不改写（保留「返回」「组合模块」「组合裁片」）。

## Activation seam（接线位）

`AppShellViewModel.ActivateContextCommand(ShellMenuCommand)`：

- 当前实现 = 校验非空 → 委托 `ActivateMenuCommand(command)`（路由 M03 + `ShowTodo`，诚实占位）。
- 方法内以 XML 注释文档化未来映射：
  - 撤销 → `Workbench.Undo()`
  - 返回 → `Workbench.Redo()`
  - 取消 → `Workbench.Cancel()` / `ClearSelection()`（依状态）
  - 移动 → `Workbench.MoveSelected(delta)`
  - 旋转 → `Workbench.RotateSelected(degrees)`
- 后续任务只需替换本方法体，无需触碰 View 层。

## ContextMenu 挂载

- Avalonia `Control.ContextMenu` 属性：设 `Drawing.ContextMenu = menu` 后，右键自动弹出；`CanvasView.OnPointerPressed` 仅处理左键（`CanvasView.cs:105` 先判左键），右键不受干扰。
- `MenuItem` 构建镜像 `TopCommandArea.CreateCommandItem`（`TopCommandArea.cs:157`）：`Header = label`、`Foreground = AppTheme.PrimaryText`、`IsEnabled` 透传、`Click` 回调 `activateContext`。
- `activateContext` 为空（既有测试 `new CadWorkspaceHost(state)` 路径）时仍构建完整菜单（标签/顺序/置灰可断言），Click 指向空安全委托（`_ => { }`），不抛异常。

## Compatibility & Rollback

- 兼容：`CadWorkspaceHost` 新增参数带默认值 `null`，现有 4 处测试构造与 `AppShellView` 均无需强制改动；`AppShellView.cs:53` 一处同步补参。
- 无数据/持久化影响，纯 UI 层；回滚 = 撤销对 4 个源文件的改动，不影响既有测试。
- 风险点：`ContextMenu` 的 `Items` 为 `IList`（非 `ItemsSource` 一次性绑定），用 `Items.Add` 逐个添加，避免 `ItemsSource` 与 `Items` 混用。

## Verification commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadContextMenu"`（新测试）
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归）
