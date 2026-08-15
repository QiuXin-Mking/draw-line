# DXF 圆弧拓扑（bulge + ARC 解析）

## Goal

让带圆弧的裁片导入后**不失真**——解析 `LWPOLYLINE` 的 bulge（凸度）和独立 `ARC` 实体，转成 `CircularArc2D`，而不是把圆弧拉成直线。

## 现状（confirmed facts）

- `AsciiDxfGeometryReader` 只读 `LWPOLYLINE` 的 `10/20` 顶点，**忽略 `42`（bulge）**，圆弧段被拉成直线（`src/.../Dxf/AsciiDxfGeometryReader.cs:29-33`）。
- 不读独立 `ARC` 实体（只匹配 `LWPOLYLINE`，`AsciiDxfGeometryReader.cs:21`）。
- `AsciiDxfReader` 识别实体类型但**不解析几何**（bulge/ARC 均未处理）。
- 几何层已支持 `CircularArc2D`（`Loop2D` 曲线类型之一，`OffsetAdapter`/`Transform2D`/面积计算都 arc-aware）。

## Requirements

1. 解析 `LWPOLYLINE` 的 bulge（`42` 组码），把 bulge 段转成 `CircularArc2D`。
2. 解析独立 `ARC` 实体（`10/20` 中心、`40` 半径、`50/51` 起止角），转成 `CircularArc2D`。
3. bulge = 0 的段保持直线（`LineSegment2D` / `Polyline2D`），行为不变。
4. 顶点 + bulge 配对：`LWPOLYLINE` 顶点 i 的 bulge 描述「顶点 i → 顶点 i+1」之间的圆弧。

## Acceptance Criteria

- [ ] 带 bulge 的 `LWPOLYLINE` 导入后，圆弧段是 `CircularArc2D`，几何与原始圆弧一致（圆心/半径/起止角）。
- [ ] 独立 `ARC` 实体导入后转成 `CircularArc2D`。
- [ ] 纯直线多段线（bulge = 0）行为不变，回归测试通过。
- [ ] round-trip 测试：圆角矩形导入后面积/形状与原始一致，圆弧不被拉直。

## Out of Scope

- `SPLINE`（样条曲线，后续）。
- 圆弧的**写入**（DXF 导出含 bulge，后续）。
