# DXF 圆弧拓扑技术设计

## 数据流

```
DXF（LWPOLYLINE 顶点 + bulge，或 ARC 实体）
  → 解析 bulge / ARC → CircularArc2D
  → Loop2D（Curves = 直线 + 圆弧混合）
```

## 关键决策

### 1. bulge → CircularArc2D 公式

DXF bulge `b = tan(θ/4)`，θ 是圆弧圆心角（有符号，正 = 逆时针）。给定相邻顶点 `P1`、`P2` 和 bulge `b`：

- 圆心角 `θ = 4 * atan(b)`
- 弦长 `c = |P1P2|`
- 半径 `r = c * (1 + b²) / (4 * |b|)`
- 圆心在弦的垂直平分线上，到弦的有符号距离 `d = c * (1 - b²) / (4 * b)`
- 起始角 = `atan2(P1 - centre)`

由圆心 + 半径 + 起始角 + 圆心角构造 `CircularArc2D`。

### 2. LWPOLYLINE 顶点 + bulge 配对

DXF 里每个顶点 i 带一个 bulge（组码 `42`），描述「顶点 i → 顶点 i+1」之间的圆弧（bulge=0 为直线）。解析时把 `10/20`（顶点）和 `42`（bulge）按序配对。

### 3. 独立 ARC 实体

`ARC` 实体组码：`10/20` 中心、`40` 半径、`50` 起始角、`51` 结束角。转 `CircularArc2D(centre, radius, startAngle, endAngle - startAngle)`。

### 4. 输出曲线混合

一个 `Loop2D` 的 `Curves` 可同时含 `LineSegment2D`（bulge=0 段）和 `CircularArc2D`（bulge≠0 段）。连续直线段可合并为 `Polyline2D`（可选优化）。

## 兼容与回滚

- 只改 `AsciiDxfGeometryReader`（读回几何），`AsciiDxfReader`（实体清单）不动。
- 纯直线（bulge=0）路径保持不变，无回归风险。
- 回滚：还原 `AsciiDxfGeometryReader` 即可。
