# 排样结果 DXF 输出 技术设计

## 分层与边界

```
NestResult + Material(Loop2D) + gapMm
  → Application: ExportNestingDxfUseCase（编排）
  → Infrastructure: 排样 DXF 写出（图层 + LWPOLYLINE + TEXT）
  → 文件（ASCII DXF，毫米）
```

- Application 层新增 use case，依赖 Infrastructure 的写出接口（端口在 Application 定义，实现在 Infrastructure）。
- 复用 `AsciiDxfWriter` 的实体写法（LWPOLYLINE），新增 TEXT 实体与图层支持。

## 关键决策

### 1. 接口形态
- 现有 `IDxfWriter.WriteAsync(path, loops)` 是 Stage 2 round-trip 接口，签名不含图层 / 标注 / 标题，不适合排样输出。
- **新增** `INestingDxfWriter`（Infrastructure 端口，或 Application 定义的端口），接受结构化排样输出模型：
  ```csharp
  public sealed record NestingDxfDocument(
      Loop2D Material,
      IReadOnlyList<NestingDxfPiece> Pieces,
      string Title);
  public sealed record NestingDxfPiece(string PieceId, double RotationDegrees, Loop2D PlacedLoop);
  ```
- 实现 `AsciiNestingDxfWriter`：写 `LEATHER` / `PIECES` / `ANNOTATION` 三图层 + LWPOLYLINE + TEXT。

### 2. TEXT 实体（标注）
- 每片标注：`{PieceId} {rotation:g}°`，插入点在 `PlacedLoop` 顶点重心。
- 标题：`Leather {w}x{h} mm | gap {gap:g} mm | placed {n} | utilization {x:.2f}%`，插入点在皮革边界上方。
- 字体高度对齐 Python demo：`max(8, min(w,h)/80)`。

### 3. 图层与颜色
- 对齐 Python demo：`LEATHER` color 1、`PIECES` color 5、`ANNOTATION` color 3。
- 写 TABLES 图层表（保证 AutoCAD/Illustrator 正确识别图层）；若保持最小化，至少每个实体带正确 `8`（layer）组码。

### 4. round-trip 验证
- 用现有 `AsciiDxfReader` 读回导出文件，断言实体数与图层正确，裁片轮廓与 `PlacedLoop` 一致。

## 兼容与回滚

- 不改动现有 `IDxfWriter` / `AsciiDxfWriter` 语义（Stage 2 round-trip 保持不动），新增独立写出器。
- 回滚：删除新增 use case + writer 即可，无侵入。

## 风险 / 延迟项

- TEXT 实体的字符编码：ASCII DXF 对中文 PieceId 可能不支持——若 `PieceId` 含中文，标注退化为 ASCII 安全形式（首版假设 ASCII id）。
- 非矩形材料：当前按 `Material` 的实际 `Loop2D` 写轮廓（不强制矩形），首版只验证矩形。
