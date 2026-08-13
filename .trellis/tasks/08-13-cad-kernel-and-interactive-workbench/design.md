# CAD 内核 + 交互式工艺工作台 — 技术设计

## 1. 圆弧面积（R1）

`Loop2D.Area` / `FaceCandidate.ComputeArea` 对 `CircularArc2D` 改为精确计算：
- 闭合轮廓面积 = 弦多边形面积 + Σ 每段圆弧的「扇形 − 三角形」贡献（符号随 winding）。
- 单测用已知圆/半圆样本对比弦近似，验证差异可观测。

## 2. 曲线求交（R2）

新增 `Geometry/Intersection` 工具：
- 线-线（已有）、线-弧、弧-弧。
- 线-弧：直线参数代入圆方程解二次；弧-弧：圆-圆交点 + 弧范围过滤。
- 供 `FaceCandidate` 与 `PlacementValidator` 的自交检测使用（替代仅线段）。

## 3. 裁片变换（R5）

- `Transform2D` 增加 `Apply(Point2D)`：镜像 → 旋转 → 平移（顺序固定并注释）。
- `Transform2D` 增加 `Apply(Loop2D)`：逐曲线变换（线段/折线变换控制点；圆弧变换圆心、起始角 ± 旋转角、镜像翻转 sweep）。
- 新增 `TransformCommand`（移动/旋转/镜像一个裁片），快照式撤销，纳入命令层。

## 4. 交互式工作台（R3/R4/R6）

- `CadWorkbenchViewModel` 增加：
  - 参数输入状态（offset 距离/方向、节点索引/坐标、剪断点、剪口参数、变换量）。
  - `SelectPiece(point)`：命中检测（点在轮廓内，ray-casting）。
  - `MoveSelected(delta)` / `RotateSelected(angle)`：构造 `TransformCommand` 走 preview/commit。
- `CadWorkbenchView` 增加：
  - 工具参数输入面板（数值框 + 下拉，offset/节点/剪断/剪口/变换）。
  - 画布点击 → 坐标反算（`CanvasView` 暴露 `ToModel(Point)`），驱动点拾取与选择。
- `CanvasView` 增加：
  - `ToModel(Point)`（像素 → 毫米逆变换）。
  - 点选/框选、拖动移动、旋转手柄（R6），选中态高亮。
- 表单与画布两种坐标输入并存（已确认「两者都做」）。

## 5. 数据流

画布点击/表单输入 → 构造参数 → `ViewModel.Preview*(...)` → `CadOperationSession.Preview(command)` → `CanvasView.SetData(CurrentLoops)` 重绘 → Commit/Undo/Redo 走事务栈。

## 6. 风险与回退

- 圆弧变换（旋转角/sweep/镜像）易错 → 用「矩形+已知角度」golden 单测锁死。
- 命中检测/坐标反算错误 → 纯展示/选择层，不破坏几何数据；几何与命令层可独立回退。
- 范围较大 → 分阶段落地（内核 → 命令 → 输入 → 选择/变换），每阶段 build+test 门。
