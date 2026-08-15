# 设计：排样输出对齐远程 DXF 格式（语义一致）

## 1. 目标与范围

- **语义一致**：颜色码 62 区分线角色、绝对坐标、闭合位正确，下游工具可按同约定消费。
- **三种线端到端**：外轮廓(0) + 切割线(3) + 标记线(5)，从导入→排样→导出一路打通。
- **无刀路**（下游切片软件生成）；**文字标签保持现状**（`{PieceId} {角度}°`）。
- 不追远程系统的结构细节（HEADER/TABLES/子类标记/句柄/图层 0·text_8）。

## 2. 现状 vs 目标（差距）

| 层 | 现状 | 目标 |
|----|------|------|
| 导入 | 只读闭合多段线→Loop2D(全 Role=Outer)；丢弃颜色 62；开放线判 Blocking | 读颜色/图层区分线角色；开放线作切割线；ContainmentTree 分内孔 |
| 域模型 | 部件 = 单个 Loop2D | 部件 = 外环 + 内孔 + 内部线（一个部件一个变换） |
| 排样 | NestRequest.Pieces = List<Loop2D> 逐条独立排 | 外环定位，孔/线随同一变换 |
| 导出 | 只写外环，无颜色 62 | 写三种线，颜色 62 = 0/3/5 |

## 3. 核心设计决策

- D1 语义一致（见 PRD）。
- D2 无刀路（见 PRD）。
- D3 三种线端到端（见 PRD）。
- D4 文字标签保持现状（见 PRD）。
- D5 **扩展现有导出链，不新增并行 writer**：`AsciiNestingDxfWriter` 是唯一排样导出路径，直接扩展其输入模型 + 输出；同步更新既有测试。

## 4. 数据模型

### 4.1 线角色

统一为三值（对应目标颜色码）：

| 角色 | 颜色 62 | 几何形态 |
|------|--------|---------|
| Outline（外轮廓） | 0 | 闭合 Loop2D |
| Cut（切割线） | 3 | 内孔（闭合 Loop2D）或开放切缝（开放曲线） |
| Mark（标记线） | 5 | 刀口/记号（闭合小环或短开放段） |

落点：在 Geometry 层新增枚举（如 `LineRole { Outline, Cut, Mark }`），或复用并扩展现有 `LoopRole`/`CadObjectCategory`/`NotchFeature.OutputMode`。推荐新增独立 `LineRole`，避免破坏现有 `LoopRole`（Outer/Hole）在几何算法中的语义。

### 4.2 部件聚合（新增 Piece）

```
Piece(string Id, Loop2D Outer, IReadOnlyList<Loop2D> Holes, IReadOnlyList<InternalLine> Lines)
InternalLine(LineRole Role, IReadOnlyList<Curve2D> Curves)   // 开放或闭合，按 Role 归 Cut/Mark
```

- 现有 `Loop2D` 保持「闭合轮廓」语义；开放切缝用 `InternalLine`（`Curve2D` 序列，无需闭合）。
- `Holes` 用 `ContainmentTree` 从导入的闭合 loop 集分类得到。
- 内部线的归属（切缝/刀口属于哪个部件）：优先靠共享 id 前缀（如 `PIECE-A-*`，demo 已用此法），缺省靠「线的包围盒落在哪个外环内」。

### 4.3 放置结果

`NestPlacement` 目前是 `(PieceId, Transform, PlacedLoop)`。扩展为携带整组放置几何：

```
NestPlacement(PieceId, Transform, PlacedOuter, PlacedHoles, PlacedLines)
```

排样算法仍只对**外环**做定位（碰撞检测以最小改动沿用现状），命中后把同一 `Transform` 施加到孔与内部线上，得到整组放置结果。孔是否参与碰撞（不允许他件落入孔内）作为**后续增强**，本期不纳入。

## 5. 数据流（端到端契约）

```
DXF 文件
  → AsciiDxfReader/AsciiDxfGeometryReader  读颜色(62)+图层(8)+闭合位(70)，开放线不再判 Blocking
  → 导入装配：闭合 loop 集 + 内部线集
  → ContainmentTree 分类 Outer/Hole → 组装 Piece 列表（外环 + 内孔 + 内部线）
  → NestRequest（外环参与排样；Piece 映射按 Outer.StableId 关联）
  → NestResult（Placement 携带整组放置几何）
  → NestingDxfDocument（每 Piece 含 Outer + Holes + Lines + 各自 LineRole）
  → AsciiNestingDxfWriter 输出：LWPOLYLINE 带颜色 62 = 0/3/5，闭合位正确；TEXT 保持现状
```

## 6. 颜色映射契约（导出）

- `LineRole.Outline` → 62 = 0，`70` = 1（闭合）。
- `LineRole.Cut` → 62 = 3；内孔闭合 `70=1`，开放切缝 `70=0`。
- `LineRole.Mark` → 62 = 5，闭合 `70=1`。
- TEXT 保持现状（图层/内容不变），不含刀路。

## 7. 兼容与迁移

- **破坏性改动集中在**：`NestRequest.Pieces`、`NestPlacement`、`NestingDxfPiece/Document`、`AsciiNestingDxfWriter` 的契约。现有测试（`NestingDxfExportTests`、`DxfRoundTripTests` 等）需同步更新。
- `Loop2D`/`LoopRole`/`Curve2D` 等底层几何类型**不改语义**，新增 `LineRole`/`Piece`/`InternalLine` 独立类型，避免波及几何算法与 CAD 画布。
- 导入侧对开放多段线的「Blocking」诊断需改为「识别为切割线」或保留但不再拦阻。

## 8. 权衡与风险

- **收益**：一次打通后，导出与远程系统语义对齐，下游切片软件可直接消费。
- **风险 1（数据归属）**：内部线（切缝/刀口）如何可靠归属到部件，是本期最大不确定点；用 id 前缀 + 包围盒包含双策略兜底。
- **风险 2（孔参与碰撞）**：本期孔不参与碰撞检测，可能出现「他件落入大孔」的次优排样；不影响格式对齐，留待后续。
- **风险 3（改动面广）**：跨 4 层（导入/域/排样/导出），建议按 implement.md 分阶段提交，每阶段可独立验证。

## 9. 暂缓（Deferred）

- 孔参与碰撞检测 / 优化排样质量。
- 文字标签改为尺码（需新增「尺码」字段，用户已选保持现状）。
- 完整 DXF 结构（HEADER/TABLES/子类标记/句柄/图层 0·text_8）。
