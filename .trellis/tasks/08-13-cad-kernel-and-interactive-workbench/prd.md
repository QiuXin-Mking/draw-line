# 完成 CAD 内核 + 交互式工艺工作台

## Goal

补齐 CAD 内核中圆弧相关的未完成部分，并把工艺工作台从「只有按钮的空壳」做成**可交互的子页面**——用户能对每个工具输入参数、预览、提交、撤销，真正完成一次 CAD 编辑。

## Background / Confirmed Facts

- 几何层主体已完成，38/38 测试通过；命令层（9 个 `CadCommand`）+ session/undo 已完成。
- **内核未完成点（圆弧相关）**：
  1. `Loop2D.cs:114` 圆弧面积按弦近似（注释「real arc-aware area is Stage 3+」）。
  2. `FaceCandidate.cs:61` 与 `EndpointIndex.cs:81` 曲线求交只支持线段（注释「curve-curve intersection deferred」）。
  3. `PlacementValidator.cs:88` 重叠检测仅包围盒（非真实多边形求交）。
- **工作台未完成点**：`CadWorkbenchView` 只有工具模式按钮 + Preview/Commit/Cancel/Undo/Redo，但「Preview」是空操作，**没有任何参数输入**（offset 距离、节点位置、剪断点、剪口参数都无法输入），因此无法真正交互。
- `CadWorkbenchViewModel` 的 `Preview*` 方法已带参数（`PreviewOffset(distance,direction,join)`、`PreviewMoveNode(nodeIndex,pos)`、`PreviewBreakAtPoint(point)`、`PreviewNotch(...)` 等），只是视图层没接输入。
- 画布 `CanvasView` 当前只读 + 缩放平移，无点击拾取。

## Requirements

- **R1 圆弧面积**：`Loop2D.Area`（及 `FaceCandidate.ComputeArea`）对 `CircularArc2D` 精确计算（扇形面积 + 弦多边形，而非弦近似）。
- **R2 曲线求交**：自交检测（`FaceCandidate`/`PlacementValidator`）支持线-弧、弧-弧求交，替代仅线段判断。
- **R3 交互式工艺工作台**：每个工具提供参数输入 UI（offset 距离/方向、节点索引/坐标、剪断点、剪口参数），点击后调对应 `Preview*`，再 Commit/Cancel/Undo/Redo，画布随之更新。
- **R4 画布交互**：点击画布拾取点/节点（坐标反算），用于节点编辑、剪断等需要坐标的工具；坐标输入同时支持表单与画布点击（用户已确认「两者都做」）。
- **R5 裁片变换**：新增 `Transform2D` 应用到 `Loop2D` 的能力（移动/旋转/镜像，含圆弧变换），并新增变换命令（移动/旋转裁片），可撤销。
- **R6 裁片选择与交互**：画布点选/框选裁片，拖动移动、旋转（手柄或角度输入），选中态高亮。

## Acceptance Criteria

- [ ] `dotnet build LeatherNesting.sln -c Release` 0 警告 0 错误，全解测试不回归。
- [ ] 含圆弧的轮廓面积精确（新增圆弧面积单测，与弦近似结果区分）。
- [ ] 自交检测能识别弧段相交（新增线-弧/弧-弧求交单测）。
- [ ] 工作台每个工具都能输入参数 → 预览 → 提交 → 撤销，画布与轮廓随之变化（headless 测试覆盖 ViewModel 已有，视图参数输入以手动验证）。
- [ ] 画布点击拾取到节点/点，驱动节点编辑/剪断。
- [ ] 裁片可点选/框选、拖动移动、旋转，变换后几何正确、可撤销。

## Out of Scope

- UI 布局/视觉设计（用户明确「布局暂时不跟进」）。
- 码齿规则库、材料与排样约束（阶段 3）。
- 排样引擎（阶段 4）。
- `PlacementValidator` 真实多边形重叠（属阶段 4 排样校验，本任务只做弧段求交使其自交检测更准）。

## Open Questions

无阻塞问题。坐标输入已确认「两者都做」（画布点击 + 表单）。裁片选择/移动/旋转/平移已纳入 R5/R6。
