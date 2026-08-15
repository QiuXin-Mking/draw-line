# CAD 画布右键菜单（21 项）

## Goal

在 G 区（CAD 画布）实现与参照软件 AXTNester 一致的右键上下文菜单，共 21 项、按固定顺序排列，含快捷键提示与置灰状态；激活方式与顶部菜单一致（路由到 CAD + TODO 占位提示，保持诚实，不伪造成功）。

## Confirmed Facts（代码库证据）

- **目标宿主**：G 区画布是 `CadWorkspaceHost` 内常驻显示的 `CanvasView`（`src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs:23`，`Drawing` 属性），位于 shell 中心 `CanvasSurface`，始终可见；shell 默认模块即 M03（`AppShellView.cs:88`）。
- **无既有右键菜单**：全仓 `grep ContextMenu` 零命中；`CanvasView` 仅处理左键按下/移动/释放（`src/LeatherNesting.Desktop/Views/CanvasView.cs:105`）。
- **可复用命令模型**：`ShellMenuCommand`（Label / TargetModuleId / IsPlaceholderAction / IsEnabled / NavigateToModule）、`ShellMenuSeparator` 定义在 `src/LeatherNesting.Desktop/Shell/ShellTopCommands.cs:122`；`AppShellViewModel.ActivateMenuCommand`（`AppShellViewModel.cs:87`）负责路由 + `ShowTodo` 占位提示。
- **「编辑」菜单已含本批命令的大部分**（撤销/回撤/剪切/复制/粘贴/全选/反选/删除/删除外部/清空全部/镜像/组合/取消组合/导到订单），当前全部为占位动作（`ShellTopCommands.cs:25`）。
- **工作台已有真实逻辑**（`src/LeatherNesting.Desktop/ViewModels/CadWorkbenchViewModel.cs`）：`Undo/Redo/Cancel/MoveSelected/RotateSelected/SelectPiece/ClearSelection`；但菜单层目前不接线，统一走 TODO。
- **UI 全代码构建**（无 .axaml），上下文菜单应仿照 `TopCommandArea.CreateCommandItem` 以代码构建 `MenuItem`。
- **测试模式**：`tests/LeatherNesting.Desktop.Tests/Shell/TopCommandAreaTests.cs` 以 `[Collection("Avalonia UI")]` + `[Trait("TestId", ...)]` 覆盖菜单契约/激活路由。

## Requirements

1. 在 G 区 CAD 画布（`CadWorkspaceHost.Drawing`）右击弹出上下文菜单，21 项按下列顺序排列：
   1. 手动排版（F5）
   2. 添加分界
   3. 删除分界（置灰）
   4. 撤销（Ctrl+Z）
   5. 返回（Ctrl+Y）
   6. 取消（Esc）
   7. 移动
   8. 旋转
   9. 剪切（Ctrl+X）
   10. 复制（Ctrl+C）
   11. 粘贴（Ctrl+V，置灰）
   12. 全选（Ctrl+A）
   13. 反选（Shift+A）
   14. 删除（Del）
   15. 删除外部
   16. 清空全部
   17. 镜像（Ctrl+M）
   18. 组合模块（Ctrl+G）
   19. 取消组合（Shift+G）
   20. 导到订单（Ctrl+T）
   21. 组合裁片（Ctrl+Shift+G）
2. 标签与快捷键提示照抄用户提供文本（即 `02-功能整理.md` §8.1 原文，含「返回」「组合模块」「组合裁片」术语，不改写为编辑菜单的「回撤/组合」）。
3. 快捷键仅作标签提示（`撤销(Ctrl+Z)` 形式，与编辑菜单一致），本任务不实现全局按键绑定。
4. 「删除分界」「粘贴」两项置灰禁用；其余项可用。
5. 激活行为与顶部菜单一致：路由到 M03 + `ShowTodo` 占位提示（诚实 TODO），不在本任务伪造成功。
6. **预留接线位**：为工作台已支持的命令（撤销→Undo、返回→Redo、取消→Cancel/ClearSelection、移动→MoveSelected、旋转→RotateSelected）留出单一激活方法作为接线位，方法内注释文档化映射，但当前仍走 TODO 占位；真实接线由后续任务完成。
7. 新增菜单契约单一事实源（仿 `ShellTopMenu`），供 View 构建与测试复用。

## Acceptance Criteria

- [ ] AC-1：右击 G 区画布弹出上下文菜单，21 项标签与顺序与需求 1 完全一致。
- [ ] AC-2：「删除分界」「粘贴」两项 `IsEnabled == false`，其余可用。
- [ ] AC-3：点击任意可用项：当前模块路由到 M03，状态栏出现含该项标签与 `TodoBadge.StandardText` 的 TODO 提示（不伪造成功）。
- [ ] AC-4：新增契约类（仿 `ShellTopMenu`）为唯一数据源；`CadWorkspaceHost` 用它构建 `ContextMenu`。
- [ ] AC-5：新增测试（仿 `TopCommandAreaTests`）覆盖：21 项顺序/标签、置灰项、激活路由到 M03 + TODO 提示。
- [ ] AC-6：`dotnet test` 全绿，无新增警告。

## Out of Scope

- 真实接线工作台逻辑（撤销/返回/取消/移动/旋转/删除等真正改数据）——本任务只预留接线位（单一激活方法 + 注释文档化映射），不调用工作台。
- 全局键盘快捷键绑定（仅显示提示文本）。
- 排样组右键菜单（C 区）等其他右键菜单。
- M03 模块视图 `CadCanvasView`（当前在 shell 中不渲染，非 G 区宿主）。

## Resolved Decisions

- 激活行为：**全部 TODO 占位 + 预留接线位**（用户已定）。21 项点击后路由到 M03 + 状态栏 TODO；为工作台已支持的命令（撤销/返回/取消/移动/旋转）预留单一激活方法作为接线位，当前不接真实逻辑。
