# 恢复 CAD 面板可用性 — 技术设计

## 1. 根因

当前存在三个 CAD 表面，状态和可达性互相割裂：

1. Shell 常驻 `CadWorkspaceHost` 使用只读 `CadEvidenceCanvas`，工具大多进入 `ReportUnsupported`。
2. 已实现缩放/平移的 M03 `CadCanvasView` 会被创建，但 `AppShellView.RefreshModuleOverlay` 只显示 M02，所以 M03 永远不可见。
3. 真正连接 `CadOperationSession` 的 `CadWorkbenchView` 只藏在 M02 的二级标签内，且拥有独立 `CadWorkbenchViewModel`。

因此测试可以全绿，用户仍只能看到一个不能编辑的黑色画布。问题来自 Shell 复刻时以截图结构替换功能表面，而没有把已有交互能力重新接回常驻宿主。

## 2. 方案比较

### A. 在 Shell 上覆盖显示完整 M03 / Workbench 页面

改动最快，但会产生第二套工具栏、标尺、属性区和滚动容器，违反五区工作站约束，也继续保留两套状态。拒绝。

### B. 常驻 Shell 复用现有交互内核（采用）

让 `CadHostState` 成为唯一 Desktop CAD 会话适配器，内部持有 `CadWorkbenchViewModel`。`CadWorkspaceHost` 直接使用 `CanvasView`；`CadPropertyPane` 只把本轮承诺的控件接到同一会话。既保留截图结构，又恢复真实功能，改动范围可控。

### C. 重写统一 CAD 模块与持久化模型

长期最干净，但会同时牵涉项目文档、DXF 导出、图层模型和全部专业命令，不适合作为本次可用性修复。延期。

## 3. 组件边界

### `CadWorkbenchViewModel`

- 继续拥有 `CadOperationSession`、工具模式、选中对象和事务状态。
- 增加统一 `Changed` 通知；所有会影响画布、选择、按钮可用性、状态或诊断的方法只在状态真正变化后通知。
- 增加 `ClearSelection()`，并保证 `LoadLoops` 清理旧选择。
- `Commit`、`Cancel`、`Undo`、`Redo` 将底层结果转成稳定的状态与诊断；没有待预览/撤销项时不伪造成功。

### `CadHostState`

- Desktop 组合根仍只创建一个实例并同时传给 M02 与 Shell。
- 内部只创建一个 `CadWorkbenchViewModel Workbench`，`Loops` 投影 `Workbench.CurrentLoops`，不再维护第二份几何数组。
- `LoadConfirmedImport` 复制输入快照后调用 `Workbench.LoadLoops`；文件信息与单位确认语义保持不变。
- 订阅 Workbench 的 `Changed`，统一派生面向 Shell 的状态文本并转发 `Changed`。
- 提供 `ReportError(string message)` 供输入适配层报告解析错误；它只更新状态，不修改 Workbench 或几何。
- `Clear` 清空文件信息、会话、选择和诊断。

### `CanvasView` / `CadWorkspaceHost`

- `CadWorkspaceHost` 用 `CanvasView` 替换 `CadEvidenceCanvas`，因此获得现有滚轮缩放、空白拖拽平移、坐标反算、点击命中和拖动能力。
- `CanvasView` 增加可配置的背景和轮廓笔；旧工作台保留浅色默认值，常驻 Shell 注入 `AppTheme.CanvasBlack` 与语义几何色，避免深色画布上出现不可见的 Navy 轮廓。
- `CanvasView.Refit()` 只请求下一帧重新适配，不改变几何。
- 点击调用 `Workbench.SelectPiece`；拖动调用 `Workbench.MoveSelected`，结果仍处于预览，必须显式提交或取消。
- “范围缩放”接 `Refit`，“选择”切换选择模式；多段线、矩形、删除保持禁用 TODO。

### `CadPropertyPane`

- 保留参考截图的字段顺序与密度，但新增一个明确的“CAD 会话”操作区，展示当前状态/诊断。
- 接通：闭合轮廓、内缩/外扩（读取并校验距离和方向）、旋转 +15°、提交预览、取消预览、撤销、重做、清除选择。
- 输入解析失败只更新诊断，不调用几何命令。
- 其他尚未实现的按钮设置 `IsEnabled = false` 并带 TODO 提示；值输入只在对应真实命令存在时启用。
- 按钮可用性随 Workbench `Changed` 刷新，不使用构造时的一次性 `IsEnabled` 快照。

### `AppShellView`

- M02 仍以 overlay 负责 DXF 选择、检查和毫米确认。
- 确认后共享 `CadHostState` 装载会话，Shell 回到 M03，常驻主画布立即显示同一会话几何。
- M03 不再依靠隐藏的 `WorkspaceContent` 来提供功能；`CurrentModule == M03` 与常驻 CAD 宿主的真实功能保持一致。

## 4. 数据流

```text
M02 Inspect -> pending geometry (not visible)
M02 Confirm millimetres
  -> CadHostState.LoadConfirmedImport
  -> one CadWorkbenchViewModel.LoadLoops
  -> CadHostState.Changed
  -> CanvasView.SetData(CurrentLoops) + property/status refresh

Pointer / property input
  -> CadWorkbenchViewModel.Preview*/MoveSelected/RotateSelected
  -> CadOperationSession.Preview
  -> Changed -> redraw preview
  -> Commit | Cancel | Undo | Redo
  -> Changed -> redraw stable session state
```

几何唯一来源为 Workbench 的 `CurrentLoops`。视图只做输入适配与渲染，不保留独立业务副本。

## 5. 错误与状态规则

- 没有已确认几何：编辑命令禁用，画布显示选择 DXF 的引导。
- 没有选中对象：移动/旋转不改变几何，显示明确诊断。
- 数值无效或非有限数：拒绝预览，保留当前几何和事务状态。
- 预览失败：显示 `CadCommandResult.Diagnostics`，保持上一次稳定几何。
- 有待提交预览时：只允许提交或取消；避免叠加第二个预览命令。
- “提交”文案明确为“提交到 CAD 会话”，不暗示项目已保存。

## 6. 测试策略

- ViewModel：通知、选择清理、无效事务、预览/提交/取消/撤销/重做状态。
- Host state：确认导入只建立一个 Workbench 会话，几何投影不漂移，清空完整复位。
- Shell/控件：主画布类型为 `CanvasView`；真实按钮改变会话；TODO 按钮禁用；按钮状态随事件刷新。
- 集成：M02 确认 -> M03 常驻画布使用相同 loop；偏移或拖动 -> 预览 -> 提交 -> 撤销。
- 原生 1366×768：验证滚轮、平移、选择高亮、拖动预览、参数预览、提交/取消/撤销/重做，以及右侧核心控件可见可点。

## 7. 兼容、回滚与延期

- 不改 DXF reader/writer、几何算法或 `ProjectDocument` schema，避免与并行 DXF 工作冲突。
- 保留 `CadCanvasView` 与 `CadWorkbenchView`，直到后续任务决定合并或删除；本轮只停止依赖它们作为主入口。
- 回滚可按三层进行：共享会话、Shell 画布接线、属性面板命令接线；每层都有独立契约测试。
- 编辑持久化、编辑后 DXF 导出、框选/多选、绘制多段线/矩形、删除、图层持久化和复杂曲线编辑延期。
