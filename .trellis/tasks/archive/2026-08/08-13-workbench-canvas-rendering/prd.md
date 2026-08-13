# 工艺工作台画布渲染

## Goal

在 U4 工艺工作台（`CadWorkbenchView`）里用真实画布替换当前的占位文字，把工作台当前轮廓（`CurrentLoops`）及其预览/诊断状态可视化出来，让用户能直观看到正在编辑的裁片几何，并据此操作修复工具。

## Background / Confirmed Facts

- 当前画布是占位 `Border` + `TextBlock`（"画布（Stage 2 占位）"），见 `CadWorkbenchView.cs:48-60`。
- 工作台 `CadWorkbenchViewModel` 已暴露 `CurrentLoops`（`IReadOnlyList<Loop2D>?`）、`ToolMode`、`State`、`ProblemMessages`。
- 几何类型：`Loop2D`（`StableId`/`Role`(Outer/Hole)/`Curves`/`Area`/`Length`），`Curve2D` 子类 `LineSegment2D`/`Polyline2D`/`CircularArc2D`，均有 `StartPoint`/`EndPoint`/`PointAt(t)`/`Bounds`。
- 业务坐标是毫米（Y 向上）；屏幕坐标是像素（Y 向下），渲染需要缩放 + Y 翻转。
- 技术栈：Avalonia 12.1.0（`Avalonia.Desktop`，Skia 后端），无第三方 canvas 库，仓库内**无任何既有渲染代码**。
- 阶段 4 的「排样结果可视化」是另一个画布，不在本任务内。

## Requirements

- **R1 轮廓渲染**：自定义 Avalonia `Control` 重写 `Render(DrawingContext)`，把 `CurrentLoops` 的所有曲线绘制出来；外环与孔用不同颜色（颜色仅为区分，配合文字/图例）。
- **R2 坐标变换**：毫米 → 像素的等比缩放（uniform scale）+ Y 轴翻转 + 自动 fit-to-view（所有轮廓包围盒适配画布，留边距）。
- **R3 预览状态渲染**：Previewing 状态下渲染预览几何——桥接段（`Bridges`）、offset 结果、剪口几何（`NotchFeature.GenerateGeometry`）。
- **R4 问题定位**：对 `ProblemMessages` 关联的轮廓做可视化高亮，使用户能从画布定位问题实体。
- **R5 性能**：中等规模轮廓（≥100 条曲线）渲染不卡顿；仅在几何或状态变化时重绘。
- **R6 缩放平移**：滚轮缩放（以光标为中心）、拖拽平移；初始自动 fit-to-view。

## Acceptance Criteria

- [ ] 工作台 `LoadLoops` 后画布显示轮廓，非空画布不再显示占位文字。
- [ ] 外环与孔颜色可区分（并有非颜色提示）。
- [ ] 预览闭合/offset 后画布立即反映结果几何。
- [ ] `dotnet build LeatherNesting.sln -c Release` 0 警告 0 错误，全解测试不回归。
- [ ] （若做交互）缩放平移 / 节点拾取拖动按约定档位可用。

## Out of Scope

- 阶段 4 的排样结果画布（另一个任务）。
- 3D 渲染、硬件加速优化、老旧显卡软件渲染回退（阶段 6）。
- 完整 CAD 标注（尺寸线、栅格、坐标轴）——后续按需。

## Open Questions

无阻塞问题。Q1 已确认：**只读渲染 + 缩放平移**（不做节点拾取拖动）。Q2 渲染技术选型在设计阶段定为自定义 `Control` + `DrawingContext`（Skia）。
