# 实施计划：排样输出对齐远程 DXF 格式

> 顺序执行，每阶段可独立编译/测试，通过后再进下一阶段。

## Phase 1 — 数据模型（Geometry 层）

新增独立类型，不改现有几何语义。

- [ ] 新增 `LineRole { Outline, Cut, Mark }`（`src/LeatherNesting.Geometry/`）。
- [ ] 新增 `InternalLine(LineRole Role, IReadOnlyList<Curve2D> Curves)`。
- [ ] 新增 `Piece(string Id, Loop2D Outer, IReadOnlyList<Loop2D> Holes, IReadOnlyList<InternalLine> Lines)`。
- [ ] 单测：构造/不变量（Outer 非空、Role 校验）。

**验证**：`dotnet build src/LeatherNesting.Geometry` + 相关单测。

## Phase 2 — 导入打通（Infrastructure/Application）

- [ ] `AsciiDxfReader`：读取颜色 `62` 与 `42`（bulge 已读）；开放 LWPOLYLINE 不再判「Blocking 未闭合」，改记为「内部线候选」（`DxfEntityKind` 或新标志）。
- [ ] `AsciiDxfGeometryReader`：读颜色 `62`/图层 `8`，开放多段线产出 `InternalLine`；闭合 loop 保留。
- [ ] 装配：用 `ContainmentTree` 把闭合 loop 分为 Outer/Hole；内部线按 id 前缀 + 包围盒包含归入 `Piece`。
- [ ] 更新 `ImportDxfUseCaseTests`/`AsciiDxfReaderTests` 对新分类的断言。

**验证**：`dotnet test tests/LeatherNesting.Infrastructure.Tests`。

## Phase 3 — 排样打通（Geometry.Nesting）

- [ ] `NestRequest.Pieces` 由 `List<Loop2D>` 改为携带 `Piece`（或保留外环列表 + 外环→Piece 映射，二选一，推荐后者以最小化改动）。
- [ ] `NestPlacement` 扩展为 `(PieceId, Transform, PlacedOuter, PlacedHoles, PlacedLines)`；命中后同一 `Transform` 施加到孔/内部线。
- [ ] `NestEngine` 仍只对外环做碰撞/定位（本期孔不参与碰撞）。

**验证**：`dotnet test tests/LeatherNesting.Geometry.Tests` + 排样相关测试。

## Phase 4 — 导出打通（Application/Infrastructure）

- [ ] `NestingDxfPiece`/`NestingDxfDocument` 扩展：每 Piece 含 Outer + Holes + Lines（带 `LineRole`）。
- [ ] `AsciiNestingDxfWriter`：为每条线写颜色码 `62`（Outline=0/Cut=3/Mark=5）与闭合位 `70`；TEXT 保持现状。
- [ ] 更新 `NestingDxfExportTests` 断言颜色码与闭合位。

**验证**：`dotnet test tests/LeatherNesting.Infrastructure.Tests`；用 `06-首次远程/排版测试/` 下的样例跑一次端到端导出，与 `dxf骨架图.md` 的三色约定人工比对。

## Phase 5 — 收尾

- [ ] `dotnet test`（全量，确认无回归）。
- [ ] 更新 `docs/`（若需记录新契约/ADR，按 CLAUDE.md 放 `docs/adr/`）。
- [ ] 端到端验收：导出一份含三色线的 DXF，确认颜色 62 = 0/3/5、闭合位正确、无刀路。

## 关键风险 / 回滚点

- **Phase 3 的 `NestRequest` 契约改动**是最大破坏点；若排样行为回归，回滚 Phase 3 并退回「外环列表 + 映射」方案。
- 每阶段独立提交，便于按阶段回滚。
