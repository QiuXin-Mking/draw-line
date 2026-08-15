# CAD 交互功能：动态标尺 + 坐标提示 + 缩放联动

## Goal

让 G 区 CAD 画布的标尺与坐标显示「活」起来：垂直/水平标尺改为自绘控件，随画布缩放（滚轮）与平移实时更新刻度；画布左上角显示鼠标所在位置的模型 x/y 坐标（红色）。当前标尺是静态死文字，被用户判定为不达标。

## Confirmed Facts（代码库证据）

- **标尺现状（死文字）**：`AppShellView.cs:235-260` 的 `BuildVerticalRuler`/`BuildHorizontalRuler` 是静态 `Border` + `TextBlock`，写死 `"0\n\n100\n\n200..."` 文本，完全不感知画布缩放/平移。用户明确反馈「标尺很异常，是死的」。
- **画布缩放/平移已存在**：`src/LeatherNesting.Desktop/Views/CanvasView.cs` 已实现滚轮缩放（`OnPointerWheelChanged`，`CanvasView.cs:91`，光标锚定）与左键拖拽平移（`OnPointerMoved`，`CanvasView.cs:129`）。但 `_scale`/`_offset` 为私有字段，无视图状态公开属性和变更事件——外部控件无法读取当前缩放/偏移。
- **像素→模型换算已就绪**：`CanvasView.ToModel(Point)`（`CanvasView.cs:56`）返回模型毫米坐标；`ToPixel` 为私有。
- **G 区宿主**：画布在 `CadWorkspaceHost.Drawing`（`Shell/CadWorkspaceHost.cs`），`AppShellView` 的 `CanvasSurface` 是常驻中心。G 区标尺即 `AppShellView.VerticalRuler`/`HorizontalRuler`。
- **M03 模块视图已有坐标思路**：`CadCanvasView.cs:230` 的 `OnCanvasPointerMoved` 调用 `_viewModel.ReportCoordinates(point)`，`CadCanvasViewModel.cs:58` 生成 `X {point.X:F2} mm · Y {point.Y:F2} mm` 状态文本——可作为坐标显示格式参考，但 G 区宿主当前未接入。
- **参照目标（视频梳理文档已定义）**：
  - CAD-31 / CAD-32：垂直 / 水平标尺 = **自绘 Control，黑底灰刻度，随 pan/zoom 更新**
  - CAD-33：坐标提示 = **TextBlock overlay，红色坐标，鼠标移动时更新**
  - §8.4：鼠标位置显示 x/y 坐标（左上角）；左侧标尺画布 -5000~5000mm；下侧同步有标尺；滚轮放大/缩小整个 CAD 投射
- **现有测试锚点**：`tests/LeatherNesting.Desktop.Tests/Shell/ShellFrameTests.cs:73-78`（FRAME-003）断言标尺尺寸 22/20、行列位置、`CanvasSurface` 布局；`CloneSurfaceColorTests.cs:38-39` 断言 `RulerChrome` 背景。改动需保持兼容或同步更新。
- **现有视觉资源**：`AppTheme.RulerChrome`（0x32 灰）/ `RulerTick`（0xD8 浅灰）/ `CanvasBlack`（0x00）/ `RulerBackground`/`RulerForeground` 别名已存在。

## Requirements

1. **动态垂直标尺**（G 区左侧，宽 22）与**动态水平标尺**（G 区下侧，高 20）：自绘刻度，反映当前画布视图（缩放 + 平移）；随滚轮缩放与拖拽平移实时刷新。外观沿用 `RulerChrome` 背景 + `RulerTick` 刻度。
2. **坐标提示**：G 区画布左上角 TextBlock overlay，鼠标移动时显示指针所在位置的模型 x/y 坐标（mm，两位小数），红色文字；画布外/无画布时不残留陈旧坐标。
3. **缩放联动**：滚轮缩放/平移改变画布视图时，标尺刻度同步更新（已具备缩放/平移本体，本任务补齐「标尺跟随」）。
4. **视图状态可观测**：`CanvasView` 暴露当前视图状态（缩放比例、偏移/原点），并提供视图变更通知，供标尺与坐标提示订阅；不改变现有缩放/平移交互行为。
5. **坐标参考系**：与现有 `CanvasView.ToModel` 一致（模型毫米，Y-up）；标尺刻度显示模型坐标位置。
6. **保持既有 shell 布局契约**：标尺的尺寸（22/20）、位置（左/下）、`CanvasSurface` 布局不被破坏；相关既有测试保持通过或同步更新断言。

## Acceptance Criteria

- [ ] AC-1：G 区左侧与下侧标尺为自绘控件，背景 `RulerChrome`、刻度 `RulerTick`，非静态文本。
- [ ] AC-2：滚轮缩放后标尺刻度随之变化（放大 → 刻度更稀疏/范围更小；缩小 → 更密集/范围更大）；拖拽平移后标尺刻度随视图移动。
- [ ] AC-3：画布左上角显示鼠标位置模型坐标 `X … mm · Y … mm`（红色文字），随鼠标移动实时更新；鼠标离开画布时清空或停止更新。
- [ ] AC-4：`CanvasView` 暴露视图状态（缩放 + 偏移）并发出变更通知；既有缩放/平移交互行为不回归。
- [ ] AC-5：新增测试覆盖：标尺自绘非死文本、缩放后标尺状态变化、坐标提示文本格式与更新、视图状态暴露与事件通知。
- [ ] AC-6：`ShellFrameTests`（FRAME-003）等既有测试保持通过（或按新控件结构同步更新断言）。
- [ ] AC-7：`dotnet test` 全绿，无新增警告。

## Out of Scope

- 改变现有滚轮缩放/平移交互逻辑（缩放锚点、速度等）。
- CAD 微型工具条、填充开关、范围缩放按钮等其余 CAD-01..30 元素。
- 全局画布世界网格 / 坐标网格线（仅标尺刻度与坐标提示）。
- M03 模块视图 `CadCanvasView` 的坐标显示改造（G 区宿主为本任务目标；M03 已具备坐标状态文本，可复用其格式约定）。

## Resolved Decisions

- 坐标参考系：**数据驱动视图**（用户已定）。标尺与坐标提示反映当前视口对应的模型坐标范围，随缩放/平移实时更新；不引入固定 -5000~5000mm 世界参考系，不改变现有 `CanvasView.ToModel`/FitToView 数据驱动行为。
