# 排样引擎技术设计

## 分层与命名空间

新增 `src/LeatherNesting.Geometry/Nesting/`：

| 类型 | 职责 |
|---|---|
| `NestRequest` | 输入：`pieces`（`IReadOnlyList<Loop2D>`）、`material`（`Loop2D`）、`gapMm`、`allowedRotations` |
| `NestPlacement` | 单个结果：`pieceId` + `Transform2D` + 放置后 `Loop2D` |
| `NestResult` | `placements` + `unplaced` + `utilization` |
| `ClipperCollisionDetector` | Clipper2 布尔判重叠 / 间隙 / 出界 |
| `PlacementCandidateGenerator` | 0°/90° × 候选位置枚举 |
| `NfpCalculator` | NFP 计算（阶段 B） |
| `NestEngine` | 编排：BLF 贪心 → 遗传/局部搜索 |

复用 `OffsetAdapter` 的 Loop↔Path64 转换，提取为共享 helper（`ClipperPathAdapter`），避免重复。

## 关键决策

### 1. 变换：复用 `Transform2D.Apply`
`Transform2D` 已完整实现 `Apply(Point2D)` / `Apply(Loop2D)`（顺序：镜像 → 旋转 → 平移，含圆弧角度调整）与 `RotateAbout(centre, degrees)`。排样直接复用，无需新增变换方法。

### 2. 碰撞检测：Clipper2 布尔
- **重叠**：`Clipper.Intersect(pathA, pathB)` 非空 ⇒ 重叠（比 `PlacementValidator` 的 bounding box 精确）。
- **间隙**：对已放置 path 用 `ClipperOffset` 膨胀 `gap/2` 后再判交（或对候选 path 膨胀 gap 后与已放置判交）。
- **出界**：候选 path 必须落在材料 path 内（`Clipper.Intersect` == 候选自身，或判差集为空）。
- **整数坐标**：沿用 `GeometryConstants.IntegerScale = 1e6`，注意 `MaxSafeMillimetreCoordinate` 溢出检查。

### 3. 位姿枚举：0°/90°
`PlacementCandidateGenerator` 对每个裁片生成 {0°, 90°} 两种旋转后的 Loop；位置候选用「左下贴合」策略（对齐已放置裁片的 x/y 极值 + gap）。

### 4. 分阶段
- **阶段 A（MVP）**：0°/90° + Clipper2 布尔碰撞 + 贪心 BLF（bottom-left-fill）。目标：先跑通、替换货架填充、测试齐全。
- **阶段 B（优化）**：NFP（Clipper2 `MinkowskiSum`）+ 遗传算法/模拟退火，进一步提升利用率。

## 数据流

```
pieces[] + material + gap + {0°, 90°}
  → Loop2D.Transform（旋转候选）
  → PlacementCandidateGenerator（位姿 × 位置）
  → ClipperCollisionDetector（重叠 / 间隙 / 出界）
  → NestEngine（BLF 贪心 → GA 搜索）
  → NestResult（placements / unplaced / utilization）
```

## 兼容与回滚

- 新增命名空间，不改动现有 `Loop2D` 语义（`Transform` 为新增方法）。
- `PlacementValidator` 保留不动；碰撞判定独立实现，互不影响。
- 回滚：删除 `Nesting/` 目录即可，无侵入改动。
