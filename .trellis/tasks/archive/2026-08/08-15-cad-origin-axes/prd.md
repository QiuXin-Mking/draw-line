# CAD 坐标轴固定在模型原点（非左上角）

## Goal

把 G 区 CAD 画布的 X/Y 坐标轴从「固定左上角」改为「固定在模型原点 (0,0) 处」，随视图平移/缩放联动：原点投影到像素坐标处绘制轴，原点移出可视区时轴隐藏。当前轴是静态 TextBlock（`+X\n│\n└── +Y`）钉在左上角，用户判定不符合预期。

## Confirmed Facts（代码库证据）

- **当前轴是静态 TextBlock**：`CadWorkspaceHost.BuildAxes()`（`src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs:206-213`）返回 `TextBlock`，`Text="+X\n│\n└── +Y"`、`Foreground=AppTheme.MaterialBoundary`、`Margin=(8,7)` 固定左上角，不随视图移动。
- **视图状态基础已就绪**（上一任务「动态标尺」已落地）：
  - `CanvasView.ViewScale`（px/mm）、`ViewOriginModel`（模型原点 (0,0) 处对应的像素坐标，Y-up 镜像）、`ViewChanged` 事件（缩放/平移/refit 触发）——`src/LeatherNesting.Desktop/Views/CanvasView.cs:22-29`。
  - 自绘控件范式：`CadRuler` 订阅 `ViewChanged` 后 `InvalidateVisual()` 重绘（`src/LeatherNesting.Desktop/Views/CadRuler.cs:18-23`）。
- **语义色已存在**：`AppTheme.MaterialBoundary`（红 0xFF0000，`AppTheme.cs:47`）可作轴色；`AppTheme.RulerTick`/`RulerChrome` 作辅助。
- **坐标提示 overlay 已实现**：`CadWorkspaceHost` 内 `_coordinates` 为红色 TextBlock，`IsHitTestVisible=false`（`CadWorkspaceHost.cs:22-31`）。
- **测试锚点**：`CadInteractionRulerTests`（RUL-001..006）覆盖视图状态/标尺/坐标提示；`CadHostEvidenceTests` 构造 `CadWorkspaceHost`。

## Requirements

1. **轴随原点移动**：X 轴（水平线）与 Y 轴（垂直线）绘制在模型原点 (0,0) 的像素投影处；随拖拽平移、滚轮缩放实时重绘。
2. **原点出屏隐藏**：原点像素坐标超出画布可视区时，轴整体隐藏（不残留左上角符号）。
3. **轴标注**：沿轴显示 `+X`/`+Y` 方向指示（沿用现有 `+X/+Y` 语义），位置随原点定位，不固定 Margin。
4. **实现方式**：自绘控件（仿 `CadRuler`，订阅 `ViewChanged` 重绘），而非静态 TextBlock；颜色用 `AppTheme.MaterialBoundary` 语义色。
5. **保留坐标提示**：左上角红色坐标提示 overlay 保持不变（那是鼠标坐标读数，非坐标轴；与轴是两回事）。
6. **不改变缩放/平移交互**：仅改变轴的渲染位置，`CanvasView` 交互行为不回归。

## Acceptance Criteria

- [ ] AC-1：G 区画布坐标轴绘制在模型原点 (0,0) 像素投影处，非左上角固定位置。
- [ ] AC-2：拖拽平移后轴随原点移动；滚轮缩放后轴位置随视图更新。
- [ ] AC-3：原点移出可视区时轴隐藏，画布上无残留的左上角轴符号。
- [ ] AC-4：轴为自绘控件（非静态 TextBlock），颜色为 `AppTheme.MaterialBoundary` 语义色。
- [ ] AC-5：左上角坐标提示（`CoordinateText`）保持行为不变。
- [ ] AC-6：新增/更新测试覆盖：轴位置随视图、原点出屏隐藏、语义色；既有测试（RUL/CAD-HOST/FRAME）保持通过。
- [ ] AC-7：`dotnet test` 全绿，无新增警告。

## Out of Scope

- 改变 `CanvasView` 缩放/平移交互逻辑。
- 网格线 / 刻度线（标尺已覆盖，见 cad-interaction-rulers 任务）。
- 轴的颜色/样式自定义选项。

## Resolved Decisions

- 轴形态：**带箭头轴线**（用户已定）。自绘 X 轴水平线（正向 `→` 箭头 + `+X` 标注）与 Y 轴垂直线（正向 `↑` 箭头 + `+Y` 标注），相交于原点像素投影处。
- 原点像素换算：不暴露私有 `ToPixel`，自绘轴控件通过公开 `CanvasView.ViewOriginModel`/`ViewScale` 计算。原点像素：`X = -ViewOriginModel.X * ViewScale`，`Y = ViewOriginModel.Y * ViewScale`（Y-up 镜像，与 `CanvasView.cs:56` `ToModel` 反推一致）。
