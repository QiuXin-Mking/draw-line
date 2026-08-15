# 统一 Piece 模型（待办）

> 状态：**待处理**。两个裁片模型并存，需要统一。

## 背景

- `Application.Domain.Piece`（`Id/Name/Size/Outline`）：早期做的持久化裁片模型（领域模型补全任务）。
- `Geometry.PieceGeometry`（`Outer/Holes/Lines`）：后来做的排样裁片模型，含内孔 + 内部线，更完整（内部线功能引入）。

## 问题

- 两个模型并存，导致命名冲突（`Piece` vs `PieceGeometry`）。
- 代码里用别名补丁临时规避：`using PieceEntity = LeatherNesting.Application.Domain.Piece;`（`NestingProjectFactory`、持久化测试）。
- `NestingProject` 持久化用 `Application.Domain.Piece`，排样/导出用 `Geometry.PieceGeometry`，两套裁片模型割裂。

## 待办

1. 统一为一个裁片模型（倾向 `Geometry.PieceGeometry`，含洞 + 内部线，是排样的真实结构）。
2. `NestingProject` 改用统一后的模型。
3. 清理别名补丁（`PieceEntity`）。
4. 几何多态序列化覆盖 `PieceGeometry` / `InternalLine`（`[JsonConstructor]` 等）。

## 关联

- 快照恢复任务（`ProjectSnapshotStore` / `NestingProjectFactory` 当前用旧 `Piece`）。
- DXF 导出（`AsciiNestingDxfWriter` 已用 `PieceGeometry` 的颜色码约定 62=0/3/5）。
