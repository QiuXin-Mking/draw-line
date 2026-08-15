# 快捷键真实绑定（§8.3 快捷键表）

## Goal

把参照软件 AXTNester 的快捷键表（`02-功能整理.md` §8.3，含 F5/F7/F8、Ctrl+Z/Y/X/C/V/A、Shift+A、Del、Ctrl+M/G/T、Shift+G、Ctrl+Shift+G、A/D、空格、方向键）从「仅写在菜单/工具标签里的提示文本」升级为**真实按键绑定**：在 G 区 CAD 画布聚焦时按下快捷键即触发对应命令（路由到 M03 + TODO 占位，或已实现的真实逻辑）。

## Confirmed Facts（代码库证据）

- **无键盘事件基础设施**：全仓 `grep KeyBinding/KeyDown/OnKeyDown/HotKey` 零命中。快捷键仅以文本形式写在标签里（如「撤销(Ctrl+Z)」「导到订单 Ctrl+T」），无实际按键触发。
- **快捷键文本分布**：
  - `ShellTopCommands.cs` 编辑菜单（`ShellTopCommands.cs:25-48`）：撤销(Ctrl+Z)/回撤(Ctrl+Y)/剪切(Ctrl+X)/复制(Ctrl+C)/粘贴(Ctrl+V)/全选(Ctrl+A)/反选(Shift+A)/取消选择(Esc)/删除(Del)/镜像(Ctrl+M)/组合(Ctrl+G)/取消组合(Shift+G)/导到订单(Ctrl+T)。
  - `ShellContextMenu`（`ShellTopCommands.cs:130-162`）：21 项含 F5/Ctrl+Z/Y/X/C/V/A/Shift+A/Esc/Del/Ctrl+M/G/T/Shift+G/Ctrl+Shift+G（全角括号文本）。
  - `CadToolCatalog`（`src/LeatherNesting.Desktop/Modules/CadCanvas/Toolbar/CadToolCatalog.cs`）：`CadToolDefinition.Shortcut` 已带部分快捷键——Ctrl+T(导到订单, 12-13)、Esc(鼠标选择, 14-15)、Ctrl+Z(撤销, 60-61)、Ctrl+Y(重做, 62-63)。
- **命令分发现有统一入口**：`CadToolbarController.TryExecute(CadToolCommandKey)`（`CadToolbarController.cs:28-48`）路由 Undo/Redo/Cancel/Delete/Refit/ExportToOrder 等；`CadToolbarState` 管理可用性（Undo/Redo/Selection/PendingStep）。**但 `CadToolbarController` 无任何挂载点**（全仓搜索仅定义无使用），未接入 G 区或模块 UI。
- **命令键缺口**：§8.3 的 A/D（左右旋转）、空格（旋转90°）、方向键（移动）、F7（区域阵列）、F8（区域混合）、Shift+A（反选）、Ctrl+M（镜像）、Ctrl+G（组合）、Shift+G（取消组合）、Ctrl+Shift+G（组合裁片）、Ctrl+X/C/V（剪切/复制/粘贴）**不在 `CadToolCommandKey` 枚举中**（`CadToolDefinition.cs:4-32`）。
- **现有命令路由**：`AppShellViewModel.ActivateContextCommand`（`AppShellViewModel.cs`，CAD 右键菜单接线位，当前委托 `ActivateMenuCommand` 路由 M03 + TODO）、`CadHostState.ReportUnsupported`（占位提示）。
- **工作台已有真实逻辑**：`CadWorkbenchViewModel` 的 `Undo/Redo/Cancel/RotateSelected/MoveSelected`（`CadWorkbenchViewModel.cs`）。
- **G 区宿主**：`CadWorkspaceHost`（`Shell/CadWorkspaceHost.cs`）持有 `Drawing`（CanvasView）与 `Axes`；CanvasView 是聚焦目标。

## Requirements

1. **真实按键绑定**：G 区画布（`CadWorkspaceHost.Drawing`）获得键盘焦点时，以下快捷键触发对应命令（§8.3 表）：
   - Esc → 取消当前命令（`CadToolCommandKey.Cancel`，已有 `HandleEscape`）
   - F5 → 手动排版；F7 → 区域阵列排版；F8 → 区域混合排版
   - Ctrl+Z → 撤销；Ctrl+Y → 返回/重做；Ctrl+X → 剪切；Ctrl+C → 复制；Ctrl+V → 粘贴
   - Ctrl+A → 全选；Shift+A → 反选；Del → 删除
   - Ctrl+M → 镜像；Ctrl+G → 组合模块；Shift+G → 取消组合；Ctrl+T → 导到订单；Ctrl+Shift+G → 组合裁片
   - A → 向左旋转；D → 向右旋转；空格 → 旋转90°；方向键 → 移动
2. **命令路由**：已实现的命令（撤销/返回/取消/移动/旋转）走工作台真实逻辑（经 `CadWorkbenchViewModel` 或既有 `ActivateContextCommand` 接线位）；未实现的命令走「路由 M03 + TODO 占位」（`ShowTodo`/`ReportUnsupported`），诚实不伪造。
3. **快捷键表单一事实源**：把 §8.3 全表建模为契约（仿 `ShellContextMenu`/`CadToolCatalog`），供键盘路由、菜单/工具标签、测试复用。与既有 `CadToolDefinition.Shortcut` 合并或对齐，避免重复定义。
4. **聚焦管理**：`CanvasView` 可聚焦（`Focusable=true`），按键在画布聚焦时生效；`KeyDown` 处理不吞掉其他控件的正常键盘行为。
5. **不改变现有缩放/平移/选择交互**：方向键/A/D/空格绑定旋转/移动时，不干扰滚轮缩放与左键拖拽平移。

## Acceptance Criteria

- [ ] AC-1：G 区画布聚焦时按下 §8.3 任一快捷键，触发对应命令（已有 `CadToolCommandKey` 的命令经 `TryExecute`，其余经统一路由层）。
- [ ] AC-2：快捷键表契约（单一事实源）覆盖 §8.3 全部项，含快捷键与命令的映射。
- [ ] AC-3：未实现命令显示诚实 TODO（`ShowTodo`/`ReportUnsupported`），已实现命令（撤销/返回/取消/移动/旋转）走真实逻辑或接线位。
- [ ] AC-4：`CanvasView` 可聚焦，`KeyDown` 处理不破坏滚轮缩放/左键拖拽选择/坐标提示。
- [ ] AC-5：新增测试覆盖：快捷键→命令映射、聚焦触发、未实现 TODO、已实现命令路由。
- [ ] AC-6：`dotnet test` 全绿，无新增警告。

## Out of Scope

- 改动 `CanvasView` 缩放/平移/选择交互逻辑。
- 修改菜单/工具标签文本（快捷键显示保持现状，仅新增真实绑定）。
- 设置窗口内的快捷键自定义编辑（§8.3 是参照软件的只读快捷键表）。
- 全局（非 CAD 画布聚焦时）快捷键。

## Resolved Decisions

- 路由归属：**独立 `CadShortcutRouter`**（用户已定）。持有快捷键→命令映射契约表 + 命令分发回调，挂到 `CadWorkspaceHost`；`CanvasView` 仅设 `Focusable=true` 并转发 `KeyDown`。
- 冲突规避：方向键/A/D/空格绑定旋转/移动时，路由仅在 `CanvasView` 已聚焦时生效；不拦截左键拖拽平移（`PointerPressed` 已捕获）与滚轮缩放（`OnPointerWheelChanged` 单独处理）。空格若与按钮激活冲突，优先绑定旋转90°（参照 §8.3）。
