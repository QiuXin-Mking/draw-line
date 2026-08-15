# 排样输出对齐远程 DXF 格式

## Goal

让本项目排样（nesting）结果能输出与「远程系统」**语义一致**的 DXF 格式
（参照 `06-首次远程/排版测试/dxf骨架图.md` 及样例 `40码100片-幅宽1380-间距1.dxf`）：
颜色码 62 区分线角色、绝对坐标、闭合位正确，下游工具可按同一约定消费。

## 背景 / 已确认事实

### 目标格式（远程系统 DXF）
- ENTITIES 只有 `LWPOLYLINE` + `TEXT`。
- **颜色码 62 = 线角色**：`0` 外轮廓 / `3` 切割线 / `5` 标记线；`8` 文字。
- 顶点为绝对坐标、已排好位；`70` 闭合位：外轮廓/标记闭合，切割线开放为主。
- **无刀路**：只有几何线，刀路由下游切片软件生成（与 CLAUDE.md 一致）。

### 当前代码
- 导出链：`ExportNestingDxfUseCase` → `AsciiNestingDxfWriter`；模型 `NestingDxfPiece(PieceId, RotationDegrees, PlacedLoop)` 每部件只有一条外轮廓，writer 无颜色 62。
- 导入：`AsciiDxfReader`/`AsciiDxfGeometryReader` 只读闭合多段线→Loop2D(全 Outer)，**丢弃颜色 62**，开放线判 Blocking。
- 域模型已有相关概念但未接主线：`LoopRole{Outer,Hole}`、`NotchFeature.OutputMode{Cut,Mark}`、`ContainmentTree`（分内孔）、`CadObjectCategory{OuterContour,Hole,InternalLine}`（仅 Demo/UI）。

### 关键映射
- 外轮廓(62=0) ↔ Outer；切割线(62=3) ↔ 内孔 + 开放切缝；标记线(62=5) ↔ 刀口/记号。

## 决策

- D1 一致程度 = 语义一致（不追 HEADER/句柄/子类标记/图层命名）。
- D2 无需生成刀路。
- D3 本期范围 = 三种线端到端打通（导入→排样→导出）。
- D4 文字标签保持现状（`{PieceId} {角度}°`），不新增「尺码」字段。
- D5 扩展现有导出链，不新增并行 writer。

## Requirements

- R1 绝对坐标：放置后几何以绝对坐标写出。
- R2 线角色：颜色码 62 区分 0/3/5。
- R3 数据打通：新增 `Piece` 聚合（外环 + 内孔 + 内部线），打通导入/排样/导出。
- R4 文字标签保持现状。

## Acceptance Criteria

- [ ] 导出 DXF 中外轮廓/切割线/标记线颜色码分别为 0/3/5。
- [ ] 绝对坐标、闭合位正确（外轮廓/标记闭合，切割线开放为主）。
- [ ] 不生成刀路。
- [ ] 文字标签内容为 `{PieceId} {角度}°`。

## Out of Scope

- 生成切割刀路（下游切片软件完成）。
- 完整 DXF 结构（HEADER/TABLES/子类标记/句柄/图层 0·text_8）。
- 文字标签改为尺码（需新增「尺码」字段）。
- 孔参与碰撞检测（Deferred，见 design.md §8）。
