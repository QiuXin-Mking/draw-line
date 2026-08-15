# DXF 圆弧拓扑实现计划

1. 扩展 `AsciiDxfGeometryReader` 解析 `LWPOLYLINE`：同时读 `10/20`（顶点）与 `42`（bulge），按序配对。
2. 实现 bulge → `CircularArc2D`（圆心角/半径/圆心/起始角公式）。
3. bulge = 0 的段保持直线，连续直线段合并为 `Polyline2D`。
4. 解析独立 `ARC` 实体 → `CircularArc2D`。
5. 单元测试：
   - 带 bulge 的 `LWPOLYLINE` → 圆弧段是 `CircularArc2D`，圆心/半径/起止角正确。
   - 独立 `ARC` 实体 → `CircularArc2D`。
   - 纯直线多段线（bulge=0）行为不变。
   - 圆角矩形 round-trip 面积/形状一致。
6. 全量测试：`dotnet test`，确认无回归。

**验证**：`dotnet test tests/LeatherNesting.Infrastructure.Tests`

## 回滚点

- 纯改 `AsciiDxfGeometryReader`，回滚还原该文件即可。
