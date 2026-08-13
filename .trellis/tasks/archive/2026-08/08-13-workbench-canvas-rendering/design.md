# 画布渲染 — 技术设计

## 1. 渲染方案

自定义 `Control`（`CanvasView`），重写 `Render(DrawingContext)`，用 Avalonia 内置 `DrawingContext`（Skia 后端）绘制。不用 Avalonia `Shapes`（大量图元时性能差、无 CAD 语义、不好做统一坐标变换）。

## 2. 坐标变换

- 毫米（Y 向上）→ 像素（Y 向下）：`px.X = offX + mm.X * scale`；`px.Y = offY − mm.Y * scale`。
- 统一等比 `scale`（不变形）。
- fit-to-view：取所有轮廓包围盒，`scale = min(w/bboxW, h/bboxH) × (1 − margin)`，居中。
- 实现用仿射矩阵（`PushTransform`/手动点变换均可，实现期定），避免逐点硬编码。

## 3. 数据流

`CanvasView` 不直接耦合 ViewModel，暴露 `SetData(loops)`；`CadWorkbenchView` 在 `LoadLoops`/预览/提交/撤销后调用 `canvas.SetData(vm.CurrentLoops)` 触发重绘。缩放平移状态存于 `CanvasView` 内部。

## 4. 图元绘制

- 外环 / 孔不同描边色（配合文字图例，颜色不是唯一区分）。
- 曲线展平为线段：`LineSegment2D`→1 段；`Polyline2D`→N−1 段；`CircularArc2D`→按弦高容差采样（复用 `AsciiDxfWriter` 的展平思路）。
- 节点画小圆点，帮助定位（可选开关）。

## 5. 缩放平移（R6）

- 滚轮：以光标为中心缩放（调整 offset 保持光标点不动）。
- 左键拖拽：平移 offset。
- 数据变化或首次渲染：fit-to-view。

## 6. 预览与问题（R3/R4）

- 预览几何：`CurrentLoops` 已反映桥接/offset 结果，直接渲染即可，无需额外通道。
- 问题定位：沿用已有文字列表；轮廓级高亮需「problem→loop」关联（当前模型无此映射），本任务不做，留后续。

## 7. 风险与回退

- Y 翻转/矩阵错误 → 用已知矩形 golden（`fixtures/golden/cad-repair/rectangle.dxf`）做 headless 视觉断言或坐标断言。
- 大轮廓重绘卡顿 → 只在数据变化时 `InvalidateVisual`；展平结果缓存；fit-to-view 不逐帧重算。
- 画布改动不影响工作台状态机 → 纯展示层，命令/撤销逻辑不动。
