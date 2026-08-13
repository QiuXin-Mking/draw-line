# UI M03 CAD 画布、选择与显示

## Goal

实现 M03 CAD 浏览与显示控制演示页：深色画布宿主、对象树、图层可见性、全图/缩放、坐标/标尺、图例。复用现有 `CanvasView` 的缩放/平移/点选/拖拽能力；未真实接入的命中/框选/多选/图层持久化/曲线编辑标 TODO。

## Background / Confirmed Facts

- 父任务 `implement.md` T03 与 `product-function-specification-table.md` M03 定义合同。
- `Views/CanvasView.cs` 已有：缩放（滚轮）、平移（拖拽）、点选/拖拽移动、选中高亮、`SetData(loops, refit)`、`ToModel`。
- `DemoScenario` 无几何数据（只有 `PieceCount`），M03 需要合成演示裁片。
- 当前 M03 在 Shell 中是占位页。

## Requirements

- **R1 演示几何**：新增 `Demo/DemoGeometry.cs`（`DemoObject`：Id/类别/Loop2D）+ `Demo/DemoGeometryFactory`，生成少量合成裁片（外轮廓矩形、孔圆、内部线）。
- **R2 M03 页面**（`Modules/CadCanvas/`）：深色画布宿主（复用 `CanvasView`）、左侧对象树、图层可见性开关、顶部工具栏（全图/缩放）、底部坐标/缩放状态、图例。
- **R3 显示控制**：切换类别可见性 → 画布过滤对应 loop（`SetData(loops, refit:false)`）；「全图」→ `refit:true`。
- **R4 TODO**：命中测试、框选、多选、图层持久化、复杂曲线编辑标 `TodoBadge`，点击显示文本说明。
- **R5 接入 Shell**：M03 用 `CadCanvasView` 替换占位页。

## Acceptance Criteria

- [ ] 显示控制改变演示类别可见性（隐藏/显示可观察）。
- [ ] 全图/缩放可观察。
- [ ] 未接入工具均标 TODO（`TodoBadge.StandardText`）。
- [ ] `dotnet build` 0 警告 0 错误；Desktop 测试不回归；新增 M03 测试通过（几何非空、类别齐全、可见性切换逻辑）。

## Out of Scope

- 真实命中/框选/多选/图层持久化/曲线编辑（后续任务）。
- `CanvasView` 几何算法改动（只允许展示层复用）。

## Open Questions

无阻塞问题。
