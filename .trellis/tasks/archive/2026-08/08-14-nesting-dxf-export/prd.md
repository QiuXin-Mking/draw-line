# 排样结果 DXF 输出

## Goal

把排样结果 `NestResult` 写成可供下游切片软件使用的 DXF 文件，对齐既有 Python demo 的输出结构（图层 + 皮革边界 + 裁片 + 编号标注 + 利用率标题），单位毫米。

## 现状（confirmed facts）

- 已有 `AsciiDxfWriter`（`IDxfWriter` 实现）：能写 `IReadOnlyList<Loop2D>` → ASCII DXF（LWPOLYLINE），但 layer 固定 `"0"`，无图层 / 标注 / 标题。
- 已有 `AsciiDxfReader`：能读回 DXF 实体（round-trip 验证可用）。
- Python demo `leather_nesting_demo.py::write_dxf` 定义了目标结构：
  - 图层 `LEATHER`（color 1）、`PIECES`（color 5）、`ANNOTATION`（color 3）；
  - 皮革矩形 LWPOLYLINE、每片裁片 LWPOLYLINE、每片 `P{index} {rotation}deg` TEXT 标注、利用率标题 TEXT。
- `NestResult`：`Placements`（`PieceId` + `Transform` + `PlacedLoop`）、`Unplaced`、`Utilization`。
- ADR-02（`docs/adr/02-职责.md`）：输出契约 DXF + JSON 两种，**先做 DXF**；JSON 暂缓（见 `docs/todo/01-json输出契约待办.md`）。
- 单位：业务内部一律毫米。

## Requirements

1. **应用层编排**：新增导出 use case（Application 层），输入 `NestResult` + 材料 `Loop2D` + `gapMm`，产出 DXF 文件。
2. **DXF 结构**（对齐 Python demo）：
   - `LEATHER` 图层：材料轮廓（闭合 LWPOLYLINE，当前为矩形）。
   - `PIECES` 图层：每个已放置裁片的 `PlacedLoop` 轮廓。
   - `ANNOTATION` 图层：每片编号 + 旋转角标注（TEXT），以及利用率标题（TEXT）。
3. **单位**：输出标记为毫米。
4. **未放置裁片不写入 DXF**（对齐 Python demo：`unplaced` 属 JSON 输出职责，本次不做）。

## Acceptance Criteria

- [ ] 给定一个 `NestResult`，导出 use case 产出 DXF 文件。
- [ ] 读回验证：`AsciiDxfReader` 能读回，实体数 = 皮革(1) + 裁片(N) + 标注(N + 1 标题)。
- [ ] 图层正确：`LEATHER` / `PIECES` / `ANNOTATION` 区分。
- [ ] 裁片轮廓坐标 == `PlacedLoop`（round-trip 无损）。
- [ ] 空排样（无 placement）不崩溃，产出仅含皮革边界 + 标题的 DXF。

## Out of Scope

- JSON 输出（暂缓，见 todo 文档）。
- Desktop Export 模块 UI 集成（其他 session 的 demo，后续接入）。
- 自由角度 / 镜像 / part-in-part 等排样扩展（排样任务已闭环）。
