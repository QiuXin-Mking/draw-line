# LeatherNesting.Geometry

**几何层（Geometry Layer）**：所有纯几何算法与几何模型的所在地，是排样软件的计算核心。它不碰 UI、不碰文件读写，只做“轮廓、曲线、变换、排样、编辑、修复、校验”这类可单元测试的纯计算。依赖 `LeatherNesting.Domain`（仅用到极少的基础类型），并引入 **Clipper2**（多边形布尔运算 / 偏移 / Minkowski 和）。

> 约定：所有内部单位都是**毫米（mm）**，与 Clipper2 的整数坐标之间通过 `GeometryConstants` 的缩放系数转换。

```
LeatherNesting.Geometry
   ├─ 依赖：LeatherNesting.Domain、Clipper2 (NuGet)
   └─ 被引用：Application（用例编排）、Infrastructure（DXF 转换）、Desktop（画布渲染）
```

---

## 顶层文件（几何核心模型）

| 文件 | 职责 |
| --- | --- |
| `Point2D.cs` | **二维点**，带有限坐标校验。 |
| `Curve2D.cs` | **曲线基类**及三个具体实现：`LineSegment2D`（线段）、`CircularArc2D`（圆弧）、`Polyline2D`（折线）。 |
| `Loop2D.cs` | **闭合轮廓**：一组曲线组成的封闭环，带绕向归一化（外轮廓逆时针、孔顺时针）。提供点包含判定、周长、包围盒、按弧长取点等能力。 |
| `Transform2D.cs` | **二维仿射变换**：平移 + 旋转（度，逆时针）+ 镜像，可应用到点、曲线、轮廓、内部线。 |
| `InternalLine.cs` | **裁片内部线**：开缝切线（Cut）或闭合刀口标记（Mark）。`LineRole` 枚举映射到 DXF 颜色码 62（外轮廓 0 / 切割 3 / 标记 5）。 |
| `PieceGeometry.cs` | **排样裁片**：外轮廓 + 内孔 + 内部线，作为一个整体参与排样。 |
| `GeometryConstants.cs` | **项目级几何常量**：mm→Clipper2 整数坐标的缩放系数、坐标安全上限。 |
| `ToleranceProfile.cs` | **统一容差配置**：导入吸附、拓扑、弧线展平、碰撞检测、导出回读等各环节的容差，集中管理避免散落魔法数。 |
| `ClipperPathAdapter.cs` | **Loop2D ↔ Clipper2 整数路径**的共享转换（含弧线按弦长容差展平）。 |

---

## `Nesting/` — 排样引擎

| 文件 | 职责 |
| --- | --- |
| `NestModels.cs` | 排样输入/输出模型：`NestRequest`（输入）、`NestPlacement`（单个放置）、`NestResult`（放置结果 + 未放置 + 面积利用率）。 |
| `NestEngine.cs` | **贪心左下填充排样引擎**，确定性（同样输入得到同样输出）。默认按面积从大到小排，稳定 Id 兜底。 |
| `NestOptimizer.cs` | **局部搜索优化器**：打乱放置顺序多轮迭代，保留最佳结果。 |
| `NfpCalculator.cs` | **NFP（No-Fit Polygon）计算**，基于 Clipper2 Minkowski 和。 |
| `PlacementCandidateGenerator.cs` | 生成某裁片在某旋转角度下的候选放置变换。 |
| `ClipperCollisionDetector.cs` | **碰撞检测**：基于 Clipper2 布尔运算做重叠 / 间距 / 边界判定。 |

## `NodeEditing/` — 节点编辑与剪断

| 文件 | 职责 |
| --- | --- |
| `NodeOperations.cs` | 节点显示、插入、移动、删除（少于 3 个点时阻止删除）。 |
| `BreakOperations.cs` | 剪断操作：单点剪断、两点去段。 |
| `FeatureAnchorRemap.cs` | 轮廓编辑后按弧长/局部几何重映射工艺特征锚点。 |

## `Offset/` — 偏移

| 文件 | 职责 |
| --- | --- |
| `OffsetAdapter.cs` | 用 Clipper2 做轮廓偏移（外轮廓 + 内孔整体偏移，保持包含关系）。 |
| `OffsetResult.cs` | 偏移结果 + 拓扑变化跟踪；定义 `OffsetDirection`（向内/向外）与 `OffsetJoinStyle`（斜角/直角/圆角）。 |

## `Repair/` — 修复

| 文件 | 职责 |
| --- | --- |
| `ContourCloser.cs` | 闭合开轮廓：在首尾点之间补桥接线段。 |
| `GapRepair.cs` | 检测并修复断开的曲线段之间的间隙。 |
| `BoundaryGenerator.cs` | 从断开的曲线段生成候选边界。 |
| `RepairResult.cs` | 修复结果 + 桥接段来源（延伸/裁剪/新增）。 |

## `Topology/` — 拓扑

| 文件 | 职责 |
| --- | --- |
| `PlanarGraph.cs` | **半边平面图**，用于拓扑分析、提取闭合环。 |
| `ContainmentTree.cs` | 从闭合环构建外轮廓/孔的包含树。 |
| `EndpointIndex.cs` | 曲线端点索引，O(1) 邻近查询 + 交点分裂。 |
| `FaceCandidate.cs` | 从平面图提取的面候选（闭合环）。 |

## `Intersection/`、`Validation/`、`Features/`

| 目录 | 职责 |
| --- | --- |
| `Intersection/CurveIntersection.cs` | 曲线-曲线求交（线-弧、弧-弧）。 |
| `Validation/PlacementValidator.cs` | 放置校验：重叠、越界、间距、角度、镜像、数量、开轮廓、孤立特征等问题类型。 |
| `Features/NotchFeature.cs` + `NotchValidator.cs` | 刀口（Notch）特征：形状（V/方/U/半圆/标记）、锚定到轮廓弧长位置、生成几何 + 校验。 |

---

## 依赖与构建产物

- **项目引用**：`LeatherNesting.Domain`。
- **NuGet**：`Clipper2`。
- `bin/`、`obj/`：构建产物与中间文件，**无需关注**。
