# 排样引擎实现计划

> **阶段 A 已完成（2026-08-13）**：0°/90° 位姿 + Clipper2 布尔碰撞 + 贪心 BLF。新增 `ClipperPathAdapter` + `Nesting/`（5 类），10 个单测通过，全 solution 95 测试无回归。阶段 B 待做。

## 阶段 A（MVP：0°/90° + 布尔碰撞 + 贪心 BLF）

1. 复用已存在的 `Transform2D.Apply(Loop2D)`（无需新增，`TransformTests` 已覆盖）。
2. 新增共享 `ClipperPathAdapter`（Loop↔Path64，逻辑同 `OffsetAdapter` 现有实现）。
3. 新增 `Nesting/` 类型：`NestRequest` / `NestPlacement` / `NestResult`。
4. 实现 `ClipperCollisionDetector`（重叠 + 间隙 + 出界判定）。
5. 实现 `PlacementCandidateGenerator`（0°/90° × 位置候选）。
6. 实现 `NestEngine` 贪心 BLF。
7. 单元测试：碰撞、间隙、0°/90°、`unplaced`、边界行为。

**验证**：`dotnet test tests/LeatherNesting.Geometry.Tests`

## 阶段 B（优化：NFP + 遗传/局部搜索）

8. 实现 `NfpCalculator`（Clipper2 `MinkowskiSum`）。
9. 实现遗传算法 / 模拟退火搜索（以 BLF 结果为初始解）。
10. 利用率对比测试（NFP+GA 结果 ≥ BLF 基线）。

**验证**：`dotnet test tests/LeatherNesting.Geometry.Tests`

## 评审门槛（review gates）

- 阶段 A 结束：阶段 A 全部测试通过，BLF 结果无重叠、间隙达标。
- 阶段 B 结束：GA 结果利用率 ≥ BLF，且无回归。

## 回滚点

- 阶段 A 前：纯新增，无风险。
- 阶段 B 前：阶段 A 成果可作为回退基线。
