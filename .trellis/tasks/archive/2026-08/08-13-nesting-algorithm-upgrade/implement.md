# 排样引擎实现计划

> **全部完成（2026-08-14）**：阶段 A（0°/90° + Clipper2 布尔碰撞 + 贪心 BLF）+ 阶段 B（NFP 计算 + 局部搜索优化）。Geometry 测试 60/60 通过。

## 阶段 A（MVP：0°/90° + 布尔碰撞 + 贪心 BLF）✅

1. 复用已存在的 `Transform2D.Apply(Loop2D)`（无需新增，`TransformTests` 已覆盖）。
2. 新增共享 `ClipperPathAdapter`（Loop↔Path64，逻辑同 `OffsetAdapter` 现有实现）。
3. 新增 `Nesting/` 类型：`NestRequest` / `NestPlacement` / `NestResult`。
4. 实现 `ClipperCollisionDetector`（重叠 + 间隙 + 出界判定）。
5. 实现 `PlacementCandidateGenerator`（0°/90° × 位置候选）。
6. 实现 `NestEngine` 贪心 BLF。
7. 单元测试：碰撞、间隙、0°/90°、`unplaced`、边界行为。

**验证**：`dotnet test tests/LeatherNesting.Geometry.Tests`

## 阶段 B（优化：NFP + 局部搜索）✅

8. 实现 `NfpCalculator`（Clipper2 `MinkowskiDiff`）—— NFP 语义正确 + 测试通过；**未接入优化主流程**（0°/90° 离散角度下收益有限，留待连续角度阶段再接入）。
9. 实现 `NestOptimizer`（随机重排 + 保留最优的局部搜索，固定 seed 可复现）—— 实际产生利用率提升的手段。
10. 利用率对比测试（optimized ≥ BLF 基线）+ NFP 语义测试 + 确定性测试。

**验证**：`dotnet test tests/LeatherNesting.Geometry.Tests`

## 评审门槛（review gates）

- 阶段 A 结束：全部测试通过，BLF 结果无重叠、间隙达标。✅
- 阶段 B 结束：优化结果利用率 ≥ BLF，且无回归。✅

## 回滚点

- 阶段 A 前：纯新增，无风险。
- 阶段 B 前：阶段 A 成果可作为回退基线。

## 遗留（Out of Scope，后续另立任务）

- NFP 接入候选生成（需连续角度旋转才划算）。
- 自由角度、镜像、part-in-part、瑕疵区/纹路约束。
- DXF / JSON 输出落盘（`NestResult` 已可被输出层消费）。
